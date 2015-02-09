	using UnityEngine;
	using System.Collections;	

public class PhysicBasedCharacterController : MonoBehaviour
{
	private bool isGrounded;
	public float movementForce = 30;
    public float jumpForce=20;
	public float normalLimit =0;
    public bool parentToColliders = true;
    private bool kinematic = false;
	private bool onPlatform =false;
    private int jumping =0;
	public string[] ParentsWhiteList;
	
	// Use this for initialization
		
	void Start () 
	{
			
	}
	

	public void setOnPlatform(bool op)
	{
	   onPlatform = op;
	}
	

		// Update is called once per frame
		
		
	void Update ()
	{
	   jumping--;
	   
	   kinematic = (!((Input.GetAxisRaw("Vertical")>0) || (Input.GetAxisRaw("Vertical")<0) || (Input.GetAxisRaw("Horizontal")>0) || (Input.GetAxisRaw("Horizontal")<0) || (Input.GetButtonDown("Jump")))) && (isGrounded);
			

	   if (Input.GetAxisRaw("Vertical")>0)
	   {
	      this.rigidbody.AddForce(this.transform.forward * movementForce);
	   }
	   
	   else if (Input.GetAxisRaw("Vertical")<0)
	   {
		  this.rigidbody.AddForce(-this.transform.forward * movementForce);
	   }
			

	   if (Input.GetAxisRaw("Horizontal")>0)
	   {
	      this.rigidbody.AddForce(this.transform.right * movementForce);
	   }
	   
	   else if (Input.GetAxisRaw("Horizontal")<0)
	   {
	      this.rigidbody.AddForce(-this.transform.right * movementForce);
	   }
			
	   if ((isGrounded) && (Input.GetButtonDown("Jump")))
	   {
	      this.rigidbody.isKinematic=false;
		  kinematic=false;
		  isGrounded = false;
		  jumping = 3;
		  this.rigidbody.AddForce(this.transform.up * jumpForce);				
       }
	
	   this.rigidbody.isKinematic=(kinematic) && (onPlatform) && (jumping<1);
	}
	

	void OnGUI()
	{
			//GUI.Box(new Rect(400,0,200,50),"isGrounded = "+isGrounded.ToString()+"\n"+"Jump = "+Input.GetButton("Jump").ToString());
	}
	

	void OnCollisionStay(Collision other)
	{
		foreach(ContactPoint temp in other.contacts)
		{
			if((temp.normal.y>normalLimit))
			{
				isGrounded=true;
			}
		}

		//following is needed for moving platforms to work
			
	    bool canParent = false;
		foreach (string name in ParentsWhiteList)
		{
			if (other.gameObject.name == name) {canParent = true;}
		}
	
		if ((parentToColliders) && (isGrounded) && (canParent))
		{
			this.transform.parent = other.gameObject.transform;
		}
	}
	

	void OnCollisionExit(Collision other)
	{
		isGrounded=false;
	}
}

