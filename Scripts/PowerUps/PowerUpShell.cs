using Godot;
using System;
using System.Collections.Generic;

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
            List<Vector2I> positions = GetPositionsBetweenPositions(lastGridPos, gridPos, true);
            bool destroyedAnything = false;
            bool doRebound = false;

            for (int i = 0; i < positions.Count; i++)
            {
                // if one of these tiles causes a rebound, rebound power-up and break to not do anything to any tiles past this one
                if (jarMan.DoesTileCauseRebound(positions[i]))
                {
                    doRebound = true;
                    
                    // set position of power-up to last tile before blocking/rebounding it
                    GlobalPosition = GridPosToWorldPos(i != 0 ? positions[i - 1] : positions[i] + (goingRight ? Vector2I.Left : Vector2I.Right));
                }

                if (AttemptToDestroySegment(positions[i]))
                    destroyedAnything = true;

                if (doRebound)
                    break;
            }

            if (destroyedAnything)
                sfxMan.Play("SingleHit");
            
            if (doRebound)
                Rebound();
        }
        else
        {
            // grid pos 0.5 tiles ahead of the power-up
            Vector2I aheadGridPos = WorldPosToGridPos(GlobalPosition + (goingRight ? Vector2I.Right : Vector2I.Left) * jarMan.JarCellSize / 2);

            if (jarMan.DoesTileCauseRebound(aheadGridPos))
            {
                if (AttemptToDestroySegment(aheadGridPos))
                    sfxMan.Play("SingleHit");

                Rebound();
            }
        }
        
        if (goingRight)
        {
            if (GlobalPosition.X >= jarMan.JarRightPos)
                Rebound();
        }
        else
        {
            if (GlobalPosition.X <= jarMan.JarLeftPos)
                Rebound();
        }
        
        lastGridPos = gridPos;
    }

    protected override void Rebound()
    {
        if (goingRight)
        {
            goingRight = false;
            sfxMan.Play("Ricochet");
        }
        else
        {
            sfxMan.Play("Ricochet");
            FinishPowerUp();
        }
    }
}
