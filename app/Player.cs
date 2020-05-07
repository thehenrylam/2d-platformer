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
	public bool StateGrounded
	{
		get { return (IsTouchingGround(this.raycasts) || IsOnFloor()); }
	}

	public bool StateMidair
	{
		get { return !StateGrounded; }
	}

	public bool StateDashing
	{
		get { return false; }
	}

	public bool StateDead
	{
		get { return deathFlag; }
		set { deathFlag = value; }
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
	// private PlayerState playerState = null;
	private int playerJumpTally = 0;
	private int playerFastFallTally = 0;
	private int coyoteTimeState = 0;
	private bool deathFlag = false;
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

		PlayerState playerState = new PlayerState();
		// this.playerState.PlayerDied += new EventHandler(HandleDeath);
	}

	public override void _PhysicsProcess(float delta)
	{
		// Stop the process from continuing any further
		// if the death flag has been raised.
		if (deathFlag)
		{
			if (this.animationPlayer.CurrentAnimation != "Death")
			{
				this.animationPlayer.Play("Death");
			}
			return;
		}

		// Emit collision signals for each collision detected
		foreach (KinematicCollision2D kc in this.CurCollisions)
		{
			EmitSignal("Collided", kc, this);
		}
		// CheckCollisions();

		Vector2 dirInfluence = this.DirectionalInfluence;

		// float xInput = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");

		// Determine if the player is touching the ground.
		bool isTouchingGround = this.StateGrounded;

		if (isTouchingGround)
		{
			this.coyoteTimeState = 0;
			this.playerJumpTally = 0;
			this.playerFastFallTally = 0;

			if (!this.coyoteTimer.IsStopped()) 
			{
				this.coyoteTimer.Stop();
			}
		}
		else 
		{
			if ((this.playerJumpTally == 0) && (this.coyoteTimeState == 0))
			{
				this.coyoteTimeState = 1;
				this.coyoteTimer.Start(COYOTE_TIME);
			}
		}

		// Process the player's movement (by player input)
		motion = ProcessMovement(motion, delta, isTouchingGround);

		// Determine the current animation
		string animationName = ProcessAnimation(motion, isTouchingGround);
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
			horizResistanceFactor = (isTouchingGround) ? FRICTION : AIR_RESISTANCE;
			// Gradually approach 0 from the trajectory's original x value by the weight of (resistance factor * delta)
			trajectory.x = Mathf.Lerp(trajectory.x, 0, horizResistanceFactor * delta);
		}

		// Determine the downwards trajectory in the vertical direction
		trajectory.y += GRAVITY * delta * TARGET_FPS;

		if ((isTouchingGround || (this.coyoteTimeState == 1)) && (this.playerJumpTally <= 0)) 
		{
			// If the "jump" button was just pressed, then travel in the 
			// upwards direction by the JUMP_FORCE's magnitude.
			if (Input.IsActionJustPressed("ui_up")) 
			{
				this.playerJumpTally += 1;
				this.playerFastFallTally = 0;
				trajectory.y = -JUMP_FORCE;
			}
			else
			{
				this.playerJumpTally = 0;
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
			if (this.playerFastFallTally == 0)
			{
				this.playerFastFallTally += 1;
				trajectory.y += JUMP_FORCE * FAST_FALL_FACTOR;
			}

			if (isTouchingGround) { trajectory.x = 0; }
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

		return raycastCollisions;
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
		if (damage > 0)
		{
			GD.Print("Damage");
			deathFlag = true;
		}
	}

	public void OnAnimationPlayerAnimationFinished(string animationName) 
	{
		if ((animationName == "Death") && deathFlag)
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
		this.coyoteTimeState = -1;
	}
}
