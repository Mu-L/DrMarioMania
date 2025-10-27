using Godot;
using System;

public partial class ThemePreview : Control
{
    [Export] private TextureRect bg;
    [Export] private TextureRect jar;
    [Export] private Sprite2D virus;
    [Export] private Sprite2D pill;
    [Export] private Sprite2D pill2;
    [Export] private bool useSpeedBg;

    [ExportGroup("Resources")]
    [Export] private ThemeList themeList;
    [Export] private CommonGameSettings commonGameSettings;
    private float initialPillPosY;
    
    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        initialPillPosY = pill.Position.Y;
		SetPreviewTextures(commonGameSettings.CurrentTheme);
	}

    public void RefreshPreviewTextures()
    {
        SetPreviewTextures(commonGameSettings.CurrentTheme);
    }

    public void UpdateBgAndJarTexture()
    {
        UpdateBgAndJarTexture(commonGameSettings.CurrentTheme);
    }

    private void UpdateBgAndJarTexture(int theme)
    {
        bg.Texture = themeList.GetBgTilesTexture(useSpeedBg ? commonGameSettings.CurrentPlayerGameSettings.SpeedLevel : 1, theme, commonGameSettings.CurrentIsUsingCustomBgColour);
        jar.Texture = themeList.GetJarTexture(useSpeedBg ? commonGameSettings.CurrentPlayerGameSettings.SpeedLevel : 1, theme, false, commonGameSettings.CurrentIsUsingCustomBgColour);
    }

    public void SetPreviewTextures(int theme)
    {
        UpdateBgAndJarTexture(theme);

        if (virus != null)
            virus.Texture = themeList.GetVirusTileTexture(theme);

        pill.Texture = themeList.GetPillTileTexture(theme);
        pill2.Texture = pill.Texture;

        pill.Position = new Vector2(pill.Position.X, initialPillPosY + themeList.GetPillPreviewOffset(theme));
        pill2.Position = new Vector2(pill2.Position.X, pill.Position.Y);
    }
}
