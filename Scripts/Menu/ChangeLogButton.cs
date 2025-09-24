using Godot;
using System;

public partial class ChangeLogButton : PopUpButton
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        base._Ready();

		Text = "v" + (string)ProjectSettings.GetSetting("application/config/version");
        title = title.Replace("0.0.0", (string)ProjectSettings.GetSetting("application/config/version"));
    }
}
