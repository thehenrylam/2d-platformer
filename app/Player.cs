using Godot;
using System;

public class Player : KinematicBody2D
{
	[Export]
	private float ACCELERATION = 512;
	[Export]
	private float MAX_SPEED = 64;
	[Export]
	private float FRICTION = (float)0.25;
	[Export]
	private float AIR_RESISTANCE = (float)0.02;
	[Export]
	private float GRAVITY = 200;
	[Export]
	public float JUMP_FORCE = 128;
	
	private Vector2 motion = Vector2.Zero;

	private Sprite sprite = null;
	private AnimationPlayer animationPlayer = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.sprite = this.GetNode<Sprite>("Sprite");
		this.animationPlayer = this.GetNode<AnimationPlayer>("AnimationPlayer");
	}

	public override void _PhysicsProcess(float delta)
	{
		float xInput = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");

		if (xInput != 0)
		{
			this.animationPlayer.Play("Run");
			
			motion.x += xInput * ACCELERATION * delta;
			motion.x = Mathf.Clamp(motion.x, -MAX_SPEED, MAX_SPEED);

			this.sprite.FlipH = xInput > 0;
		}
		else
		{
			this.animationPlayer.Play("Stand");
		}

		motion.y += GRAVITY * delta;

		if (IsOnFloor()) {
			if (xInput == 0) {
				motion.x = Mathf.Lerp(motion.x, 0, FRICTION);
			}

			if (Input.IsActionJustPressed("ui_up")) {
				motion.y = -JUMP_FORCE;
			}	
		}
		else 
		{
			this.animationPlayer.Play("Jump");

			if (Input.IsActionJustReleased("ui_up") && motion.y < -JUMP_FORCE/2) {
				motion.y = -JUMP_FORCE/2;
			}

			if (xInput == 0)
			{
				motion.x = Mathf.Lerp(motion.x, 0, AIR_RESISTANCE);
			}
		}

		motion = MoveAndSlide(motion, Vector2.Up);
	}
}
