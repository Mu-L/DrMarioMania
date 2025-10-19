using Godot;
using System;
using System.Collections.Generic;
using static PowerUpEnums;
using static PillEnums;

public partial class Pill : Node2D
{
	[Export] protected TileMapLayer pillTiles;
	[Export] protected Material rainbowMat;
	// whether the pill is rotated around its centre
	[Export] protected bool isRotationCentred;
	// whether the pill should use smallScale when it its starting position (aka in the ui)
	[Export] protected bool startWithSmallScale;
	[Export] protected bool neverUseSmallScale;
	// Offsets from orig position for each pill shape, used to align pills of each shape in ui nicely
	[Export] protected Godot.Collections.Array<Vector2> origPosShapeOffsets;

    protected PillType pillType = PillType.Regular;
    public PillType PillType { get { return pillType; } }
    protected PillShape pillShape = PillShape.Double;
    public PillShape PillShape { get { return pillShape; } }
    public Vector2I GridPos { get; set; }
	// 0-3 (clockwise)
	protected int pillRotation = 0;
    public int PillRotation { get { return pillRotation; } }
    public bool IsVertical
	{
		get
		{
			if (pillShape == PillShape.Luigi)
				return pillRotation % 2 == 0;
			else
				return pillRotation % 2 == 1;
		}
	}
	public bool IsFlipped { get { return pillRotation > 1; } }
    public bool IsPowerUp { get { return pillType == PillType.PowerUp; } }
    public PowerUp CurrentPowerUp { get { return IsPowerUp ? (PowerUp)GetTileAtlasCoords(Vector2I.Zero).X : (PowerUp)0; } }
    public int CentreSegmentColour { get { return rotatedTiles[Vector2I.Zero].colour; } }
    public Vector2I CentreTextureCoords { get { return rotatedTiles[Vector2I.Zero].atlas; } }

    // formation of the pills without any pillRotation
    protected Dictionary<Vector2I, JarTileData> unrotatedTiles = new Dictionary<Vector2I, JarTileData>();
    public Dictionary<Vector2I, JarTileData> UnrotatedTiles { get { return unrotatedTiles; } }
    protected Dictionary<Vector2I, JarTileData> rotatedTiles = new Dictionary<Vector2I, JarTileData>();
    public Dictionary<Vector2I, JarTileData> RotatedTiles { get { return rotatedTiles; } }

	// pill tile atlas pillRotations
	// 1st row = pillRotation 0
	// 2nd row = pillRotation 0 -> 1
	// 3rd row = pillRotation 0 -> 2
	// 4th row = pillRotation 0 -> 3
    protected int[] atlasRotations =
	{ 
		0, 1, 2, 3,
		2, 3, 1, 0,
		1, 0, 3, 2,
		3, 2, 0, 1
	};

    protected Vector2 origPos = Vector2.Zero;
    protected Vector2 origPillTilesPos;
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
    private bool hasSetOrigPos = false;
	// scale of pill when shrunked down in the ui (dependent on pill shape)
    private float smallScale = 1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		if (!hasSetOrigPos)
        	InitialiseVariables();

