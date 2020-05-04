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
		CheckCollisions();

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

		motion = MoveAndSlide(motion, Vector2.Up);		
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
		this.SetPhysicsProcess(false);
		// this.animationPlayer.Stop();
		this.animationPlayer.Play("Hurt");
		Hide();
		QueueFree();
		GD.Print("Died");
	}

	public void OnAnimationPlayerAnimationFinished(string animationName) 
	{
		//GD.Print(animationName);
		if (animationName == "Death") 
		{
			Hide();
			QueueFree();
		}
	}
}
