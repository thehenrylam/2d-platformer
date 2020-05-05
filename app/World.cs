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

    private Position2D startPosition;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.startPosition = this.GetNode<Position2D>("StartPoint");

        RemoveAllPlayers();
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("retry"))
        {
            GD.Print("Retrying...");
            RemoveAllPlayers();
            SpawnPlayer(this.startPosition.Position);
        }
    }

    private void RemoveAllPlayers() 
    {
        string regexString = String.Format("@?({0})@?.*", "Player");
        GD.Print(regexString);

        List<string> playerNodeNames = new List<string>();

        GD.Print(this.GetChildren());

        foreach (Node2D node in this.GetChildren())
        {
            GD.Print(node.Name);

            if (Regex.IsMatch(node.Name, regexString))
            {
                // GD.Print(node.Name);
                playerNodeNames.Add(node.Name);
            }
        }

        if (playerNodeNames.Count > 0)
        {
            foreach (string s in playerNodeNames)
            {
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
