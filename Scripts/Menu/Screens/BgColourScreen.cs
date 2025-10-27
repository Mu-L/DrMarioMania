using Godot;
using System;

public partial class BgColourScreen : BaseScreen
{
    [Export] private ThemePreview themePreview;
    [Export] private CommonGameSettings commonGameSettings;
    [Export] private ColorPicker colourPicker;
    [Export] private Button activeButton;
    [Export] private GameThemer gameThemer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        colourPicker.Color = commonGameSettings.CurrentCustomBgColour;
    }

    public void UpdateVisuals()
    {
        themePreview.RefreshPreviewTextures();
    }

    public void UpdateBgColour(Color col)
	{
        commonGameSettings.CurrentCustomBgColour = col;

        if (!activeButton.ButtonPressed)
            activeButton.ButtonPressed = true;
        UpdateBgColourState(true);
    }

	public void UpdateBgColourState(bool isUsingColour)
	{
        commonGameSettings.CurrentIsUsingCustomBgColour = isUsingColour;
        themePreview.UpdateBgAndJarTexture();

        if (gameThemer != null)
        {
            gameThemer.UpdateBackground();
			gameThemer.UpdateJarTexture();
        }
    }
}
