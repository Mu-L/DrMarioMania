using Godot;
using System;
using System.Collections.Generic;

public partial class GameSettingMusicButtonGroup : GameSettingButtonGroup
{
	[Export] protected BaseScreen musicScreen;
	[Export] protected BaseScreenManager screenMan;
	[Export] protected MusicPreviewPlayer musicPreviewPlayer;
	public MusicPreviewPlayer MusicPreviewPlayer { get { return musicPreviewPlayer; } }
	[Export] protected CustomMusicContainer customMusicContainer;
	private GameSettingButtonInGroup lastButtonPressed = null;
	private GameSettingButtonInGroup previewButton = null;

	public ButtonGroup MusicButtonGroup { get { return buttonGroup; } }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Find each button, add to buttons list, add the button group to each button, and assign signals
		FindButtonsUnderNode(this);

		UpdateVisuals();
	}

	// Recursive function to find every button under a node
	// EXCLUDES CUSTOM SONG BUTTONS
	private void FindButtonsUnderNode(Node node)
	{
		Godot.Collections.Array<Node> nodes = node.GetChildren();

		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i] is GameSettingButtonInGroup)
			{
				GameSettingButtonInGroup button = (GameSettingButtonInGroup)nodes[i];

				buttons.Add(button);

				button.ButtonGroup = buttonGroup;

				int buttonValue = button.GetValue();

				button.Pressed += () => SetSettingValue(buttonValue);
				button.Pressed += () => ButtonPressed(button);
				
				button.FocusEntered += () => musicPreviewPlayer.SetPreviewMusic(buttonValue);
				button.FocusEntered += () => SetPreviewButton(button);
			}
			else if (nodes[i].GetChildCount() > 0 && nodes[i] != customMusicContainer)
				FindButtonsUnderNode(nodes[i]);
		}
	}

	public void SelectPreviewButton()
	{
		if (previewButton == null)
		{
			screenMan.GoBack();
			return;
		}

		int previewButtonValue = previewButton.GetValue();

		// -5 = custom song
		if ((int)GetSettingValue() != previewButtonValue || (previewButtonValue == GameConstants.customMusicID && musicPreviewPlayer.PreviewedCustomMusic != commonGameSettings.CustomMusicFile))
		{
			SetSettingValue(previewButtonValue);
		}

		screenMan.GoBack();
	}

	public void SetPreviewButton(GameSettingButtonInGroup index)
	{
		previewButton = index;
	}

	public void ButtonPressed(GameSettingButtonInGroup button)
	{
		// Go back is pressing the already selected button
		if (lastButtonPressed == button)
			screenMan.GoBack();
		else
		{
			lastButtonPressed = button;
		}
	}

	public override void UpdateVisuals()
	{
		customMusicContainer.UpdateVisuals();

		int settingValue = (int)GetSettingValue();

		Button buttonToFocus = buttons[0];

		for (int i = 0; i < buttons.Count; i++)
		{
			bool state = buttons[i].GetValue() == settingValue && settingValue != GameConstants.customMusicID;
			buttons[i].ButtonPressed = state;

			if (state)
				buttonToFocus = buttons[i];
		}

		string secondSettingValue = commonGameSettings.CustomMusicFile;
		List<GameSettingButtonInGroup> extraButtons = customMusicContainer.Buttons;
		
		for (int i = 0; i < extraButtons.Count; i++)
		{
			bool state = settingValue == GameConstants.customMusicID && secondSettingValue.ToUpper() == extraButtons[i].Text;
			extraButtons[i].ButtonPressed = state;

			if (state)
				buttonToFocus = extraButtons[i];
		}

		if (musicScreen.Visible)
			buttonToFocus.GrabFocus();
	}
}
