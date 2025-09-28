using Godot;
using System;

public partial class PowerUpVirusBlaster : BaseInstantPowerUp
{
    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (colour == 0)
			sfxMan.Play("VirusMatch");
		
		if (!IsQueuedForDeletion())
			jarMan.DestroyAllViruses(colour);
        base._Ready();
	}
}
