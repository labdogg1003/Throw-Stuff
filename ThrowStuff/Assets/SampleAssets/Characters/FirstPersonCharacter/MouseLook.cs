using UnityEngine;
using System.Collections;
using UnitySampleAssets.CrossPlatformInput;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add a rigid body to the capsule
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSWalker script to the capsule

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour {

	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;

	float rotationX = 0F;
	float rotationY = 0F;

	public bool controller;
	
	Quaternion originalRotation;

	void Update ()
	{
		if (axes == RotationAxes.MouseXAndY)
		{
			rotationX += CrossPlatformInputManager.GetAxis("Xbox360ControllerDPadX")* sensitivityX;
			rotationY += CrossPlatformInputManager.GetAxis("Xbox360ControllerDPadY") * sensitivityY;

			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

			rotationX = ClampAngle (rotationX, minimumX, maximumX);
			rotationY = ClampAngle (rotationY, minimumY, maximumY);
			
			Quaternion xQuaternion = Quaternion.AxisAngle (Vector3.up, Mathf.Deg2Rad * rotationX);
			Quaternion yQuaternion = Quaternion.AxisAngle (Vector3.left, Mathf.Deg2Rad * rotationY);
			
			transform.localRotation = originalRotation * xQuaternion * yQuaternion;

		}
		else if (axes == RotationAxes.MouseX)
		{
		    rotationX += CrossPlatformInputManager.GetAxis("Xbox360ControllerDPadX")* sensitivityX;
		    rotationX += Input.GetAxis("Mouse X") * sensitivityX;

			rotationX = ClampAngle (rotationX, minimumX, maximumX);

			Quaternion xQuaternion = Quaternion.AxisAngle (Vector3.up, Mathf.Deg2Rad * rotationX);
			transform.rotation = Quaternion.Slerp(transform.rotation, xQuaternion, Time.deltaTime * 15.0f);
		}
		else
		{

			rotationY += CrossPlatformInputManager.GetAxis("Xbox360ControllerDPadY")* sensitivityY;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

			rotationY = ClampAngle (rotationY, minimumY, maximumY);

			Quaternion yQuaternion = Quaternion.AxisAngle (Vector3.left, Mathf.Deg2Rad * rotationY);
			transform.localRotation = originalRotation * yQuaternion;
		}
	}
	
	void Start ()
	{
		// Make the rigid body not change rotation
		if (rigidbody)
			rigidbody.freezeRotation = true;
		originalRotation = transform.localRotation;
	}
	
	public static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp (angle, min, max);
	}
}