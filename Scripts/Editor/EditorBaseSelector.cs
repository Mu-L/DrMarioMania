using Godot;
using System;
using System.Collections.Generic;

public partial class EditorBaseSelector : FlowContainer
{    
    protected List<Button> buttons = new List<Button>();
    protected List<Sprite2D> buttonSprites = new List<Sprite2D>();
    protected List<bool> shiftedSprites = new List<bool>();

    protected Vector2I virusAtlasSize;
    protected Vector2I powerUpAtlasSize;

    protected int buttonCount = 1;
    protected int selectedButton = 0;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        buttonCount = GetChildCount();

        for (int i = 0; i < buttonCount; i++)
        {
            buttons.Add(GetChild<Button>(i));
            buttonSprites.Add(buttons[i].GetChild<Sprite2D>(0));
            shiftedSprites.Add(buttons[i].ButtonPressed);

            int index = i;
            buttons[i].Pressed += () => PressButton(index);
            buttons[i].ButtonDown += () => ButtonDown(index);
            buttons[i].ButtonUp += () => ButtonUp(index);
        }
        
        buttonSprites[0].Position += Vector2.Down;
    }

    protected virtual void ButtonDown(int index)
    {
        SetSpriteShiftState(index, true);
    }

    protected virtual void ButtonUp(int index)
    {
        if (!buttons[index].ButtonPressed)
        {
            SetSpriteShiftState(index, false);
        }
    }

    protected void SetSpriteShiftState(int index, bool shifted)
    {
        if (shiftedSprites[index] == shifted)
            return;

        shiftedSprites[index] = shifted;
        buttonSprites[index].Position += shifted ? Vector2.Down : Vector2.Up;
    }

    public virtual void UpdateVisuals()
    {

    }

    public virtual void PressButton(int index)
    {
        if (selectedButton == index)
            return;
        
        SetSpriteShiftState(selectedButton, false);

        selectedButton = index;
        
        if (!buttons[index].ButtonPressed)
            buttons[index].ButtonPressed = true;

        SetSpriteShiftState(index, true);
    }
}
