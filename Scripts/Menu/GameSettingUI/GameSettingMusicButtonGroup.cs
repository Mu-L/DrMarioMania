using Godot;
using System;

public partial class GameSettingMusicButtonGroup : GameSettingButtonGroup
{
	[Export] protected BaseScreen musicScreen;
	[Export] protected BaseScreenManager screenMan;
	[Export] protected MusicPreviewPlayer musicPreviewPlayer;
	private int lastButtonPressed = -1;
	private int previewButton = -1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Get the value of the setting associated with this group
		int settingValue = (int)GetSettingValue();

		// Find each button, add to buttons list, add the button group to each button, and assign signals
		FindButtonsUnderNode(this);

		// Update visuals for each button
		UpdateVisuals();
	}

	// Recursive function to find every button under a node
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
				int buttonIndex = buttons.Count - 1;
				button.Pressed += () => SetSettingValue(buttonValue);
				button.Pressed += () => ButtonPressed(buttonIndex);
				
				button.FocusEntered += () => musicPreviewPlayer.SetPreviewMusic(buttonValue);

				button.FocusEntered += () => SetPreviewButton(buttonIndex);
			}
			else if (nodes[i].GetChildCount() > 0)
				FindButtonsUnderNode(nodes[i]);
		}
	}

	private void SelectPreviewButton()
	{
		int previewButtonValue = buttons[previewButton].GetValue();

		if ((int)GetSettingValue() != previewButtonValue)
		{
			SetSettingValue(previewButtonValue);
			musicScreen.InitialHoverNode = buttons[previewButton];
		}

		screenMan.GoBack();
	}

	private void SetPreviewButton(int index)
	{
		previewButton = index;
	}

	private void ButtonPressed(int index)
	{
		// Go back is pressing the already selected button
		if (lastButtonPressed == index)
			screenMan.GoBack();
		else
		{
			lastButtonPressed = index;
			musicScreen.InitialHoverNode = buttons[index];
		}
	}

	private void RefreshLastButtonPressed()
	{
		lastButtonPressed = -1;

		int settingValue = (int)GetSettingValue();

		for (int i = 0; i < buttons.Count; i++)
		{
			if (buttons[i].GetValue() == settingValue)
			{
				lastButtonPressed = i;
				break;
			}
		}
	}
}
