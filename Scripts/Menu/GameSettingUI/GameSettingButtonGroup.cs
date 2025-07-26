using Godot;
using System;
using System.Collections.Generic;

public partial class GameSettingButtonGroup : Control
{
	[Export] protected StringName settingName;
	// whether setting is shared between all players or not
	[Export] protected bool isCommonSetting;
	[Export] protected CommonGameSettings commonGameSettings;

	[ExportGroup("Optional")]
	// if the group's gameSetting has a value not matching any of the buttons, the fallBackButton will be highlighted
	[Export] protected GameSettingButtonInGroup fallBackHighlightButton;

	protected List<GameSettingButtonInGroup> buttons = new List<GameSettingButtonInGroup>();
	protected ButtonGroup buttonGroup = new ButtonGroup();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		int settingValue = (int)GetSettingValue();

		// Set pressed button to the button with the index matching the setting value, add the button group to each button, and assign signals

		Godot.Collections.Array<Node> nodes = GetChildren();

		for (int i = 0; i < nodes.Count; i++)
		{
			GameSettingButtonInGroup button = (GameSettingButtonInGroup)nodes[i];

			buttons.Add(button);

			button.ButtonGroup = buttonGroup;

			int buttonValue = button.GetValue();
			button.Pressed += () => SetSettingValue(buttonValue);
		}

		UpdateVisuals();
	}

	public void UpdateVisuals()
	{
		int settingValue = (int)GetSettingValue();
		bool found = false;

		for (int i = 0; i < buttons.Count; i++)
		{
			bool state = buttons[i].GetValue() == settingValue;
			buttons[i].ButtonPressed = state;

			if (!found && state)
				found = true;
		}

		if (!found && fallBackHighlightButton != null)
			fallBackHighlightButton.ButtonPressed = true;
	}

	protected Variant GetSettingValue()
	{
		if (isCommonSetting)
			return commonGameSettings.Get(settingName);
		else
			return commonGameSettings.CurrentPlayerGameSettings.Get(settingName);
	}

	protected virtual void SetSettingValue(int newValue)
	{
		if (isCommonSetting)
			commonGameSettings.Set(settingName, newValue);
		else
			commonGameSettings.CurrentPlayerGameSettings.Set(settingName, newValue);
	}
}
