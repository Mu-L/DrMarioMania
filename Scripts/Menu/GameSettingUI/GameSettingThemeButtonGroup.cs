using Godot;
using System;

public partial class GameSettingThemeButtonGroup : GameSettingButtonGroup
{
	[Export] private ThemePreview themePreview;
	[ExportGroup("Optional")]
	[Export] private GameSettingColourGroup colourGroup;
	[Export] private GameThemer gameThemer;

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
			if (colourGroup != null)
				button.Pressed += () => colourGroup.SetVirusTextures(buttonValue);
			button.Pressed += () => themePreview.SetPreviewTextures(buttonValue);
		}

		UpdateVisuals();
	}

	protected override void SetSettingValue(int newValue)
	{
		base.SetSettingValue(newValue);
		if (gameThemer != null)
			gameThemer.UpdateAllVisualsAndSfx();
	}
}
