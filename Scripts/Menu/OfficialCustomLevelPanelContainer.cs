using Godot;
using System;
using System.Collections.Generic;

public partial class OfficialCustomLevelPanelContainer : ScrollContainer
{
	// Container/list of pre-made/built-in custom level panels
	[Export] private PackedScene officialCustomLevelPanelPrefab;

	[ExportGroup("Local References")]
	[Export] private Control buttonGroup;

	[ExportGroup("External References & Resources")]
	[Export] private MenuManager menuMan;
	[Export] private NotificationBox notificationBox;
    [Export] private OfficialCustomLevelList levelList;
    [Export] private UserCustomLevelList userLevelList;
    [Export] private HighScoreList highScoreList;
    [Export] private CommonGameSettings commonGameSettings;

    private Godot.Collections.Array<string> Levels { get { return levelList.Levels; } }
	private int currentPage = 0;
	private const int levelsPerPage = 8;
	private int PageCount
    {
        get 
        {
            if (Levels.Count == 0)
                return 1;
            
            return ((Levels.Count - 1) / levelsPerPage) + 1;
        }
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetPage(commonGameSettings.LastOfficialCustomLevelPage);

		(buttonGroup.GetChild(currentPage) as Button).ButtonPressed = true;
	}

	public void UpdateVisuals()
	{
		UpdateCurrentPage();
	}

	public void SetPage(int newPage)
	{
		currentPage = newPage;
		UpdateCurrentPage();
	}

	public void UpdateCurrentPage()
	{
		// reset v scroll position
        ScrollVertical = 0;

		// delete old panels
		foreach (Node node in GetChild(0).GetChildren())
        {
            node.QueueFree();
        }

		OfficialCustomLevelPanel prevPanel = null;

		// make new panels
        for (int i = 0; i < levelsPerPage; i++)
        {
			int lvlID = currentPage * levelsPerPage + i;

            if (lvlID > Levels.Count - 1)
                break;

            OfficialCustomLevelPanel levelPanel = officialCustomLevelPanelPrefab.Instantiate<OfficialCustomLevelPanel>();
            GetChild(0).AddChild(levelPanel);

            levelPanel.SetLevelDetails(levelList.GetLevelName(lvlID), highScoreList.GetOfficialCustomLevelHighScore(lvlID), highScoreList.HasClearOfficialCustomLevel(lvlID));

			levelPanel.PlayButton.Pressed += () => menuMan.StartGame(lvlID, true);
			levelPanel.CopyButton.Pressed += () => userLevelList.ImportLevel(Levels[lvlID]);
			levelPanel.CopyButton.Pressed += () => notificationBox.ShowMessage("Copied to your custom levels!");

			if (prevPanel != null)
			{
				levelPanel.SetAboveNeighbour(prevPanel);
				prevPanel.SetBelowNeighbour(levelPanel);
			}

			prevPanel = levelPanel;
        }

		if (commonGameSettings.LastOfficialCustomLevelPage != currentPage)
			commonGameSettings.LastOfficialCustomLevelPage = currentPage;
	}
}
