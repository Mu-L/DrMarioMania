using Godot;
using System;

public partial class PowerUpBomb : BasePowerUp
{
    [Export] private AnimationPlayer aniPlayer;
    private const int radius = 2;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        aniPlayer.Play("Explode");
        sfxMan.Play("Explode");
	}

    private void DestroySegments()
    {
        for (int y = -radius; y < radius + 1; y++)
        {
		    for (int x = -radius; x < radius + 1; x++)
            {
                Vector2I pos = InitialGridPos + new Vector2I(x, y);

                if (Mathf.Abs(x) + Mathf.Abs(y) < 3 && pos != InitialGridPos && !jarMan.IsTileUnbreakable(pos))
                {
                    jarMan.DestroySegment(pos);
                }
            }
        }
    }
}