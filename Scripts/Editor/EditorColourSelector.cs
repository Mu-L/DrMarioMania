using Godot;
using System;
using System.Collections.Generic;
using static PowerUpEnums;

public partial class EditorColourSelector : EditorBaseTileSelector
{
    [Export] protected EditorTileTypeSelector tileSelector;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        // No. of colours
        buttonCount = 10;
        base._Ready();

        UpdateVisuals();
    }

    public override void UpdateVisuals()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (cursor.IsPowerUp)
            {
                buttonSprites[i].Hframes = powerUpAtlasSize.X;
                buttonSprites[i].Vframes = powerUpAtlasSize.Y;

                buttonSprites[i].Texture = powerUpTexture;
                buttonSprites[i].Frame = buttonSprites[i].Hframes * (i + 1) + (int)cursor.CurrentPowerUp;
            }
            else
            {
                buttonSprites[i].Hframes = virusAtlasSize.X;
                buttonSprites[i].Vframes = virusAtlasSize.Y;

                buttonSprites[i].Texture = virusTexture;
                buttonSprites[i].Frame = buttonSprites[i].Hframes * i;
            }
        }
    }

    public override void PressButton(int index)
    {
        base.PressButton(index);

        cursor.SetCursorColour(index + 1);

        tileSelector.UpdateVisuals();
    }
}
