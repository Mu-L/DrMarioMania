using Godot;
using System;
using System.Collections.Generic;

public partial class ControllerInputInitialiser : Node
{
    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Godot.Collections.Array<StringName> inputs = InputMap.GetActions();

		// create controller index-specific duplicates of controller single input actions for use in multiplayer when 2 or more controllers are used
		// reference: https://www.reddit.com/r/godot/comments/lw21n8/comment/i6qddvz/

		// find all "ControllerSingle" actions
		for (int i = 0; i < inputs.Count; i++)
		{
			// if a controller single action...
			if (((string)inputs[i]).Contains("ControllerSingle"))
			{
				// Get original action string
				string origInput = inputs[i];
				// Get array of events for the original action
				Godot.Collections.Array<InputEvent> origEvents = InputMap.ActionGetEvents(origInput);

				// for each controller index (0-3)...
				for (int j = 0; j < 4; j++)
				{
					// create controller index-specific input action for multiplayer
					string newInput = origInput.Replace("Single", "Multi" + j);
					InputMap.AddAction(newInput);

					// create new events with new controller index (j)
					foreach (InputEvent evnt in origEvents)
					{
						InputEvent newEvent = evnt.Duplicate() as InputEvent;
						newEvent.Device = j;

						InputMap.ActionAddEvent(newInput, evnt);
					}

				}
			}
		}
	}
}
