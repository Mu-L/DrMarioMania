using Godot;
using System;
using System.Collections.Generic;

public partial class WinIconContainer : FlowContainer
{
    [Export] private Control firstWinIcon;

    [Export] private Texture2D winIconTexture;
    [Export] private Texture2D winIconSmallTexture;

    [Export] private CommonGameSettings commonGameSettings;

    private List<Node> winIconNodes = new List<Node>();
    private List<Sprite2D> winIconSprites = new List<Sprite2D>();

    public Texture2D WinIconTexture
    {
        set
        {
            foreach (Sprite2D sprite in winIconSprites)
            {
                if (sprite.Texture == winIconTexture)
                    sprite.Texture = value;
            }

            winIconTexture = value;
        }
    }
    public Texture2D WinIconSmallTexture
    {
        set
        {
            foreach (Sprite2D sprite in winIconSprites)
            {
                if (sprite.Texture == winIconSmallTexture)
                    sprite.Texture = value;
            }

            winIconSmallTexture = value;
        }
    }

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        bool useSmallIcon = commonGameSettings.MultiplayerRequiredWinCount > 5;

        if (useSmallIcon)
        {
            firstWinIcon.CustomMinimumSize /= 2;
            firstWinIcon.GetChild<Sprite2D>(0).Position /= 2;
        }

        for (int i = 0; i < commonGameSettings.MultiplayerRequiredWinCount; i++)
		{
			Node winIcon;
            
            if (i == 0)
                winIcon = firstWinIcon;
            else
            {
                winIcon = firstWinIcon.Duplicate();
                AddChild(winIcon);
            }

			winIconNodes.Add(winIcon);
			winIconSprites.Add(winIcon.GetChild<Sprite2D>(0));
            winIconSprites[i].Texture = useSmallIcon ? winIconSmallTexture : winIconTexture;
		}
	}

    // Changes the frame of each win icon to represent the number of wins the player has, passed via the "wins" parameter
    public void SetWinAmount(int wins)
    {
        for (int i = 0; i < winIconSprites.Count; i++)
		{
            winIconSprites[i].Frame = i < wins ? 1 : 0;
		}
    }
}
