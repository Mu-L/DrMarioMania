using Godot;
using System;
using System.Collections.Generic;

public partial class ThemeList : Resource
{
    // Stores theme folder names and provides easy access to theme sprites and attributes

    [Export] private Godot.Collections.Array<string> themeFolderNames;

    private Dictionary<int, ThemeAttributes> themeAttributesCache = new Dictionary<int, ThemeAttributes>();

    private string themePath = "res://Assets/Sprites/Themes/";
    private string resourcePath = "res://Assets/Resources/Themes/";

    public Texture2D GetTexture(string name, int theme) { return ResourceLoader.Load<Texture2D>(themePath + themeFolderNames[theme] + "/" + name + ".png"); }
    public Texture2D GetPillTileTexture(int theme) { return ResourceLoader.Load<Texture2D>(themePath + themeFolderNames[theme] + "/PillTiles.png"); }
    public Texture2D GetVirusTileTexture(int theme) { return ResourceLoader.Load<Texture2D>(themePath + themeFolderNames[theme] + "/VirusTiles.png"); }
    public Texture2D GetPowerUpTileTexture(int theme) { return ResourceLoader.Load<Texture2D>(themePath + themeFolderNames[theme] + "/PowerUpTiles.png"); }
    public Texture2D GetObjectTileTexture(int theme) { return ResourceLoader.Load<Texture2D>(themePath + themeFolderNames[theme] + "/ObjectTiles.png"); }
    public Texture2D GetPowerUpIconTexture(int theme) { return ResourceLoader.Load<Texture2D>(themePath + themeFolderNames[theme] + "/PowerUpIcons.png"); }
    public Texture2D GetUIBoxTexture(int theme) { return ResourceLoader.Load<Texture2D>(themePath + themeFolderNames[theme] + "/UIBox.png"); }
    public Texture2D GetUIBoxSmallTexture(int theme) { return ResourceLoader.Load<Texture2D>(themePath + themeFolderNames[theme] + "/UIBoxSmall.png"); }
    public Texture2D GetJarTexture(int speed, int theme, bool isMultiplayer, bool bgIsTintable)
    {
        bool themeUsesCommonBg = ResourceLoader.Exists(themePath + themeFolderNames[theme] + "/Jar.png");
        string fileName;

        if (themeUsesCommonBg)
            fileName = "Jar";
        else if (bgIsTintable)
            fileName = "JarTintableBg";
        else if (speed == 2)
            fileName = "JarHi";
        else if (speed == 0)
            fileName = "JarLow";
        else
            fileName = "JarMed";

        if (isMultiplayer)
        {
            bool multiJarTexExists = ResourceLoader.Exists(themePath + themeFolderNames[theme] + "/" + fileName + "Multi.png");

            if (multiJarTexExists)
                fileName += "Multi";
        }

        return ResourceLoader.Load<Texture2D>(themePath + themeFolderNames[theme] + "/" + fileName + ".png");
    }
    public Texture2D GetBgTilesTexture(int speed, int theme, bool bgIsTintable)
    {
        bool themeUsesCommonBg = ResourceLoader.Exists(themePath + themeFolderNames[theme] + "/BG.png");
        string fileName;

        if (bgIsTintable)
            fileName = "BGTintable";
        else if (themeUsesCommonBg)
            fileName = "BG";
        else if (speed == 2)
            fileName = "BGHi";
        else if (speed == 0)
            fileName = "BGLow";
        else
            fileName = "BGMed";

        return ResourceLoader.Load<Texture2D>(themePath + themeFolderNames[theme] + "/" + fileName + ".png");
    }
    private ThemeAttributes GetThemeAttributes(int theme)
    {
        if (themeAttributesCache.ContainsKey(theme))
        {
            return themeAttributesCache[theme];
        }
        else
        {
            ThemeAttributes attributes = ResourceLoader.Load<ThemeAttributes>(resourcePath + themeFolderNames[theme] + ".tres");

            if (attributes == null)
            {
                GD.PrintErr("Attributes was null: " + resourcePath + themeFolderNames[theme] + ".tres");
            }
            else
            {
                themeAttributesCache.Add(theme, attributes);
            }
                
            return attributes;
        }
    }
    public bool GetHasRingOverlay(int theme)
    {
        return GetThemeAttributes(theme).HasRingOverlay;
    }

    public int GetJarOffset(int theme) { return GetThemeAttributes(theme).JarOffset; }
    public int GetTopLeftHudOffset(int theme) { return GetThemeAttributes(theme).TopLeftHudOffset; }
    public int GetRightHudGroupOffset(int theme) { return GetThemeAttributes(theme).RightHudGroupOffset; }
    public int GetVirusRingOffset(int theme) { return GetThemeAttributes(theme).VirusRingOffset; }
    public int GetPillPreviewOffset(int theme) { return GetThemeAttributes(theme).PillPreviewOffset; }
    public string GetFeverMusicPath(int theme, bool isMultiplayer)
    {
        if (isMultiplayer && GetThemeAttributes(theme).HasMultiMusicVariants)
            return GetThemeAttributes(theme).MultiFeverMusicPath;
        else
            return GetThemeAttributes(theme).FeverMusicPath;
    }
    public string GetChillMusicPath(int theme, bool isMultiplayer)
    {
        if (isMultiplayer && GetThemeAttributes(theme).HasMultiMusicVariants)
            return GetThemeAttributes(theme).MultiChillMusicPath;
        else
            return GetThemeAttributes(theme).ChillMusicPath;
    }
    public string GetMusicFolderPath(int theme) { return GetThemeAttributes(theme).MusicFolderPath; }
    public string GetSfxFolderPath(int theme) { return GetThemeAttributes(theme).SfxFolderPath; }
    public Color GetLabelColour(int theme) { return GetThemeAttributes(theme).LabelColour; }
    public Font GetLabelFont(int theme) { return GetThemeAttributes(theme).LabelFont; }
    public bool GetUseLabelShadow(int theme) { return GetThemeAttributes(theme).UseLabelShadow; }
    public bool GetNeverUseLightLabelColour(int theme) { return GetThemeAttributes(theme).NeverUseLightLabelColour; }
    public Color GetLightColour(int theme) { return GetThemeAttributes(theme).LightColour; }
    public Color GetPowerUpPreviewColour(int theme) { return GetThemeAttributes(theme).PowerUpPreviewColour; }
    public bool GetUseUiShadow(int theme) { return GetThemeAttributes(theme).UseUIShadows; }
    public ColourOrderCategoryEnums.ColourOrderCategory GetColourOrder(int theme) { return GetThemeAttributes(theme).ColourOrder; }
}
