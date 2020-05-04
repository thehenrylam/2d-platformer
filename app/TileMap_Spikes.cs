using Godot;
using System;

public class TileMap_Spikes : TileMap
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        KinematicBody2D player = this.GetNodeOrNull<KinematicBody2D>("../Player");
        
        player.Connect("Collided", this, nameof(OnCharacterCollided));
    }

    public void OnCharacterCollided(KinematicCollision2D collision, Node2D sender) {
        var collider = collision.Collider;
        
        ulong colliderId = collision.ColliderId;

        ulong currentId = this.GetInstanceId();

        if (currentId == colliderId) 
        {
            // GD.Print("Collision?");

            // KinematicBody2D player = (KinematicBody2D)sender;
            sender.EmitSignal("Damage", 1);
        }
    }
}
