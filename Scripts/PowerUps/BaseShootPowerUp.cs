using Godot;
using System;

public partial class BaseShootPowerUp : BasePowerUp
{
    [Export] protected float acceleration;
    [Export] protected float maxSpeed;
    
    protected float speed = 0;

    protected virtual void Rebound()
    {

    }

    // tries to destroy segment unless its indestructable, returns success of destruction
    protected bool AttemptToDestroySegment(Vector2I pos)
    {
        if (!jarMan.IsTileUnbreakable(pos))
            return jarMan.DestroySegment(pos);
        else
            return false;
    }
}
