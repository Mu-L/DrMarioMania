using Godot;
using System;

public partial class FullScreenManager : Node
{
    private DisplayServer.WindowMode lastNonFullscreenWindowMode = DisplayServer.WindowMode.Windowed;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ToggleFullScreen"))
		{
			ToggleFullScreen();
        }
	}

	public void ToggleFullScreen()
	{
		if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen)
			DisplayServer.WindowSetMode(lastNonFullscreenWindowMode);
		else
		{
			lastNonFullscreenWindowMode = DisplayServer.WindowGetMode();
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
	}
}
