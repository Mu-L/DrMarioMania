using Godot;
using System;
using System.Collections.Generic;
using static PowerUpEnums;

public partial class EditorTileTypeSelector : EditorBaseTileSelector
{
    [Export] protected EditorColourSelector colourSelector;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        // No. of tile variants (virus + power ups)
        buttonCount = 7;
        base._Ready();     

        UpdateVisuals();
    }

    public override void UpdateVisuals()
    {
        buttonSprites[0].Hframes = virusAtlasSize.X;
        buttonSprites[0].Vframes = virusAtlasSize.Y;

        buttonSprites[0].Texture = virusTexture;
        buttonSprites[0].Frame = buttonSprites[0].Hframes * (cursor.CurrentColour - 1);

        for (int i = 1; i < buttons.Count; i++)
        {
            buttonSprites[i].Hframes = powerUpAtlasSize.X;
            buttonSprites[i].Vframes = powerUpAtlasSize.Y;

            buttonSprites[i].Texture = powerUpTexture;
            buttonSprites[i].Frame = buttonSprites[i].Hframes * cursor.CurrentColour + i - 1;
        }
    }

    public override void PressButton(int index)
    {
        base.PressButton(index);

        if (index == 0)
            cursor.SetCursorToVirus();
        else
            cursor.SetCursorToPowerUp((PowerUp)(index - 1));

        colourSelector.UpdateVisuals();
    }
}
