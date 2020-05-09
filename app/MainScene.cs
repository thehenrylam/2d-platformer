using Godot;
using System;

public class MainScene : Node2D
{
    public bool IsPaused
    {
        get { return this.processStopFlag; }
        set { this.processStopFlag = value; }
    }

    private bool IsPauseScreenActive
    {
        get { return this.pauseScreen.Visible; }
        set { SetPauseScreen(value); }
    }

    private bool IsWinScreenActive
    {
        get { return this.winScreen.Visible; }
        set { SetWinScreen(value); }
    }

    private bool processStopFlag = false;

    private CanvasLayer canvasLayer = null;
    private PauseScreen pauseScreen = null;
    private WinScreen winScreen = null;
    private World curGameWorld = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.canvasLayer = this.GetNode<CanvasLayer>("CanvasLayer");
        this.pauseScreen = this.canvasLayer.GetNode<PauseScreen>("PauseScreen");
        this.winScreen = this.canvasLayer.GetNode<WinScreen>("WinScreen");
        this.curGameWorld = this.GetNode<World>("World");

        this.IsPaused = this.processStopFlag;
        this.IsPauseScreenActive = this.processStopFlag;
        this.IsWinScreenActive = false;

    }

    public override void _Process(float delta)
    {
        // Having this within _Process is the only known way to reliably halt/pause the game.
        GetTree().Paused = this.IsPaused;

        bool escPressed = Input.IsActionJustPressed("ui_cancel");
        if (escPressed && (!this.IsWinScreenActive))
        {
            // Disable the win screen (The "Pause" screen overrides the "Win" screen)
            this.IsWinScreenActive = false;

            // Toggle the flag variable resposibly with pausing the game
            this.IsPaused = !this.IsPaused;
            this.IsPauseScreenActive = this.IsPaused;
        }
    }

    private void SetPauseScreen(bool enable)
    {
        SetPopupEntity(enable, this.pauseScreen);
    }

    private void SetWinScreen(bool enable)
    {
        SetPopupEntity(enable, this.winScreen);
    }

    private void SetPopupEntity(bool enable, Popup popupObject)
    {
        if (popupObject != null)
        {
            if (enable)
            {
                popupObject.PopupCentered();
            }
            else
            {
                popupObject.Hide();
            }
        }
    }

    public void OnWorldWinStateReached()
    {
        this.IsPaused = true;
        this.IsPauseScreenActive = false;
        this.IsWinScreenActive = true;
    }

    #region Win Screen Event Handlers
    public void OnWinScreenRetry()
    {
        this.IsWinScreenActive = false;
        this.IsPaused = false;
        this.curGameWorld.Reset();
    }

    public void OnWinScreenQuitGame()
    {
        this.GetTree().Quit();
    }
    #endregion

    #region Pause Screen Event Handlers
    public void OnPauseScreenResume()
    {
        this.IsPauseScreenActive = false;
        this.IsPaused = false;
    }

    public void OnPauseScreenQuitGame()
    {
        this.GetTree().Quit();
    }
    #endregion

    

}
