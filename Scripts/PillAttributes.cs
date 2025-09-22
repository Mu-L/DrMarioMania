using Godot;
using System;
using System.Collections.Generic;
using static PillEnums;
public struct PillAttributes
{
    public PillType pillType;
    public PillShape pillShape;
    public int pillRotation;
    public Dictionary<Vector2I, JarTileData> unrotatedTiles;
    public Dictionary<Vector2I, JarTileData> rotatedTiles;
}
