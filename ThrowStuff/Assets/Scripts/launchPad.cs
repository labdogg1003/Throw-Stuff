using UnityEngine;
using System.Collections;

public class launchPad : MonoBehaviour 
{
	Vector3 launchDirection;
	Component hitRigidBody;
	public float force = 100.0f;

	// Use this for initialization
	void Start ()
	{
		launchDirection = transform.forward;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	void OnTriggerEnter(Collider other)
	{
		other.rigidbody.AddForce (launchDirection * force);
	}
}
