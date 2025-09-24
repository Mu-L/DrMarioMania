using Godot;
using System;
using System.Collections.Generic;
using static PillEnums;

// Attributes of a pill instance, used to copy a pill's state to another pill
public struct PillAttributes
{
    public PillType pillType;
    public PillShape pillShape;
    public int pillRotation;
    public Dictionary<Vector2I, JarTileData> unrotatedTiles;
    public Dictionary<Vector2I, JarTileData> rotatedTiles;
    public float smallScale;
}
