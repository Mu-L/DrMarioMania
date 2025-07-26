using Godot;
using System;

public partial class GameSettingSlider : HSlider
{
	[Export] private StringName settingName;
	// whether setting is shared between all players or not
	[Export] protected bool isCommonSetting;
	[Export] private Label numberLabel;
	[Export] protected CommonGameSettings commonGameSettings;
	[Export] protected AudioStreamPlayer audioPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UpdateVisuals();
	}

	public void UpdateVisuals()
	{
		SetValueNoSignal((double)GetSettingValue());

		if (numberLabel != null)
			numberLabel.Text = Value.ToString();
	}

	private Variant GetSettingValue()
	{
		if (isCommonSetting)
			return commonGameSettings.Get(settingName);
		else
			return commonGameSettings.CurrentPlayerGameSettings.Get(settingName);
	}

	private void SetSettingValue(int newValue)
	{
		if (isCommonSetting)
			commonGameSettings.Set(settingName, newValue);
		else
			commonGameSettings.CurrentPlayerGameSettings.Set(settingName, newValue);

		if (numberLabel != null)
			numberLabel.Text = newValue.ToString();
	}

	private void PlaySound(float newValue)
	{
		audioPlayer.PitchScale = 1.0f + 0.5f * ((newValue - (float)MinValue) / (float)(MaxValue - MinValue));
		audioPlayer.Play();
	}
}
