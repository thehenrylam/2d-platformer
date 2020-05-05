using Godot;
using System;
using System.Collections.Generic;

public class Player : KinematicBody2D
{
	[Signal]
	public delegate void Collided();

	[Signal]
	public delegate void Damage();

	[Export]
	private float ACCELERATION = 8;
	[Export]
	private float MAX_SPEED = 64;
	[Export]
	private float FRICTION = 8;
	[Export]
	private float AIR_RESISTANCE = 1;
	[Export]
	private float GRAVITY = 4;
	[Export]
	public float JUMP_FORCE = 140;
	
	private bool isDead = false;

	private Vector2 motion = Vector2.Zero;

	private Sprite sprite = null;
	private AnimationPlayer animationPlayer = null;
	private List<RayCast2D> raycasts = new List<RayCast2D>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.sprite = this.GetNode<Sprite>("Sprite");
		this.animationPlayer = this.GetNode<AnimationPlayer>("AnimationPlayer");
		this.raycasts.Add(this.GetNode<RayCast2D>("RayCast2D_0"));
		this.raycasts.Add(this.GetNode<RayCast2D>("RayCast2D_1"));
	}

	public override void _PhysicsProcess(float delta)
	{
		if (isDead) 
		{
			this.animationPlayer.Play("Death");
			return;
		}

		CheckCollisions();

		float xInput = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");

		// Determine if the player is touching the ground.
		bool isTouchingGround = IsTouchingGround(this.raycasts) || IsOnFloor();

		// Process the player's movement (by player input)
		motion = ProcessMovement(motion, delta, isTouchingGround);
		
		// Determine the current animation
		string animationName = ProcessAnimation(motion, isTouchingGround);

		# region oldcode
		/*
		float TARGET_FPS = Engine.GetFramesPerSecond();

		float xInput = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");

		if (xInput != 0)
		{
			this.animationPlayer.Play("Run");

			motion.x += xInput * ACCELERATION * delta * TARGET_FPS;
			motion.x = Mathf.Clamp(motion.x, -MAX_SPEED, MAX_SPEED);

			this.sprite.FlipH = xInput > 0;
		}
		else
		{
			this.animationPlayer.Play("Stand");
		}

		motion.y += GRAVITY * delta * TARGET_FPS;

		if (IsTouchingGround(this.raycasts) || IsOnFloor()) 
		{
			if (xInput == 0) 
			{
				motion.x = Mathf.Lerp(motion.x, 0, FRICTION * delta);
			}

			if (Input.IsActionJustPressed("ui_up")) 
			{
				motion.y = -JUMP_FORCE;
			}	
		}
		else 
		{
			if (IsPlayerMovingUp(motion))
			{
				this.animationPlayer.Play("Jump");
			}
			else 
			{
				this.animationPlayer.Play("Fall");
			}

			if (Input.IsActionJustReleased("ui_up") && motion.y < -JUMP_FORCE/2) 
			{
				motion.y = -JUMP_FORCE/2;
			}

			if (xInput == 0)
			{
				motion.x = Mathf.Lerp(motion.x, 0, AIR_RESISTANCE * delta);
			}
		}
		*/
		# endregion oldcode

		if (animationName != null) { this.animationPlayer.Play(animationName); }

		if (xInput != 0) { this.sprite.FlipH = xInput > 0; }

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

		if (isTouchingGround) 
		{
			// If the "jump" button was just pressed, then travel in the 
			// upwards direction by the JUMP_FORCE's magnitude.
			if (Input.IsActionJustPressed("ui_up")) 
			{
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

		return trajectory;
	}

	private string ProcessAnimation(Vector2 trajectory, bool isTouchingGround)
	{
		string animationName = "Stand";

		if (isTouchingGround) 
		{
			float xInput = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
			if (xInput != 0) 
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

	private void OnPlayerDamage(int damage) 
	{
		if (damage > 0)
		{
			GD.Print("Damage");
			Kill();
		}
	}

	private void Kill() 
	{
		isDead = true;
	}

	public void OnAnimationPlayerAnimationFinished(string animationName) 
	{
		if (animationName == "Death") 
		{
			Hide();
			QueueFree();
		}
	}
}
