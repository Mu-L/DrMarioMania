using Godot;
using System;
using System.Collections.Generic;

public partial class UpdateableSettingScreen : BaseScreen
{
	// A screen which contains buttons/sliders/etc that adjust game settings (player-specific or common) which get updated when switching to this screen (via RefreshUpdateableSettingScreenValues).
	// This type of screen is used when the configurable settings within it are present on multiple screens in the same scene, to avoid any instances appearing outdated.
	[Export] public bool UseRecursiveSearch { get; set; }

	private List<Node> updateableNodes = new List<Node>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// find all nodes that have values/visuals that can be changed

		if (UseRecursiveSearch)
		{
			// recursive/more thorough search
			RecursiveSearch(this);
		}
		else
		{
			// simpler search
			Godot.Collections.Array<Node> nodes = GetChildren();

			foreach (Node node in nodes)
			{
				if (node is GameSettingToggleButton or GameSettingSlider or GameSettingButtonGroup or GameSettingColourGroup or ThemePreview)
					updateableNodes.Add(node);
				else if (node is ScrollContainer)
				{
					Godot.Collections.Array<Node> containerNodes = node.GetChildren();

					if (containerNodes[0] is MarginContainer)
					{
						containerNodes = containerNodes[0].GetChildren();
					}

					foreach (Node containerNode in containerNodes)
					{
						if (containerNode is GameSettingToggleButton)
							updateableNodes.Add(containerNode);
						else if (containerNode is GameSettingSlider)
							updateableNodes.Add(containerNode);
						else if (containerNode is GameSettingButtonGroup)
							updateableNodes.Add(containerNode);
					}
				}
			}
		}

	}

	protected virtual bool IsNodeFindable(Node node)
	{
		return node is GameSettingToggleButton or GameSettingSlider or GameSettingButtonGroup or GameSettingColourGroup or ThemePreview or GameSettingPowerUpContainer;
	}

	protected void RecursiveSearch(Node startNode)
	{
		Godot.Collections.Array<Node> nodes = startNode.GetChildren();

		foreach (Node node in nodes)
		{
			if (IsNodeFindable(node))
			{
				updateableNodes.Add(node);
			}
			else if (node.GetChildCount() > 0 && node is not Label)
				RecursiveSearch(node);
		}
	}

	// updates the screen given's slider and button states based on their corrisponding game setting values
	public void RefreshUpdateableSettingScreenValues()
	{
		foreach (Node node in updateableNodes)
		{
			RefreshNodeValue(node);
		}
	}

	// returns true if updated
	protected virtual bool RefreshNodeValue(Node node)
	{
		if (node is GameSettingToggleButton)
			((GameSettingToggleButton)node).UpdateVisuals();
		else if (node is GameSettingSlider)
			((GameSettingSlider)node).UpdateVisuals();
		else if (node is GameSettingButtonGroup)
			((GameSettingButtonGroup)node).UpdateVisuals();
		else if (node is GameSettingColourGroup)
			((GameSettingColourGroup)node).UpdateVisuals();
		else if (node is ThemePreview)
			((ThemePreview)node).RefreshPreviewTextures();
		else if (node is GameSettingPowerUpContainer)
			((GameSettingPowerUpContainer)node).UpdateVisuals();
		else
			return true;

		return false;
	}
}
