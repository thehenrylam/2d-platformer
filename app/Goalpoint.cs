using Godot;
using System;

public class Goalpoint : Area2D
{
    [Signal]
    public delegate void Activated();

    private AnimationPlayer animationInstance = null;
    private string animationName = "GoalpointActive";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("Goal point Ready");

        this.animationInstance = this.GetNode<AnimationPlayer>("AnimationPlayer");

        this.animationInstance.Play(this.animationName);
    }

    public void Activate(Node entity=null)
    {
        EmitSignal("Activated", entity, this);
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

}
