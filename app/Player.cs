using Godot;
using System;
using System.Collections.Generic;

public class Player : KinematicBody2D
{
	#region SignalValues
	[Signal]
	public delegate void Collided();
	[Signal]
	public delegate void Damage();
	[Signal]
	public delegate void PlayerDied();
	#endregion

	#region ExportedValues
	[Export]
	private float ACCELERATION = 12;
	[Export]
	private float RUN_SPEED = 96;
	[Export]
	private float AIR_DASH_SPEED_BOOST = 120;
	[Export]
	private float FRICTION = 16;
	[Export]
	private float AIR_RESISTANCE = 1;
	[Export]
	private float GRAVITY = 8;
	[Export]
	public float JUMP_FORCE = 240;
	[Export]
	public float FAST_FALL_FACTOR = 0.75f;
	[Export]
	public float AIR_DASH_DISCOUNT_FACTOR = 0.15f;
	[Export]
	public float COYOTE_TIME = 0.1f;
	[Export]
	public float AIR_DASH_TIME = 0.25f;
	#endregion

	#region Constants
	private const string INPUT_UP = "ui_up";
	private const string INPUT_RIGHT = "ui_right";
	private const string INPUT_LEFT = "ui_left";
	private const string INPUT_DOWN = "ui_down";
	#endregion

	#region Properties
	public bool IsGrounded
	{
		get { return this.State.IsGrounded; }
		set { this.State.IsGrounded = value; }
	}

	public bool IsMidair
	{
		get { return !IsGrounded; }
		set { this.IsGrounded = !value; }
	}

	public bool IsAirDashing
	{
		get { return this.State.IsAirDashing; }
	}

	public bool IsDead
	{
		get { return this.State.IsDead; }
		set { this.State.IsDead = value; }
	}

	public PlayerState State
	{
		get { return this.playerState; }
	}

	public List<KinematicCollision2D> CurCollisions
	{
		get { return GetCollisions(); }
	}

	public float MaxSpeed
	{
		get 
		{
			float output = RUN_SPEED;
			
			if (this.IsAirDashing)
			{
				output += AIR_DASH_SPEED_BOOST;
			}

			return output;
		}
	}

	public float TotalFriction
	{
		get
		{
			float output = FRICTION;

			if ((!this.IsGrounded) || this.IsAirDashing)
			{
				output = AIR_RESISTANCE;
			}
			
			return output;
		}
	}

	#endregion

	#region Fields
	private PlayerState playerState = new PlayerState();
	private Vector2 motion = Vector2.Zero;
	#endregion

