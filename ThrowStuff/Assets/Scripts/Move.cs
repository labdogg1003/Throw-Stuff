using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour
{
	public float speed;
	public float rotateSpeed;
	public float jumpSpeed;
	public float gravity;
	Vector3 jumpMoveDirection;
	CharacterController controller;
	
	// Use this for initialization
	void Start ()
	{
		controller = GetComponent<CharacterController> ();
		
		//Needs To Be Set To Zero Initially But Not Reset Every Frame;
		jumpMoveDirection = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//rotate around the y axis based on input
		transform.Rotate (0, Input.GetAxis ("Horizontal") * rotateSpeed, 0); 
		
		//move forward or backward based on vertical axis input
		Vector3 forward = transform.TransformDirection (Vector3.forward);  //Also Same As transform.forward?
		float curSpeed = speed * Input.GetAxis("Vertical");
		controller.SimpleMove (forward * curSpeed);
		//Check If Character Controller Is Grounded
		if(controller.isGrounded)
		{
			jumpMoveDirection = transform.TransformDirection(jumpMoveDirection);
			jumpMoveDirection *= jumpSpeed;
			
			//Jump based on pressing jump button
			if (Input.GetButton ("Jump"))
			{
				jumpMoveDirection.y = jumpSpeed;
			}
		}
		
		// Subtract gravity times delta time from JumpMoveDirection
		jumpMoveDirection.y -= gravity * .016f;
		
		// then call controller.move
		controller.Move(jumpMoveDirection * .016f);
	}
}
