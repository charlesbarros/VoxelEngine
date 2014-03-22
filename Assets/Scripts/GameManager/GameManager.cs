using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class GameManager : MonoBehaviour
{
	public GameObject[] _nonMobileObjects;
	public GameObject[] _mobileObjects;
	
	void Awake()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			Application.targetFrameRate = 30;
			
			foreach(GameObject go in _nonMobileObjects)
			{
				GameObject.DestroyImmediate(go);
			}
		}
		else
		{
			foreach(GameObject go in _mobileObjects)
			{
				GameObject.DestroyImmediate(go);
			}
		}	
	}
	
	void Update () 
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.GetKeyDown(KeyCode.Escape)) 
			{ 
				Application.Quit(); 
			}
		}
	}
}
