using Godot;
using System;

public partial class TouchControlsManager : CanvasLayer
{
	[Export] private CommonGameSettings commonGameSettings;
	[Export] private TouchScreenButton hardDropButton;
	[Export] private TouchScreenButton holdButton;

	// Local vs external visibilty:
	// - Local visibility is the visibility value set by this script, depending on last input type pressed (touch screen or keyboard/controller button)
	// - External visibility is the visibility value set externally via ShowTouchControlsIfAvailable
	// Touch controls will only be visible if BOTH variables are true

	private bool localVisibility = true;
	private bool LocalVisibility
	{
		set
		{
			localVisibility = value;
			Visible = externalVisibility && localVisibility;
		}
		get
		{
			return localVisibility;
		}
	}
	private bool externalVisibility = true;

	public void ShowTouchControlsIfAvailable(bool b)
    {		
        externalVisibility = GameConstants.IsOnMobile && b && commonGameSettings.PlayerCount == 1;

		Visible = externalVisibility && localVisibility;

        holdButton.Visible = commonGameSettings.CurrentPlayerGameSettings.IsHoldEnabled;
        hardDropButton.Visible = commonGameSettings.IsHardDropEnabled;
    }

	// Touch controls hiding/showing depending on detected inputs
    public override void _Input(InputEvent @event)
    {
        // Not needed if not on mobile
        if (!GameConstants.IsOnMobile)
            return;

        // If touch controls are visible, hide if keyboard or controller input happened
        if (Visible)
        {
            if (@event is InputEventKey || @event is InputEventJoypadButton || @event is InputEventJoypadMotion)
            {
                LocalVisibility = false;
            }
        }
        // If touch controls are NOT visible, show if screen touched (and conditions in ShowTouchControlsIfAvailable are true)
        else
        {
            if (@event is InputEventScreenTouch)
            {
                LocalVisibility = true;
            }
        }
    }
}
