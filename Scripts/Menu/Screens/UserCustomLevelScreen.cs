using Godot;
using System;

public partial class UserCustomLevelScreen : BaseScreen
{
	[Export] private Control singleInitialHoverNode;
	[Export] private Control multiInitialHoverNode;
	[Export] private CommonGameSettings commonGameSettings;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UpdateInitialHoverNode();
	}

	private void UpdateInitialHoverNode()
	{
		Control newNode = commonGameSettings.IsMultiplayer ? multiInitialHoverNode : singleInitialHoverNode;
		
		if (newNode == InitialHoverNode)
			return;
		
		if (GetViewport().GuiGetFocusOwner() == InitialHoverNode)
		{
			newNode.GrabFocus();
		}

		InitialHoverNode = newNode;
	}
}
