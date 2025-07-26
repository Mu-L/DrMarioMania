using Godot;
using System;

public partial class EditorBaseSingleButton : Button
{
    protected Sprite2D sprite;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        sprite = GetChild<Sprite2D>(0);
        ButtonDown += () => ShiftSpriteDown();
        ButtonUp += () => ShiftSpriteUp();
    }

    protected virtual void ShiftSpriteDown()
    {
        sprite.Position += Vector2.Down;
    }

    protected virtual void ShiftSpriteUp()
    {
        sprite.Position += Vector2.Up;
    }
}
