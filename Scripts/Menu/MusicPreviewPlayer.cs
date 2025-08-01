using Godot;
using System;

public partial class MusicPreviewPlayer : AudioStreamPlayer
{
    // normal background music player (i.e. in the menus)
    [Export] private MusicList musicList;
    [Export] private CommonGameSettings commonGameSettings;

    [ExportGroup("Optional")]
    [Export] private AudioStreamPlayer normalMusicPlayer;

    public string PreviewedCustomMusic { get { return previewedCustomMusic; } }
    private string previewedCustomMusic;
    private string lastPreviewedCustomMusic;
    private int lastPreviewedMusic;

    public void SetPreviewedCustomMusic(string name)
    {
        previewedCustomMusic = name;
    }

    // sets music player stream to the music with id "music"
	public void SetPreviewMusic(int music)
	{
		AudioStream newStream = musicList.GetMusicStream(music, previewedCustomMusic);
	
		if (Playing && music == lastPreviewedMusic && (music != GameConstants.customMusicID || previewedCustomMusic == lastPreviewedCustomMusic))
			return;
		
        if (normalMusicPlayer != null)
            normalMusicPlayer.Stop();
        
		Stream = newStream;
		Play();
        
        lastPreviewedMusic = music;
        lastPreviewedCustomMusic = previewedCustomMusic;
	}

    // same as above, but sets to currently selected music in commonGameSettings ONLY IF CURRENTLY PLAYING PREVIEW MUSIC
	public void SetPreviewMusicToCurrent()
	{
        previewedCustomMusic = commonGameSettings.CustomMusicFile;
        SetPreviewMusic(commonGameSettings.CurrentMusic);
	}

    // stops preview music and start "normalMusicPlayer" music (if presemt)
    public void RestoreNormalMusic()
	{
		Stop();

        if (normalMusicPlayer != null && !normalMusicPlayer.Playing)
            normalMusicPlayer.Play();
	}
}
