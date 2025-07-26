using Godot;
using System;

public partial class EditorResetButton : Button
{
    [Export] private Sprite2D sprite;
    [Export] private GameManager gameMan;
    [Export] private EditorManager editorMan;
    [Export] private EditorUndoRedoManager undoRedoMan;
    private JarManager jarMan;
    private float timer = 0;
    private float timerSpeed = 2;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        jarMan = gameMan.Jars[0];
        SetProcess(false);
        sprite.Frame = 0;
	}

    private void BeginPress()
    {
        timer = 3;
        SetProcess(true);
        sprite.Position += Vector2.Down;
    }

    private void EndPress()
    {
        SetProcess(false);
        sprite.Frame = 0;
        sprite.Position += Vector2.Up;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (timer > 0)
        {
            timer -= timerSpeed * (float)delta;
            sprite.Frame = 3 - (int)timer;

            if (timer <= 0)
            {
                undoRedoMan.StartUndoRedoStep();
                editorMan.ClearAllTiles(true);
                undoRedoMan.EndUndoRedoStep();
                sprite.Frame = 0;
            }
        }
    }
}
