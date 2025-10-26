using Godot;
using System;

public partial class PowerUpLightning : BasePowerUp
{
    [Export] private bool isHorizontal;
    [Export] private Sprite2D lightingStrikeSprite;
    [Export] private AnimationPlayer aniPlayer;
    [Export] public float RegionRectY
    {
        get { return lightingStrikeSprite.RegionRect.Position.Y; }
        set
        {
            if (lightingStrikeSprite == null)
                return;

            var rect = lightingStrikeSprite.RegionRect;
            var pos = lightingStrikeSprite.RegionRect.Position;

            pos.Y = value;
            rect.Position = pos;
            lightingStrikeSprite.RegionRect = rect;
        }
    }

    private float RegionHeight
    {
        set
        {
            if (lightingStrikeSprite == null)
                return;

            var rect = lightingStrikeSprite.RegionRect;
            var size = lightingStrikeSprite.RegionRect.Size;

            size.Y = value;
            rect.Size = size;
            lightingStrikeSprite.RegionRect = rect;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        aniPlayer.Play("Strike");
        sfxMan.Play("Explode");
        DestroyTiles();
        UpdateStrikeSprite();
    }

    private void UpdateStrikeSprite()
    {
        Vector2 strikePos;

        if (isHorizontal)
        {
            strikePos = new Vector2(-(InitialGridPos.X + 0.5f) * GameConstants.tileSize, 0);
            RegionHeight = jarMan.JarSize.X * GameConstants.tileSize;

            lightingStrikeSprite.Rotation = Mathf.Pi / 2.0f;
        }
        else
        {
            strikePos = new Vector2(0, -(InitialGridPos.Y + 0.5f) * GameConstants.tileSize);
            RegionHeight = jarMan.JarSize.Y * GameConstants.tileSize;
        }

        lightingStrikeSprite.Position = strikePos;
    }

    private void DestroyTiles()
    {
        if (IsQueuedForDeletion())
            return;

        Vector2I startPos = jarMan.JarOrigin;

		if (isHorizontal)
            startPos.Y = InitialGridPos.Y;
		else
            startPos.X = InitialGridPos.X;

        Vector2I direction = isHorizontal ? Vector2I.Right : Vector2I.Down;
        int repeat = isHorizontal ? jarMan.JarSize.X : jarMan.JarSize.Y;

        // first find any of the same power-up an destroy it, since there's no need to set off parallel lightnings of the same direction

        for (int i = 0; i < repeat; i++)
		{
            Vector2I pos = startPos + direction * i;
			
			if (pos != InitialGridPos)
            {
                bool isSamePowerUp = jarMan.IsTilePowerUp(pos, powerUp);

                if (isSamePowerUp)
                    jarMan.DestroyTile(pos, false, true);
            }
        }

        // then loop to destroy any remaining tiles 
        for (int i = 0; i < repeat; i++)
		{
            Vector2I pos = startPos + direction * i;
			
			if (pos != InitialGridPos && jarMan.IsTilePresent(pos) && !jarMan.IsTileUnbreakable(pos))
            {
                bool isSamePowerUp = jarMan.IsTilePowerUp(pos, powerUp);

                if (!isSamePowerUp)
                    jarMan.DestroyTile(pos);
            }
        }
    }
}
