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

    // Returns a list of tile tile positions between given start and end positions
    protected List<Vector2I> GetPositionsBetweenPositions(Vector2I startPos, Vector2I endPos, bool skipFirstTile)
    {
        Vector2I diff = endPos - startPos;
        List<Vector2I> positions = new List<Vector2I>();
        
        if (startPos == endPos)
        {
            if (!skipFirstTile)
                positions.Add(startPos);
        }
        else if (diff.Length() == 1)
        {
            if (!skipFirstTile)
                positions.Add(startPos);

            positions.Add(endPos);
        }
        else
        {
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
        }

        return positions;
    }

    protected Vector2I WorldPosToGridPos(Vector2 worldPos)
    {
        Vector2 scaledPos = (worldPos - jarMan.TilemapGlobalPos) / jarMan.JarCellSize - Vector2.One / 2.0f;
        Vector2I gridPos = new Vector2I(Mathf.RoundToInt(scaledPos.X), Mathf.RoundToInt(scaledPos.Y));
        
        return gridPos;
    }

    protected Vector2 GridPosToWorldPos(Vector2I gridPos)
    {
        Vector2 worldPos = jarMan.TilemapGlobalPos + gridPos * jarMan.JarCellSize;
		worldPos += jarMan.JarCellSize / 2;

        return worldPos;
    }
}
