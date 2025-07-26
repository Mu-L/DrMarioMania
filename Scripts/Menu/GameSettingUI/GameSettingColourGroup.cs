using Godot;
using System;
using System.Collections.Generic;

public partial class GameSettingColourGroup : Control
{
	[Export] protected CommonGameSettings commonGameSettings;
	[Export] protected ThemeList themeList;
	[Export] protected Label colourCountLabel;
	[Export] private Button firstButton;

	private List<Button> buttons = new List<Button>();
	private List<Sprite2D> buttonSprites = new List<Sprite2D>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int i = 0; i < GameConstants.noOfColours; i++)
		{
			Button button;
            
            if (i == 0)
                button = firstButton;
            else
            {
                button = firstButton.Duplicate() as Button;
                AddChild(button);
            }

			buttons.Add(button);
			buttonSprites.Add(button.GetChild<Sprite2D>(0));

			if (i == 0)
            {
                buttonSprites[0].Hframes = GameConstants.virusTileSetWidth;
                buttonSprites[0].Vframes = GameConstants.noOfColours;
            }

			buttonSprites[i].Frame = i * buttonSprites[i].Hframes;

			int colour = i + 1;
			button.Pressed += () => SetColourState(button.ButtonPressed, colour);
		}

		SetVirusTextures(commonGameSettings.CurrentTheme);
	}

	// Updates the size of the SegmentColours array depending on ColourCount's value, either adding additional colours or removing unnessicary ones
	private void FixUnequalColourCountss()
	{
		PlayerGameSettings settings = commonGameSettings.CurrentPlayerGameSettings;

		if (settings.ColourCount == settings.SegmentColours.Count)
			return;
		
		commonGameSettings.CurrentPlayerGameSettings.FixSegmentColoursList();
	}

	public void UpdateVisuals()
	{
		FixUnequalColourCountss();

		SetVirusTextures(commonGameSettings.CurrentTheme);
		UpdateColourCountWarning();
		RefreshButtonStates();
	}

	// Checks whether the count of the SegmentColours array is unequal to the value of ColourCount, updates colourCountLabel accordingly
	private void UpdateColourCountWarning()
	{
		int colourArrayCount = commonGameSettings.CurrentPlayerGameSettings.SegmentColours.Count;
		int colourCount = commonGameSettings.CurrentPlayerGameSettings.ColourCount;

		bool unequal = colourArrayCount != colourCount;

		colourCountLabel.Text = "" + colourArrayCount + "/" + colourCount;
		colourCountLabel.Modulate = unequal ? new Color(1,0,0,1) : new Color(1,1,1,1);
	}

	// Updates the on/off states of each button based on the no. of colours selected in gameSettings
	private void RefreshButtonStates()
	{
		for (int i = 0; i < buttons.Count; i++)
		{
			// (i + 1) because first colour, red, has the id of 1 (0 is reserved for non-coloured tiles in the jar grid)
			buttons[i].ButtonPressed = commonGameSettings.CurrentPlayerGameSettings.SegmentColours.Contains(i + 1);

			// unpressed buttons should have partially-transparent sprites
			buttonSprites[i].SelfModulate = new Color(1,1,1, buttons[i].ButtonPressed ? 1 : 0.25f);
			buttonSprites[i].Position = new Vector2(11, buttons[i].ButtonPressed ? 12 : 11);
		}
	}

	// set whether or not the colour "i" is included in the SegmentColours list or not
	private void SetColourState(bool state, int colour)
	{
		if (state)
		{
			if (!commonGameSettings.CurrentPlayerGameSettings.SegmentColours.Contains(colour))
				commonGameSettings.CurrentPlayerGameSettings.SegmentColours.Add(colour);
		}
		else
		{
			if (commonGameSettings.CurrentPlayerGameSettings.SegmentColours.Contains(colour))
				commonGameSettings.CurrentPlayerGameSettings.SegmentColours.Remove(colour);
		}
		
		buttonSprites[colour - 1].SelfModulate = new Color(1,1,1, state ? 1 : 0.25f);
		buttonSprites[colour - 1].Position = new Vector2(11, state ? 12 : 11);

		// Sort order of colours
		commonGameSettings.CurrentPlayerGameSettings.SegmentColours.Sort();
		
		UpdateColourCountWarning();
	}

	// Set virus sprites based on theme value given
	public void SetVirusTextures(int theme)
	{
		Texture2D newTex = themeList.GetVirusTileTexture(theme);

		// skip loop if already using same theme textures
		if (buttonSprites[0].Texture == newTex)
			return;

		for (int i = 0; i < buttonSprites.Count; i++)
		{
			buttonSprites[i].Texture = newTex;
		}
	}
}
