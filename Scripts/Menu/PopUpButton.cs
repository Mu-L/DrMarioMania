using Godot;
using System;

public partial class PopUpButton : Button
{
    [Export] protected string title;
    [Export(PropertyHint.MultilineText)] protected string description;
    [Export] protected bool alignDescToLeft;
    [Export] protected PopUpGroup popUpGroup;
    public PopUpGroup PopUpGroup
    {
        set
        {
            // don't set if already set to a non-null value
            if (popUpGroup != null)
                return;

            popUpGroup = value;
            Pressed += () => value.ShowPopUp(title, description, alignDescToLeft);
        }
    }

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        if (popUpGroup != null)
            Pressed += () => popUpGroup.ShowPopUp(title, description, alignDescToLeft);
    }
}