		if (startWithSmallScale)
            Scale = Vector2.One * smallScale;
    }

	// set scale between 1 to smallScale
	public void InterpolatedScale(float range)
	{
		if (neverUseSmallScale)
			Scale = Vector2.One;
		else
			Scale = Vector2.One * ((1.0f - range) + smallScale * range);
	}

	public void ResetScale()
	{
        Scale = Vector2.One * (startWithSmallScale && !neverUseSmallScale ? smallScale : 1.0f);
	}

	public void SetOrigPosToCurrentPos()
	{
        origPos = Position;
    }

	public void InitialiseVariables()
	{
		if (hasSetOrigPos)
            return;
		
        hasSetOrigPos = true;

        origPos = Position;
        origPillTilesPos = pillTiles.Position;
    }

	public PillAttributes GetAttributes()
	{
        PillAttributes atts;

        atts.pillType = pillType;
        atts.pillShape = pillShape;
        atts.pillRotation = PillRotation;
        atts.smallScale = smallScale;

        atts.unrotatedTiles = UnrotatedTiles;
        atts.rotatedTiles = RotatedTiles;
		
        return atts;
    }
	public void SetAttributes(PillAttributes atts)
	{
		if (pillShape != atts.pillShape)
        	OffsetFromOrigPos((int)atts.pillShape);
			
        pillType = atts.pillType;
        pillShape = atts.pillShape;
        pillRotation = atts.pillRotation;
        smallScale = atts.smallScale;

        unrotatedTiles = new Dictionary<Vector2I, JarTileData>(atts.unrotatedTiles);
        rotatedTiles = new Dictionary<Vector2I, JarTileData>(atts.rotatedTiles);
		

        UpdateTileMap();
    }

	public Vector2 GetOrigPosOffset(int index)
	{
        return (origPosShapeOffsets != null && origPosShapeOffsets.Count - 1 >= index) ? origPosShapeOffsets[index] : Vector2.Zero;
    }

	private void OffsetFromOrigPos(int index)
	{
        Position = origPos + GetOrigPosOffset(index);
	}

	public void OffsetOrigPos(Vector2 offset)
	{
		if (hasSetOrigPos)
        	origPos += offset;
    }

	public int GetTileColour(Vector2I pos)
	{
		TileData data = pillTiles.GetCellTileData(pos);

		if (data == null)
			return -1;

		int colour = (int)data.GetCustomData("Colour");

		if (pillTiles.GetCellSourceId(pos) != GameConstants.powerUpSourceID && colour == 0)
			return -1;

		return colour;
	}

	public Vector2I GetTileAtlasCoords(Vector2I pos)
	{
		return pillTiles.GetCellAtlasCoords(pos);
	}

	public void ResetState()
	{
		if (hasSetOrigPos)
			Position = origPos + GetOrigPosOffset((int)pillShape);

        SetRotation(0);
        ResetScale();
    }

	public virtual void SetRandomPillColours(List<int> possibleColours, bool guaranteeSingleColour, PillType pType, PillShape pShape, RandomNumberGenerator rng)
	{
        int colourCount  = possibleColours.Count;
		bool bothSame = guaranteeSingleColour || rng.RandiRange(0, 2) == 0 || colourCount < 2;

		int mainColourIndex = rng.RandiRange(0, colourCount - 1);
		int secondaryColourIndex;

		if (bothSame)
		{
			secondaryColourIndex = mainColourIndex;
		}
		else
		{
			secondaryColourIndex = rng.RandiRange(0, colourCount - 1);
			if (secondaryColourIndex == mainColourIndex)
				secondaryColourIndex = (secondaryColourIndex + rng.RandiRange(1, colourCount - 1)) % colourCount;
		}

        SetPillColours(possibleColours[mainColourIndex], possibleColours[secondaryColourIndex], pType, pShape);
    }
	public virtual void SetPillColours(int mainColour, int secondaryColour, PillType pType, PillShape pShape, bool skipTileMapUpdate = false)
	{
        pillType = pType;
        // set pill type to unrotatedTiles 
        SetPillShape(pShape, true);

        // update colours in 
		foreach (Vector2I pos in unrotatedTiles.Keys)
		{
            JarTileData tile = unrotatedTiles[pos];

            int col = tile.colour == 1 ? secondaryColour : mainColour;
            tile.colour = col;
			tile.atlas.Y = col;

			if (tile.sourceID != GameConstants.powerUpSourceID)
                tile.atlas.Y--;

            unrotatedTiles[pos] = tile;
        }

		// update rotatedTiles andtile map
		UpdateRotatedTiles();

		if (!skipTileMapUpdate)
        	UpdateTileMap();
	}
	public virtual void SetPowerUp(PowerUp powerUp, int colour)
	{
        SetPillColours(colour, colour, PillType.PowerUp, PillShape.Single, true);

        JarTileData tileData = unrotatedTiles[Vector2I.Zero];
        tileData.atlas.X = (int)powerUp;

        unrotatedTiles[Vector2I.Zero] = tileData;
        rotatedTiles[Vector2I.Zero] = tileData;

		UpdateTileMap();
    }

	protected void SetPillShape(PillShape pShape, bool skipRotatedTiles)
	{
		if (pillShape != pShape)
            OffsetFromOrigPos((int)pShape);

        pillShape = pShape;

        unrotatedTiles.Clear();

        int sourceID = IsPowerUp ? GameConstants.powerUpSourceID :  GameConstants.pillSourceID;

		// tiles with colour 0 will use main colour
		// tiles with colour 1 will use main secondary colour

		// DOUBLE (2x1)
		if (pShape == PillShape.Double)
		{
            unrotatedTiles.Add(Vector2I.Zero, new JarTileData(sourceID, new Vector2I(PillConstants.atlasLeft, 0)));
            unrotatedTiles.Add(Vector2I.Right, new JarTileData(sourceID, new Vector2I(PillConstants.atlasRight, 1)));
            smallScale = 1;
        }
		// SINGLE (1x1)
		else if (pShape == PillShape.Single)
		{
            unrotatedTiles.Add(Vector2I.Zero, new JarTileData(sourceID, new Vector2I(PillConstants.atlasSingle, 0)));
            smallScale = 1;
        }
		// L-PILL (DR. LUIGI)
		else if (pShape == PillShape.Luigi)
		{
			// upper/vertical pill
			unrotatedTiles.Add(Vector2I.Up, new JarTileData(sourceID, new Vector2I(PillConstants.atlasTop, 0)));
			unrotatedTiles.Add(Vector2I.Zero, new JarTileData(sourceID, new Vector2I(PillConstants.atlasBottom, 0)));

			// lower/horizontal pill
			unrotatedTiles.Add(Vector2I.Down, new JarTileData(sourceID, new Vector2I(PillConstants.atlasLeft, 0)));
            unrotatedTiles.Add(Vector2I.Down + Vector2I.Right, new JarTileData(sourceID, new Vector2I(PillConstants.atlasRight, 1)));
            smallScale = 0.5f;
        }
		// QUAD (2x2)
		else
		{
			unrotatedTiles.Add(Vector2I.Zero, new JarTileData(sourceID, new Vector2I(PillConstants.atlasLeft, 0)));
            unrotatedTiles.Add(Vector2I.Right, new JarTileData(sourceID, new Vector2I(PillConstants.atlasRight, 0)));
			unrotatedTiles.Add(Vector2I.Zero + Vector2I.Up, new JarTileData(sourceID, new Vector2I(PillConstants.atlasLeft, 0)));
            unrotatedTiles.Add(Vector2I.Right + Vector2I.Up, new JarTileData(sourceID, new Vector2I(PillConstants.atlasRight, 1)));
            smallScale = 0.5f;
		}

		if (!skipRotatedTiles)
        	UpdateRotatedTiles();
    }

	public void SetRotation(int r)
	{
		if (pillRotation == r)
            return;
			
        pillRotation = r;

        UpdateRotatedTiles();
        UpdateTileMap();
	}

	public void Rotate(bool right)
	{
        int newRot = pillRotation + (right ? 1 : -1);

		if (newRot > 3)
            newRot -= 4;
		else if (newRot < 0)
            newRot += 4;

        SetRotation(newRot);
    }

	public void Flip()
	{
		int newRot = pillRotation + 2;

		if (newRot > 3)
            newRot -= 4;

        SetRotation(newRot);
	}

	private Vector2I RotatePosition(Vector2I ogPos, int newRotation)
	{
        Vector2I newPos = ogPos;

		// turned 90 right
		if (newRotation == 1)
		{
			newPos = new Vector2I(-newPos.Y, newPos.X);

			// if double or quad, ensure the bottom-left-most pill stays as the centre
			// so move y up one tile
			if (pillShape == PillShape.Double || pillShape == PillShape.Quad)
				newPos.Y--;
		}
		// flipped 180
		else if (newRotation == 2)
		{
			newPos = new Vector2I(-newPos.X, -newPos.Y);

			// if double or quad, ensure the bottom-left-most pill stays as the centre
			// so move x to the right one tile (and y up for quad)
			if (pillShape == PillShape.Double)
				newPos.X++;
			else if (pillShape == PillShape.Quad)
			{
				newPos.X++;
				newPos.Y--;
			}
		}
		// turned 90 left / 270 right
		else if (newRotation == 3)
		{
			newPos = new Vector2I(newPos.Y, -newPos.X);

			// if quad, ensure the bottom-left-most pill stays as the centre
			// so move x to the right one tile
			if (pillShape == PillShape.Quad)
				newPos.X++;
		}

        return newPos;
    }

	protected void UpdateRotatedTiles()
	{
        rotatedTiles.Clear();
		
        foreach (Vector2I ogPos in unrotatedTiles.Keys)
		{
            JarTileData tileData = unrotatedTiles[ogPos];
            Vector2I newPos = RotatePosition(ogPos, pillRotation);

            if (tileData.sourceID == GameConstants.pillSourceID && tileData.atlas.X != PillConstants.atlasSingle)
			{
                tileData.atlas.X = atlasRotations[tileData.atlas.X + 4 * pillRotation];
			}

            rotatedTiles.Add(newPos, tileData);
        }
	}

	public Vector2I[] GetRotatedTilePositions(int newRotation)
	{
        Vector2I[] positions = new Vector2I[unrotatedTiles.Count];

        int i = 0;
        foreach (Vector2I ogPos in unrotatedTiles.Keys)
		{
            Vector2I newPos = RotatePosition(ogPos, newRotation);

            positions[i] = newPos;
            i++;
        }

        return positions;
    }

    protected void UpdateTileMap()
	{
        pillTiles.Clear();
		
		foreach (Vector2I pos in rotatedTiles.Keys)
		{
            JarTileData tileData = rotatedTiles[pos];
            pillTiles.SetCell(pos, tileData.sourceID, tileData.atlas);
        }

        if (isRotationCentred)
		{
            if (pillShape == PillShape.Double && IsVertical)
			{
				pillTiles.Position = origPillTilesPos + Vector2.One * 4.0f;
			}
			else
                pillTiles.Position = origPillTilesPos;
        }
    }
}
