using Godot;
using System;

public partial class GameSettingMultiGamestyleButtonGroup : GameSettingButtonGroup
{
    [Export] private GameSettingColourGroup colourGroup;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        base._Ready();

		for (int i = 0; i < buttons.Count; i++)
		{
			buttons[i].Pressed += () => colourGroup.UpdateVisuals();
		}
	}
}
