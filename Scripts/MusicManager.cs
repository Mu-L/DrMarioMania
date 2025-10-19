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
    private AudioStream hurryUpJingle;
    public AudioStream HurryUpJingle { set { hurryUpJingle = value; } }

    private bool hasHurriedUp = false;
    private float preJingleMusicPos = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetProcess(false);
    }

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
    public void PlayHurryUpJingle()
    {
        if (hasHurriedUp)
            return;

        preJingleMusicPos = GetPlaybackPosition();
        hasHurriedUp = true;
        Stream = hurryUpJingle;
        Play();

        SetProcess(true);
    }

    public void PlayStream(AudioStream stream)
    {
        hasHurriedUp = false;

        if (IsProcessing())
            SetProcess(false);

        if (PitchScale != 1)
            PitchScale = 1;

        Stream = stream;
        Play();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // ONLY ACTIVE WHEN HURRY UP JINGLE IS PLAYING

        if (!StreamPaused && !Playing)
        {
            PitchScale = 1.059f;
            Stream = gameMusic;
            Play();
            Seek(preJingleMusicPos);
            SetProcess(false);
        }
    }
}
