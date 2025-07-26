using Godot;
using System;

public partial class NotificationManager : Node
{
	[Export] private CommonGameSettings commonGameSettings;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetTree().AutoAcceptQuit = false;
	}

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
		{
			commonGameSettings.SaveToFile();
			GetTree().Quit();
		}
		else if (GameConstants.IsOnMobile && what == NotificationApplicationPaused)
		{
			commonGameSettings.SaveToFile();
		}
		else if (what == NotificationWMGoBackRequest)
		{
			Input.ActionPress("Pause");
			Input.ActionPress("ui_cancel");

			Input.ActionRelease("Pause");
			Input.ActionRelease("ui_cancel");
		}
    }
}
