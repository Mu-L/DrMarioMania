using Godot;
using System;
using System.Collections.Generic;
using static PowerUpEnums;

public partial class GameSettingPowerUpContainer : FlowContainer
{
    [Export] private bool isSpecialPowerUps;

    [ExportGroup("References & Resources")]
    [Export] private CommonGameSettings commonGameSettings;
    [Export] private ThemeList themeList;
    [Export] private Button firstButton;
    private List<Button> buttons = new List<Button>();
	private List<Sprite2D> buttonSprites = new List<Sprite2D>();

    private List<PowerUp> AvailablePowerUps { get { return isSpecialPowerUps ? commonGameSettings.CurrentPlayerGameSettings.AvailableSpecialPowerUps : commonGameSettings.CurrentPlayerGameSettings.AvailablePowerUps; } }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        for (int i = 0; i < GameConstants.NoOfPowerUps; i++)
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
                buttonSprites[0].Hframes = GameConstants.PowerUpTileSetWidth;
                buttonSprites[0].Vframes = GameConstants.noOfColours + 1;
            }

            buttonSprites[i].Frame += i;

			PowerUp id = (PowerUp)i;
			button.Pressed += () => SetPowerUpState(id);
		}

        UpdateVisuals();
	}

    private void SetPowerUpState(PowerUp id)
    {
        int index = (int)id;

        if (buttons[index].ButtonPressed)
		{
			if (!AvailablePowerUps.Contains(id))
				AvailablePowerUps.Add(id);
		}
		else
		{
			if (AvailablePowerUps.Contains(id))
				AvailablePowerUps.Remove(id);
		}

        buttonSprites[index].SelfModulate = new Color(1,1,1, buttons[index].ButtonPressed ? 1 : 0.25f);
    }

    public void UpdateVisuals()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].ButtonPressed = AvailablePowerUps.Contains((PowerUp)i);

            buttonSprites[i].SelfModulate = new Color(1,1,1, buttons[i].ButtonPressed ? 1 : 0.25f);
            buttonSprites[i].Texture = themeList.GetPowerUpTileTexture(commonGameSettings.CurrentTheme);
        }
    }
}
