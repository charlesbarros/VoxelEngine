using UnityEngine;
using System.Collections;

public static class GUITextureExtension 
{
	// Samsung Galaxy S2 was choosen as the target resolution of this project
	public static int _targetWidth = 800;
	public static int _tagetHeight = 480;
	
	public static void AutoResize(this GUITexture guiTexture)
	{
		Rect rect = guiTexture.pixelInset;
		
		float widthRatio = (float)Screen.width / _targetWidth;
		rect.x *= widthRatio;
		rect.width *= widthRatio;
		
		float heightRatio = (float)Screen.height / _tagetHeight;
		rect.y *= heightRatio;
		rect.height *= heightRatio;
		
		guiTexture.pixelInset = rect;
	}  
}
