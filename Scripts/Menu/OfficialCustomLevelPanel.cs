using Godot;
using System;

public partial class OfficialCustomLevelPanel : Panel
{
	// Represents a pre-made, built-in custom level panel on the official level screen

	[Export] private Label nameLabel;
	[Export] private Label clearLabel;
	[Export] private Label scoreLabel;

	[Export] private Button playButton;
	public Button PlayButton { get { return playButton; } }
	[Export] private Button copyButton;
	public Button CopyButton { get { return copyButton; } }
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public void SetLevelDetails(string name, int score, bool cleared)
	{
		clearLabel.Visible = cleared;
		nameLabel.Text = name;
		scoreLabel.Text = score.ToString();
	}

	public void SetAboveNeighbour(OfficialCustomLevelPanel neighbour)
	{
		playButton.FocusNeighborTop = neighbour.PlayButton.GetPath();
		copyButton.FocusNeighborTop = neighbour.CopyButton.GetPath();
	}

	public void SetBelowNeighbour(OfficialCustomLevelPanel neighbour)
	{
		playButton.FocusNeighborBottom = neighbour.PlayButton.GetPath();
		copyButton.FocusNeighborBottom = neighbour.CopyButton.GetPath();
	}
}
