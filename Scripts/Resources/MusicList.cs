using Godot;
using System;

public partial class MusicList : Resource
{
    // Stores list of music paths and provides audio streams from these paths
    
    [Export] private Godot.Collections.Array<string> musicPaths;
    [Export] private CommonGameSettings commonGameSettings;
    [Export] private ThemeList themeList;

    private string pathPrefix = "res://Assets/Audio/Music/";

    public AudioStream GetMusicStream(int id)
    {
        if (id == 0)
        // mute/nothing
            return null;

        string path;

        // fever based on theme
        if (id == -1)
            path = themeList.GetFeverMusicPath(commonGameSettings.CurrentTheme);
        // chill based on theme
        else if (id == -2)
            path = themeList.GetChillMusicPath(commonGameSettings.CurrentTheme);
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
