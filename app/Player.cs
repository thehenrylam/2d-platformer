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
	private float MAX_SPEED = 96;
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
	public float COYOTE_TIME = 0.1f;
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

	public Vector2 DirectionalInfluence
	{
		get { return GetDirectionalInflence(); }
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

		// Emit collision signals for each collision detected
		foreach (KinematicCollision2D kc in this.CurCollisions)
		{
			EmitSignal("Collided", kc, this);
		}

		Vector2 dirInfluence = this.DirectionalInfluence;

		this.IsGrounded = IsTouchingGround(this.raycasts);
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

		// Process the player's movement (by player input)
		motion = ProcessMovement(motion, delta, this.IsGrounded);

		// Determine the current animation
		string animationName = ProcessAnimation(motion, this.IsGrounded);
		if (animationName != null) { this.animationPlayer.Play(animationName); }

		// if (xInput != 0) { this.sprite.FlipH = xInput > 0; }
		if (dirInfluence.x != 0) { this.sprite.FlipH = dirInfluence.x > 0; }

		motion = MoveAndSlide(motion, Vector2.Up);	
	}

	private Vector2 ProcessMovement(Vector2 trajectory, float delta, bool isTouchingGround) 
	{
		// Horizontal Resistance Factor is determines how fast the player stops moving in the x axis.
		float horizResistanceFactor = 0;
		// Get target fps from the Game Engine.
		float TARGET_FPS = Engine.GetFramesPerSecond();

		// Retrieve the player's input in the horizontal direction.
		float xInput = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");

		if (xInput != 0)
		{
			// If the horizontal input is not 0... (Player is pressing the left or right direction but not both)
			trajectory.x += xInput * ACCELERATION * delta * TARGET_FPS;
			trajectory.x = Mathf.Clamp(trajectory.x, -MAX_SPEED, MAX_SPEED);
		}
		else
		{
			// If the horizontal input is 0... (Player is not inputting to any specific direction horizontally)

			// Determine the horizontal resistance factor (regular friction on ground? Or air resistance in air?)
			horizResistanceFactor = (this.IsGrounded) ? FRICTION : AIR_RESISTANCE;
			// Gradually approach 0 from the trajectory's original x value by the weight of (resistance factor * delta)
			trajectory.x = Mathf.Lerp(trajectory.x, 0, horizResistanceFactor * delta);
		}

		// Determine the downwards trajectory in the vertical direction
		trajectory.y += GRAVITY * delta * TARGET_FPS;

		if (this.State.CanJump) 
		{
			// If the "jump" button was just pressed, then travel in the 
			// upwards direction by the JUMP_FORCE's magnitude.
			if (Input.IsActionJustPressed("ui_up")) 
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
			if (Input.IsActionJustReleased("ui_up") && trajectory.y < -JUMP_FORCE/2) 
			{
				trajectory.y = -JUMP_FORCE/2;
			}
		}

		bool crouch = Input.IsActionPressed("ui_down");
		if (crouch) 
		{
			if (this.State.CanFastFall)
			{
				this.State.FastFallen();
				trajectory.y += JUMP_FORCE * FAST_FALL_FACTOR;
			}

			if (this.IsGrounded) { trajectory.x = 0; }
		}

		return trajectory;
	}

	private string ProcessAnimation(Vector2 trajectory, bool isTouchingGround)
	{
		string animationName = "Stand";

		if (isTouchingGround) 
		{
			float xInput = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");

			bool crouch = Input.IsActionPressed("ui_down");

			if (crouch)
			{
				animationName = "Crouch";
			}
			else if (xInput != 0) 
			{
				animationName = "Run";
			}
		}
		else
		{
			if (IsPlayerMovingUp(motion))
			{
				animationName = "Jump";
			}
			else
			{
				animationName = "Fall";
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

		return raycastCollisions && IsOnFloor();
	}

	private bool IsPlayerMovingUp(Vector2 trajectory) 
	{
		if (trajectory.y < 0) {
			return true;
		}

		return false;
	}

	private void CheckCollisions() 
	{
		for (int i = 0; i < GetSlideCount(); i++)
		{
			var collision = GetSlideCollision(i);
			if (collision != null)
			{
				// GD.Print(collision.Collider);
				EmitSignal("Collided", collision, this);
			}
		}
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

	private Vector2 GetDirectionalInflence()
	{
		// Something to note to help make what is happening below make more sense:
		// The TOP-LEFT corner is (0, 0)
		// The BOTTOM-RIGHT corner is (maxWidth, maxHeight),

		// If the user has their input more towards the right, the x value will be positive.
		// If the user has their input more towards the left, the x value will be negative.
		// If the user has their input neutral or providing equal input to left and right, the x value will be 0.
		float x = Input.GetActionStrength(INPUT_RIGHT) - Input.GetActionStrength(INPUT_LEFT);

		// If the user has their input more downwards, the y value will be positive.
		// If the user has their input more upwards, the y value will be negative.
		// If the user has their input neutral or providing equal input to up and down, the y value will be 0.
		float y = Input.GetActionStrength(INPUT_DOWN) - Input.GetActionStrength(INPUT_UP);

		return new Vector2(x, y);
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
		// this.coyoteTimeState = -1;
		this.State.CoyoteTimeFinished();
	}

}
