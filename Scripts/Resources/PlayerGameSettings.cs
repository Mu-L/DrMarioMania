using Godot;
using System;
using System.Collections.Generic;
using static PowerUpEnums;
using static PillEnums;
using System.Numerics;

public partial class PlayerGameSettings : Resource
{
    [Export] private bool isCustomLevelSettings;
	// Game settings that can be specified for each player

    public void Reset()
    {
        ChosenColours.Clear();
        ColourCount = 3;

        GameplayStyle = 0;
        InitialVirusLevel = 0;
        SpeedLevel = 1;
    }

    public void CopySettings(PlayerGameSettings otherPlayer)
    {
        ChosenColours = new Godot.Collections.Array<int>(otherPlayer.ChosenColours);

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
            AvailablePillShapes = new List<PillShape>(otherPlayer.AvailablePillShapes);
            JarSize = otherPlayer.JarSize;
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
                    PowerUpMeterMaxLevel = 16;
                    FasterAutoRepeat = false;
                    JarSize = GameConstants.DefaultJarSize;

                    ClearPowerUps();
                    SetSinglePillShape(PillShape.Double);

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
                    PowerUpMeterMaxLevel = 16;
                    FasterAutoRepeat = true;
                    JarSize = GameConstants.DefaultJarSize;

                    ClearPowerUps();
                    SetSinglePillShape(PillShape.Double);

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
                    PowerUpMeterMaxLevel = 16;
                    FasterAutoRepeat = true;
                    JarSize = GameConstants.DefaultJarSize;

                    ClearPowerUps();
                    SetSinglePillShape(PillShape.Double);

                    for (int i = 0; i < Enum.GetValues<PowerUp>().Length; i++)
                    {
                        AvailablePowerUps.Add((PowerUp)i);
                    }

                    for (int i = 0; i < Enum.GetValues<PowerUp>().Length; i++)
                    {
                        AvailableSpecialPowerUps.Add((PowerUp)i);
                    }

                    break;
                // luigi
                case 5:
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
                    JarSize = GameConstants.DefaultJarSize;

                    ClearPowerUps();
                    SetSinglePillShape(PillShape.Luigi);

                    break;
                // all pill shapes
                case 6:
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
                    JarSize = GameConstants.DefaultJarSize;

                    ClearPowerUps();

                    AvailablePillShapes.Clear();
                    for (int i = 0; i < GameConstants.NoOfPillShapes; i++)
                    {
                        AvailablePillShapes.Add((PillShape)i);
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
                    JarSize = GameConstants.DefaultJarSize;
                    
                    ClearPowerUps();
                    SetSinglePillShape(PillShape.Double);
                    
                    break;
            }
        }
    }
    private int gameplayStyle;
    [Export] public int InitialVirusLevel { get; set; }
	[Export] public int SpeedLevel { get; set; }
	// the pill/virus colours used by this player
	[Export] public Godot.Collections.Array<int> ChosenColours { get; set; }

    // tile grid dimensions of jar
    public Vector2I JarSize { get; set; } = GameConstants.DefaultJarSize;
    public int JarWidth
    {
        get { return JarSize.X; }
        set { JarSize = new Vector2I(value, JarSize.Y); }
    }

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

            // if ChosenColours doesn't match colourCount, fix it
            if (ChosenColours != null && ChosenColours.Count != colourCount)
            {
                FixChosenColoursList();
            }
        }
    }
    private int colourCount;
    // If ChosenColours list doesn't match colourCount, add or remove colours so its count matches colourCount
    public void FixChosenColoursList()
    {
        if (ChosenColours.Count > colourCount)
        {
            for (int i = 10; i > 0; i--)
            {
                if (ChosenColours.Contains(i))
                    ChosenColours.Remove(i);
                
                if (ChosenColours.Count == colourCount)
                    break;
            }
        }
        else if (ChosenColours.Count < colourCount)
        {
            for (int i = 1; i <= 10; i++)
            {
                if (!ChosenColours.Contains(i))
                    ChosenColours.Add(i);
                
                if (ChosenColours.Count == colourCount)
                    break;
            }
        }

        // Sort order of colours
        ChosenColours.Sort();
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
    private void ClearPowerUps()
    {
        AvailablePowerUps.Clear();
        AvailableSpecialPowerUps.Clear();
    }
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
    public List<PillShape> AvailablePillShapes = new List<PillShape> { PillShape.Double };

    private void SetSinglePillShape(PillShape shape)
    {
        AvailablePillShapes.Clear();
        AvailablePillShapes.Add(shape);
    }

    // If AvailablePillShapes is empty, add the double shape to it
    public void FixAvailablePillShapes()
    {
        if (AvailablePillShapes.Count == 0)
            AvailablePillShapes.Add(PillShape.Double);
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

        for (int i = 0; i < ChosenColours.Count; i++)
        {
            code += ChosenColours[i];
            if (i < ChosenColours.Count - 1)
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

            code += PowerUpMeterMaxLevel;

            code += itemDivider;

            code += BoolToString(FasterAutoRepeat);

            code += itemDivider;

            for (int i = 0; i < AvailablePillShapes.Count; i++)
            {
                code += (int)AvailablePillShapes[i];
                if (i < AvailablePillShapes.Count - 1)
                    code += subItemDivider;
            }

            code += itemDivider;

            code += JarSize.X;
            code += subItemDivider;
            code += JarSize.Y;
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
                ChosenColours.Clear();

                if (segmentColData[0] != "")
                {
                    for (int i = 0; i < segmentColData.Length; i++)
                    {
                        ChosenColours.Add(int.Parse(segmentColData[i]));
                    }
                }
            }

            GameplayStyle = int.Parse(codeChunks[1]);
            InitialVirusLevel = int.Parse(codeChunks[2]);
            SpeedLevel = int.Parse(codeChunks[3]);

            if (gameplayStyle == 3)
            {
                ColourCount = ChosenColours.Count;
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

                if (codeChunks.Length > 18)
                {
                    string[] pillShapeData = codeChunks[18].Split(subItemDivider);

                    AvailablePillShapes.Clear();

                    if (pillShapeData[0] != "")
                    {
                        for (int i = 0; i < pillShapeData.Length; i++)
                        {
                            AvailablePillShapes.Add((PillShape)int.Parse(pillShapeData[i]));
                        }
                    }
                }

                if (codeChunks.Length > 19)
                {
                    string[] jarSizeData = codeChunks[19].Split(subItemDivider);

                    JarSize = new Vector2I(int.Parse(jarSizeData[0]), int.Parse(jarSizeData[1]));
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
    // whether or not the dr luigi sprite should be used over mario
    public bool UseLuigiSprite
    {
        get
        {
            return AvailablePillShapes.Contains(PillShape.Luigi) && AvailablePillShapes.Count == 1;
        }
    }
}
