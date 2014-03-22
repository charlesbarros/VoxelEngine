using UnityEngine;
using System.Collections;

// This class is used to let the player add a custom seed to the 
// Simplex3D (The noise algorithm used to generate the world data)
public class WorldConfig : MonoBehaviour 
{
	private static string _worldSeed = "Legolas";
	public static string WorldSeed { get { return _worldSeed; } }
	
	private float _widthRatio = 1;
	private float _heightRatio = 1;
	private string _buttonText = "CREATE!";
	
	void Awake()
	{
		int targetWidth = 800;
		int tagetHeight = 480;
		
		_widthRatio = (float)Screen.width / targetWidth;
		_heightRatio = (float)Screen.height / tagetHeight;
	}
	
    void OnGUI() 
	{
		float leftMargin = 50 *_widthRatio;
		
		float fildsWidth = 100 * _widthRatio;
		float fildsHeight = 20 * _heightRatio;
		
		GUI.Box(new Rect(30*_widthRatio, 30 *_heightRatio, 240 *_widthRatio, 280 *_heightRatio),"");
		
		GUI.color = Color.black;
		GUI.Label(new Rect(leftMargin, 50 * _heightRatio, fildsWidth, fildsHeight), "World Seed:");
		GUI.color = Color.white;
        _worldSeed = GUI.TextField(new Rect(leftMargin, 70 * _heightRatio, fildsWidth, fildsHeight), _worldSeed, 9);
		
		
		float buttonWorldWidth = 200 * _widthRatio;
		float buttonWorldHeight = 60 * _heightRatio;
		
		if (GUI.Button( new Rect( leftMargin, 100 * _heightRatio , buttonWorldWidth, buttonWorldHeight), _buttonText) == true)
		{
			_buttonText = "LOADING...";
			Application.LoadLevel("worldScene");	
		}
	
		float buttonBlogWidth = 300 * _widthRatio;
		float buttonBlogHeight = 20 * _heightRatio;
		
		if (GUI.Button( new Rect( 10, 450 * _heightRatio , buttonBlogWidth, buttonBlogHeight), "http://gamecoderbr.blogspot.com.br/") == true)
		{
			Application.OpenURL ("http://gamecoderbr.blogspot.com.br/");	
		}
		
		GUI.color = Color.black;
		GUI.Label(new Rect(leftMargin, 180 * _heightRatio, 220, 20), "Instructions:");
		
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			GUI.Label(new Rect(leftMargin, 200 * _heightRatio, 220, 20), "- Black buttons moves the character");
			GUI.Label(new Rect(leftMargin, 220 * _heightRatio, 220, 20), "- White buttons Create/Destroy blocks");
			GUI.Label(new Rect(leftMargin, 240 * _heightRatio, 220, 20), "- Double Tap the bottom-right button to jump");
			GUI.Label(new Rect(leftMargin, 260 * _heightRatio, 220, 20), "- Back button closes the App");				
		}
		else
		{
			GUI.Label(new Rect(leftMargin, 200 * _heightRatio, 220, 20), "- WASD moves the character:");
			GUI.Label(new Rect(leftMargin, 220 * _heightRatio, 220, 20), "- Press space to jump");
			GUI.Label(new Rect(leftMargin, 240 * _heightRatio, 220, 20), "- Left-Mouse button create blocks");
			GUI.Label(new Rect(leftMargin, 260 * _heightRatio, 220, 20), "- Right-Mouse button destroy blocks");
			GUI.Label(new Rect(leftMargin, 280 * _heightRatio, 220, 20), "- ESC enter/leave the block selection mode");
		}
    }
}
