using UnityEngine;
using System.Collections;

[RequireComponent (typeof(CharacterController))]
[AddComponentMenu ("Character/Character Motor")]
public class CharacterMotor : MonoBehaviour {
	
	private CharacterController controller;

	[System.NonSerialized]
	public Vector3 inputMoveDirection = Vector3.zero;
	
	[System.NonSerialized]
	public bool inputJump = false;

	public CharacterMotorMovement movement = new CharacterMotorMovement();
	public CharacterMotorJumping jumping = new CharacterMotorJumping();
	public CharacterMotorSliding sliding = new CharacterMotorSliding();
	public CharacterSoundController soundController;
	
	private Vector3 defaultPositon = Vector3.zero;
	private Vector3 groundNormal = Vector3.zero;
	private Collider ground = null;

	void Awake() {
		controller = GetComponent<CharacterController>();
		defaultPositon = transform.position;
	}

	private void FixedUpdate() 
	{
		if (transform.position.y <= 0.0f)
		{
			transform.position = defaultPositon;
			soundController.PlayWaterSplash();
		}
		
		Vector3 velocity = movement.velocity;
		velocity = ApplyMoveDirection(velocity);
		velocity = ApplyGravity(velocity);
		velocity = ApplyJumping(velocity);
		
		Vector3 offset = Vector3.zero;
		if(IsGrounded() && !IsJumping()) {
			offset = Vector3.up * Mathf.Max(controller.stepOffset/Time.deltaTime, new Vector2(velocity.x, velocity.z).magnitude);
		}

		bool oldGrounded = IsGrounded();
		groundNormal = Vector3.zero;
		ground = null;
		controller.Move( (velocity-offset)*Time.deltaTime );
	
		// Calculate the velocity based on the current and previous position.  
		// This means our velocity will only be the amount the character actually moved as a result of collisions.
		Vector3 oldHVelocity = new Vector3(velocity.x, 0, velocity.z);
		movement.velocity = controller.velocity;
		Vector3 newHVelocity = new Vector3( movement.velocity.x, 0, movement.velocity.z );
	
		if(offset != Vector3.zero && !IsGrounded()) {
			transform.position += offset * Time.deltaTime;
			movement.velocity += offset;
		}
		
		// The CharacterController can be moved in unwanted directions when colliding with things.
		// We want to prevent this from influencing the recorded velocity.
		if (oldHVelocity != Vector3.zero) {
			float projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
			movement.velocity = oldHVelocity * Mathf.Clamp01(projectedNewVelocity) + movement.velocity.y * Vector3.up;
		}
	
		if(oldGrounded && !IsGrounded()) {
			if(IsJumping()) {
				SendMessage( "OnJump", SendMessageOptions.DontRequireReceiver );
			} else {
				SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
			}
		}
		if(!oldGrounded && IsGrounded()) {
			SendMessage("OnLand", SendMessageOptions.DontRequireReceiver);
		}
	}

	private Vector3 ApplyMoveDirection( Vector3 velocity ) {
		// Find desired velocity
		Vector3 desiredVelocity;
		if ( IsSliding() ) { // скользим
			// The direction we're sliding in
			desiredVelocity = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
			// Find the input movement direction projected onto the sliding direction
			Vector3 projectedMoveDir = Vector3.Project(inputMoveDirection, desiredVelocity);
			// Add the sliding direction, the spped control, and the sideways control vectors
			desiredVelocity = desiredVelocity + projectedMoveDir * sliding.speedControl + (inputMoveDirection - projectedMoveDir) * sliding.sidewaysControl;
			
			float slidingSpeed = movement.gravity * (1-Mathf.Clamp01(groundNormal.y)); //скорость скольжения
			desiredVelocity *= slidingSpeed;
		} else {
			desiredVelocity = GetInputVelocity();
		}
	
		if( IsGrounded() ) {
			desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, groundNormal);
		}
		