	#region Nodes
	private Sprite sprite = null;
	private AnimationPlayer animationPlayer = null;
	private List<RayCast2D> raycasts = new List<RayCast2D>();
	private Timer coyoteTimer = null;
	private Timer airDashTimer = null;
	#endregion

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.sprite = this.GetNode<Sprite>("Sprite");
		this.animationPlayer = this.GetNode<AnimationPlayer>("AnimationPlayer");
		this.raycasts.Add(this.GetNode<RayCast2D>("RayCast2D_0"));
		this.raycasts.Add(this.GetNode<RayCast2D>("RayCast2D_1"));
		this.coyoteTimer = this.GetNode<Timer>("CoyoteTimer");
		this.coyoteTimer.WaitTime = COYOTE_TIME;
		this.airDashTimer = this.GetNode<Timer>("AirDashTimer");
		this.airDashTimer.WaitTime = AIR_DASH_TIME;
	}

	public override void _PhysicsProcess(float delta)
	{
		if (this.IsDead)
		{
			// Plays the "Death" animation once 
			// (The reason why we don't use "Play" is because it will 
			// repeatedly play the death animation over and over again)
			this.animationPlayer.CurrentAnimation = "Death";
			return;
		}

		// GD.Print(motion);

		// Emit collision signals for each collision detected
		foreach (KinematicCollision2D kc in this.CurCollisions)
		{
			EmitSignal("Collided", kc, this);
		}

		Vector2 dirInfluence = PlayerInput.GetDirectionalInflence();

		bool currentlyTouchingGround = IsTouchingGround(this.raycasts);
		
		bool justLanded = (this.IsGrounded != currentlyTouchingGround) ? currentlyTouchingGround : false;
		bool justOnAir = (this.IsGrounded != currentlyTouchingGround) ? !currentlyTouchingGround : false;

		this.IsGrounded = currentlyTouchingGround;
		if (this.IsGrounded)
		{
			// If the player is somehow grounded before the coyoteTimer has triggered, stop it. 
			if (!this.coyoteTimer.IsStopped())
			{
				this.coyoteTimer.Stop();
			}
		}
		else 
		{
			if (this.State.CanStartCoyoteTime)
			{
				this.State.CoyoteTimeStarted();
				this.coyoteTimer.Start(COYOTE_TIME);
			}
		}

		if (this.IsAirDashing)
		{
			int horizDI = Mathf.RoundToInt(dirInfluence.x);
			horizDI = (horizDI != 0) ? horizDI/Mathf.Abs(horizDI) : 0;
			
			int horizTrajectory = Mathf.RoundToInt(motion.x);
			horizTrajectory = (horizTrajectory != 0) ? horizTrajectory/Mathf.Abs(horizTrajectory) : 0;

			if (justOnAir || ((horizTrajectory + horizDI) == 0))
			{
				this.State.AirDashFinished();
			}

		}

		// GD.Print(String.Format("IsGrounded: {0}", this.IsGrounded));

		// Process the player's movement (by player input)
		motion = ProcessMovement(motion, delta);

		// Determine the current animation
		string animationName = ProcessAnimation(motion);
		if (animationName != null) { this.animationPlayer.Play(animationName); }

		bool changeSpriteDirection = (dirInfluence.x != 0) && ((!this.State.IsAirDashing) || this.IsGrounded);
		if (changeSpriteDirection) 
		{ 
			this.sprite.FlipH = dirInfluence.x > 0; 
		}

		motion = MoveAndSlide(motion, Vector2.Up);
	}

	private Vector2 ProcessMovement(Vector2 trajectory, float delta) 
	{
		// Horizontal Resistance Factor is determines how fast the player stops moving in the x axis.
		float totalFriction = this.TotalFriction;
		// Get target fps from the Game Engine.
		float TARGET_FPS = Engine.GetFramesPerSecond();

		// Retrieve the player's input in the horizontal/vertical direction.
		Vector2 dirInfluence = PlayerInput.GetDirectionalInflence();

		if (dirInfluence.x != 0)
		{
			// If the horizontal input is not 0... (Player is pressing the left or right direction but not both)
			trajectory.x += dirInfluence.x * ACCELERATION * delta * TARGET_FPS;

			// Acts as a way to gently slow the player to the MAX_SPEED in either direction if
			// the player has exceeded the speed limit.
			float excessSpeed = Mathf.Abs(trajectory.x) - this.MaxSpeed;
			if (excessSpeed > 0)
			{
				// Retrieve the current diection the trajectory is travelling towards
				float xdirection = (trajectory.x > 0) ? 1 : -1;
				// Obtain the new speed using linear interpolation
				float newSpeed = Mathf.Lerp(Mathf.Abs(trajectory.x), this.MaxSpeed, (excessSpeed) * totalFriction * delta);
				// There may be an edge case that causes the newSpeed to go below MAX_SPEED 
				// (this causes stuttering for the player character),
				// so if the newSpeed is below MAX_SPEED, set it to MAX_SPEED.
				trajectory.x = xdirection * ((newSpeed < this.MaxSpeed) ? this.MaxSpeed : newSpeed);
			}
		}
		else
		{
			// If the horizontal input is 0... (Player is not inputting to any specific direction horizontally)

			// Gradually approach 0 from the trajectory's original x value by the weight of (resistance factor * delta)
			trajectory.x = Mathf.Lerp(trajectory.x, 0, totalFriction * delta);
		}

		// Determine the downwards trajectory in the vertical direction
		float gravity = (this.IsAirDashing) ? GRAVITY/2 : GRAVITY;
		trajectory.y += gravity * delta * TARGET_FPS;

		if (this.State.CanJump) 
		{
			// If the "jump" button was just pressed, then travel in the 
			// upwards direction by the JUMP_FORCE's magnitude.
			if (PlayerInput.IsJumpButtonJustPressed()) 
			{
				this.State.Jumped();
				trajectory.y = -JUMP_FORCE;
			}
		}
		else 
		{
			// If the "jump" button was just released AND that the upwards trajectory 
			// is still greater than half of the JUMP_FORCE,
			// then set the trajectory to half their jump force.
			if (PlayerInput.IsJumpButtonJustReleased() && trajectory.y < -JUMP_FORCE/2) 
			{
				trajectory.y = -JUMP_FORCE/2;
			}
		}

		if (this.State.CanAirDash)
		{
			if (PlayerInput.IsDashButtonPressed() && (Math.Abs(dirInfluence.x) != 0))
			{
				float totalMomentum = (Mathf.Abs(trajectory.x) + Mathf.Abs(trajectory.y)) * (1 - AIR_DASH_DISCOUNT_FACTOR);
				totalMomentum += AIR_DASH_SPEED_BOOST;
				totalMomentum = Mathf.Clamp(totalMomentum, 0, 2 * AIR_DASH_SPEED_BOOST);

				float totalInfluence = Mathf.Abs(dirInfluence.x) + Mathf.Abs(dirInfluence.y);
				Vector2 normalizedInfluence = (totalInfluence != 0) ? dirInfluence/totalInfluence : dirInfluence;

				trajectory.x = normalizedInfluence.x * totalMomentum;
				trajectory.y = normalizedInfluence.y * totalMomentum;

				this.State.AirDashed();
				this.airDashTimer.Start(AIR_DASH_TIME);
			}
		}

		if ((dirInfluence.y > 0.6f))
		{
			// If the directional influence is pointing downward 
			// with enough of a magnitude (0.6f = 60%)...
			if (this.State.CanFastFall && false)
			{
				// If the player can fast fall, 
				// then increase the player's momentum downwards
				this.State.FastFallen();
				trajectory.y += JUMP_FORCE * FAST_FALL_FACTOR;
			}

			if (this.IsGrounded) 
			{ 
				this.State.AirDashFinished();
				trajectory.x = 0; 
			}
		}

		return trajectory;
	}

	private string ProcessAnimation(Vector2 trajectory)
	{
		string animationName = "Stand";

		if (this.IsGrounded) 
		{
			// Determine the sprite based on the directional influence of the player
			Vector2 dirInfluence = PlayerInput.GetDirectionalInflence();

			if (dirInfluence.y > 0.6f)
			{
				animationName = "Crouch";
			}
			else if (dirInfluence.x != 0) 
			{
				animationName = "Run";
			}
		}
		else
		{
			if (this.State.IsAirDashing)
			{
				animationName = "AirDash";
			}
			else
			{
				// Determine the sprite depending on the trajectory/motion of the player
				animationName = (IsPlayerMovingUp(motion)) ? "Jump" : "Fall";
			}
			
		}

		return animationName;
	} 

	private bool IsTouchingGround(List<RayCast2D> raycasts) 
	{
		bool raycastCollisions = false;
		foreach (RayCast2D raycast in raycasts) 
		{
			raycastCollisions = raycastCollisions || raycast.IsColliding();
		}

		return raycastCollisions || IsOnFloor();
	}

	private bool IsPlayerMovingUp(Vector2 trajectory) 
	{
		if (trajectory.y < 0) {
			return true;
		}

		return false;
	}

	private List<KinematicCollision2D> GetCollisions()
	{
		List<KinematicCollision2D> output = new List<KinematicCollision2D>();
		for (int i=0; i < GetSlideCount(); i++)
		{
			KinematicCollision2D c = GetSlideCollision(i);
			if (c != null) { output.Add(c); }
		}
		return output;
	}

	private void OnPlayerDamage(int damage) 
	{
		this.State.Damaged(damage);
		this.IsDead = true;
	}

	public void OnAnimationPlayerAnimationFinished(string animationName) 
	{
		if ((animationName == "Death") && this.IsDead)
		{
			Hide();
			SetPhysicsProcess(false);
			EmitSignal(nameof(PlayerDied), this);
		}
	}

	public void OnCoyoteTimerTimeout()
	{
		// "Locks" the coyote time down so that it cannot be
		// reactivated again until the player touches the ground.
		this.State.CoyoteTimeFinished();
	}

	public void OnAirDashTimerTimeout()
	{
		this.State.AirDashFinished();
	}

}
