using UnityEngine;
using System.Collections;
using System;

public class Button : MonoBehaviour 
{
	public bool _toggle = false;
	public Color _pressedColor = Color.white;
	public Color _releasedColor = Color.white;
	public Action EventPressed;
	
	private GUITexture _gui;
	private bool _pressed = false;
	
	public void Awake()
	{
		_gui = GetComponent<GUITexture>();	
		
		_gui.AutoResize();
	}
	
	public void Update()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			int touchCount = Input.touchCount;	
			
			if (touchCount >= 1)
			{
				for(int i = 0;i < touchCount; i++)
				{
					Touch touch = Input.GetTouch(i);
					
					if ( touch.phase == TouchPhase.Began && _gui.HitTest( touch.position ) == true)
					{
						ToggleFx();
						
						if (EventPressed != null)
						{
							EventPressed();	
						}
					}
				}
			}
		}
		else
		{
			if (Input.GetMouseButtonDown(0) == true)
			{
				if ( _gui.HitTest( Input.mousePosition ) == true)
				{
					ToggleFx();
					
					if (EventPressed != null)
					{
						EventPressed();	
					}
				}
			}
		}
	}
	
	public void ForcePress()
	{
		_pressed = true;
		_gui.color = _pressedColor;			
	}
	
	public void ForceRelease()
	{
		_pressed = false;
		_gui.color = _releasedColor;		
	}
	
	private void ToggleFx()
	{
		if (_toggle == true)
		{
			if (_pressed == false)
			{
				_pressed = true;
				_gui.color = _pressedColor;
			}
			else
			{
				_pressed = false;
				_gui.color = _releasedColor;			
			}	
		}
	}
}
