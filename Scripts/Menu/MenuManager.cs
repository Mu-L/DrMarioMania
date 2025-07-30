using Godot;
using System;

public partial class MenuManager : Node
{
	[ExportGroup("References")]
	[Export] private ScreenTransition screenTransition;
	[Export] private MenuScreenManager screenMan;
	[Export] private MultiplayerControlContainer multiControlCon;

	[ExportGroup("Resources")]
	[Export] private CommonGameSettings commonGameSettings;
	[Export] private UserCustomLevelList userCustomLevelList;
	[Export] private OfficialCustomLevelList officialCustomLevelList;
	[Export] private HighScoreList highScoreList;
	[Export] private ThemeList themeList;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (!commonGameSettings.HasLoadedSettings)
		{
			commonGameSettings.LoadFromFile();
			userCustomLevelList.LoadFromFile();
			highScoreList.LoadFromFile();
		}

		// Set CustomLevelID to -2 (-2 means not in a custom level)
		commonGameSettings.CustomLevelID = -2;
	}

	public void SetPlayerCount(int players)
	{
		commonGameSettings.PlayerCount = players;
		commonGameSettings.RefreshMultiplayerInputIndexes();
		multiControlCon.RefreshVisuals();
	}

	public void StartEditor(int levelID = -1)
	{
		SetPlayerCount(1);
		screenTransition.NextScene = "EditorScene";
		StartGame(levelID, false);
	}

	public void StartGame()
	{
		StartGame(-2, false);
	}

	public void StartGame(int levelID, bool isOfficialCustomLevel)
	{
		screenMan.UpdateLastMenuScreenAndHistory();

		commonGameSettings.CurrentPlayerGameSettings.FixChosenColoursList();
		SaveSettings();
		
		commonGameSettings.CustomLevelID = levelID;
		commonGameSettings.CustomLevelName = (levelID < 0) ? "" : (isOfficialCustomLevel ? officialCustomLevelList.GetLevelName(levelID) : userCustomLevelList.CustomLevels[levelID].name);
		commonGameSettings.IsOfficialCustomLevel = isOfficialCustomLevel;
		screenTransition.Cover();
		ProcessMode = ProcessModeEnum.Disabled;
		screenMan.ProcessMode = ProcessModeEnum.Disabled;
	}

	public void BackToTitle()
	{
		screenMan.UpdateLastMenuScreenAndHistory();

		screenTransition.NextScene= "TitleScene";
		screenTransition.CoverStyle = 0;
		screenTransition.Cover();
	}

	public void SaveSettings()
	{
		commonGameSettings.SaveToFile();
	}

	public void SaveAndQuit()
	{
		commonGameSettings.SaveToFile();
		screenTransition.CoverStyle = 1;
		screenTransition.NextScene = "Quit";
		screenTransition.Cover();
	}

	public void OpenURL(string url)
	{
		OS.ShellOpen(url);
	}
}
