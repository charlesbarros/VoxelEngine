using UnityEngine;
using System.Collections;

public class FirstPersonControl : MonoBehaviour 
{		
	// This script must be attached to a GameObject that has a CharacterController
	public Joystick moveTouchPad;
	public Joystick rotateTouchPad;
	
	public Transform cameraPivot; // The transform used for camera rotation
	
	public float forwardSpeed = 4f;
	public float backwardSpeed = 1f;
	public float sidestepSpeed = 1f;
	public float jumpSpeed = 8f;
	public float inAirMultiplier = 0.25f; // Limiter for ground speed while jumping
	public Vector2 rotationSpeed = new Vector2( 50f, 25f );	// Camera rotation speed for each axis
	public float tiltPositiveYAxis = 0.6f;
	public float tiltNegativeYAxis = 0.4f;
	public float tiltXAxisMinimum = 0.1f;
	public CharacterSoundController characterSoundController;
	
	private Transform thisTransform;
	private CharacterController character;
	private Vector3 cameraVelocity;
	private Vector3 velocity; // Used for continuing momentum while in air
	private bool canJump = true;
	private Vector3 defaultPositon;
	
	void Start()
	{	
		// Cache component lookup at startup instead of doing this every frame		
		thisTransform = GetComponent<Transform>();
		character = GetComponent<CharacterController>();	
	
		defaultPositon = thisTransform.position;
		
		// Move the character to the correct start position in the level, if one exists
		var spawn = GameObject.Find( "PlayerSpawn" );
		if ( spawn )
			thisTransform.position = spawn.transform.position;
	}
	
	void OnEndGame()
	{
		// Disable joystick when the game ends	
		moveTouchPad.Disable();
		
		if ( rotateTouchPad )
			rotateTouchPad.Disable();	
	
		// Don't allow any more control changes when the game ends
		this.enabled = false;
	}
	
	void Update()
	{
		if (thisTransform.position.y <= 0.0f)
		{
			thisTransform.position = defaultPositon;
			characterSoundController.PlayWaterSplash();
		}
			
		Vector3 movement = thisTransform.TransformDirection( new Vector3( moveTouchPad.position.x, 0, moveTouchPad.position.y ) );
	
		// We only want horizontal movement
		movement.y = 0;
		movement.Normalize();
	
		// Apply movement from move joystick
		Vector2 absJoyPos = new Vector2( Mathf.Abs( moveTouchPad.position.x ), Mathf.Abs( moveTouchPad.position.y ) );	
		if ( absJoyPos.y > absJoyPos.x )
		{
			if ( moveTouchPad.position.y > 0 )
				movement *= forwardSpeed * absJoyPos.y;
			else
				movement *= backwardSpeed * absJoyPos.y;
		}
		else
		{
			movement *= sidestepSpeed * absJoyPos.x;		
		}
		
		if (absJoyPos.x > 0 || absJoyPos.y > 0)
		{
			characterSoundController.PlayWalk(true);	
		}
		if (absJoyPos.x == 0 && absJoyPos.y == 0)
		{
			characterSoundController.StopWalk();	
		}
		
		// Check for jump
		if ( character.isGrounded )
		{		
			bool jump = false;
			Joystick touchPad;
			if ( rotateTouchPad )
				touchPad = rotateTouchPad;
			else
				touchPad = moveTouchPad;
		
			if ( !touchPad.IsFingerDown() )
				canJump = true;
			
		 	if ( canJump && touchPad.tapCount >= 2 )
		 	{
				jump = true;
				canJump = false;
				//characterSoundController.PlayJump();
		 	}	
			
			if ( jump )
			{
				// Apply the current movement to launch velocity		
				velocity = character.velocity;
				velocity.y = jumpSpeed;	
			}
		}
		else
		{			
			// Apply gravity to our velocity to diminish it over time
			velocity.y += Physics.gravity.y * Time.deltaTime;
					
			// Adjust additional movement while in-air
			movement.x *= inAirMultiplier;
			movement.z *= inAirMultiplier;
		}
			
		movement += velocity;	
		movement += Physics.gravity;
		movement *= Time.deltaTime;
		
		// Actually move the character	
		character.Move( movement );
		
		if ( character.isGrounded )
		{
			// Remove any persistent velocity after landing	
			velocity = Vector3.zero;
		}
		
		// Apply rotation from rotation joystick
		if ( character.isGrounded )
		{
			var camRotation = Vector2.zero;
			
			if ( rotateTouchPad )
				camRotation = rotateTouchPad.position;
			
			camRotation.x *= rotationSpeed.x;
			camRotation.y *= rotationSpeed.y;
			camRotation *= Time.deltaTime;
			
			// Rotate the character around world-y using x-axis of joystick
			thisTransform.Rotate( 0, camRotation.x, 0, Space.World );
			
			// Rotate only the camera with y-axis input
			cameraPivot.Rotate( -camRotation.y, 0, 0 );
		}
	}
	
}