using Godot;
using System;

public class Button : Control
{
    public enum ButtonState
    {
        ACTIVE,
        INACTIVE,
        LOCKEDOUT,
        LOCKEDIN
    }

    [Export]
    public string Text
    {
        get { return this.text; }
        set 
        {
            if (this.label != null)
            {
                this.label.Text = this.text;
            }
            this.text = value;
        }
    }

    [Export]
    public ButtonState State
    {
        get { return this.state; }
        set 
        {
            if (this.state != value)
            {
                this.state = value;
                SetButtonState(value);
            }
        }
    }

    [Export]
    public Color ActiveColor
    {
        get { return this.activeColor; }
        set 
        {
            this.activeColor = value;

            if (this.State == ButtonState.ACTIVE)
            {
                ChangeButtonColor(value);
            }
        }
    }

    [Export]
    public Color InactiveColor
    {
        get { return this.inactiveColor; }
        set 
        {
            this.inactiveColor = value;

            if (this.State == ButtonState.INACTIVE)
            {
                ChangeButtonColor(value);
            }
        }
    }

    [Export]
    public Color LockedInColor
    {
        get { return this.lockedInColor; }
        set 
        {
            this.lockedInColor = value;

            if (this.State == ButtonState.LOCKEDIN)
            {
                ChangeButtonColor(value);
            }
        }
    }

    [Export]
    public Color LockedOutColor
    {
        get { return this.lockedOutColor; }
        set 
        {
            this.lockedOutColor = value;
            
            if (this.State == ButtonState.LOCKEDOUT)
            {
                ChangeButtonColor(value);
            }
        }
    }

    private string text = "";
    private ButtonState state = ButtonState.INACTIVE;
    private Color activeColor = Color.Color8(0, 255, 0);
    private Color inactiveColor = Color.Color8(255, 255, 255);
    private Color lockedInColor = Color.Color8(35, 100, 35);
    private Color lockedOutColor = Color.Color8(75, 75, 75);

    private Label label = null;
    private ColorRect background = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.label = this.GetNode<Label>("Label");
        this.background = this.GetNode<ColorRect>("Background");

        this.label.Text = this.text;
        SetButtonState(this.state);
    }

    private void SetButtonState(ButtonState state)
    {
        Color color = this.InactiveColor;

        switch (state)
        {
            case ButtonState.ACTIVE:
                color = this.ActiveColor;
                break;
            case ButtonState.INACTIVE:
                color = this.InactiveColor;
                break;
            case ButtonState.LOCKEDOUT:
                color = this.LockedOutColor;
                break;
            case ButtonState.LOCKEDIN:
                color = this.LockedInColor;
                break;
            default:
                break;
        }

        this.background.Color = color;
    }

    private void ChangeButtonColor(Color color)
    {
        this.background.Color = color;
    }

}
