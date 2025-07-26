using Godot;
using System;

public struct UserCustomLevelEntry
{
    // Entry for a user-made/imported custom level
    public UserCustomLevelEntry()
    {
        name = "";
        date = new Date();
        code = "";
        isCorrupted = false;
        hasCleared = false;
        highScore = 0;
    }

    public string name;
    public Date date;
    public string code;
    public bool isCorrupted;
    public bool hasCleared;
    public int highScore;
}
