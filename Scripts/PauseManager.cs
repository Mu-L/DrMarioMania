using Godot;
using System;
using System.Collections.Generic;

public partial class PauseManager : BaseHistoryScreenManager
{
    [ExportGroup("Local References")]
    [Export] private AnimationPlayer pauseAni;
    [ExportGroup("External References")]
    [Export] private GameManager gameMan;
    private bool menuVisible = false;

    public override void _Ready()
	{
        base._Ready();
    }

    public void SetPauseMenuVisibility(bool b)
    {
        pauseAni.Play(b ? "Show" : "Hide");
        menuVisible = true;
    }

    public override void GoBack()
    {
        PopHistory();

        if (screenHistory.Count < 1)
        {
            gameMan.SetIsPaused(false);
            return;
        }

        screens[currentScreen].ResetLastHoverNode();
        screens[currentScreen].Visible = false;
        screens[prevScreen].Visible = true;

		currentScreen = prevScreen;

		GrabFocusOfLastButton();
    }
}
