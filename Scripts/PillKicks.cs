using Godot;
using System;
using System.Linq;
using static PillEnums;

// Handles pill offsets upon collisions - these are known as "kicks"
public static class PillKicks
{
    private static Vector2I[] v2hDoubleKicks = { Vector2I.Left };
    private static Vector2I[] h2vDoubleKicks = { Vector2I.Right, Vector2I.Down, Vector2I.Right + Vector2I.Down };
    private static Vector2I[] luigiKicks = { Vector2I.Right, Vector2I.Left, Vector2I.Down, Vector2I.Right + Vector2I.Down, Vector2I.Left + Vector2I.Down };

    // pill types that will have a different formation when swapped (turned 180 degrees), so requires check if its swapped positions are all free
    // not requires for pills such as double since they would have the same formation when turned 180 degrees
    private static PillShape[] pillShapesRequiringSwapChecks = { PillShape.Luigi };
	// pills that don't change shape (same positions) when rotated
    private static PillShape[] pillShapesWithConsistentShape = { PillShape.Single, PillShape.Quad };

    private static bool AreAnyTilesColliding(Vector2I targetPos, Vector2I[] segmentPositions, JarManager jarMan)
	{
        bool isColliding = false;
        foreach (Vector2I segPos in segmentPositions)
		{
            if (!jarMan.IsCellFree(targetPos + segPos))
            {
                isColliding = true;
                break;
            }
        }

        return isColliding;
    }

	private static Vector2I[] GetKickList(PillShape pillShape, bool isVertical)
	{
		if (pillShape == PillShape.Double)
            return isVertical ? v2hDoubleKicks : h2vDoubleKicks;
		else if (pillShape == PillShape.Luigi)
            return luigiKicks;
		else
			return null;
    }

	public static Vector3I DoKickChecks(int dir, PillActive activePill, JarManager jarMan)
	{
        Vector2I targetPos = activePill.GridPos;
        PillShape pillShape = activePill.PillShape;

        Vector2I[] kicks = GetKickList(pillShape, activePill.IsVertical);

        int newRotation = activePill.PillRotation + dir;
		// keep rotation within 0-3 range
		if (newRotation > 3)
            newRotation -= 4;
		else if (newRotation < 0)
            newRotation += 4;

		// if pill is a type without kicks, return current pos and rotation
		if (kicks == null)
		{
            bool doRotation = pillShapesWithConsistentShape.Contains(pillShape);
			
            return new Vector3I(targetPos.X, targetPos.Y, doRotation ? newRotation : activePill.PillRotation);
		}
		
        Vector2I[] potentialSegmentPositions = activePill.GetRotatedTilePositions(newRotation);

		// offset zero
        bool isColliding = AreAnyTilesColliding(targetPos, potentialSegmentPositions, jarMan);
		
		// if colliding, do kicks
		if (isColliding)
		{
            bool foundFreeSpace = false;

            for (int i = 0; i < kicks.Length; i++)
			{
                if (!AreAnyTilesColliding(targetPos + kicks[i], potentialSegmentPositions, jarMan))
                {
					targetPos += kicks[i];
                    foundFreeSpace = true;
                    break;
                }
            }

			// if no spaces found by offsetting...
			if (!foundFreeSpace)
			{
				// set newRotation to original rotation turned by 180 degrees (aka swapped)
				newRotation = (activePill.PillRotation + 2) % 4;

				// if pill type requires swap checks, turn 180 degrees and check for collisions. keep swap if no collisions, else revert rotation
				if (pillShapesRequiringSwapChecks.Contains(pillShape))
				{
					// only allow swaps if vertical
					if (activePill.IsVertical)
					{
						// update potentialSegmentPositions with 180 degree turned positions
						potentialSegmentPositions = activePill.GetRotatedTilePositions(newRotation);

						Vector2I offset = newRotation == 0 ? Vector2I.Left : Vector2I.Right;

						if (AreAnyTilesColliding(targetPos + offset, potentialSegmentPositions, jarMan))
							// revert rotation
							newRotation = activePill.PillRotation;
						else
							// offset targetPos
                            targetPos += offset;
                    }
					// if not verical, revert rotation
					else
						newRotation = activePill.PillRotation;
				}
			}
        }

        return new Vector3I(targetPos.X, targetPos.Y, newRotation);
    }
}
