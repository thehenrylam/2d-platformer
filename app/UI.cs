using Godot;
using System;

public class UI : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // ...
    }

    public void ShowWinScreen()
    {
        this.Show();
    }

    public void HideWinScreen()
    {
        this.Hide();
    }
}
