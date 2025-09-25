using Godot;
using System;
using System.Collections.Generic;
using static PillEnums;

public partial class GameSettingPillShapeContainer : FlowContainer
{
    [Export] private CommonGameSettings commonGameSettings;
    [Export] private ThemeList themeList;
    [Export] private Label pillShapeLabel;
    private List<Button> buttons = new List<Button>();
	private List<List<Sprite2D>> buttonSprites = new List<List<Sprite2D>>();

    private List<PillShape> AvailablePillShapes { get { return commonGameSettings.CurrentPlayerGameSettings.AvailablePillShapes; } }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        for (int i = 0; i < GameConstants.NoOfPillShapes; i++)
		{
			Button button = GetChild<Button>(i);

			buttons.Add(button);
            buttonSprites.Add(new List<Sprite2D>());
            foreach (Node node in button.GetChildren())
			{
				if (node is Sprite2D)
				{
                    Sprite2D sprite = node as Sprite2D;
                    buttonSprites[i].Add(sprite);

					sprite.Hframes = GameConstants.pillTileSetWidth;
					sprite.Vframes = GameConstants.noOfColours;
				}
            }

			PillShape id = (PillShape)i;
			button.Pressed += () => SetPillShapeState(id);
		}

        UpdateVisuals();
	}

    private void SetPillShapeState(PillShape id)
    {
        int index = (int)id;

        if (buttons[index].ButtonPressed)
		{
			if (!AvailablePillShapes.Contains(id))
			{
				AvailablePillShapes.Add(id);
                AvailablePillShapes.Sort();
            }
		}
		else
		{
			if (AvailablePillShapes.Contains(id))
				AvailablePillShapes.Remove(id);
		}

		foreach (Sprite2D sprite in buttonSprites[index])
		{
			sprite.SelfModulate = new Color(1,1,1, buttons[index].ButtonPressed ? 1 : 0.25f);
		}

        SetLabelColourState(AvailablePillShapes.Count != 0);
    }

	private void SetLabelColourState(bool valid)
	{
		pillShapeLabel.Modulate = valid ? new Color(1,1,1,1) : new Color(1,0,0,1);
	}

    public void UpdateVisuals()
    {
        // If AvailablePillShapes list is empty, fix it before updating visuals
        if (AvailablePillShapes.Count == 0)
			commonGameSettings.CurrentPlayerGameSettings.FixAvailablePillShapes();

		SetLabelColourState(true);

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].ButtonPressed = AvailablePillShapes.Contains((PillShape)i);

			foreach (Sprite2D sprite in buttonSprites[i])
			{
				sprite.SelfModulate = new Color(1,1,1, buttons[i].ButtonPressed ? 1 : 0.25f);
            	sprite.Texture = themeList.GetPillTileTexture(commonGameSettings.CurrentTheme);
			}
        }
    }
}
