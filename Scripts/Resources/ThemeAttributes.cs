using Godot;
using System;
using static ColourOrderCategoryEnums;

public partial class ThemeAttributes : Resource
{
    [ExportGroup("UI Offsets")]
    [Export] public int JarOffset { get; set; }
    [Export] public int MultiplayerJarOffset { get; set; }
    [Export] public int TopLeftHudOffset { get; set; }
    [Export] public int RightHudGroupOffset { get; set; }
    [Export] public int VirusRingOffset { get; set; }
    [Export] public int PillPreviewOffset { get; set; }

    [ExportGroup("Audio")]
    [Export] public bool HasMultiMusicVariants { get; set; }
    [Export] public string MusicFolder { get; set; }
    [Export] public string SfxFolder { get; set; }

    [ExportGroup("Label Text")]
    [Export] public Color LabelColour { get; set; }
    [Export] public Font LabelFont { get; set; }
    [Export] public bool UseLabelShadow { get; set; }

    // Whether this theme should not use the light colour for any of its labels
    [Export] public bool NeverUseLightLabelColour { get; set; }

    [ExportGroup("Colour")]
    // Some themes have different colour orders than others (e.g. nes/snes themes have cyan and dark blue swapped, because the nes/snes blue virus is closer to cyan than its modern design)
    // This value represents each unique colour order
    [Export] public ColourOrderCategory ColourOrder { get; set; }
    // The lightest colour used by these theme
    [Export] public Color LightColour { get; set; }

    // The second lightest colour used by these theme
    [Export] public Color PreLightColour { get; set; }

    // Colour of the power-up preview (e.g. bomb radius, shell directional arrows)
    [Export] public Color PowerUpPreviewColour { get; set; }

    [ExportGroup("Other")]
    [Export] public bool UseUIShadows { get; set; }

    // The ring/magnifying glass have a separate overlay sprite to be used on top of the viruses
    [Export] public bool HasRingOverlay { get; set; }

    public string FeverMusicPath { get { return MusicFolderPath + "/Fever.ogg"; } }
    public string ChillMusicPath { get { return MusicFolderPath + "/Chill.ogg"; } }
    public string MultiFeverMusicPath { get { return MusicFolderPath + "/VSFever.ogg"; } }
    public string MultiChillMusicPath { get { return MusicFolderPath + "/VSChill.ogg"; } }
    public string MusicFolderPath
    {
        get
        {
            return pathPrefixAudio + "Music/" + MusicFolder;
        }
    }
    public string SfxFolderPath
    {
        get
        {
            return pathPrefixAudio + "Sounds/" + SfxFolder;
        }
    }
    private string pathPrefixAudio = "res://Assets/Audio/";
}
