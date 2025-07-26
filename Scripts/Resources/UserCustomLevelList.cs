using Godot;
using System;
using System.Collections.Generic;

public partial class UserCustomLevelList : Resource
{
    // Resource for storing, saving and loading user-made/imported custom levels
    [Export] private Godot.Collections.Array<string> exampleLevels;
    public List<UserCustomLevelEntry> CustomLevels { get { return customLevels; } }
    private List<UserCustomLevelEntry> customLevels = new List<UserCustomLevelEntry>();

    public void MarkAsCorrupted(int id)
    {
        UserCustomLevelEntry lvl = customLevels[id];
        lvl.isCorrupted = true;
        customLevels[id] = lvl;
    }

    public void SetHighScore(int lvlID, int newScore)
    {
        UserCustomLevelEntry lvl = customLevels[lvlID];
        lvl.highScore = newScore;
        customLevels[lvlID] = lvl;

        GD.Print("new highscore");
    }

    public void SetClearState(int lvlID, bool cleared)
    {
        UserCustomLevelEntry lvl = customLevels[lvlID];

        // return if unchanged
        if (lvl.hasCleared == cleared)
            return;

        lvl.hasCleared = cleared;
        customLevels[lvlID] = lvl;
    }

    // Return true if successful, false if unsuccessful
    public bool ImportLevel(string code)
    {
        try
        {
            UserCustomLevelEntry newLvl = new UserCustomLevelEntry();

            string[] codeSections = code.Split('/');

            string[] basicSettingChunks = codeSections[0].Split(';');

            newLvl.name = basicSettingChunks[1];
            newLvl.date = new Date();
            newLvl.code = code;

            customLevels.Insert(0, newLvl);

            SaveToFile();
            
            return true;
        }
        catch (Exception e)
        {
            GD.Print("Import failed: " + e);
            return false;
        }
    }

    public void SaveToFile()
    {
        GD.Print("About to save levels file...");

        ConfigFile config = new ConfigFile();

        // Build levels array
        Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>> levelsArray = new Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>>();

        for (int i = 0; i < customLevels.Count; i++)
        {
            Godot.Collections.Dictionary<string, Variant> levelEntryDict = new Godot.Collections.Dictionary<string, Variant>();

            UserCustomLevelEntry levelEntry = customLevels[i];

            levelEntryDict.Add("Name", levelEntry.name);
            levelEntryDict.Add("Date", levelEntry.date.ExportAsString());
            levelEntryDict.Add("Code", levelEntry.code);
            levelEntryDict.Add("IsCorrupted", levelEntry.isCorrupted);
            levelEntryDict.Add("HasCleared", levelEntry.hasCleared);
            levelEntryDict.Add("HighScore", levelEntry.highScore);

            levelsArray.Add(levelEntryDict);
        }

        config.SetValue("", "Level List", levelsArray);

        Error error = config.Save("user://levels.cfg");

        if (error == Error.Ok)
            GD.Print("Saved levels file!");
        else
            GD.Print("Couldn't save levels file...");
    }

    public void LoadFromFile()
    {
        GD.Print("About to load levels file...");

        ConfigFile config = new ConfigFile();
        Error error = config.Load("user://levels.cfg");

        if (error != Error.Ok)
        {
            GD.Print("Levels file not found");

            // import example levels upon new save
            for (int i = 0; i < exampleLevels.Count; i++)
            {
                ImportLevel(exampleLevels[i]);
            }

            return;
        }

        // Restore levels from array
        Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>> levelsArray = (Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>>)config.GetValue("", "Level List");

        customLevels.Clear();

        for (int i = 0; i < levelsArray.Count; i++)
        {            
            UserCustomLevelEntry levelEntry = new UserCustomLevelEntry();

            levelEntry.name = (string)levelsArray[i]["Name"];
            levelEntry.date.ImportFromString((string)levelsArray[i]["Date"]);
            levelEntry.code = (string)levelsArray[i]["Code"];
            levelEntry.isCorrupted = (bool)levelsArray[i]["IsCorrupted"];

            if (levelsArray[i].ContainsKey("HighScore"))
                levelEntry.highScore = (int)levelsArray[i]["HighScore"];

            if (levelsArray[i].ContainsKey("HasCleared"))
                levelEntry.hasCleared = (bool)levelsArray[i]["HasCleared"];

            customLevels.Add(levelEntry);
        }

        GD.Print("Loaded levels file!");
    }
}
