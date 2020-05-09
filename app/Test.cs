using Godot;
using System;

public class Test : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private bool testFlag = false;

    private Popup popupPause = null;
    private Popup popupWin = null;
    private Popup popupMainMenu = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        CanvasLayer canvas = this.GetNode<CanvasLayer>("CanvasLayer");

        this.popupPause = canvas.GetNode<Popup>("PauseScreen");
        this.popupWin = canvas.GetNode<Popup>("WinScreen");
        this.popupMainMenu = canvas.GetNode<Popup>("MainMenuScreen");
        
        // this.popupPause.PopupCentered();
        // this.popupWin.PopupCentered();
        this.popupMainMenu.PopupCentered();

        //GD.Print(GetViewport().Size);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.   
    public override void _Process(float delta)
    {
        bool escPressed = Input.IsActionJustPressed("ui_cancel");
        if (escPressed && (!this.popupWin.Visible))
        {
            if (this.popupPause.Visible)
            {
                GD.Print("Pause Hide!");
                this.popupPause.Hide();
            }
            else
            {
                GD.Print("Pause Popped Up!");
                this.popupPause.PopupCentered();
            }

            return;
        }
        
        bool winActivate = Input.IsActionJustPressed("dash");
        if (winActivate && (!this.popupPause.Visible))
        {
            if (this.popupWin.Visible)
            {
                GD.Print("Win Hide!");
                this.popupWin.Hide();
            }
            else
            {
                GD.Print("Win Popped Up!");
                this.popupWin.PopupCentered();
            }

            return;
        }
        
    }

    public void OnPauseScreenResume()
    {
        this.popupPause.Hide();
        GD.Print("[Pause] Resume Game!");
    }

    public void OnPauseScreenMainMenu()
    {
        this.popupPause.Hide();
        GD.Print("[Pause] Main Menu!");
    }

    public void OnPauseScreenQuitGame()
    {
        this.popupPause.Hide();
        GD.Print("[Pause] Quit Game!");
    }

    public void OnWinScreenRetry()
    {
        this.popupWin.Hide();
        GD.Print("[Win] Retry!");
    }

    public void OnWinScreenNextLevel()
    {
        this.popupWin.Hide();
        GD.Print("[Win] Next Level!");
    }

    public void OnWinScreenMainMenu()
    {
        this.popupWin.Hide();
        GD.Print("[Win] Main Menu!");
    }

    public void OnWinScreenQuitGame()
    {
        this.popupWin.Hide();
        GD.Print("[Win] Quit!");
    }


}
