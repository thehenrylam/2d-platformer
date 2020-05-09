using Godot;
using System;

public class MainMenuScreen : Popup, IButtonListSelector
{
    [Signal]
    public delegate void StartGame();
    [Signal]
    public delegate void QuitGame();

    [Export]
    public int DefaultIndex { get; set; } = 0;

    public bool Focused
    { 
        get { return this.focused; }
        set 
        {
            this.SetProcess(value);
            this.focused = value; 
        }
    }

    public ButtonList ButtonList { get; set; } = null;

    #region Constants
    private const string UI_DOWN = "ui_down";
    private const string UI_UP = "ui_up";
    private const string UI_SELECT = "ui_select";
    #endregion

    private bool focused = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.ButtonList = this.GetNode<ButtonList>("ButtonList");
        
        this.SetProcess(this.Focused);
    }

    public override void _Process(float delta)
    {
        int newIndex = UpdateSelectedIndex();
        UpdateButtonListSelection(newIndex);

        bool select = Input.IsActionJustPressed(UI_SELECT);
        if (select)
        {
            string nodeName = EvaluateCurrentSelection();

            switch (nodeName)
            {
                case "StartGame":
                    EmitSignal(nameof(StartGame));
                    break;
                case "Quit":
                    EmitSignal(nameof(QuitGame));
                    break;
                default:
                    break;
            }
        }
    }

    public void UpdateButtonListSelection(int index)
    {
        if (this.ButtonList.SelectedIndex != index) 
        {
            this.ButtonList.SelectedIndex = index;
        }
    }

    public int UpdateSelectedIndex()
    {
        int changeIndex = (Input.IsActionJustPressed(UI_DOWN) ? 1 : 0) - (Input.IsActionJustPressed(UI_UP) ? 1 : 0);

        int totalButtons = this.ButtonList.Count;
        int index = this.ButtonList.SelectedIndex;

        if (changeIndex != 0)
        {
            changeIndex = Mathf.Clamp(changeIndex, -1, 1);

            index = (index + changeIndex) % totalButtons;
            index += (index >= 0) ? 0 : totalButtons;
        }

        return index;
    }

    public string EvaluateCurrentSelection()
    {
        Button button = this.ButtonList.CurrentButton;
        string nodeName = (button.Locked) ? "" : button.Name;
        return nodeName;
    }

    public void SelectorBegin()
    {
        this.ButtonList.SelectedIndex = DefaultIndex;
        this.Focused = true;
    }

    public void SelectorEnd()
    {
        this.Focused = false;
    }

    public void OnMainMenuScreenAboutToShow()
    {
        SelectorBegin();
    }

    public void OnMainMenuScreenPopupHide()
    {
        SelectorEnd();
    }

}
