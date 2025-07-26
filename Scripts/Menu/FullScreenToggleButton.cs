using Godot;
using System;

public partial class FullScreenToggleButton : Button
{
    private FullScreenManager fullScreenMan;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        fullScreenMan = GetNode<FullScreenManager>("/root/FullScreenManager");
	}

    private void ToggleFullScreen()
    {
        fullScreenMan.ToggleFullScreen();
    }
}
