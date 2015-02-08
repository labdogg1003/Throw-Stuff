using UnityEngine;
using System.Collections;

/*
 * Fill this class in with any data you want to be able to access
 * pertaining to an object the controller has collided with
 */
public class SuperCollisionType : MonoBehaviour {

    public float StandAngle = 80.0f;
    public float SlopeLimit = 80.0f;
    public float SlideAngle = 30.0f;
    public float SlideContinueAngle = 30.0f;

    public bool Destructable;

    void Start()
    {
        SlideAngle = Mathf.Clamp(SlideAngle, SlideContinueAngle, Mathf.Infinity);
    }

    public void Trigger()
    {
        gameObject.SendMessage("OnCollisionTrigger", SendMessageOptions.DontRequireReceiver);
    }
}
