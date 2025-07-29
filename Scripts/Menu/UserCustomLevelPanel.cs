using Godot;
using System;

public partial class UserCustomLevelPanel : Panel
{
    // Represents a custom level panel - a box on the custom level screen contain level details and with buttons for playing, editing, deleting, etc the level

    [ExportGroup("Button References")]
    [Export] private Button playButton;
    public Button PlayButton { get { return playButton; } }
    [Export] private Button editButton;
    public Button EditButton { get { return editButton; } }
    [Export] private Button exportButton;
    private Button privateButton;
    public Button ExportButton { get { return exportButton; } }
    [Export] private MenuButton deleteButton;
    public Button DeleteButton { get { return deleteButton; } }
    [Export] private MenuButton arrangeButton;
    public Button ArrangeButton { get { return arrangeButton; } }
    
    [ExportGroup("Misc References")]
    [Export] private Label nameLabel;
    [Export] private Label dateLabel;

    [ExportGroup("Resources")]
    [Export] private UserCustomLevelList levelList;
    [Export] private CommonGameSettings commonGameSettings;

    public UserCustomLevelPanelContainer LevelContainer { set { levelContainer = value; } }
	private UserCustomLevelPanelContainer levelContainer;
    public NotificationBox NotificationBox { set { notificationBox = value; } }
	private NotificationBox notificationBox;

    private int levelID = -1;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        if (commonGameSettings.IsMultiplayer)
        {
            editButton.Visible = false;
        }

        // Connect signals to drop-down popups
        deleteButton.GetPopup().IdPressed += Delete;
        arrangeButton.GetPopup().IdPressed += Rearrange;
    }

    public void SetLevelDetails(int id, UserCustomLevelEntry levelData)
    {
        levelID = id;
        nameLabel.Text = levelData.name == "" ? "Untitled" : levelData.name;

        if (levelData.isCorrupted)
        {
            nameLabel.SelfModulate = new Color(1,0,0);
            playButton.Visible = false;
            editButton.Visible = false;

            dateLabel.Text = "CORRUPTED =(";
        }
        else
        {
            dateLabel.Text = levelData.date.day + "/" + levelData.date.month + "/" + levelData.date.year + " ";
            dateLabel.Text += levelData.date.hour.ToString("D2") + ":" + levelData.date.minute.ToString("D2");
        }

    }

    private void Export()
    {
        DisplayServer.ClipboardSet(levelList.CustomLevels[levelID].code);
        notificationBox.ShowMessage("Level code copied to clipboard!", 3000);
    }

    // Deletes the level at levelID
    private void Delete(long id)
    {
        levelList.CustomLevels.RemoveAt(levelID);
        levelContainer.UpdateCurrentPage();
        levelList.SaveToFile();
    }

    private void Rearrange(long id)
    {
        switch (id)
        {
            // ^^^ To top
            case 0:
                MovePosInList(-levelID);
                break;
            // ^ Up one
            case 1:
                MovePosInList(-1);
                break;
            // V Down one
            case 2:
                MovePosInList(1);
                break;
            // VVV To bottom
            case 3:
                MovePosInList(levelList.CustomLevels.Count - 1 - levelID);
                break;
            default:
                break;
        }
    }

    private void MovePosInList(int offset)
    {
        int newIndex = levelID + offset;

        if (offset == 0 || newIndex < 0 || newIndex > levelList.CustomLevels.Count - 1)
            return;
        
        UserCustomLevelEntry lvlEntry = levelList.CustomLevels[levelID];

        levelList.CustomLevels.RemoveAt(levelID);
        levelList.CustomLevels.Insert(newIndex, lvlEntry);

        levelContainer.UpdateCurrentPage();
        levelList.SaveToFile();
    }

    public void SetAboveNeighbour(UserCustomLevelPanel neighbour)
	{
		playButton.FocusNeighborTop = neighbour.PlayButton.GetPath();
		editButton.FocusNeighborTop = neighbour.EditButton.GetPath();
		exportButton.FocusNeighborTop = neighbour.ExportButton.GetPath();
		deleteButton.FocusNeighborTop = neighbour.DeleteButton.GetPath();
		arrangeButton.FocusNeighborTop = neighbour.ArrangeButton.GetPath();
	}

	public void SetBelowNeighbour(UserCustomLevelPanel neighbour)
	{
        playButton.FocusNeighborBottom = neighbour.PlayButton.GetPath();
		editButton.FocusNeighborBottom = neighbour.EditButton.GetPath();
		exportButton.FocusNeighborBottom = neighbour.ExportButton.GetPath();
		deleteButton.FocusNeighborBottom = neighbour.DeleteButton.GetPath();
		arrangeButton.FocusNeighborBottom = neighbour.ArrangeButton.GetPath();
	}
}
