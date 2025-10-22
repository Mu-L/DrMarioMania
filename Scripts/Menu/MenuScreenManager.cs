using Godot;
using System;
using System.Collections.Generic;

public partial class MenuScreenManager : BaseHistoryScreenManager
{
	// manages switching between the different screens in the menu scene

	[Export] private MenuManager menuMan;
	[Export] private MusicPreviewPlayer musicPreviewPlayer;
	[Export] private CommonGameSettings commonGameSettings;
	[Export] private MultiplayerPlayerScreen multiPlayerScreen;
	[Export] private MultiplayerControlContainer multiControlCon;
	[Export] private PopUpGroup popUpGroup;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (commonGameSettings.LastMenuScreen != -1)
		{
			int lastScreen = commonGameSettings.LastMenuScreen;
			screenHistory = new List<int>(commonGameSettings.LastMenuScreenHistory);

			switch (lastScreen)
			{
				// single player - last screen = visuals, new screen = game rules
				case 2:
					if (commonGameSettings.GameMode == 0)
						currentScreen = 1;
					else
						currentScreen = 17;
						
					screenHistory.RemoveAt(screenHistory.Count - 1);
					break;
				// multiplayer player - last screen = player-specific settings, new screen = shared settings
				case 6:
					commonGameSettings.PlayerConfiguring = 1;
					currentScreen = 7;
					screenHistory.RemoveAt(screenHistory.Count - 1);
					break;
				default:
					currentScreen = lastScreen;
					break;
			}
		}

		base._Ready();

		if (commonGameSettings.LastMenuScreen == -1)
			screenHistory.Add(currentScreen);
	}

	public override void SetScreen(int nextScreen)
	{
		if (backFrame == Engine.GetFramesDrawn())
            return;

		base.SetScreen(nextScreen);

		if (IsScreenUpdateableSettingScreen(currentScreen))
			RefreshUpdateableSettingScreenValues(currentScreen);
	}

	private bool IsScreenUpdateableSettingScreen(int screen)
	{
		return screens[screen] is UpdateableSettingScreen;
	}

	public override void GoBack()
	{
		backFrame = Engine.GetFramesDrawn();
		
		if (popUpGroup.IsOpen)
		{
			popUpGroup.HidePopUp();
			return;
		}

		// if currentScreen is 0, return
		if (currentScreen == 0)
		{
			//menuMan.BackToTitle();
			//ProcessMode = ProcessModeEnum.Disabled;
			return;
		}
		
		// if exiting common multiplayer settings, set PlayerConfiguring to zero to indicate p1 is not configuing multiplayer settings anymore
		if (currentScreen == 7)
			commonGameSettings.PlayerConfiguring = 0;
		
		// if on multiplayer player screen and PlayerConfiguring is not player 1, call PrevPlayer and return
		if (currentScreen == 6 && commonGameSettings.PlayerConfiguring > 1)
		{
			multiPlayerScreen.PrevPlayer();
			return;
		}

		PopHistory();
		screens[currentScreen].ResetLastHoverNode();
		screens[currentScreen].Visible = false;
		screens[prevScreen].Visible = true;
		
		// if backing out of the first screen of single or multiplayer modes with game settings (gamerules and mode selction (since it has win slider) respectively), save settings
		if (currentScreen == 1 || currentScreen == 14)
		{
			menuMan.SaveSettings();

			// if backing out multiplayer mode selection menu, update player controller container
			if (currentScreen == 14)
			{
				multiControlCon.RefreshVisuals();
			}
		}
		// if backing out of visuals/sound (single and multi) or credits screen, revert background music to standard menu music
		else if (currentScreen == 2 || currentScreen == 7 || currentScreen == 9)
		{
			musicPreviewPlayer.RestoreNormalMusic();
		}
		// if backing out of music menu, set preview music to the selected music button
		else if (currentScreen == 4)
		{
			musicPreviewPlayer.SetPreviewMusicToCurrent();
		}

		currentScreen = prevScreen;

		if (IsScreenUpdateableSettingScreen(currentScreen))
			RefreshUpdateableSettingScreenValues(currentScreen);

		GrabFocusOfLastButton();
	}

	// updates the screen given's slider and button states based on their corrisponding game setting values
	public void RefreshUpdateableSettingScreenValues(int screen)
	{
		UpdateableSettingScreen gsScreen = screens[screen] as UpdateableSettingScreen;
		gsScreen.RefreshUpdateableSettingScreenValues();
	}

	public void UpdateLastMenuScreenAndHistory()
	{
		commonGameSettings.LastMenuScreen = currentScreen;
		commonGameSettings.LastMenuScreenHistory = new List<int>(screenHistory);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			GoBack();
		}
	}
}
