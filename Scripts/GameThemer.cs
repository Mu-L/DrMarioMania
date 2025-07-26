using Godot;
using System;

public partial class GameThemer : Node
{
	[ExportGroup("Audio Player References")]
	[Export] private SfxManager sfxMan;
	[Export] private MusicManager musicMan;

	[ExportGroup("Visual References")]
	[Export] private TextureRect bgTexture;
	
	[ExportGroup("Other References")]
	[Export] private GameManager gameMan;

	[ExportGroup("Optional References")]
	[Export] private EditorCursor editorCursor;
	[Export] private EditorTileTypeSelector tileTypeSelector;
	[Export] private EditorColourSelector colourSelector;

	[ExportGroup("Resources")]
	[Export] private CommonGameSettings commonGameSettings;
	[Export] private ThemeList themeList;
	[Export] private MusicList musicList;

	public void UpdateAllVisualsAndSfx()
	{
		UpdateCommonVisuals();

		foreach (JarManager jarMan in gameMan.Jars)
		{
			jarMan.UIMan.UpdateJarVisuals(commonGameSettings.CurrentTheme, themeList);
		}

		UpdateEditorVisuals();
		UpdateMusic();
		UpdateSoundEffects();
	}

	public void UpdateHoldGroup()
	{
		foreach (JarManager jarMan in gameMan.Jars)
		{
			jarMan.UIMan.UpdateHoldGroup(commonGameSettings.CurrentTheme, themeList);
		}
	}
	
	public void UpdateBackground()
	{
		bgTexture.Texture = themeList.GetBgTilesTexture(commonGameSettings.CurrentPlayerGameSettings.SpeedLevel, commonGameSettings.CurrentTheme);
	}

	private void UpdateCommonVisuals()
	{
		UpdateBackground();
	}

	private void UpdateSoundEffects()
	{
		sfxMan.LoadSoundEffects(themeList.GetSfxFolderPath(commonGameSettings.CurrentTheme));
	}

	public void SetMusic(int music)
	{
		AudioStream strm = musicList.GetMusicStream(music);

		musicMan.GameMusic = strm;
	}

	private void UpdateMusic()
	{
		SetMusic(commonGameSettings.CurrentMusic);

		musicMan.WinMusic = musicList.GetThemeMusicStream("Win");
		musicMan.MultiWinMusic = musicList.GetThemeMusicStream("MultiWin");
		musicMan.LoseMusic = musicList.GetThemeMusicStream("Lose");
	}

	private void UpdateEditorVisuals()
	{
		if (editorCursor != null)
		{
			editorCursor.VirusTexture = themeList.GetVirusTileTexture(commonGameSettings.CurrentTheme);
			editorCursor.PowerUpTexture = themeList.GetPowerUpTileTexture(commonGameSettings.CurrentTheme);
		}

		if (tileTypeSelector != null)
		{
			tileTypeSelector.VirusTexture = themeList.GetVirusTileTexture(commonGameSettings.CurrentTheme);
			tileTypeSelector.PowerUpTexture = themeList.GetPowerUpTileTexture(commonGameSettings.CurrentTheme);
		}
		
		if (colourSelector != null)
		{
			colourSelector.VirusTexture = themeList.GetVirusTileTexture(commonGameSettings.CurrentTheme);
			colourSelector.PowerUpTexture = themeList.GetPowerUpTileTexture(commonGameSettings.CurrentTheme);
		}
	}
}
