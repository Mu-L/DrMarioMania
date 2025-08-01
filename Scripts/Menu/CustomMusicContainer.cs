using Godot;
using System;
using System.Collections.Generic;

public partial class CustomMusicContainer : FlowContainer
{
	[Export] private PackedScene buttonPrefab;
	[Export] private CommonGameSettings commonGameSettings;
	[Export] private GameSettingMusicButtonGroup musicGroup;
	private List<GameSettingButtonInGroup> buttons = new List<GameSettingButtonInGroup>();
	public List<GameSettingButtonInGroup> Buttons { get { return buttons; } }

	public void OpenMusicFolder()
	{
		DirAccess dir = DirAccess.Open(GameConstants.UserFolderPath);

		// if user folder exists but not the music folder, make it
		if (dir != null && !dir.DirExists("music"))
			dir.MakeDir("music");

		OS.ShellShowInFileManager(GameConstants.UserFolderPath + "music");
	}

	private List<string> GetCustomMusic()
	{
		List<string> customMusicList = new List<string>();

		DirAccess dir = DirAccess.Open(GameConstants.UserFolderPath);

		// return if folder doesnt exist
		if (dir == null || !dir.DirExists("music"))
			return customMusicList;

		dir.ChangeDir("music");

		string[] files = dir.GetFiles();

		for (int i = 0; i < files.Length; i++)
		{
			string file = files[i];
			bool invalid = false;

			foreach (char forbiddenChar in GameConstants.forbiddenLevelNameChars)
			{
				if (file.Contains(forbiddenChar))
				{
					invalid = true;
					break;
				}
			}

			if (invalid)
				continue;

			if (file.Contains(".mp3") || file.Contains(".ogg"))
				customMusicList.Add(file);
		}
		
		return customMusicList;
	}

	private void Refresh()
	{
		Control lastFocus = GetViewport().GuiGetFocusOwner();

		musicGroup.UpdateVisuals();

		if (lastFocus != null)
			lastFocus.GrabFocus();
	}

	public void UpdateVisuals()
	{
		// Remove old song buttons
		foreach (var button in buttons)
		{
			button.QueueFree();
		}
		buttons.Clear();

		List<string> customMusicList = GetCustomMusic();

		for (int i = 0; i < customMusicList.Count; i++)
		{
			GameSettingButtonInGroup button = buttonPrefab.Instantiate<GameSettingButtonInGroup>();
			AddChild(button);

			string name = customMusicList[i];
			button.Text = name.ToUpper();
			button.ButtonGroup = musicGroup.MusicButtonGroup;

			button.SetValue(GameConstants.customMusicID);
			int buttonValue = button.GetValue();
			
			button.FocusEntered += () => musicGroup.MusicPreviewPlayer.SetPreviewedCustomMusic(name);
			button.Pressed += () => SetCustomMusic(name);

			button.Pressed += () => musicGroup.ButtonPressed(button);
				
			button.FocusEntered += () => musicGroup.MusicPreviewPlayer.SetPreviewMusic(buttonValue);
			button.FocusEntered += () => musicGroup.SetPreviewButton(button);

			buttons.Add(button);
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
