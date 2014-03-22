using UnityEngine;
using System.Collections;

public class GUITextureResizer : MonoBehaviour 
{
	void Awake () 
	{
		GUITexture gui = GetComponent<GUITexture>();
		gui.AutoResize();
	}

}
