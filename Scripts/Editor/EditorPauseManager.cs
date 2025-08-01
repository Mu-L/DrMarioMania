using Godot;
using System;

public partial class EditorPauseManager : PauseManager
{
    [Export] private EditorManager editorMan;
    [Export] private PopUpGroup popUpGroup;
    [Export] private MusicPreviewPlayer musicPreviewPlayer;
    [Export] private CommonGameSettings commonGameSettings;
    
    public override void GoBack()
    {
        backFrame = Engine.GetFramesDrawn();
        
        if (popUpGroup.IsOpen)
		{
			popUpGroup.HidePopUp();
			return;
		}
        
        PopHistory();

        // if backing out of level settings screen AND editor music disabled, revert background music
		if (currentScreen == 3)
		{
            if (!commonGameSettings.EnableEditorMusic)
			    musicPreviewPlayer.RestoreNormalMusic();
		}
        else if (currentScreen == 5)
        {
            if (commonGameSettings.EnableEditorMusic)
            {
                if (!musicPreviewPlayer.Playing)
			        musicPreviewPlayer.SetPreviewMusicToCurrent();
            }
            else if (musicPreviewPlayer.Playing)
                musicPreviewPlayer.Stop();
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
