using Godot;
using System;
using System.Collections.Generic;

public partial class PowerUpThwomp : BaseShootPowerUp
{
    private bool goingUpwards = true;
    protected Vector2I gridPos;
    protected Vector2I lastGridPos;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		lastGridPos = InitialGridPos;
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        speed += acceleration * (float)delta;
        if (speed > maxSpeed)
            speed = maxSpeed;
        
        Position += (goingUpwards ? Vector2.Up : Vector2.Down) * speed * (float)delta;
        
        gridPos = WorldPosToGridPos(GlobalPosition);

        if (gridPos != lastGridPos)
        {
            if (DestroyTilesBetweenPositions(lastGridPos, gridPos, true))
            {
                sfxMan.Play("SingleHit");
            }
        }

        if (goingUpwards)
        {
            if (GlobalPosition.Y <= jarMan.JarTopPos)
            {
                goingUpwards = false;
                speed = 0;
                sfxMan.Play("Thud");
            }
        }
        else
        {
            if (GlobalPosition.Y >= jarMan.JarBottomPos)
            {
                sfxMan.Play("Thud");
                FinishPowerUp();
            }
        }
        
        lastGridPos = gridPos;
    }
}
