using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class World : Node2D
{
    [Signal]
    public delegate void WinStateReached();
    [Signal]
    public delegate void NewPlayer();

    [Export]
    public PackedScene player;
    [Export]
    public bool RespawnOnPlayerDeath = true;

    private Vector2 spawnPosition;

    private HashSet<string> listCheckpointNodeNames = new HashSet<string>();
    private HashSet<string> listPlayerNodeNames = new HashSet<string>();

    private Position2D startPosition = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ConnectAll();

        this.startPosition = this.GetNode<Position2D>("StartPosition");

        this.spawnPosition = this.startPosition.Position;

        RemoveAllPlayers();
        SpawnPlayer(this.spawnPosition);
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("retry"))
        {
            // If the player's input pressed retry, 
            // then remove the current player (if there is any),
            // and spawn a new playable character.
            RemoveAllPlayers();
            SpawnPlayer(this.spawnPosition);
        }
    }

    public void Reset()
    {
        this.GetTree().ReloadCurrentScene();
    }

    private void ConnectAll()
    {
        foreach (Node2D node in this.GetChildren())
        {
            Spikes s = AttemptCast<Spikes>(node);
            if (s != null)
            {
                // Connect this node's NewPlayer signal to the spike's OnNewPlayer method
                this.Connect("NewPlayer", s, "OnNewPlayer");
            }

            Checkpoint c = AttemptCast<Checkpoint>(node);
            if (c != null)
            {
                // Connect the checkpoint's Activated signal to this node's OnCheckpointActivate method
                c.Connect("Activated", this, nameof(OnCheckpointActivate));
                // Add the checkpoint name to the list of checkpoint node names
                listCheckpointNodeNames.Add(c.Name);
            }

            Goalpoint g = AttemptCast<Goalpoint>(node);
            if (g != null)
            {
                // Connect the goalpoint's Activated signal to this node's OnGoalpointActivated method
                g.Connect("Activated", this, nameof(OnGoalpointActivated));
            }

        }
    }

    private T AttemptCast<T>(Node node)
    {
        T output;
        try
        {
            // Attempt to cast the node 
            output = (T)(object)node;
        }
        catch (InvalidCastException)
        {
            output = default(T);
        }

        return output;
    }

    private void RemoveAllPlayers() 
    {
        // Removes ALL the entries from list of player node names due to the method's purpose.
        this.listPlayerNodeNames.Clear();

        // Create a regex string
        string regexString = String.Format("@?({0})@?.*", "Player");
        // Get a list of player node names
        List<string> playerNodeNames = new List<string>();

        foreach (Node2D node in this.GetChildren())
        {
            // For each node within the current node's (world node) children...

            if (Regex.IsMatch(node.Name, regexString))
            {
                // Add the player node names onto the list if the regex string matches it.
                playerNodeNames.Add(node.Name);
            }
        }

        if (playerNodeNames.Count > 0)
        {
            // If the node name list is not empty, then...

            foreach (string s in playerNodeNames)
            {
                // Get all the nodes from the pertaining to 
                // those names and remove them from the game. 
                KinematicBody2D instance = this.GetNode<KinematicBody2D>(s);
                instance.Hide();
                instance.QueueFree();
            }
        }
    }

    private void SpawnPlayer(Vector2 position) 
    {
        // Start a new instance of the player object.
        var playerInstance = (KinematicBody2D)this.player.Instance(); 
        // Set up a connection to listen to a signal regarding the death of the current player
        playerInstance.Connect("PlayerDied", this, nameof(OnPlayerDied));
        // Immediately hide the player instance.
        playerInstance.Hide();
        // Add the player instance to the tree as a child node.
        AddChild(playerInstance);
        // Set the player instance's position to the designated position in the world.
        playerInstance.Position = position;
        // Show the player instance;
        playerInstance.Show();

        EmitSignal("NewPlayer", playerInstance);

        if (this.listPlayerNodeNames.Contains(playerInstance.Name))
        {
            throw new ApplicationException("New player instance name already within listPlayerNodeNames");
        }

        // Add the playerInstance's name into the list of player node names.
        this.listPlayerNodeNames.Add(playerInstance.Name);
    }

    public void OnCheckpointActivate(Node entity, Checkpoint sender)
    {
        if (!this.listPlayerNodeNames.Contains(entity.Name)) { return; }

        Checkpoint checkpoint;
        foreach (string s in this.listCheckpointNodeNames)
        {
            checkpoint = this.GetNode<Checkpoint>(s);
            
            if (sender.Name == s) 
            {
                this.spawnPosition = checkpoint.Position;
            }
            else
            {
                checkpoint.Deactivate();
            }
        }
    }

    public void OnGoalpointActivated(Node entity, Goalpoint sender)
    {
        if (!this.listPlayerNodeNames.Contains(entity.Name)) { return; }

        entity.SetPhysicsProcess(false);

        System.Threading.Thread.Sleep(10);

        EmitSignal(nameof(WinStateReached));

        // this.spawnPosition = this.startPosition.Position;
    }

    public void OnPlayerDied(Player player)
    {
        player.QueueFree();

        if (this.RespawnOnPlayerDeath)
        {
            RemoveAllPlayers();
            SpawnPlayer(this.spawnPosition);
        }
    }

}
