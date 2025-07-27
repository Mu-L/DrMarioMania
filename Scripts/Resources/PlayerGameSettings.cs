using Godot;
using System;
using System.Collections.Generic;
using static PowerUpEnums;

public partial class PlayerGameSettings : Resource
{
    [Export] private bool isCustomLevelSettings;
	// Game settings that can be specified for each player

    public void Reset()
    {
        SegmentColours.Clear();
        ColourCount = 3;

        GameplayStyle = 0;
        InitialVirusLevel = 0;
        SpeedLevel = 1;
    }

    public void CopySettings(PlayerGameSettings otherPlayer)
    {
        SegmentColours = new Godot.Collections.Array<int>(otherPlayer.SegmentColours);

        GameplayStyle = otherPlayer.gameplayStyle;
        InitialVirusLevel = otherPlayer.InitialVirusLevel;
        SpeedLevel = otherPlayer.SpeedLevel;

        if (gameplayStyle == 3)
        {
            ColourCount = otherPlayer.ColourCount;
            IsHoldEnabled = otherPlayer.IsHoldEnabled;
            MinStreakLength = otherPlayer.MinStreakLength;
            GenerousLockDelay = otherPlayer.GenerousLockDelay;
            AutoFallSpeed = otherPlayer.AutoFallSpeed;
            ImpatientMatching = otherPlayer.ImpatientMatching;
            FasterSoftDrop = otherPlayer.FasterSoftDrop;
            FasterAutoFall = otherPlayer.FasterAutoFall;
            InstantSoftDropLock = otherPlayer.InstantSoftDropLock;
            NoFallSpeedIncrease = otherPlayer.NoFallSpeedIncrease;
            OnlySingleColourPills = otherPlayer.OnlySingleColourPills;
            AvailablePowerUps = new List<PowerUp>(otherPlayer.AvailablePowerUps);
            AvailableSpecialPowerUps = new List<PowerUp>(otherPlayer.AvailableSpecialPowerUps);
            PowerUpMeterMaxLevel = otherPlayer.PowerUpMeterMaxLevel;
            FasterAutoRepeat = otherPlayer.FasterAutoRepeat;
        }
    }

    [ExportGroup("General")]
    // general game settings ======================================
	// settings that are configured during the game setup screens (game rules, visuals and sounds), not counting the advanced/custom settings
    [Export] public int GameplayStyle
    {
        get
        {
            return gameplayStyle;
        }
        set
        {
            gameplayStyle = value;

            // auto-set advanced settings based on given value
            switch (value)
            {
                // classic
                case 1:
                    if (!isCustomLevelSettings)
                        ColourCount = 3;
                    IsHoldEnabled = false;
                    MinStreakLength = 4;
                    GenerousLockDelay = false;
                    AutoFallSpeed = 4;
                    ImpatientMatching = false;
                    FasterSoftDrop = true;
                    FasterAutoFall = false;
                    InstantSoftDropLock = true;
                    NoFallSpeedIncrease = false;
                    OnlySingleColourPills = false;
                    ClearPowerUps();
                    PowerUpMeterMaxLevel = 16;
                    FasterAutoRepeat = false;

                    break;
                // quad
                case 2:
                    if (!isCustomLevelSettings)
                        ColourCount = 4;
                    IsHoldEnabled = true;
                    MinStreakLength = 4;
                    GenerousLockDelay = true;
                    AutoFallSpeed = 6;
                    ImpatientMatching = true;
                    FasterSoftDrop = false;
                    FasterAutoFall = true;
                    InstantSoftDropLock = false;
                    NoFallSpeedIncrease = false;
                    OnlySingleColourPills = false;
                    ClearPowerUps();
                    PowerUpMeterMaxLevel = 16;
                    FasterAutoRepeat = true;

                    break;
                // custom
                case 3:
                    // retain previous advanced settings
                    break;
                // power-ups
                case 4:
                    if (!isCustomLevelSettings)
                        ColourCount = 3;
                    IsHoldEnabled = true;
                    MinStreakLength = 4;
                    GenerousLockDelay = true;
                    AutoFallSpeed = 6;
                    ImpatientMatching = true;
                    FasterSoftDrop = false;
                    FasterAutoFall = true;
                    InstantSoftDropLock = false;
                    NoFallSpeedIncrease = false;
                    OnlySingleColourPills = false;
                    ClearPowerUps();
                    PowerUpMeterMaxLevel = 16;
                    FasterAutoRepeat = true;

                    for (int i = 0; i < Enum.GetValues<PowerUp>().Length; i++)
                    {
                        AvailablePowerUps.Add((PowerUp)i);
                    }

                    for (int i = 0; i < Enum.GetValues<PowerUp>().Length; i++)
                    {
                        AvailableSpecialPowerUps.Add((PowerUp)i);
                    }

                    break;
                // modern (default)
                default:
                    if (!isCustomLevelSettings)
                        ColourCount = 3;
                    IsHoldEnabled = true;
                    MinStreakLength = 4;
                    GenerousLockDelay = true;
                    AutoFallSpeed = 6;
                    ImpatientMatching = true;
                    FasterSoftDrop = false;
                    FasterAutoFall = true;
                    InstantSoftDropLock = false;
                    NoFallSpeedIncrease = false;
                    OnlySingleColourPills = false;
                    PowerUpMeterMaxLevel = 16;
                    FasterAutoRepeat = true;
                    
                    ClearPowerUps();
                    
                    break;
            }
        }
    }
    private int gameplayStyle;
    [Export] public int InitialVirusLevel { get; set; }
	[Export] public int SpeedLevel { get; set; }
	// the pill/virus colours used by this player
	[Export] public Godot.Collections.Array<int> SegmentColours { get; set; }

