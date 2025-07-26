using Godot;
using System;

public partial class EditorPauseManager : PauseManager
{
    [Export] private EditorManager editorMan;
    [Export] private PopUpGroup popUpGroup;
    [Export] private MusicPreviewPlayer musicPreviewPlayer;
    
    public override void GoBack()
    {
        if (popUpGroup.IsOpen)
		{
			popUpGroup.HidePopUp();
			return;
		}
        
        PopHistory();

        // if backing out of level settings screen, revert background music
		if (currentScreen == 3)
		{
			musicPreviewPlayer.RestoreNormalMusic();
		}

        if (screenHistory.Count < 1)
        {
            editorMan.SetIsPaused(false);
            return;
        }

        screens[currentScreen].ResetLastHoverNode();
        screens[currentScreen].Visible = false;
        screens[prevScreen].Visible = true;

		currentScreen = prevScreen;

        if (IsScreenUpdateableSettingScreen(currentScreen))
			RefreshUpdateableSettingScreenValues(currentScreen);

		GrabFocusOfLastButton();
    }

    public override void SetScreen(int nextScreen)
	{
		base.SetScreen(nextScreen);

		if (IsScreenUpdateableSettingScreen(currentScreen))
			RefreshUpdateableSettingScreenValues(currentScreen);
	}

	private bool IsScreenUpdateableSettingScreen(int screen)
	{
		return screens[screen] is UpdateableSettingScreen;
	}

    // updates the screen given's slider and button states based on their corrisponding game setting values
	public void RefreshUpdateableSettingScreenValues(int screen)
	{
        if (screens[screen] is UpdateableSettingScreen)
        {
            UpdateableSettingScreen gsScreen = screens[screen] as UpdateableSettingScreen;
            gsScreen.RefreshUpdateableSettingScreenValues();
        }
	}
}
