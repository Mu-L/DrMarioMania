using Godot;
using System;

public partial class PowerUpPushDown : BasePowerUp
{
	[Export] private AnimationPlayer aniPlayer;
	[Export] private Sprite2D lineSprite;
	[Export] public float RegionRectY
    {
        get { return lineSprite.RegionRect.Position.Y; }
        set
        {
            if (lineSprite == null)
                return;

            var rect = lineSprite.RegionRect;
            var pos = lineSprite.RegionRect.Position;

            pos.Y = value;
            rect.Position = pos;
            lineSprite.RegionRect = rect;
        }
    }
	private float RegionHeight
    {
		get { return lineSprite.RegionRect.Size.Y; }
        set
        {
            if (lineSprite == null)
                return;

            var rect = lineSprite.RegionRect;
            var size = lineSprite.RegionRect.Size;

            size.Y = value;
            rect.Size = size;
            lineSprite.RegionRect = rect;
        }
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        aniPlayer.Play("PushDown");
        sfxMan.Play("Explode");
        PushDown();
		UpdateLineSprite();
    }

	private void UpdateLineSprite()
    {
        Vector2 linePos;

        RegionHeight = jarMan.JarSize.Y * GameConstants.tileSize;
        linePos = new Vector2(0, -(InitialGridPos.Y + 0.5f) * GameConstants.tileSize);

        lineSprite.Position = linePos;
    }

	private void PushDown()
	{
		for (int y = jarMan.JarOrigin.Y + jarMan.JarSize.Y - 1; y >= jarMan.JarOrigin.Y; y--)
		{
            Vector2I pos = new Vector2I(InitialGridPos.X, y);

            int sourceID = jarMan.GetTileSourceID(pos);


            if (sourceID == GameConstants.pillSourceID || sourceID == GameConstants.virusSourceID || sourceID == GameConstants.powerUpSourceID)
			{
				// split pill if horizontal double
				if (sourceID == GameConstants.pillSourceID)
				{
                    int atlas = jarMan.GetTileAtlas(pos).X;

					if (atlas == PillConstants.atlasLeft || atlas == PillConstants.atlasRight)
					{
                        jarMan.SplitPillInTwo(pos);
                    }
                }
                
                // if solid below pos, don't fall
                if (!jarMan.IsPosEmptyOrWillUpdate(pos + Vector2I.Down))
                    continue;
                    
            	jarMan.AddTileToFall(pos);
			}
        }
	}
}