    [ExportGroup("Advanced")]
    // advanced gameplay settings ======================================
	// configurable via "custom" gameplay style menu
    [Export] public int ColourCount
    {
        get
        {
            return colourCount;
        }
        set
        {
            colourCount = value;

            // if SegmentColours doesn't match colourCount, fix it
            if (SegmentColours != null && SegmentColours.Count != colourCount)
            {
                FixSegmentColoursList();
            }
        }
    }
    private int colourCount;
    // If SegmentColours list doesn't match colourCount, add or remove colours so its count matches colourCount
    public void FixSegmentColoursList()
    {
        if (SegmentColours.Count > colourCount)
        {
            for (int i = 10; i > 0; i--)
            {
                if (SegmentColours.Contains(i))
                    SegmentColours.Remove(i);
                
                if (SegmentColours.Count == colourCount)
                    break;
            }
        }
        else if (SegmentColours.Count < colourCount)
        {
            for (int i = 1; i <= 10; i++)
            {
                if (!SegmentColours.Contains(i))
                    SegmentColours.Add(i);
                
                if (SegmentColours.Count == colourCount)
                    break;
            }
        }

        // Sort order of colours
        SegmentColours.Sort();
    }
    [Export] public int MinStreakLength { get; set; }
    [Export] public bool IsHoldEnabled { get; set; }
    [Export] public bool FasterSoftDrop { get; set; }
	[Export] public bool GenerousLockDelay
    {
        get
        {
            return generousLockDelay;
        }
        set
        {
            generousLockDelay = value;
            
            UseFallSpeedAsLockSpeed = !value;
            MaxLockResets = value ? 8 : 0;
        }
    }
    private bool generousLockDelay;
    [Export] public bool ImpatientMatching { get; set; }
    [Export] public bool FasterAutoFall
    {
        get
        {
            return fasterAutoFall;
        }
        set
        {
            fasterAutoFall = value;
            
            AutoFallSpeed = value ? 6 : 4;
        }
    }
    private bool fasterAutoFall;
    [Export] public bool InstantSoftDropLock { get; set; }
    [Export] public bool NoFallSpeedIncrease { get; set; }
    [Export] public bool OnlySingleColourPills { get; set; }
    public List<PowerUp> AvailablePowerUps = new List<PowerUp>();
    public List<PowerUp> AvailableSpecialPowerUps = new List<PowerUp>();
    [Export] public int PowerUpMeterMaxLevel { get; set; }
    public bool FasterAutoRepeat
    {
        get
        {
            return fasterAutoRepeat;
        }
        set
        {
            fasterAutoRepeat = value;

            firstMoveSpeed = value ? 6 : 3.75f;
            repeatedMoveSpeed = value ? 20 : 10;
        }
    }
    private bool fasterAutoRepeat = true;
    private float firstMoveSpeed;
    public float FirstMoveSpeed { get { return firstMoveSpeed; } }
	private float repeatedMoveSpeed;
    public float RepeatedMoveSpeed { get { return repeatedMoveSpeed; } }

    private void ClearPowerUps()
    {
        AvailablePowerUps.Clear();
        AvailableSpecialPowerUps.Clear();
    }

    private string BoolToString(bool b)
    {
        return b ? "1" : "0";
    }
    private bool StringToBool(string s)
    {
        return s == "1";
    }

