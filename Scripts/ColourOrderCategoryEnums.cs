using Godot;
using System;

public static class ColourOrderCategoryEnums
{
	// Some themes have different colour orders than others (e.g. nes/snes themes have cyan and dark blue swapped, because the nes/snes blue virus is closer to cyan than its modern design)
    // This represents each unique colour order
	public enum ColourOrderCategory { Standard, Classic, GameBoy, WhatsApp }
}
