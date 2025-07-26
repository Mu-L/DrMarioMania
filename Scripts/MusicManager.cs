using Godot;
using System;

public partial class MusicManager : AudioStreamPlayer
{
    private AudioStream gameMusic;
    public AudioStream GameMusic { set { gameMusic = value; } }
    private AudioStream winMusic;
    public AudioStream WinMusic { set { winMusic = value; } }
    private AudioStream multiWinMusic;
    public AudioStream MultiWinMusic { set { multiWinMusic = value; } }
    private AudioStream loseMusic;
    public AudioStream LoseMusic { set { loseMusic = value; } }

    public void PlayGameMusic()
    {
        PlayStream(gameMusic);
    }
    public void PlayWinMusic()
    {
        PlayStream(winMusic);
    }
    public void PlayMultiWinMusic()
    {
        PlayStream(multiWinMusic);
    }
    public void PlayLoseMusic()
    {
        PlayStream(loseMusic);
    }

    public void PlayStream(AudioStream stream)
    {
        Stream = stream;
        Play();
    }
}
