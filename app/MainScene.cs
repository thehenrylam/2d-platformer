using Godot;
using System;

public class MainScene : Node2D
{
    public bool IsPaused
    {
        get
        {
            return this.pauseFlag;
        }
        set
        {
            this.pauseFlag = value;
            this.SetPaused(value);
        }
    }

    private bool pauseFlag = false;

    private CanvasLayer canvasLayer = null;
    private PauseScreen pauseScreen = null;
    private World curGameWorld = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.canvasLayer = this.GetNode<CanvasLayer>("CanvasLayer");
        this.pauseScreen = this.canvasLayer.GetNode<PauseScreen>("PauseScreen");
        this.curGameWorld = this.GetNode<World>("World");

        this.IsPaused = this.pauseFlag;
    }

    public override void _Process(float delta)
    {
        // Having this within _Process is the only known way to reliably halt/pause the game.
        GetTree().Paused = this.IsPaused;

        bool escPressed = Input.IsActionJustPressed("ui_cancel");
        if (escPressed)
        {
            // Toggle the flag variable resposibly with pausing the game
            this.IsPaused = !this.IsPaused;
        }
    }

    private void SetPaused(bool enable)
    {
        if (this.pauseScreen != null)
        {
            if (enable)
            {
                this.pauseScreen.PopupCentered();
            }
            else
            {
                this.pauseScreen.Hide();
            }
        }
    }


    public void OnPauseScreenResume()
    {
        this.IsPaused = false;
    }

    public void OnPauseScreenQuitGame()
    {
        this.GetTree().Quit();
    }

}
