using UnityEngine;
using System.Collections;

public class Respawn : MonoBehaviour 
{
	int pointValue = 0;

	void OnControllerColliderHit(ControllerColliderHit  hit)
	{
		if(hit.gameObject.tag == "Enemy")
		{
			//transform.position = new Vector3((float)0.4331596,(float)1.978648,(float)-3.308478);
			Destroy(hit.gameObject);
			Debug.Log ("Hit Enemy!");
		}

		if(hit.gameObject.name == "Point")
		{

			//transform.position = new Vector3((float)0.4331596,(float)1.978648,(float)-3.308478);
			Destroy(hit.gameObject);
			pointValue ++;
			Debug.Log ("Gained point : " + pointValue);

		}
	}


}