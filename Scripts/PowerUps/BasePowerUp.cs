using Godot;
using System;
using System.Collections.Generic;
using static PowerUpEnums;

public partial class BasePowerUp : Node2D
{
    [Export] protected Sprite2D sprite;
    [Export] protected Sprite2D previewSprite;
    [Export] protected PowerUp powerUp;
    [Export] private Material rainbowMat;
    protected JarManager jarMan;
    public JarManager JarMan { set { jarMan = value; } }
    protected SfxManager sfxMan;
    public SfxManager SfxMan { set { sfxMan = value; } }
    public Vector2I InitialGridPos { get; set; }
    public virtual Texture2D Texture { set { sprite.Texture = value; } }

    protected int colour;
    public int Colour
    {
        set
        {
            colour = value;
            sprite.Frame = (int)powerUp + value * sprite.Hframes;

            if (value == 0)
                sprite.Material = rainbowMat;
        }
    }

    protected void FinishPowerUp()
    {
        jarMan.FinishPowerUp(this);
        QueueFree();
    }

    // Destroyes all jar tiles between start and end positions given
    // returns true if successfully destroyed at least one tile, false if nothing was destroyed
    protected bool DestroyTilesBetweenPositions(Vector2I startPos, Vector2I endPos, bool skipFirstTile)
    {
        Vector2I diff = endPos - startPos;
        bool destroyedSomething = false;
        
        if (startPos == endPos)
        {
            if (!skipFirstTile && jarMan.DestroySegment(startPos))
                destroyedSomething = true;
        }
        else if (diff.Length() == 1)
        {
            if (!skipFirstTile && jarMan.DestroySegment(startPos))
                destroyedSomething = true;

            if (jarMan.DestroySegment(endPos))
                destroyedSomething = true;
        }
        else
        {
            // create array for storing positions
            List<Vector2I> positions = new List<Vector2I>();

            // size of x and y box area
            Vector2I areaSize;
            areaSize.X = Mathf.Abs(diff.X) + 1;
            areaSize.Y = Mathf.Abs(diff.Y) + 1;

            // min and max lengths (one is x length, the other is y)
            int maxLength = Mathf.Max(areaSize.X, areaSize.Y);

            // draw line based on line to pixel interpolation (skip first tile, i = 1, if skipFirstTile is true)
            for (int i = skipFirstTile ? 1 : 0; i < maxLength; i++)
            {
                Vector2I interpolatedPos;
                interpolatedPos.X = startPos.X + Mathf.RoundToInt(diff.X * (i / (float)(maxLength - 1)));
                interpolatedPos.Y = startPos.Y + Mathf.RoundToInt(diff.Y * (i / (float)(maxLength - 1)));

                positions.Add(interpolatedPos);
            }

            // destroy tiles at each position
            foreach (Vector2I pos in positions)
            {
                if (jarMan.DestroySegment(pos))
                    destroyedSomething = true;
            }
        }

        return destroyedSomething;
    }

    protected Vector2I WorldPosToGridPos(Vector2 worldPos)
    {
        Vector2 scaledPos = (worldPos - jarMan.TilemapGlobalPos) / jarMan.JarCellSize - Vector2.One / 2.0f;
        Vector2I gridPos = new Vector2I(Mathf.RoundToInt(scaledPos.X), Mathf.RoundToInt(scaledPos.Y));
        
        return gridPos;
    }
}
