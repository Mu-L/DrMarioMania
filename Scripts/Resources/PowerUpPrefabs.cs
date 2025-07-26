using Godot;
using System;
using static PowerUpEnums;

public partial class PowerUpPrefabs : Resource
{
    // if only dictonaries could be exported properly
    [Export] private Godot.Collections.Array<PowerUp> powerUpPrefabKeys;
    [Export] private Godot.Collections.Array<PackedScene> powerUpPrefabs;

    public PackedScene GetPowerUpPrefab(PowerUp id)
    {
        return powerUpPrefabs[powerUpPrefabKeys.IndexOf(id)];
    }
}
