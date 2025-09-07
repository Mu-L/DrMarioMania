using Godot;
using System;
using System.Collections.Generic;
using static PillTypeEnums;
public struct PillAttributes
{
    public PillType pillType;
    public int pillRotation;
    public Dictionary<Vector2I, JarTileData> unrotatedTiles;
    public Dictionary<Vector2I, JarTileData> rotatedTiles;
}
