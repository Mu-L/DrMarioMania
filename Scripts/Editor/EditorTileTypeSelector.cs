using Godot;
using System;
using System.Collections.Generic;
using static PowerUpEnums;

public partial class EditorTileTypeSelector : EditorBaseTileSelector
{
    [Export] protected EditorColourSelector colourSelector;
    private List<int> buttonTileTypes = new List<int>();

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        // No. of tile variants (pill + virus + power ups + objects)
        buttonCount = 1 + 1 + GameConstants.NoOfPowerUps + GameConstants.noOfObjects;
        base._Ready();
        
        buttonTileTypes.Add(1);
        buttonTileTypes.Add(0);
        for (int i = 0; i < GameConstants.NoOfPowerUps; i++)
        {
            buttonTileTypes.Add(2);
        }
        for (int i = 0; i < GameConstants.noOfObjects; i++)
        {
            buttonTileTypes.Add(3);
        }

        UpdateVisuals();
    }

    public override void UpdateVisuals()
    {
        int i = 0;
        i += UpdateButtonSprites(i, virusAtlasSize, virusTexture, 1, 0, 0, false);
        i += UpdateButtonSprites(i, pillAtlasSize, pillTexture, 1, 4, 0, false);
        i += UpdateButtonSprites(i, powerUpAtlasSize, powerUpTexture, GameConstants.NoOfPowerUps, 0, 1, false);
        i += UpdateButtonSprites(i, objectAtlasSize, objectTexture, GameConstants.noOfObjects, 0, 0, true);
    }

    private int UpdateButtonSprites(int startingIndex, Vector2I size, Texture2D tex, int repeat, int xOffset, int yOffset, bool noColour)
    {
        for (int i = 0; i < repeat; i++)
        {
            buttonSprites[startingIndex + i].Hframes = size.X;
            buttonSprites[startingIndex + i].Vframes = size.Y;

            buttonSprites[startingIndex + i].Texture = tex;

            if (noColour)
                buttonSprites[startingIndex + i].Frame = buttonSprites[startingIndex].Hframes * i;
            else
                buttonSprites[startingIndex + i].Frame = buttonSprites[startingIndex].Hframes * (cursor.CurrentColour - 1 + yOffset) + i + xOffset;
        }

        return repeat;
    }

    public void PressButton(int tileType, int tileID)
    {
        int firstIndexOfType = buttonTileTypes.IndexOf(tileType);

        PressButton(firstIndexOfType + tileID);
    }

    public override void PressButton(int index)
    {
        base.PressButton(index);

        int firstIndexOfType = buttonTileTypes.IndexOf(buttonTileTypes[index]);

        cursor.SetCursorType(buttonTileTypes[index], index - firstIndexOfType);

        colourSelector.UpdateVisuals();
    }
}
