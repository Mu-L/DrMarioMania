using Godot;
using System;

public partial class MultiplayerPlayerScreen : UpdateableSettingScreen
{
    // Multiplayer player-specific settings screen

    [ExportGroup("Internal References")]
    [Export] private Label titleLabel;
    [Export] private Button nextButton;
    [Export] private Button allUseP1SettingsButton;
    [Export] private Button copyButton;
    [Export] private GameSettingColourGroup colourGroup;

    [ExportGroup("External References/Resources")]
    [Export] private MenuManager menuMan;
    [Export] private MenuScreenManager screenMan;
    [Export] private CommonGameSettings commonGameSettings;

    // set PlayerConfiguring to 1 to start configuring player 1 in multiplayer mode
    private void InitialisePlayerConfiguring()
    {
        commonGameSettings.PlayerConfiguring = 1;
    }

    private void CopyPlayer1Settings()
    {
        GD.Print("copy!");
        commonGameSettings.CurrentPlayerGameSettings.CopySettings(commonGameSettings.P1GameSettings);

        screenMan.RefreshUpdateableSettingScreenValues(GetIndex());
    }

    private void UpdateUI()
    {
        titleLabel.Text = "PLAYER " + commonGameSettings.PlayerConfiguring + " SETTINGS";
        nextButton.Text = (commonGameSettings.PlayerConfiguring  == commonGameSettings.PlayerCount) ? "START!" : "NEXT";
        copyButton.Visible = commonGameSettings.PlayerConfiguring > 1;
        allUseP1SettingsButton.Visible = !copyButton.Visible;

        screenMan.RefreshUpdateableSettingScreenValues(GetIndex());
        InitialHoverNode.GrabFocus();
    }

    public void AllUseP1SettingsAndStart()
    {
        commonGameSettings.P2GameSettings.CopySettings(commonGameSettings.P1GameSettings);
        commonGameSettings.P3GameSettings.CopySettings(commonGameSettings.P1GameSettings);
        commonGameSettings.P4GameSettings.CopySettings(commonGameSettings.P1GameSettings);

        commonGameSettings.PlayerConfiguring = 0;
		menuMan.StartGame();
    }

    public void NextPlayer()
	{
        commonGameSettings.CurrentPlayerGameSettings.FixSegmentColoursList();

		if (commonGameSettings.PlayerConfiguring == commonGameSettings.PlayerCount)
        {
            commonGameSettings.PlayerConfiguring = 0;
			menuMan.StartGame();
        }
		else
		{
			commonGameSettings.PlayerConfiguring++;
            UpdateUI();
		}
	}

	public void PrevPlayer()
	{
		if (commonGameSettings.PlayerConfiguring > 1)
		{            
			commonGameSettings.PlayerConfiguring--;
            UpdateUI();
		}
	}
}
