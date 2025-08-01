using Godot;
using System;
using System.Collections.Generic;

public partial class BaseScreenManager : Node
{
	// Base screen manager - manage switch between different screens (BaseScreen)
    [Export] protected Node screenContainer;
    protected List<BaseScreen> screens = new List<BaseScreen>();
    protected int currentScreen = 0;
	// frame at which GoBack was called
	protected int backFrame;

	// Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		// Get each "screen" control node under this node and add them to the screens list
		foreach (Node node in screenContainer.GetChildren())
		{
			if (node is Control)
			{
				screens.Add((BaseScreen)node);
				// hide each screen too
				screens[screens.Count - 1].Visible = false;
			}
		}

		// show currentScreen screen
		screens[currentScreen].Visible = true;

		GrabFocusOfFirstButton();
	}

    // focus on the current screen's initial hover node
	protected void GrabFocusOfFirstButton()
	{
		if (screens[currentScreen].InitialHoverNode != null)
			screens[currentScreen].InitialHoverNode.GrabFocus();
	}

    // focus on the current screen's last hovered node (first button as fallback)
	protected void GrabFocusOfLastButton()
	{
		if (screens[currentScreen].LastHoverNode != null)
			screens[currentScreen].LastHoverNode.GrabFocus();
		else
			GrabFocusOfFirstButton();
	}

    public virtual void SetScreen(int nextScreen)
	{
		if (backFrame == Engine.GetFramesDrawn())
            return;

		screens[currentScreen].LastHoverNode = GetViewport().GuiGetFocusOwner();
		screens[currentScreen].Visible = false;
		screens[nextScreen].Visible = true;

		currentScreen = nextScreen;

		GrabFocusOfFirstButton();
	}

    public virtual void GoBack()
	{
        backFrame = Engine.GetFramesDrawn();
    }
}
