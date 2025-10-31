using Godot;
using System;
using System.Collections.Generic;

public partial class CustomMusicContainer : FlowContainer
{
	[Export] private Button desktopFolderButton;
	[Export] private Button popUpButtonMobile;
	[Export] private Button popUpButtonPC;
	[Export] private PackedScene buttonPrefab;
	[Export] private CommonGameSettings commonGameSettings;
	[Export] private GameSettingMusicButtonGroup musicGroup;
	[Export] private MusicList musicList;
	private List<GameSettingButtonInGroup> buttons = new List<GameSettingButtonInGroup>();
	public List<GameSettingButtonInGroup> Buttons { get { return buttons; } }

	public void OpenMusicFolder()
	{
		MakeMusicFolder();

		OS.ShellShowInFileManager(GameConstants.MusicFolderPath);
	}

	private void MakeMusicFolder()
	{
		if (GameConstants.IsOnMobile)
		{	
			if (OS.RequestPermissions())
				GD.Print("storage permission granted");
			else
				GD.Print("storage permission NOT granted");
		}

		DirAccess dir = DirAccess.Open(GameConstants.ExternalFolderPath);
			
		// if user folder exists but not the music folder, make it
		if (dir != null && !dir.DirExists(GameConstants.MusicFolder))
			dir.MakeDirRecursive(GameConstants.MusicFolder);
	}

	private void Refresh()
	{
		GD.Print("refresh custom music");

		MakeMusicFolder();

		Control lastFocus = GetViewport().GuiGetFocusOwner();

		musicGroup.UpdateVisuals();

		if (lastFocus != null)
			lastFocus.GrabFocus();
	}

	public void UpdateVisuals()
	{
		desktopFolderButton.Visible = !GameConstants.IsOnMobile;
		popUpButtonMobile.Visible = GameConstants.IsOnMobile;
		popUpButtonPC.Visible = !GameConstants.IsOnMobile;

		// Remove old song buttons
		foreach (var button in buttons)
		{
			button.QueueFree();
		}
		buttons.Clear();

		List<string> customMusicList = musicList.GetCustomMusicList();

        string prevButtonText = "";

        for (int i = 0; i < customMusicList.Count; i++)
		{
			string fileName = customMusicList[i];
            string buttonText = fileName.ToUpper().Split('.')[0];

			if (buttonText == prevButtonText)
                continue;

            GameSettingButtonInGroup button = buttonPrefab.Instantiate<GameSettingButtonInGroup>();
			AddChild(button);

			button.Text = buttonText;
			button.ButtonGroup = musicGroup.MusicButtonGroup;

			button.SetValue(GameConstants.customMusicID);
			int buttonValue = button.GetValue();
			
			button.FocusEntered += () => musicGroup.MusicPreviewPlayer.SetPreviewedCustomMusic(fileName);
			button.Pressed += () => SetCustomMusic(fileName);

			button.Pressed += () => musicGroup.ButtonPressed(button);
				
			button.FocusEntered += () => musicGroup.MusicPreviewPlayer.SetPreviewMusic(buttonValue);
			button.FocusEntered += () => musicGroup.SetPreviewButton(button);

			buttons.Add(button);

            prevButtonText = button.Text;
        }
	}

	private void SetCustomMusic(string name)
	{
		commonGameSettings.Set(musicGroup.SettingName, GameConstants.customMusicID);

		if (commonGameSettings.IsCustomLevel)
			commonGameSettings.CustomLevelCustomMusicFile = name;
		else
			commonGameSettings.CustomMusicFile = name;
	}
}
