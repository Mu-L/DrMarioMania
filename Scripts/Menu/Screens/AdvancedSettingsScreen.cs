using Godot;
using System;

public partial class AdvancedSettingsScreen : UpdateableSettingScreen
{
	[Export] private PopUpGroup popUpGroup;

	protected override bool IsNodeFindable(Node node)
	{
		return base.IsNodeFindable(node) || node is PopUpButton;
	}

	// returns true if updated
	protected override bool RefreshNodeValue(Node node)
	{
		if (base.RefreshNodeValue(node))
			return true;
		else if (node is PopUpButton)
		{
			(node as PopUpButton).PopUpGroup = popUpGroup;
			return true;
		}
		else
			return false;

	}
}
