using Godot;
using System;

public partial class EditorBaseTileSelector : EditorBaseInputSelector
{
    [ExportGroup("Textures")]
    [Export] protected Texture2D virusTexture;
    [Export] protected Texture2D powerUpTexture;

    [ExportGroup("References")]
    [Export] protected Button firstButton;
    [Export] protected EditorCursor cursor;
    [Export] protected EditorBaseSelector drawingToolSelector;
    
    public Texture2D VirusTexture
    {
        set
        {
            foreach (Sprite2D sprite in buttonSprites)
            {
                if (sprite.Texture == virusTexture)
                    sprite.Texture = value;
            }

            virusTexture = value;
        }
    }
    public Texture2D PowerUpTexture
    {
        set
        {
            foreach (Sprite2D sprite in buttonSprites)
            {
                if (sprite.Texture == powerUpTexture)
                    sprite.Texture = value;
            }

            powerUpTexture = value;
        }
    }

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        virusAtlasSize = cursor.VirusAtlasSize;
        powerUpAtlasSize = cursor.PowerUpAtlasSize;

        for (int i = 0; i < buttonCount; i++)
        {
            Button newButton;
            
            if (i == 0)
            {
                newButton = firstButton;
            }
            else
            {
                newButton = firstButton.Duplicate() as Button;
                AddChild(newButton);
                newButton.ButtonPressed = false;
            }

            buttons.Add(newButton);
            buttonSprites.Add(newButton.GetChild<Sprite2D>(0));
            shiftedSprites.Add(newButton.ButtonPressed);

            int index = i;
            newButton.Pressed += () => PressButton(index);
            newButton.Pressed += () => drawingToolSelector.PressButton(0);
            newButton.Pressed += () =>  cursor.SetDrawingTool(0);

            newButton.ButtonDown += () => ButtonDown(index);
            newButton.ButtonUp += () => ButtonUp(index);
        }
        
        buttonSprites[0].Position += Vector2.Down;
    }

    protected override void Previous()
    {
        if (editorMan.CanPressButtons)
            PressButton((selectedButton - 1 + buttons.Count) % buttons.Count);
    }

    protected override void Next()
    {
        if (editorMan.CanPressButtons)
            PressButton((selectedButton + 1) % buttons.Count);
    }        
}
