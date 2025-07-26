using Godot;
using System;

public partial class EditorBaseInputSelector : EditorBaseSelector
{
    // Same as EditorBaseSelector but with input support - allows a key to be associated with each button in the selector
    [Export] protected Godot.Collections.Array<StringName> inputNames;
    [Export] protected StringName backwardsInput;
    [Export] protected StringName forwardsInput;
    [Export] protected StringName ignoreBackwardsInput;
    [Export] protected StringName ignoreForwardsInput;
    [Export] protected EditorManager editorMan;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        for (int i = 0; i < inputNames.Count; i++)
        {
            if (Input.IsActionJustPressed(inputNames[i]) && editorMan.CanPressButtons)
            {
                buttons[i].EmitSignal("pressed");
            }
        }

        if (backwardsInput != null && Input.IsActionJustPressed(backwardsInput) && (ignoreBackwardsInput == null || !Input.IsActionJustPressed(ignoreBackwardsInput)))
        {
            Previous();
        }
        else if (forwardsInput != null && Input.IsActionJustPressed(forwardsInput) && (ignoreForwardsInput == null || !Input.IsActionJustPressed(ignoreForwardsInput)))
        {
            Next();
        }
    }

    protected virtual void Previous()
    {
        if (editorMan.CanPressButtons)
            buttons[(selectedButton - 1 + buttons.Count) % buttons.Count].EmitSignal("pressed");
    }

    protected virtual void Next()
    {
        if (editorMan.CanPressButtons)
            buttons[(selectedButton + 1) % buttons.Count].EmitSignal("pressed");
    }
}
