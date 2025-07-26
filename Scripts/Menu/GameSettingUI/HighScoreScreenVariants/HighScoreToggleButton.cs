using Godot;
using System;

public partial class HighScoreToggleButton : GameSettingToggleButton
{
    // GameSettingToggleButton variant for the highscore screen, so making changes updates the score list

    [Export] private HighScorePanel highScorePanel;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        base._Ready();

        Pressed += () => highScorePanel.UpdateVisuals();
    }
}
