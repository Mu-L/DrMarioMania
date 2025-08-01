using Godot;
using System;

public partial class MusicList : Resource
{
    // Stores list of music paths and provides audio streams from these paths
    
    [Export] private Godot.Collections.Array<string> musicPaths;
    [Export] private CommonGameSettings commonGameSettings;
    [Export] private ThemeList themeList;

    private string pathPrefix = "res://Assets/Audio/Music/";

    private AudioStream LoadCustomMusic(string path)
    {
        if (FileAccess.FileExists(path))
        {
            FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

            if (path.Contains(".mp3"))
            {
                AudioStreamMP3 audio = new AudioStreamMP3();
                audio.Data = file.GetBuffer((long)file.GetLength());
                audio.Loop = true;
                return audio;
            }
            else if (path.Contains(".ogg"))
            {
                AudioStreamOggVorbis audio = AudioStreamOggVorbis.LoadFromFile(path);
                audio.Loop = true;
                return audio;
            }
            
            return null;
        }
        else
            return null;
    }

    public AudioStream GetMusicStream(int id, string customMusicFile = "")
    {
        if (id == 0)
        // mute/nothing
            return null;

        string path;

        // custom music
        if (id == GameConstants.customMusicID)
        {
            if (customMusicFile == "")
                customMusicFile = commonGameSettings.CustomMusicFile;

            path = GameConstants.UserFolderPath + "music/" + customMusicFile;

            AudioStream audio = LoadCustomMusic(path);
            
            // if audio is null, fallback themed-fever
            if (audio == null)
                id = -1;
            else
                return LoadCustomMusic(path);
        }

        // fever based on theme
        if (id == -1)
            path = themeList.GetFeverMusicPath(commonGameSettings.CurrentTheme, commonGameSettings.IsMultiplayer);
        // chill based on theme
        else if (id == -2)
            path = themeList.GetChillMusicPath(commonGameSettings.CurrentTheme, commonGameSettings.IsMultiplayer);
        // random
        else if (id == -4)
            path = pathPrefix + musicPaths[GD.RandRange(0, musicPaths.Count - 1)];
        else
            path = pathPrefix + musicPaths[id];
        
        return ResourceLoader.Load<AudioStream>(path);
    }

    public AudioStream GetThemeMusicStream(string name)
    {
        string path;

        path = themeList.GetMusicFolderPath(commonGameSettings.CurrentTheme) + "/" + name + ".ogg";

        return ResourceLoader.Load<AudioStream>(path);
    }
}
