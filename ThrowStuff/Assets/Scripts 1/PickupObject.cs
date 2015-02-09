using UnityEngine;
using System.Collections;

public class PickupObject : MonoBehaviour {

	GameObject mainCamera;
	bool carrying;
	GameObject carriedObject;
	Component cc;
	public float distance = 1.0f;
	public float smooth = 4.0f;
	public float force;
	Vector3 targetPoint;
	float cooldown = 0;
	bool triggerPulled;
	bool axisChanged;
	float previousAxisValue;

	public float mCorrectionForce = 50.0f;
	public float mPointDistance = 3.0f;

	// Use this for initialization
	void Start ()
	{
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
	}
	
	// Update is called once per frame
	void Update ()
	{
        //Check if Trigger Is Pulled
       	if( Input.GetAxisRaw("Fire2") == 1)
		{
			//Check If Input Is Same As Previous Input, Prevent Multiple Inputs On Same Trigger Pull.
			axisChanged = (previousAxisValue != Input.GetAxisRaw ("Fire2"));

		  	if(axisChanged)
		   	{
				triggerPulled = true;

				//Store Trigger Value From Trigger Pull
				previousAxisValue = Input.GetAxisRaw("Fire2");
		   	}
			else
			{
				triggerPulled = false;
			}
		}
		else
		{
			previousAxisValue = 0;
		}

	  
	   if(carrying)
	   {
			carry(carriedObject);
			checkDrop();
	   }
	   else
	   {
			pickup();
	   }

	   triggerPulled = false;
	}

	void carry(GameObject o)
	{
		moveObject();
		//o.GetComponent<Pickupable>().held = true;
		//o.rigidbody.AddForce (mainCamera.transform.position - o.transform.position);
		//o.transform.position = Vector3.Slerp(o.transform.position, mainCamera.transform.position + mainCamera.transform.forward * distance, Time.deltaTime * smooth);
		//o.gameObject.GetComponent<Rigidbody>().MovePosition( Vector3.Slerp(o.transform.position, mainCamera.transform.position + mainCamera.transform.forward * distance, Time.deltaTime * smooth));
		//o.gameObject.GetComponent<MeshCollider> ().enabled = false;
	}

	void pickup()
	{
		if( triggerPulled )
		{
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;

			if(Physics.Raycast(ray, out hit))
			{
				Pickupable p = hit.collider.GetComponent<Pickupable>();
				if(p != null)
				{
					carrying = true;
					carriedObject = p.gameObject;
				}
			}
			cooldown = 1.0f;
		}
	}

	void checkDrop()
	{
		if( triggerPulled )
		{
			dropObject();
		}
	}
	
	void dropObject()
	{
		carrying = false;
		carriedObject.gameObject.GetComponent<Rigidbody> ().velocity = new Vector3 (0, 0, 0) + (mainCamera.transform.forward * force); // carriedObject.gameObject.GetComponent<Rigidbody> ().mass);
		carriedObject.rigidbody.freezeRotation = false;
		carriedObject = null;

	}

	void moveObject()
	{
		carriedObject.rigidbody.freezeRotation = true;

		targetPoint = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
		targetPoint += Camera.main.transform.forward * mPointDistance;
		Vector3 force = targetPoint - carriedObject.transform.position;
		carriedObject.rigidbody.velocity = force.normalized * carriedObject.rigidbody.velocity.magnitude;
		carriedObject.rigidbody.AddForce(force * mCorrectionForce);
		
		carriedObject.rigidbody.velocity *= Mathf.Min(1.0f, force.magnitude / 2);
	}
}
