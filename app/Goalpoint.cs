using Godot;
using System;
using System.Collections.Generic;

public class Goalpoint : Area2D
{
    [Signal]
    public delegate void PlayerTouchedGoalPoint();
    [Signal]
    public delegate void Activated();

    private const string DEFAULT_ANIMATION = "GoalpointActive";
    private const string GOALPOINT_REACHED_ANIMATION = "GoalpointReached";

    private List<Node> activationQueue = new List<Node>();

    private AnimationPlayer animationInstance = null;
    private string animationName = "GoalpointActive";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("Goal point Ready");

        this.animationInstance = this.GetNode<AnimationPlayer>("AnimationPlayer");

        this.animationInstance.Play(DEFAULT_ANIMATION);
    }

    public void Activate(Node entity=null)
    {
        EmitSignal(nameof(PlayerTouchedGoalPoint), entity, this);

        this.activationQueue.Add(entity);
        
        this.animationInstance.Play(GOALPOINT_REACHED_ANIMATION);
    }

    public void OnGoalpointBodyEntered(Node entity)
    {
        Player playerEntity = null;

        try
        {
            playerEntity = (Player)entity;
        }
        catch (InvalidCastException)
        {
            // Failed to cast entity
        }

        if (playerEntity == null) { return; }

        Activate(entity);
    }

    public void OnAnimationPlayerAnimationFinished(string animationName)
    {
        if (animationName == GOALPOINT_REACHED_ANIMATION)
        {
            foreach (Node n in this.activationQueue)
            {
                EmitSignal(nameof(Activated), n, this);
            }
        }
    }

}
