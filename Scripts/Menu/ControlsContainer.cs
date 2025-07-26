using Godot;
using System;

public partial class ControlsContainer : PageContainer
{
    [Export] private Godot.Collections.Array<Control> RotateUpKeyboardGroups;
    [Export] private Godot.Collections.Array<Control> HardDropUpKeyboardGroups;

    [Export] private Godot.Collections.Array<Control> NoneUpControllerGroups;
    [Export] private Godot.Collections.Array<Control> HardDropUpControllerGroups;

    [Export] private CommonGameSettings commonGameSettings;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        base._Ready();

        UpdateInputVisuals();
	}

    public void UpdateInputVisuals()
    {
        bool useHardDropUpKeyboard = commonGameSettings.IsUpHardDropKeyboard;
        bool useHardDropUpController = commonGameSettings.IsUpHardDropController;

        foreach (Control group in RotateUpKeyboardGroups)
        {
            group.Visible = !useHardDropUpKeyboard;
        }
        foreach (Control group in HardDropUpKeyboardGroups)
        {
            group.Visible = useHardDropUpKeyboard;
        }

        foreach (Control group in NoneUpControllerGroups)
        {
            group.Visible = !useHardDropUpController;
        }
        foreach (Control group in HardDropUpControllerGroups)
        {
            group.Visible = useHardDropUpController;
        }
    }
}
