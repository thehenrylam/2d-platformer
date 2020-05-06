using Godot;
using System;

public class Spikes : TileMap
{
    public string pathOfPlayer = "../Player";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        EstablishPlayerConnection(pathOfPlayer);
    }

    public void EstablishPlayerConnection(string playerPath)
    {
        if (this.HasNode(playerPath))
        {
            Node player = this.GetNode<Node>(playerPath);
            EstablishPlayerConnection(player);
        }
    }

    public void EstablishPlayerConnection(Node player)
    {
        if (player != null)
        {
            player.Connect("Collided", this, nameof(OnCharacterCollided));
        }
    }

    public void OnNewPlayer(Node newPlayer) 
    {
        EstablishPlayerConnection(newPlayer);
    }

    public void OnCharacterCollided(KinematicCollision2D collision, Node2D sender) {
        var collider = collision.Collider;
        
        ulong colliderId = collision.ColliderId;

        ulong currentId = this.GetInstanceId();

        if (currentId == colliderId) 
        {
            sender.EmitSignal("Damage", 1);
        }
    }
}
