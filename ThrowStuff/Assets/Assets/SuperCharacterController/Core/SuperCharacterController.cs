// External release version 1.0.2

using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class SuperCharacterController : MonoBehaviour
{
    public Vector3 debugMove = Vector3.zero;
    public bool debugSpheres;
    public bool debugPushbackMesssages;

    [SerializeField]
    public struct Ground
    {
        public RaycastHit Hit;
        public RaycastHit NearHit;
        public RaycastHit FarHit;
        public SuperCollisionType CollisionType;

        public Ground(RaycastHit hit, RaycastHit nearHit, RaycastHit farHit, SuperCollisionType superCollisionType)
        {
            Hit = hit;
            NearHit = nearHit;
            FarHit = farHit;
            CollisionType = superCollisionType;
        }
    }

    [SerializeField]
    CollisionSphere[] spheres =
        new CollisionSphere[3] {
            new CollisionSphere(0.5f, true, false),
            new CollisionSphere(1.0f, false, false),
            new CollisionSphere(1.5f, false, true),
        };

    public LayerMask walkable;

    public float radius = 0.5f;

    public Ground currentGround { get; private set; }
    public CollisionSphere feet { get; private set; }
    public CollisionSphere head { get; private set; }
    public float DeltaTime { get; private set; }
    public float Height { get { return Vector3.Distance(OffsetPosition(head.Offset), OffsetPosition(feet.Offset)); } }
    public Vector3 Up { get { return transform.up; } }
    public Vector3 Down { get { return -transform.up; } }

    private List<SuperCollision> collisionData;

    private Vector3 initialPosition;
    private bool clamping = true;
    private bool slopeLimiting = true;

    private List<Collider> ignoredColliders;
    private List<IgnoredCollider> ignoredColliderStack;

    private const float Tolerance = 0.05f;
    private const float TinyTolerance = 0.01f;
    private const string TemporaryLayer = "TempCast";
    private int TemporaryLayerIndex;

    public void Start()
    {
        collisionData = new List<SuperCollision>();

        TemporaryLayerIndex = LayerMask.NameToLayer(TemporaryLayer);

        ignoredColliders = new List<Collider>();
        ignoredColliderStack = new List<IgnoredCollider>();

        foreach (var sphere in spheres)
        {
            if (sphere.IsFeet)
                feet = sphere;

            if (sphere.IsHead)
                head = sphere;
        }

        if (feet == null)
            Debug.LogError("[SuperCharacterController] Feet not found on controller");

        if (head == null)
            Debug.LogError("[SuperCharacterController] Head not found on controller");
			
		gameObject.SendMessage("SuperStart", SendMessageOptions.DontRequireReceiver);
    }

    void Update()
    {
        initialPosition = transform.position;

        ProbeGround();

        transform.position += debugMove * Time.deltaTime;

        gameObject.SendMessage("SuperUpdate", SendMessageOptions.DontRequireReceiver);

        Pushback();

        ProbeGround();

        if (slopeLimiting)
            SlopeLimit();

        ProbeGround();

        if (clamping)
            ClampToGround();
    }

    bool SlopeLimit()
    {
        Vector3 n = currentGround.Hit.normal;
        float a = Vector3.Angle(n, Up);

        if (a > currentGround.CollisionType.SlopeLimit)
        {
            Vector3 absoluteMoveDirection = Math3d.ProjectVectorOnPlane(n, transform.position - initialPosition);

            // Retrieve a vector pointing down the slope
            Vector3 r = Vector3.Cross(n, Down);
            Vector3 v = Vector3.Cross(r, n);

            float angle = Vector3.Angle(absoluteMoveDirection, v);

            if (angle <= 90.0f)
                return false;

            // Calculate where to place the controller on the slope, or at the bottom, based on the desired movement distance
            Vector3 resolvedPosition = Math3d.ProjectPointOnLine(initialPosition, r, transform.position);
            Vector3 direction = Math3d.ProjectVectorOnPlane(n, resolvedPosition - transform.position);

            RaycastHit hit;

            // Check if our path to our resolved position is blocked by any colliders
            if (Physics.CapsuleCast(OffsetPosition(feet.Offset), OffsetPosition(head.Offset), radius, direction.normalized, out hit, direction.magnitude, walkable))
            {
                transform.position += v.normalized * hit.distance;
            }
            else
            {
                transform.position += direction;
            }

            return true;
        }

        return false;
    }

    void ClampToGround()
    {
        float d = currentGround.Hit.distance;
        transform.position -= Up * d;
    }

    public void EnableClamping()
    {
        clamping = true;
    }

    public void DisableClamping()
    {
        clamping = false;
    }

    public void EnableSlopeLimit()
    {
        slopeLimiting = true;
    }

    public void DisableSlopeLimit()
    {
        slopeLimiting = false;
    }

    void ProbeGround()
    {
        PushIgnoredColliders();

        Vector3 o = OffsetPosition(feet.Offset) + (Up * Tolerance);
        
        RaycastHit hit;

        if (Physics.SphereCast(o, radius, Down, out hit, Mathf.Infinity, walkable))
        {
            var wall = hit.collider.gameObject.GetComponent<SuperCollisionType>();

            if (wall == null)
            {
                // TODO: just use some derived default values?
                Debug.LogError("[SuperCharacterComponent]: Object on SuperCharacterController walkable layer does not have SuperCollisionType component attached");
            }

            // Remove the tolerance from the distance travelled
            hit.distance -= Tolerance;

            Vector3 toCenter = Math3d.ProjectVectorOnPlane(Up, (transform.position - hit.point).normalized * TinyTolerance);

            if (toCenter == Vector3.zero)
            {
                currentGround = new Ground(hit, hit, hit, wall);
				PopIgnoredColliders();
                return;
            }

            Vector3 awayFromCenter = Quaternion.AngleAxis(-80.0f, Vector3.Cross(toCenter, Up)) * -toCenter;

            Vector3 nearPoint = hit.point + toCenter + (Up * TinyTolerance);
            Vector3 farPoint = hit.point + (awayFromCenter * 3);

            RaycastHit nearHit;
            RaycastHit farHit;

            Physics.Raycast(nearPoint, Down, out nearHit, Mathf.Infinity, walkable);
            Physics.Raycast(farPoint, Down, out farHit, Mathf.Infinity, walkable);

            currentGround = new Ground(hit, nearHit, farHit, wall);
        }
        else
        {
            // Debug.LogError("[SuperCharacterComponent]: No ground was found below the player; player has escaped level");
        }

        PopIgnoredColliders();
    }

    void Pushback()
    {
        PushIgnoredColliders();

        collisionData.Clear();

        foreach (var sphere in spheres)
        {
            foreach (Collider col in Physics.OverlapSphere(OffsetPosition(sphere.Offset), radius, walkable))
            {
                Vector3 position = OffsetPosition(sphere.Offset);
                Vector3 contactPoint = SuperCollider.ClosestPointOnSurface(col, position, radius);

                if (contactPoint != Vector3.zero)
                {
                    if (debugPushbackMesssages)
                        DebugDraw.DrawMarker(contactPoint, 2.0f, Color.cyan, 0.0f, false);

                    Vector3 v = contactPoint - position;

                    if (v != Vector3.zero)
                    {
                        // Cache the collider's layer so that we can cast against it
                        int layer = col.gameObject.layer;

                        col.gameObject.layer = TemporaryLayerIndex;

                        // Check which side of the normal we are on
                        bool facingNormal = Physics.SphereCast(new Ray(position, v.normalized), TinyTolerance, v.magnitude + TinyTolerance, 1 << TemporaryLayerIndex);

                        col.gameObject.layer = layer;

                        // Orient and scale our vector based on which side of the normal we are situated
                        if (facingNormal)
                        {
                            if (Vector3.Distance(position, contactPoint) < radius)
                            {
                                v = v.normalized * (radius - v.magnitude) * -1;
                            }
                            else
                            {
                                // A previously resolved collision has had a side effect that moved us outside this collider
                                continue;
                            }
                        }
                        else
                        {
                            v = v.normalized * (radius + v.magnitude);
                        }

                        transform.position += v;

                        col.gameObject.layer = TemporaryLayerIndex;

                        // Retrieve the surface normal of the collided point
                        RaycastHit normalHit;

                        Physics.SphereCast(new Ray(position + v, contactPoint - (position + v)), TinyTolerance, out normalHit, 1 << TemporaryLayerIndex);

                        col.gameObject.layer = layer;

                        // Our collision affected the collider; add it to the collision data
                        var collision = new SuperCollision()
                        {
                            collisionSphere = sphere,
                            superCollisionType = col.gameObject.GetComponent<SuperCollisionType>(),
                            gameObject = col.gameObject,
                            point = contactPoint,
                            normal = normalHit.normal
                        };

                        collisionData.Add(collision);
                    }
                }
            }
        }
        
        PopIgnoredColliders();
    }

    protected struct IgnoredCollider
    {
        public Collider collider;
        public int layer;

        public IgnoredCollider(Collider collider, int layer)
        {
            this.collider = collider;
            this.layer = layer;
        }
    }

    private void PushIgnoredColliders()
    {
        ignoredColliderStack.Clear();

        for (int i = 0; i < ignoredColliders.Count; i++)
        {
            Collider col = ignoredColliders[i];
            ignoredColliderStack.Add(new IgnoredCollider(col, col.gameObject.layer));
            col.gameObject.layer = TemporaryLayerIndex;
        }
    }

    private void PopIgnoredColliders()
    {
        for (int i = 0; i < ignoredColliderStack.Count; i++)
        {
            IgnoredCollider ic = ignoredColliderStack[i];
            ic.collider.gameObject.layer = ic.layer;
        }

        ignoredColliderStack.Clear();
    }

    void OnDrawGizmos()
    {
        if (debugSpheres)
        {
            if (spheres != null)
            {
                foreach (var sphere in spheres)
                {
                    Gizmos.color = sphere.IsFeet ? Color.green : (sphere.IsHead ? Color.yellow : Color.cyan);
                    Gizmos.DrawWireSphere(OffsetPosition(sphere.Offset), radius);
                }
            }
        }
    }

    public Vector3 OffsetPosition(float y)
    {
        Vector3 p;

        p = transform.position;

        p += Up * y;

        return p;
    }

    public bool BelowHead(Vector3 point)
    {
        return Vector3.Angle(point - OffsetPosition(head.Offset), Up) > 89.0f;
    }

    public bool AboveFeet(Vector3 point)
    {
        return Vector3.Angle(point - OffsetPosition(feet.Offset), Down) > 89.0f;
    }

    public void IgnoreCollider(Collider col)
    {
        ignoredColliders.Add(col);
    }

    public void RemoveIgnoredCollider(Collider col)
    {
        ignoredColliders.Remove(col);
    }

    public void ClearIgnoredColliders()
    {
        ignoredColliders.Clear();
    }
}

[Serializable]
public class CollisionSphere
{
    public float Offset;
    public bool IsFeet;
    public bool IsHead;

    public CollisionSphere(float offset, bool isFeet, bool isHead)
    {
        Offset = offset;
        IsFeet = isFeet;
        this.IsHead = isHead;
    }
}

public struct SuperCollision
{
    public CollisionSphere collisionSphere;
    public SuperCollisionType superCollisionType;
    public GameObject gameObject;
    public Vector3 point;
    public Vector3 normal;
}
