using Godot;
using System;

public class Checkpoint : Area2D
{
    [Signal]
    public delegate void Activated();
    [Signal]
    public delegate void Deactivated();

    [Export]
    private PackedScene animationPlayer;
    [Export]
    private bool active = false;

    private AnimationPlayer animationInstance = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        string animationName = (active) ? "CheckpointActive" : "CheckpointInactive";

        this.animationInstance = (AnimationPlayer)animationPlayer.Instance();
        this.AddChild(this.animationInstance);
        this.animationInstance.Play(animationName);
    }

    public void Activate(Node entity=null)
    {
        // If the checkpoint is active, then don't perform the animation (ActiveCheckpoint),
        // Otherwise, show an animation from inactive to active (CheckpointRecieved)
        string animationName = (active) ? "CheckpointActive" : "CheckpointRecieved";
        
        this.animationInstance.Play(animationName);

        active = true;

        EmitSignal("Activated", entity, this);
    }

    public void Deactivate(Node entity=null)
    {
        // If the checkpoint is active, show an animation from active to inactive (InactiveCheckpoint),
        // Otherwise, don't perform the animation (CheckpointLost),
        string animationName = (active) ? "CheckpointLost" : "CheckpointInactive";

        this.animationInstance.Play(animationName);

        active = false;

        EmitSignal("Deactivated", entity, this);
    }

    public void OnCheckpointBodyEntered(Node entity)
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

    public void OnAnimationPlayerAnimationStarted(string animationName)
    {
        // ...
    }

}
