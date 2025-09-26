using Godot;
using System;

public partial class GameSettingEditorFallSpeedButtonGroup : GameSettingButtonGroup
{
    [Export] private ThemePreview themePreview;
    [Export] private GameThemer gameThemer;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        base._Ready();

        for (int i = 0; i < buttons.Count; i++)
		{
			buttons[i].Pressed += () => themePreview.RefreshPreviewTextures();
			buttons[i].Pressed += () => gameThemer.UpdateBackground();
			buttons[i].Pressed += () => gameThemer.UpdateJarTexture();
		}
    }
}
