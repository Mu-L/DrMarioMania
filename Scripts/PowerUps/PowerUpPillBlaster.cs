using Godot;
using System;

public partial class PowerUpPillBlaster : BaseInstantPowerUp
{
    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (colour == 0)
			sfxMan.Play("PillMatch");
		
		jarMan.DestroyAllPillSegments(colour);
        base._Ready();
	}
}
