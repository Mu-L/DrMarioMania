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
            List<Vector2I> positions = GetPositionsBetweenPositions(lastGridPos, gridPos, true);
            bool destroyedAnything = false;
            bool doRebound = false;

            for (int i = 0; i < positions.Count; i++)
            {
                if (jarMan.DoesTileCauseRebound(positions[i]))
                {
                    doRebound = true;
                    
                    // set position of power-up to last tile before blocking/rebounding it
                    GlobalPosition = GridPosToWorldPos(i != 0 ? positions[i - 1] : positions[i] + (goingUpwards ? Vector2I.Down : Vector2I.Up));
                }

                if (!jarMan.IsTileUnbreakable(positions[i]) && jarMan.DestroySegment(positions[i]))
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
            Vector2I aheadGridPos = WorldPosToGridPos(GlobalPosition + (goingUpwards ? Vector2I.Up : Vector2I.Down) * jarMan.JarCellSize / 2);

            if (jarMan.DoesTileCauseRebound(aheadGridPos))
            {
                if (AttemptToDestroySegment(aheadGridPos))
                    sfxMan.Play("SingleHit");

                Rebound();
            }
        }

        if (goingUpwards)
        {
            if (GlobalPosition.Y <= jarMan.JarTopPos)
                Rebound();
        }
        else
        {
            if (GlobalPosition.Y >= jarMan.JarBottomPos)
                Rebound();
        }
        
        lastGridPos = gridPos;
    }

    protected override void Rebound()
    {
        if (goingUpwards)
        {
            goingUpwards = false;
            speed = 0;
            sfxMan.Play("Thud");
        }
        else
        {
            sfxMan.Play("Thud");
            FinishPowerUp();
        }
    }
}
