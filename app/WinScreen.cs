using Godot;
using System;

public class WinScreen : Popup
{
    [Signal]
    public delegate void Retry();
    [Signal]
    public delegate void NextLevel();
    [Signal]
    public delegate void MainMenu();
    [Signal]
    public delegate void QuitGame();

    [Export]
    public int DEFAULT_SELECTION = 0;

    public bool Focused 
    { 
        get { return this.focused; }
        set 
        {
            this.SetProcess(value);
            this.focused = value; 
        }
    }

    private const string UI_DOWN = "ui_down";
    private const string UI_UP = "ui_up";
    private const string UI_SELECT = "ui_select";

    private bool focused = false;

    private ButtonList buttonList = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.buttonList = this.GetNode<ButtonList>("ButtonList");

        this.SetProcess(this.Focused);
    }

    public override void _Process(float delta)
    {
        int newIndex = GetNewSelectedIndex();
        if (this.buttonList.SelectedIndex != newIndex) 
        {
            this.buttonList.SelectedIndex = newIndex;
        }

        bool select = Input.IsActionJustPressed(UI_SELECT);
        if (select)
        {
            string nodeName = this.buttonList.NodeNames[this.buttonList.SelectedIndex];

            switch (nodeName)
            {
                case "Retry":
                    EmitSignal(nameof(Retry));
                    break;
                case "NextLevel":
                    EmitSignal(nameof(NextLevel));
                    break;
                case "MainMenu":
                    EmitSignal(nameof(MainMenu));
                    break;
                case "Quit":
                    EmitSignal(nameof(QuitGame));
                    break;
                default:
                    throw new IndexOutOfRangeException("Invalid selection, pause screen can only support Resume or Quit");
            }
        }
    }

    private int GetNewSelectedIndex()
    {
        int changeIndex = (Input.IsActionJustPressed(UI_DOWN) ? 1 : 0) - (Input.IsActionJustPressed(UI_UP) ? 1 : 0);

        int totalButtons = this.buttonList.Count;
        int index = this.buttonList.SelectedIndex;

        if (changeIndex != 0)
        {
            changeIndex = Mathf.Clamp(changeIndex, -1, 1);

            index = (index + changeIndex) % totalButtons;
            index += (index >= 0) ? 0 : totalButtons;
        }

        return index;
    }

    public void OnWinScreenAboutToShow()
    {
        this.buttonList.SelectedIndex = DEFAULT_SELECTION;
        this.Focused = true;
    }

    public void OnWinScreenPopupHide()
    {
        this.Focused = false;
    }

}
