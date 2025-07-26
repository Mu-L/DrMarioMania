using Godot;
using System;
using System.Collections.Generic;

public partial class MultiplayerControlContainer : VBoxContainer
{
    [Export] protected Texture2D controllerTex;
    [Export] protected Button nextButton;
    [Export] protected Godot.Collections.Array<Texture2D> multiControllerTexs;
    [Export] protected Texture2D keyboardTex;
    [Export] protected Label hardDropNotice;
    [Export] protected Godot.Collections.Array<Texture2D> multiKeyboardTexs;
    [Export] protected CommonGameSettings commonGameSettings;
    private List<Button> buttons = new List<Button>();

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        // Get each button node under this node and add them to the buttons list
		foreach (Button button in GetChildren())
		{
			buttons.Add(button);
		}
	}

    public void RefreshVisuals()
    {
        hardDropNotice.Visible = false;
        
        for (int i = 0; i < buttons.Count; i++)
        {
            if (i > commonGameSettings.PlayerCount - 1)
            {
                buttons[i].Visible = false;
                continue;
            }

            PlayerMultiInputSettings player = commonGameSettings.GetPlayerMultiInputSettings(i);
            buttons[i].Visible = true;

            buttons[i].Text = "P" + (i + 1);

            buttons[i].FocusNeighborBottom = (i == commonGameSettings.PlayerCount - 1) ? nextButton.GetPath() : null;

            if (player.MultiplayerIsUsingController)
            {
                buttons[i].Text += ": CONTROLLER";
                
                if (player.MultiplayerIsControlMethodExclusive)
                    buttons[i].Icon = controllerTex;
                else
                    buttons[i].Icon = multiControllerTexs[player.MultiplayerInputID];
            }
            else
            {
                buttons[i].Text += ": KEYBOARD";

                if (player.MultiplayerIsControlMethodExclusive)
                    buttons[i].Icon = keyboardTex;
                else
                {
                    buttons[i].Icon = multiKeyboardTexs[player.MultiplayerInputID];
                    hardDropNotice.Visible = true;
                }
            }
        }
    }

    public void ToggleControlMethod(int playerID)
    {
        PlayerMultiInputSettings player = commonGameSettings.GetPlayerMultiInputSettings(playerID);
        player.MultiplayerIsUsingController = !player.MultiplayerIsUsingController;

        commonGameSettings.RefreshMultiplayerInputIndexes();
        RefreshVisuals();
    }
}