    public string ExportToString()
    {
        string itemDivider = ";";
        string subItemDivider = ",";
        
        string code = "";

        for (int i = 0; i < SegmentColours.Count; i++)
        {
            code += SegmentColours[i];
            if (i < SegmentColours.Count - 1)
                code += subItemDivider;
        }
        code += itemDivider;

        code += GameplayStyle + itemDivider;
        code += InitialVirusLevel + itemDivider;
        code += SpeedLevel;

        if (gameplayStyle == 3)
        {
            code += itemDivider;

            code += BoolToString(IsHoldEnabled) + itemDivider;
            code += MinStreakLength + itemDivider;
            code += BoolToString(GenerousLockDelay) + itemDivider;
            code += AutoFallSpeed + itemDivider;
            code += BoolToString(ImpatientMatching) + itemDivider;
            code += BoolToString(FasterSoftDrop) + itemDivider;
            code += BoolToString(FasterAutoFall) + itemDivider;
            code += BoolToString(InstantSoftDropLock) + itemDivider;
            code += BoolToString(NoFallSpeedIncrease) + itemDivider;
            code += BoolToString(OnlySingleColourPills) + itemDivider;
            
            for (int i = 0; i < AvailablePowerUps.Count; i++)
            {
                code += (int)AvailablePowerUps[i];
                if (i < AvailablePowerUps.Count - 1)
                    code += subItemDivider;
            }

            code += itemDivider;

            for (int i = 0; i < AvailableSpecialPowerUps.Count; i++)
            {
                code += (int)AvailableSpecialPowerUps[i];
                if (i < AvailableSpecialPowerUps.Count - 1)
                    code += subItemDivider;
            }

            code += itemDivider;

            code += PowerUpMeterMaxLevel + itemDivider;

            code += BoolToString(FasterAutoRepeat);

        }

        return code;
    }
    public bool ImportFromString(string code)
    {
        string backUpCode = ExportToString();

        try
        {
            string itemDivider = ";";
            string subItemDivider = ",";
            
            // if : detected, use : instead of ; (legacy code compatibility)
            if (code.Contains(':'))
                itemDivider = ":";

            string[] codeChunks = code.Split(itemDivider);

            {
                string[] segmentColData = codeChunks[0].Split(subItemDivider);
                SegmentColours.Clear();

                if (segmentColData[0] != "")
                {
                    for (int i = 0; i < segmentColData.Length; i++)
                    {
                        SegmentColours.Add(int.Parse(segmentColData[i]));
                    }
                }
            }

            GameplayStyle = int.Parse(codeChunks[1]);
            InitialVirusLevel = int.Parse(codeChunks[2]);
            SpeedLevel = int.Parse(codeChunks[3]);

            if (gameplayStyle == 3)
            {
                ColourCount = SegmentColours.Count;
                IsHoldEnabled = StringToBool(codeChunks[4]);
                MinStreakLength = int.Parse(codeChunks[5]);
                GenerousLockDelay = StringToBool(codeChunks[6]);
                AutoFallSpeed = int.Parse(codeChunks[7]);
                ImpatientMatching = StringToBool(codeChunks[8]);
                FasterSoftDrop = StringToBool(codeChunks[9]);
                FasterAutoFall = StringToBool(codeChunks[10]);
                InstantSoftDropLock = StringToBool(codeChunks[11]);
                NoFallSpeedIncrease = StringToBool(codeChunks[12]);
                OnlySingleColourPills = StringToBool(codeChunks[13]);

                {
                    string[] powerUpData = codeChunks[14].Split(subItemDivider);

                    AvailablePowerUps.Clear();

                    if (powerUpData[0] != "")
                    {
                        for (int i = 0; i < powerUpData.Length; i++)
                        {
                            AvailablePowerUps.Add((PowerUp)int.Parse(powerUpData[i]));
                        }
                    }
                }

                {
                    string[] specialPowerUpData = codeChunks[15].Split(subItemDivider);

                    AvailableSpecialPowerUps.Clear();

                    if (specialPowerUpData[0] != "")
                    {
                        for (int i = 0; i < specialPowerUpData.Length; i++)
                        {
                            AvailableSpecialPowerUps.Add((PowerUp)int.Parse(specialPowerUpData[i]));
                        }
                    }
                }

                PowerUpMeterMaxLevel = int.Parse(codeChunks[16]);

                if (codeChunks.Length > 17)
                {
                    FasterAutoRepeat = StringToBool(codeChunks[17]);
                }
            }
            // successful
            return true;
        }
        catch (Exception e)
        {
            ImportFromString(backUpCode);
            GD.PrintErr("Game rule settings caused an error, settings were unchanged: " + e.Message);

            // unsuccessful
            return false;
        }
    }

    // automatic/hidden gameplay settings ======================================
	// only gets changed whenever another setting changes, not adjustable in menus

    // determined by GenerousLockDelay:
    public bool UseFallSpeedAsLockSpeed { get; set; }
    public int MaxLockResets { get; set; }

    // determined by FasterAutoFall
    public float AutoFallSpeed { get; set; }
    
    // whether or not this player is using any power-ups
    public bool IsUsingPowerUps { get { return AvailablePowerUps.Count != 0 || AvailableSpecialPowerUps.Count != 0; } }
}
