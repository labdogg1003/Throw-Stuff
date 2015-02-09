using UnityEngine;
using System.Collections;

public class Pickupable : MonoBehaviour 
{
	GameObject mainCamera;
	public bool held = false;
	public float gravity = 20.0f;

	// Use this for initialization
	void Start ()
	{
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		rigidbody.useGravity = false;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		if(held == true)
		{
			moveObject();
		}
		else
		{
			rigidbody.AddForce(new Vector3 (0, -gravity * rigidbody.mass, 0));
		}
	}

	void moveObject()
	{
		rigidbody.AddForce (mainCamera.transform.position - transform.position);
	}
}
