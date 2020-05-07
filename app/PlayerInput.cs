using Godot;
using System;
using System.Collections.Generic;

public class PlayerInput
{
    #region Constants
    private const string INPUT_JUMP = "jump";
	private const string INPUT_UP = "ui_up";
	private const string INPUT_RIGHT = "ui_right";
	private const string INPUT_LEFT = "ui_left";
	private const string INPUT_DOWN = "ui_down";
	#endregion

    public Vector2 DirectionalInfluence
    {
        get { return GetDirectionalInflence(); }
    }

    public bool JumpButtonPressed
    {
        get { return (Input.IsActionPressed(INPUT_UP) || Input.IsActionPressed(INPUT_JUMP)); }
    }

    public PlayerInput()
    {
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

}