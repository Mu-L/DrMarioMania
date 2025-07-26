using Godot;
using System;

public partial class GameSettingToggleButton : Button
{
	[ExportGroup("Strings")]
	[Export] private string onString;
	[Export] private string offString;

	[ExportGroup("Setting-related")]
	[Export] private StringName settingName;
	// whether setting is shared between all players or not
	[Export] protected bool isCommonSetting;
	[Export] protected CommonGameSettings commonGameSettings;
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UpdateVisuals();
	}

	public void UpdateVisuals()
	{	
		ButtonPressed = (bool)GetSettingValue();
		Text = ButtonPressed ? onString : offString;
	}

	private Variant GetSettingValue()
	{
		if (isCommonSetting)
			return commonGameSettings.Get(settingName);
		else
			return commonGameSettings.CurrentPlayerGameSettings.Get(settingName);
	}

	private void SetSettingValue(bool newValue)
	{
		if (isCommonSetting)
			commonGameSettings.Set(settingName, newValue);
		else
			commonGameSettings.CurrentPlayerGameSettings.Set(settingName, newValue);

		Text = ButtonPressed ? onString : offString;
	}
}
