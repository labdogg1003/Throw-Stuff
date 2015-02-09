using UnityEngine;
using System.Collections;
using UnitySampleAssets.CrossPlatformInput;

public class Look : MonoBehaviour {

	public float XSensitivity = 15f;
	public float YSensitivity = 15f;
	public float MinimumX = -360F;
	public float MaximumX = 360F;
	public float MinimumY = -90F;
	public float MaximumY = 90F;
	
	private float xvel = 0f;
	private float yvel = 0f;
	public bool smooth;
	public float smoothtime;
	public bool Controller = false;
	
	
	public Vector2 UnClamped(float x, float y)
	{
		if(!Controller)
		{
			Vector2 value;
			value.x = y + CrossPlatformInputManager.GetAxis("Mouse X")*XSensitivity;
			value.y = x + CrossPlatformInputManager.GetAxis("Mouse Y")*YSensitivity;
			
			if (smooth)
			{
				value.x = Mathf.SmoothDamp(y, value.x, ref xvel, smoothtime);
				value.y = Mathf.SmoothDamp(x, value.y, ref yvel, smoothtime);
			}
			
			return value;
		}
		else
		{
			Vector2 value;
			value.x = y + CrossPlatformInputManager.GetAxis("Xbox360ControllerDPadX")*XSensitivity;
			value.y = x + CrossPlatformInputManager.GetAxis("Xbox360ControllerDPadY")*YSensitivity;
			
			if (smooth)
			{
				value.x = Mathf.SmoothDamp(y, value.x, ref xvel, smoothtime);
				value.y = Mathf.SmoothDamp(x, value.y, ref yvel, smoothtime);
			}	
			
			return value;
		}
	}
	
	
	public Vector2 Clamped(float x, float y)
	{
		if(!Controller)
		{
			Vector2 value;
			value.x = y + CrossPlatformInputManager.GetAxis("Mouse X")*XSensitivity;
			value.y = x + CrossPlatformInputManager.GetAxis("Mouse Y")*YSensitivity;
			
			value.x = Mathf.Clamp(value.x, MinimumX, MaximumX);
			value.y = Mathf.Clamp(value.y, MinimumY, MaximumY);
			
			if (smooth)
			{
				value.x = Mathf.SmoothDamp(y, value.x, ref xvel, smoothtime);
				value.y = Mathf.SmoothDamp(x, value.y, ref yvel, smoothtime);
			}	
			
			return value;
		}
		else
		{
			Vector2 value;
			value.x = y + CrossPlatformInputManager.GetAxis("Xbox360ControllerDPadX")*XSensitivity;
			value.y = x + CrossPlatformInputManager.GetAxis("Xbox360ControllerDPadY")*YSensitivity;
			
			value.x = Mathf.Clamp(value.x, MinimumX, MaximumX);
			value.y = Mathf.Clamp(value.y, MinimumY, MaximumY);
			
			if (smooth)
			{
				value.x = Mathf.SmoothDamp(y, value.x, ref xvel, smoothtime);
				value.y = Mathf.SmoothDamp(x, value.y, ref yvel, smoothtime);
			}	
			
			return value;
		}
	}

}
