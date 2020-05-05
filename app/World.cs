using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class World : Node2D
{
    [Export]
    public PackedScene player;

    [Signal]
    public delegate void NewPlayer();

    private Area2D checkpoint;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.checkpoint = this.GetNode<Area2D>("Checkpoint");
        RemoveAllPlayers();
        SpawnPlayer(this.checkpoint.Position);
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("retry"))
        {
            GD.Print("Retrying...");
            // If the player's input pressed retry, 
            // then remove the current player (if there is any),
            // and spawn a new playable character.
            RemoveAllPlayers();
            SpawnPlayer(this.checkpoint.Position);
        }
    }

    private void RemoveAllPlayers() 
    {
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
        // Immediately hide the player instance.
        playerInstance.Hide();
        // Add the player instance to the tree as a child node.
        AddChild(playerInstance);
        // Set the player instance's position to the designated position in the world.
        playerInstance.Position = position;
        // Show the player instance;
        playerInstance.Show();

        EmitSignal("NewPlayer", playerInstance);
        GD.Print(playerInstance.Name);
    }
}