		Vector3 deltaVelocity = desiredVelocity - new Vector3(velocity.x, 0, velocity.z);
		float maxDeltaVelocity = movement.GetMaxAcceleration(IsGrounded()) * Time.deltaTime;
		deltaVelocity = Vector3.ClampMagnitude(deltaVelocity, maxDeltaVelocity);
		return velocity + deltaVelocity;
	}
	
	private Vector3 ApplyGravity(Vector3 velocity) {
		velocity.y -= movement.gravity * Time.deltaTime;
		velocity.y = Mathf.Max(velocity.y, -movement.maxFallSpeed);
		if(IsGrounded()) velocity.y = Mathf.Min(velocity.y, 0);
		return velocity;
	}

	private Vector3 ApplyJumping(Vector3 velocity) {
		if(IsGrounded()) {
			jumping.jumping = false;
		}
		if(!inputJump || !jumping.jumping || IsTouchingCeiling()) {
			jumping.holdingJumpButton = false;
		}
	
		// When jumping up we don't apply gravity for some time when the user is holding the jump button.
		// This gives more control over jump height by pressing the button longer.
		if (jumping.jumping && jumping.holdingJumpButton) {
			// Calculate the duration that the extra jump force should have effect.
			// If we're still less than that duration after the jumping time, apply the force.
			if (Time.time < jumping.jumpStartTime + jumping.extraHeight / CalculateJumpVerticalSpeed(jumping.baseHeight)) {
				// Negate the gravity we just applied, except we push in jumpDir rather than jump upwards.
				velocity += jumping.jumpDir * movement.gravity * Time.deltaTime;
			}
		}
		
		// Jump only if the jump button was pressed down in the last 0.2f seconds.
		// We use this check instead of checking if it's pressed down right now
		// because players will often try to jump in the exact moment when hitting the ground after a jump
		// and if they hit the button a fraction of a second too soon and no new jump happens as a consequence,
		// it's confusing and it feels like the game is buggy.
		if (IsGrounded() && inputJump) {
			jumping.jumping = true;
			jumping.holdingJumpButton = true;
			jumping.jumpStartTime = Time.time;
			
			// Calculate the jumping direction
			if (IsSliding())
				jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.steepPerpAmount);
			else
				jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.perpAmount);
			
			// Apply the jumping force to the velocity. Cancel any vertical velocity first.
			velocity.y = 0;
			velocity += jumping.jumpDir * CalculateJumpVerticalSpeed(jumping.baseHeight);
				
			SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);
		}
	
		return velocity;
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.normal.y > 0.001f && hit.normal.y > groundNormal.y && hit.moveDirection.y < 0) {
			groundNormal = hit.normal;
			ground = hit.collider;
		}
	}

	private Vector3 GetInputVelocity() {
		// Find desired velocity
		Vector3 localDirection = transform.InverseTransformDirection(inputMoveDirection);
		float maxSpeed = movement.GetMaxSpeedInDirection(localDirection);
		if (IsGrounded()) {
			// Modify max speed on slopes based on slope speed multiplier curve
			float movementSlopeAngle = Mathf.Asin(movement.velocity.normalized.y) * Mathf.Rad2Deg;
			maxSpeed *= movement.slopeSpeedMultiplier.Evaluate(movementSlopeAngle);
		}
		return transform.TransformDirection(localDirection) * maxSpeed;
	}

	private Vector3 AdjustGroundVelocityToNormal(Vector3 hVelocity, Vector3 groundNormal) {
		Vector3 sideways = Vector3.Cross(Vector3.up, hVelocity);
		return Vector3.Cross(sideways, groundNormal).normalized * hVelocity.magnitude;
	}

	public float CalculateJumpVerticalSpeed(float targetJumpHeight) {
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt (2 * targetJumpHeight * movement.gravity);
	}

	public bool IsJumping() {
		return jumping.jumping;
	}

	public bool IsSliding() {
		return IsGrounded() && groundNormal.y <= Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad);
	}

	public bool IsTouchingCeiling() {
		return (controller.collisionFlags & CollisionFlags.CollidedAbove) != 0;
	}

	public bool IsGrounded() {
		return ground != null;
	}
	
	public Collider GetGround() {
		return ground;
	}

	public Vector3 GetDirection() {
		return inputMoveDirection;
	}

}



[System.Serializable]
public class CharacterMotorMovement 
{
	public float maxForwardSpeed = 6f;
	public float maxSidewaysSpeed = 6f;
	public float maxBackwardsSpeed = 6f;
	public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 0));
	public float maxGroundAcceleration = 20.0f;
	public float maxAirAcceleration = 10.0f;
	public float gravity = 20.0f;
	public float maxFallSpeed = 20.0f;

	// We will keep track of the character's current velocity,
	[System.NonSerialized]
	public Vector3 velocity;
	
	public float GetMaxAcceleration(bool grounded) {
		// Maximum acceleration on ground and in air
		if(grounded) return maxGroundAcceleration;
		return maxAirAcceleration;
	}
	
	public float GetMaxSpeedInDirection(Vector3 localDirection) {
		if (localDirection == Vector3.zero) return 0;
		float zAxisEllipseMultiplier = (localDirection.z > 0 ? maxForwardSpeed : maxBackwardsSpeed) / maxSidewaysSpeed;
		Vector3 temp = new Vector3(localDirection.x, 0, localDirection.z / zAxisEllipseMultiplier).normalized;
		return new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * maxSidewaysSpeed;
	}
}

[System.Serializable]
public class CharacterMotorSliding 
{
	// If the value is 0.5f the player can slide sideways with half the speed of the downwards sliding speed.
	public float sidewaysControl = 1.0f;

	// если значение 0.5, то игрок может замедлить скольжение на 50% или ускорить на 150% 
	public float speedControl = 0.4f;
}

// We will contain all the jumping related variables in one helper class for clarity.
[System.Serializable]
public class CharacterMotorJumping 
{
	public float baseHeight = 1.0f;
	// We add extraHeight units (meters) on top when holding the button down longer while jumping
	public float extraHeight = 1.0f;
	public float perpAmount = 0.0f;
	public float steepPerpAmount = 0.5f;
	
	// For the next variables, @System.NonSerialized tells Unity to not serialize the variable or show it in the inspector view.
	// Very handy for organization!
	[System.NonSerialized] public bool jumping = false;

	[System.NonSerialized] public float jumpStartTime = 0.0f;
	
	[System.NonSerialized] public bool holdingJumpButton = false;
	
	[System.NonSerialized] public Vector3 jumpDir = Vector3.up;
}