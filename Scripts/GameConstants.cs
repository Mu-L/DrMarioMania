using Godot;
using System;

public static class GameConstants
{
	// common constant values used across multiple other classes

	// number of different colours in the game (excluding rainbow variant)
	public const int noOfColours = 10;

	// no. of tiles making up the width of the pill tileset
	public const int pillTileSetWidth = 6;

	// no. of tiles making up the width of the virus tileset
	public const int virusTileSetWidth = 3;

	// number of different power-up types
	public static int NoOfPowerUps { get { return Enum.GetValues(typeof(PowerUpEnums.PowerUp)).Length; } }
	// no. of tiles making up the width of the power-up tileset
	public static int PowerUpTileSetWidth { get { return NoOfPowerUps + 1; } }

	// returns true if on mobile
	public static bool IsOnMobile { get { return OS.HasFeature("android"); } }
	
	// size of camera zoom when "EnableLargerView" common game setting is enabled
	public const float largerViewZoom = 1.265f;
}
