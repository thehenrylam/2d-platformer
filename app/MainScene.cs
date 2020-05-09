using Godot;
using System;
using System.Collections.Generic;

public class MainScene : Node2D
{
    public List<string> List_Of_World_Paths = new List<string>()
    {
        "World.tscn"
    };

    public List<PackedScene> LevelList = new List<PackedScene>();

    public bool IsPaused
    {
        get { return this.processStopFlag; }
        set { this.processStopFlag = value; }
    }

    private bool IsPauseScreenActive
    {
        get { return this.pauseScreen.Visible; }
        set { SetPopupEntity(value, this.pauseScreen); }
    }

    private bool IsWinScreenActive
    {
        get { return this.winScreen.Visible; }
        set { SetPopupEntity(value, this.winScreen); }
    }

    private bool IsMainMenuActive
    {
        get { return this.mainMenuScreen.Visible; }
        set { SetPopupEntity(value, this.mainMenuScreen); }
    }

    private bool processStopFlag = true;
    private int curWorldIndex = 0;

    private CanvasLayer canvasLayer = null;
    private PauseScreen pauseScreen = null;
    private WinScreen winScreen = null;
    private MainMenuScreen mainMenuScreen = null;
    private World curGameWorld = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.canvasLayer = this.GetNode<CanvasLayer>("CanvasLayer");
        this.pauseScreen = this.canvasLayer.GetNode<PauseScreen>("PauseScreen");
        this.winScreen = this.canvasLayer.GetNode<WinScreen>("WinScreen");
        this.mainMenuScreen = this.canvasLayer.GetNode<MainMenuScreen>("MainMenuScreen");

        foreach (string scenePath in List_Of_World_Paths)
        {
            string formattedPath = String.Format("res://{0}", scenePath);
            PackedScene scene = (PackedScene)ResourceLoader.Load(formattedPath);

            this.LevelList.Add(scene);
        }

        // this.curGameWorld = this.GetNode<World>("World");

        this.IsPaused = this.processStopFlag;
        this.IsMainMenuActive = true;
        this.IsPauseScreenActive = false;
        this.IsWinScreenActive = false;

    }

    public override void _Process(float delta)
    {
        // Having this within _Process is the only known way to reliably halt/pause the game.
        GetTree().Paused = this.IsPaused;

        bool escPressed = Input.IsActionJustPressed("ui_cancel");
        if (escPressed && (!this.IsWinScreenActive) && (!this.IsMainMenuActive))
        {
            // Disable the win screen (The "Pause" screen overrides the "Win" screen)
            this.IsWinScreenActive = false;

            // Toggle the flag variable resposibly with pausing the game
            this.IsPaused = !this.IsPaused;
            this.IsPauseScreenActive = this.IsPaused;
        }
    }

    private void ReloadWorld()
    {
        FlushAllWorlds();
        LoadWorld(this.curWorldIndex);
    }

    private void LoadWorld(int index)
    {
        World world = GetWorldFromLevelList(index) ?? throw new NullReferenceException("World is not valid");
        
        this.AddChild(world);
        world.Connect("WinStateReached", this, nameof(OnWorldWinStateReached));
        world.Show();
    }

    private void FlushAllWorlds()
    {
        List<World> worldList = new List<World>();
        foreach (Node n in this.GetChildren())
        {
            try 
            {
                World w = (World)n;
                worldList.Add(w);
            }
            catch (InvalidCastException)
            {
            }
            
        }

        foreach (World w in worldList)
        {
            w.QueueFree();
        }
    }

    private World GetWorldFromLevelList(int index)
    {
        if ((index < 0) && (index >= this.LevelList.Count)) { return null; }
        
        World output = null;

        try
        {
            PackedScene scene = this.LevelList[index];
            output = (World)scene.Instance();
        }
        catch (InvalidCastException)
        {
            output = null;
        }

        return output;
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
        ReloadWorld();
    }

    public void OnWinScreenMainMenu()
    {
        this.IsPaused = true;
        this.IsWinScreenActive = false;
        this.IsMainMenuActive = true;
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

    public void OnPauseScreenMainMenu()
    {
        this.IsPaused = true;
        this.IsPauseScreenActive = false;
        this.IsMainMenuActive = true;
    }

    public void OnPauseScreenQuitGame()
    {
        this.GetTree().Quit();
    }
    #endregion

    #region Main Menu Event Handlers
    public void OnMainMenuScreenStartGame()
    {
        this.IsMainMenuActive = false;
        this.IsPaused = false;

        ReloadWorld();
    }
    public void OnMainMenuScreenQuitGame()
    {
        this.GetTree().Quit();
    }
    #endregion

}
