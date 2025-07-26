using Godot;
using System;
using System.Collections.Generic;

public partial class ScreenTransition : Control
{
	[ExportGroup("Parameters")]
	[Export] private AudioStreamPlayer musicPlayer;
	[Export] private string nextScene;

	// style of in/out transitions
	[Export] private int coverStyle;
	[Export] private int uncoverStyle;

	[ExportGroup("Local References")]
	[Export] private AnimationPlayer aniPlayer;

	[ExportGroup("Transitions")]
	[Export] private Godot.Collections.Array<StringName> coverTransitions;
	[Export] private Godot.Collections.Array<StringName> uncoverTransitions;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = true;
		Uncover();
	}

	public void Cover()
	{
		aniPlayer.Play(coverTransitions[coverStyle]);
	}

	public void Uncover()
	{
		aniPlayer.Play(uncoverTransitions[uncoverStyle]);
	}

	public void GoToNextScene()
	{
		if (nextScene == "Quit")
			GetTree().Quit();
		else
			GetTree().ChangeSceneToFile("res://Scenes/" + nextScene + ".tscn");
	}

	public int CoverStyle { get { return coverStyle; } set { coverStyle = value; } }
	public int UncoverStyle { get { return coverStyle; } set { uncoverStyle = value; } }
	public string NextScene { get { return nextScene; } set { nextScene = value; } }
	public bool IsCovering { get { return Visible && aniPlayer.CurrentAnimation != uncoverTransitions[uncoverStyle]; } }
	// How much left of the currently playing animation is left
	public float RemainingAnimationTime { get { return aniPlayer.IsPlaying() ? (float)(aniPlayer.CurrentAnimationLength - aniPlayer.CurrentAnimationPosition) : 0; } }
}
