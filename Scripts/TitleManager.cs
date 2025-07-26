using Godot;
using System;

public partial class TitleManager : Node
{
	[ExportGroup("Strings")]
	[Export] private string mobileStartText;
	[Export] private string disclaimerTitleText;
	[Export(PropertyHint.MultilineText)] private string disclaimerText;

	[ExportGroup("Animation Players")]
	[Export] private AnimationPlayer logoAniPlayer;

	[ExportGroup("External References")]
	[Export] private Label versionLabel;
	[Export] private Label startLabel;
	[Export] private ScreenTransition screenTransition;
	[Export] private PopUpGroup popUpGroup;

	[ExportGroup("Resources")]
	[Export] private CommonGameSettings commonGameSettings;
	[Export] private UserCustomLevelList userCustomLevelList;
	[Export] private HighScoreList highScoreList;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (!commonGameSettings.HasLoadedSettings)
		{
			commonGameSettings.LoadFromFile();
			userCustomLevelList.LoadFromFile();
			highScoreList.LoadFromFile();
		}

		if (GameConstants.IsOnMobile)
			startLabel.Text = mobileStartText;

		versionLabel.Text = "v" + (string)ProjectSettings.GetSetting("application/config/version");
		logoAniPlayer.Play("FallIn");
		SetProcess(false);
	}

	public void AcceptDisclaimer()
	{
		commonGameSettings.HasSeenDisclaimer = true;
		commonGameSettings.SaveToFile();
		GoToMainMenu();
	}

	private void GoToMainMenu()
	{
		screenTransition.Cover();
		SetProcess(false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsAnythingPressed())
		{
			if (commonGameSettings.HasSeenDisclaimer)
			{
				GoToMainMenu();
			}
			else
			{
				popUpGroup.ShowPopUp(disclaimerTitleText, disclaimerText, true);
				SetProcess(false);
			}
		}
	}
}
