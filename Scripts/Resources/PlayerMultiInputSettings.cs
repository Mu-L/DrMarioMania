using Godot;
using System;

public partial class PlayerMultiInputSettings : Resource
{
	// multiplayer control settings ======================================
    
    // whether this player is using a controller or keyboard to play in multiplayer mode
    public bool MultiplayerIsUsingController { get; set; }
    // id of keyboard "arrow" equivalents (arrows, wasd, etc) OR controller id associated with this player in multiplayer mode
    public int MultiplayerInputID
    {
        get
        {
            return multiplayerInputID;
        }
        set
        {
            multiplayerInputID = value;

            switch (value)
            {
                case 0:
                    multiKeyboardKeys = "WASD";
                    break;
                case 1:
                    multiKeyboardKeys = "Arrows";
                    break;
                case 2:
                    multiKeyboardKeys = "TFGH";
                    break;
                case 3:
                    multiKeyboardKeys = "IJKL";
                    break;
                default:
                    multiKeyboardKeys = "WASD";
                    break;
            }
        }
    }
    private int multiplayerInputID;
    // Represents which keys are used to control if keyboard is shared in multiplayer - WASD, Arrows, TFGH or IJKL
    private string multiKeyboardKeys;

    // whether or not this player is the only player using their control scheme (keyboard or controller)
    // if using keyboard, this is whether or not this player has access to the full keyboard (true) or only their "arrow" equivalents (arrows, wasd, etc) (false)
    // if using controller, this is whether or not any controller (true) or just a particular controller (false) can control this player
    public bool MultiplayerIsControlMethodExclusive { get; set; }

    // The prefex based on the Multiplayer Control Settings used for checking inputs (e.g. InputPrefix + "Hold" should be used to check the hold button state)
    public string MultiplayerInputPrefix
    {
        get
        {
            if (MultiplayerIsControlMethodExclusive)
            {
                if (MultiplayerIsUsingController)
                    return "ControllerSingle";
                else
                    return "KeyboardFull";
            }
            else
            {
                if (MultiplayerIsUsingController)
                    return "ControllerMulti" + MultiplayerInputID;
                else
                    return "KeyboardMulti" + multiKeyboardKeys;
            }
        }
    }
}
