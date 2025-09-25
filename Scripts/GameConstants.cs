using Godot;
using System;

public static class GameConstants
{
	// common constant values used across multiple other classes

	// level code format version
	public const int levelCodeVer = 3;
	// path for external user files (e.g. music). This is just the user path, except on android where its documents instead because android file management is pain
	public static string ExternalFolderPath { get { return IsOnMobile ? OS.GetSystemDir(OS.SystemDir.Documents) + "/" : ProjectSettings.GlobalizePath("user://"); }	}
	public static string MusicFolderPath { get { return ExternalFolderPath + MusicFolder + "/"; } }
	public static string MusicFolder { get { return IsOnMobile ? "DrMarioMania/Music" : "music"; } }

	// music id used to declare than a custom song is being used (customMusicFile in common game settings)
	public const int customMusicID = -5;
	public const string forbiddenLevelNameChars = "/;,\"";

	// source IDs
	public const int pillSourceID = 0;
	public const int virusSourceID = 1;
	public const int powerUpSourceID = 2;
	public const int objectSourceID = 3;
	public const int toolPreviewSourceID = 4;

	// number of different colours in the game (excluding rainbow variant)
	public const int noOfColours = 10;

	// no. of tiles making up the width of the pill tileset
	public const int pillTileSetWidth = 6;

	// no. of tiles making up the width of the virus tileset
	public const int virusTileSetWidth = 3;

	// number of different power-up types AND no. of tiles making up the width of the power-up tileset
	public static int NoOfPowerUps { get { return Enum.GetValues(typeof(PowerUpEnums.PowerUp)).Length; } }
	public static int PowerUpTileSetWidth { get { return NoOfPowerUps + 1; } }

	// number of object tiles AND no. of tiles making up the width of the object tileset
	public const int noOfObjects = 4;
	public const int objectTileSetWidth = 3;

	// returns true if on mobile
	public static bool IsOnMobile { get { return OS.HasFeature("android"); } }
	
	// size of camera zoom when "EnableLargerView" common game setting is enabled
	public const float largerViewZoom = 1.265f;
}
