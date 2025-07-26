using Godot;
using System;

public partial class MultiplayerCountScreen : BaseScreen
{
	// Multiplayer player count screen

    [Export] private int nextScreen;

    [ExportGroup("Refernces and Resources")]
	[Export] private MenuManager menuMan;
	[Export] private MenuScreenManager screenMan;
	[Export] private MultiplayerControlContainer multiControlCon;

    private void SetPlayerCount(int players)
	{
		menuMan.SetPlayerCount(players);
		multiControlCon.RefreshVisuals();
        screenMan.SetScreen(nextScreen);
	}
}
