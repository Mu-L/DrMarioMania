using Godot;
using System;
using Godot.Collections;

public partial class HighScoreList : Resource
{
    // Resource for storing, saving and loading highscore records and which official levels have been cleared
    // Saves classic/endless scores, official level scores but NOT for user-made/imported levels

    // Common game settings resource
    [Export] private CommonGameSettings commonGameSettings;

    // Custom level resource - custom level highscores are saved here instead (only 1 score saved per level)
    [Export] private UserCustomLevelList userCustomLevelList;

    // string key = highscore type (which gameplay style, fall speed and score keep state)
    private Dictionary<string, Array<int>> highScores = new Dictionary<string, Array<int>>();
    private Dictionary<int, int> officialLevelHighScores = new Dictionary<int, int>();
    private Array<int> clearedOfficialLevels = new Array<int>();

    // the maximum no. of score records saved for each specific game rule condition (aka a key)
    private const int maxScoreCount = 40;

    // Gets an array of highscores for the current game rules (GameRuleKey, which includes GameplayStyle, SpeedLevel and UseScoreKeep)
    public Array<int> GameRuleHighScores
    {
        get
        {
            string key = GameRuleKey;

            if (highScores.ContainsKey(key))
            {
                // key, and therefore scores, exists
                return highScores[key];
            }
            else
            {
                // key doesn't exists
                return null;
            }
        }
    }

    // A string representing the current game rule conditions, used as the key in highScores
    private string GameRuleKey
    {
        get
        {
            string key = "";
            
            key += commonGameSettings.CurrentPlayerGameSettings.GameplayStyle;
            key += ",";
            key += commonGameSettings.CurrentPlayerGameSettings.SpeedLevel;
            key += ",";
            key += commonGameSettings.UseScoreKeep ? 1 : 0;

            return key;
        }
    }

    public int GetOfficialCustomLevelHighScore(int lvlID)
    {
        if (officialLevelHighScores.ContainsKey(lvlID))
            return officialLevelHighScores[lvlID];
        else
            return 0;  
    }

    public bool HasClearOfficialCustomLevel(int lvlID)
    {
        return clearedOfficialLevels.Contains(lvlID);
    }

    // Get the highscore for the current game rule conditions
    public int GetGameRuleHighScore()
    {
        if (commonGameSettings.IsCustomLevel)
        {
            int lvlID = commonGameSettings.CustomLevelID;

            // new level (not saved)
            if (lvlID == -1)
                return 0;
            // official level - get from officialLevelHighScores
            else if (commonGameSettings.IsOfficialCustomLevel)
            {
                if (officialLevelHighScores.ContainsKey(lvlID))
                    return officialLevelHighScores[lvlID];
                else
                    return 0;                
            }
            // user-made level - get from level entry in customLevelList
            else
                return userCustomLevelList.CustomLevels[lvlID].highScore;
        }

        string key = GameRuleKey;

        if (highScores.ContainsKey(key))
        {
            // key, and therefore score, exists
            return highScores[key][0];
        }
        else
        {
            // key doesn't exists
            return 0;
        }
    }

    // Add a score to the list corrisponding to the current game rule conditions
    public void AddScore(int newScore, bool saveData)
    {
        GD.Print("adding score...");

        if (commonGameSettings.IsCustomLevel)
        {
            int lvlID = commonGameSettings.CustomLevelID;

            // official, build-in level
            if (commonGameSettings.IsOfficialCustomLevel)
            {
                if (officialLevelHighScores.ContainsKey(lvlID))
                {
                    if (newScore > officialLevelHighScores[lvlID])
                        officialLevelHighScores[lvlID] = newScore;
                }
                else
                    officialLevelHighScores.Add(lvlID, newScore);
                
                // save to file
                if (saveData)
                    SaveToFile();
            }
            // user-made level
            else
            {
                if (newScore > userCustomLevelList.CustomLevels[lvlID].highScore)
                {
                    userCustomLevelList.SetHighScore(lvlID, newScore);
                }

                // save to file
                if (saveData)
                    userCustomLevelList.SaveToFile();
            }

            return;
        }

        string key = GameRuleKey;

        if (!highScores.ContainsKey(key))
        {
            highScores.Add(key, new Array<int>());
            highScores[key].Add(newScore);
        }
        else
        {
            bool scoreAdded = false;
            for (int i = 0; i < highScores[key].Count; i++)
            {
                if (newScore > highScores[key][i])
                {
                    highScores[key].Insert(i, newScore);
                    scoreAdded = true;
                    break;
                }
            }
            
            if (!scoreAdded)
                highScores[key].Add(newScore);

            // keep high score list for this key at a max of "maxScoreCount"
            while (highScores[key].Count > maxScoreCount)
            {
                highScores[key].RemoveAt(highScores[key].Count - 1);
            }
        }

        // save to file
        if (saveData)
            SaveToFile();
    }

    public void AddClearedLevel(bool saveData)
    {
        if (commonGameSettings.IsCustomLevel)
        {
            int lvlID = commonGameSettings.CustomLevelID;

            if (commonGameSettings.IsOfficialCustomLevel)
            {
                if (!clearedOfficialLevels.Contains(lvlID))
                {
                    clearedOfficialLevels.Add(lvlID);

                    if (saveData)
                        SaveToFile();
                }
            }
            else
            {
                userCustomLevelList.SetClearState(lvlID, true);

                if (saveData)
                    userCustomLevelList.SaveToFile();
            }
        }
    }

    public void SaveToFile()
    {
        GD.Print("About to save highscores file...");

        ConfigFile config = new ConfigFile();

        foreach (string key in highScores.Keys)
        {
            config.SetValue("HighScores", key, highScores[key]);
        }

        foreach (int key in officialLevelHighScores.Keys)
        {
            config.SetValue("OfficialLevelHighScores", key.ToString(), officialLevelHighScores[key]);
        }

        config.SetValue("ClearedOfficialLevels", "ClearedLevels", clearedOfficialLevels);

        Error error = config.Save("user://highscores.cfg");

        if (error == Error.Ok)
            GD.Print("Saved highscores file!");
        else
            GD.Print("Couldn't save highscores file...");
    }

    public void LoadFromFile()
    {
        GD.Print("About to load highscores file...");

        ConfigFile config = new ConfigFile();
        Error error = config.Load("user://highscores.cfg");

        if (error != Error.Ok)
        {
            GD.Print("Highscores file not found");
            return;
        }

        highScores.Clear();
        officialLevelHighScores.Clear();

        if (config.HasSection("HighScores"))
        {
            foreach (string key in config.GetSectionKeys("HighScores"))
            {
                highScores.Add(key, (Array<int>)config.GetValue("HighScores", key));
            }
        }

        if (config.HasSection("OfficialLevelHighScores"))
        {
            foreach (string key in config.GetSectionKeys("OfficialLevelHighScores"))
            {
                officialLevelHighScores.Add(int.Parse(key), (int)config.GetValue("OfficialLevelHighScores", key));
            }
        }

        if (config.HasSection("ClearedOfficialLevels"))
        {
            clearedOfficialLevels = new Array<int>((Array<int>)config.GetValue("ClearedOfficialLevels", "ClearedLevels"));
        }

        GD.Print("Loaded highscores file!");
    }
}
