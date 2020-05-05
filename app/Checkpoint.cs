using Godot;
using System;

public class Checkpoint : Area2D
{
    [Export]
    public bool active = false;

    private AnimationPlayer animationPlayer = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.animationPlayer = this.GetNode<AnimationPlayer>("AnimationPlayer");

        string animationName = (active) ? "ActiveCheckpoint" : "InactiveCheckpoint";

        this.animationPlayer.Play(animationName);
    }

    public void Activate()
    {
        // If the checkpoint is active, then don't perform the animation (ActiveCheckpoint),
        // Otherwise, show an animation from inactive to active (CheckpointRecieved)
        string animationName = (active) ? "ActiveCheckpoint" : "CheckpointRecieved";
        
        this.animationPlayer.Play(animationName);
    }

    public void Deactivate()
    {
        // If the checkpoint is active, show an animation from active to inactive (InactiveCheckpoint),
        // Otherwise, don't perform the animation (CheckpointLost),
        string animationName = (active) ? "CheckpointLost" : "InactiveCheckpoint";

        this.animationPlayer.Play(animationName);
    }

}
