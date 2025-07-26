using Godot;
using System;

public partial class ActiveOverrideColourIcon : Control
{
	[Export] private Sprite2D pillSprite;
	public Sprite2D PillSprite { get { return pillSprite; } }
	[Export] private Sprite2D arrowSprite;

	public void SetArrowVisibility(bool b)
	{
		arrowSprite.Visible = b;
	}
}
