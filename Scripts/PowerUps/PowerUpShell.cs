using Godot;
using System;

public partial class PowerUpShell : BaseShootPowerUp
{
    private bool goingRight = true;
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

        Position += (goingRight ? Vector2.Right : Vector2.Left) * speed * (float)delta;
        
        gridPos = WorldPosToGridPos(GlobalPosition);

        if (gridPos != lastGridPos)
        {
            if (DestroyTilesBetweenPositions(lastGridPos, gridPos, true))
            {
                sfxMan.Play("SingleHit");
            }
        }
        
        if (goingRight)
        {
            if (GlobalPosition.X >= jarMan.JarRightPos)
            {
                goingRight = false;
                sfxMan.Play("Ricochet");
            }
        }
        else
        {
            if (GlobalPosition.X <= jarMan.JarLeftPos)
            {
                sfxMan.Play("Ricochet");
                FinishPowerUp();
            }
        }

        
        lastGridPos = gridPos;
    }
}
