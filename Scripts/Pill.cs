using Godot;
using System;
using System.Collections.Generic;
using static PowerUpEnums;

public partial class Pill : Node2D
{
	[Export] protected Sprite2D centreSegmentSprite;
	[Export] protected Sprite2D secondarySegmentSprite;
	[Export] protected Material rainbowMat;
	[Export] protected bool isRotationCentred;

	protected int centreSegmentColour = 1;
	public int CentreSegmentColour { get { return centreSegmentColour; } }
	protected int secondarySegmentColour = 2;
	public int SecondarySegmentColour { get { return secondarySegmentColour; } }
	public Vector2I GridPos { get; set; }
	protected bool isVertical = false;
	public bool IsVertical { get { return isVertical; } }
	protected bool areSegmentsSwapped = false;
	public bool AreSegmentsSwapped { get { return areSegmentsSwapped; } }
	protected bool isPowerUp = false;
	public bool IsPowerUp { get { return isPowerUp; } }
	public PowerUp CurrentPowerUp { get { return isPowerUp ? (PowerUp)(centreSegmentSprite.Frame % GameConstants.PowerUpTileSetWidth) : (PowerUp)(-1) ; } }

	public Vector2I SecondaryGridPos { get { return GridPos + (isVertical ? Vector2I.Up : Vector2I.Right); } }
	public Vector2I CentreTextureCoords { get { return centreSegmentSprite.FrameCoords; } }
	public Vector2I SecondaryTextureCoords { get { return secondarySegmentSprite.FrameCoords; } }

	protected Vector2 origPos = Vector2.Zero;
	public Vector2 OrigPos
	{
		get
		{
			if (origPos == Vector2.Zero)
				origPos = Position;

			return origPos;
		}
		set
		{
			if (Position == origPos)
				Position = value;
			
			origPos = value;
		}
	}
	protected Texture2D pillTex;
	protected Texture2D powerUpTex;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		origPos = Position;
		UpdateTextures();
	}

	public void ResetState()
	{
		if (origPos != Vector2.Zero)
			Position = origPos;

		areSegmentsSwapped = false;
		SetOrientation(false);
	}

	public void SetPillTexture(Texture2D tex)
	{
		pillTex = tex;

		if (isPowerUp)
			return;

		centreSegmentSprite.Texture = tex;
		secondarySegmentSprite.Texture = tex;
	}
	public void SetPowerUpTexture(Texture2D tex)
	{
		powerUpTex = tex;

		if (!isPowerUp)
			return;

		centreSegmentSprite.Texture = tex;
		secondarySegmentSprite.Texture = null;
	}
	public virtual void SetRandomSegmentColours(List<int> possibleColours, bool guaranteeSingleColour, RandomNumberGenerator rng)
	{
		if (isPowerUp)
		{
			isPowerUp = false;
			UpdateTextures();
		}

		int colourCount  = possibleColours.Count;
		bool bothSame = guaranteeSingleColour || rng.RandiRange(0, 2) == 0 || colourCount < 2;

		int centreColourIndex = rng.RandiRange(0, colourCount - 1);
		int secondaryColourIndex;

		if (bothSame)
		{
			secondaryColourIndex = centreColourIndex;
		}
		else
		{
			secondaryColourIndex = rng.RandiRange(0, colourCount - 1);
			if (secondaryColourIndex == centreColourIndex)
				secondaryColourIndex = (secondaryColourIndex + rng.RandiRange(1, colourCount - 1)) % colourCount;
		}
		
		centreSegmentColour = possibleColours[centreColourIndex];
		secondarySegmentColour = possibleColours[secondaryColourIndex];
		UpdateTextureFrame();
	}
	public virtual void SetSegmentColours(int centreColour, int secondaryColour)
	{
		centreSegmentColour = centreColour;
		secondarySegmentColour = secondaryColour;

		if (isPowerUp)
		{
			isPowerUp = false;
			UpdateTextures();
		}

		UpdateTextureFrame();
	}
	public virtual void SetPowerUp(PowerUp powerUp, int colour)
	{
		centreSegmentColour = colour;
		secondarySegmentColour = 0;
		
		isPowerUp = true;
		UpdateTextures();

		centreSegmentSprite.Frame = (int)powerUp + GameConstants.PowerUpTileSetWidth * centreSegmentColour;
	}

	protected void SwapSegmentPositions()
	{
		secondarySegmentSprite.Position = new Vector2(-secondarySegmentSprite.Position.Y, -secondarySegmentSprite.Position.X);
	}

	public void SetOrientation(bool vertical)
	{
		if (isPowerUp)
			return;

		// Return if already same orientation
		if (isVertical == vertical)
			return;

		isVertical = vertical;

		if (isRotationCentred)
		{
			if (!vertical)
			{
				centreSegmentSprite.Position -= Vector2.One * 4.0f;
				secondarySegmentSprite.Position -= Vector2.One * 4.0f;
				SwapSegmentPositions();
			}
			else
			{
				SwapSegmentPositions();
				centreSegmentSprite.Position += Vector2.One * 4.0f;
				secondarySegmentSprite.Position += Vector2.One * 4.0f;
			}
		}
		else
			SwapSegmentPositions();

		UpdateTextureFrame();
	}

	public void SwapOrientation()
	{
		if (isPowerUp)
			return;

		SetOrientation(!isVertical);
	}

	public void SwapSegments()
	{
		if (isPowerUp)
			return;

		int oldCentreColour = centreSegmentColour;

		centreSegmentColour = secondarySegmentColour;
		secondarySegmentColour = oldCentreColour;

		UpdateTextureFrame();

		areSegmentsSwapped = !areSegmentsSwapped;
	}

	public void ResetSwappedState()
	{
		areSegmentsSwapped = false;
	}

	protected void UpdateTextures()
	{
		if (isPowerUp)
		{
			centreSegmentSprite.Texture = powerUpTex;
			centreSegmentSprite.Hframes = GameConstants.PowerUpTileSetWidth;
			centreSegmentSprite.Vframes = GameConstants.noOfColours + 1;
			secondarySegmentSprite.Texture = null;
			centreSegmentSprite.Material = centreSegmentColour == 0 ? rainbowMat : null;
		}
		else
		{
			centreSegmentSprite.Texture = pillTex;
			centreSegmentSprite.Hframes = GameConstants.pillTileSetWidth;
			centreSegmentSprite.Vframes = GameConstants.noOfColours;
			secondarySegmentSprite.Texture = pillTex;
			centreSegmentSprite.Material = null;
		}
	}
	protected void UpdateTextureFrame()
	{
		if (isPowerUp)
			return;
		
		centreSegmentSprite.Frame = (isVertical ? PillConstants.atlasBottom : PillConstants.atlasLeft) + centreSegmentSprite.Hframes * (centreSegmentColour - 1);
		secondarySegmentSprite.Frame = (isVertical ? PillConstants.atlasTop : PillConstants.atlasRight) + centreSegmentSprite.Hframes * (secondarySegmentColour - 1);
	}
}
