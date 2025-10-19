using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PowerUpEnums;

public partial class JarManager : Node
{
	// handles an individual player's jar, including checking for matching colours, destroying segments, causing tiles to fall, etc AND scoring
	// basically a player-spefific game manager rather than the entire game

	[ExportGroup("Tilemap")]
	[Export] private TileMapLayer jarTiles;
	public TileMapLayer JarTiles { get { return jarTiles; } }
	[Export] private TileMapLayer previewTiles;
	public TileMapLayer PreviewTiles { get { return previewTiles; } }
	[Export] private TileMapLayer foregroundTiles;
	public TileMapLayer ForegroundTiles { get { return foregroundTiles; } }

	// Top-left tile of jar
	[Export] private Vector2I jarOrigin;
	public Vector2I JarOrigin { get	{ return jarOrigin; } }
	public Vector2I JarSize { get { return PlayerGameSettings.JarSize; } }
	private Vector2I baseJarSize = new Vector2I(8, 16);
	public Vector2I BaseJarSize { get { return baseJarSize; } }

	[ExportGroup("Speeds")]
	[Export] private float destroyRowSpeed;
	[Export] private float destroyDisappearSpeed;
	public float DestroyDisappearSpeed { get { return destroyDisappearSpeed; } }
	[Export] private int virusGenerationSpeed;

	[ExportGroup("References")]
	[Export] private PowerUpPrefabs powerUpPrefabs;
	private PowerUpMeter PowerUpMeter { get { return uiMan.PowerUpMeter; } }
	private VirusRing VirusRing  { get { return uiMan.VirusRing; } }
	[Export] private JarUIManager uiMan;
	public JarUIManager UIMan { get { return uiMan; } }
	[Export] private PillManager pillMan;
	public PillManager PillMan { get { return pillMan; } }

	// common/global settings shared between all players
	public CommonGameSettings CommonGameSettings { get; set; }
	// player-specific game settings, this will either be the player's own chosen settings or the one provided by the custom level
	public PlayerGameSettings PlayerGameSettings { get; set; }
	public PlayerMultiInputSettings PlayerMultiInputSettings { get; set; }
	public HighScoreList HighScoreList { get; set; }
	public int PlayerID { get; set; }
	
	private SfxManager sfxMan;
	public SfxManager SfxMan
	{
		get
		{
			return sfxMan;
		}
		set
		{
			sfxMan = value;
			PillMan.SfxMan = value;
			if (VirusRing != null)
				VirusRing.SfxMan = value;
		}
	}
	public GameManager GameMan { get; set; }
	public GameThemer GameThemer { get; set; }

	public Vector2 TilemapGlobalPos { get { return jarTiles.GlobalPosition; } }
	public Vector2I JarCellSize { get { return jarTiles.TileSet.TileSize; } }

	// Jar edge positions
	public float JarTopPos { get { return jarTiles.GlobalPosition.Y + JarCellSize.Y * (-JarSize.Y / 2.0f + 0.5f); } }
	public float JarBottomPos { get { return jarTiles.GlobalPosition.Y + JarCellSize.Y * (JarSize.Y / 2.0f - 0.5f); } }
	public float JarLeftPos { get { return jarTiles.GlobalPosition.X + JarCellSize.X * (-JarSize.X / 2.0f + 0.5f); } }
	public float JarRightPos { get { return jarTiles.GlobalPosition.X + JarCellSize.X * (JarSize.X / 2.0f - 0.5f); } }


	// key = y pos
	// value = x pos list
	// Tiles to be destroyed (turned into "popped" tiles)
	private Dictionary<int, List<int>> tilesToDestroy = new Dictionary<int, List<int>>();
	// Destroyed/popped tiles that need to be cleared
	private Dictionary<int, List<int>> destroyedTiles = new Dictionary<int, List<int>>();
	// Tiles to be shifted downwards
	private Dictionary<int, List<int>> tilesToFall = new Dictionary<int, List<int>>();

	// Tiles that have landed after auto-falling that will be checked if any matches are present at them
	private List<Vector2I> uncheckedLandedTiles = new List<Vector2I>();

	private bool destructionContainsVirus = false;
	private double destroyTimer = 0;
	private double autoFallTimer = 0;
	private int destroyRow;
	// key = pos, value = colour
	private Dictionary<Vector2I, int> virusesRemaining = new Dictionary<Vector2I, int>();

	// possible colours pill/power-ups can be
	private List<int> possibleColours = new List<int>();
	public List<int> PossibleColours { get { return possibleColours; } }

	// if overrideCustomLevelColours exist, the original colours prior to being replaced are added here
	private List<int> originalPossibleColours = new List<int>();
	private bool HaveColoursBeenReplaced { get { return originalPossibleColours.Count > 0; } }


	// power-up tiles which are forced to not fall - used for power-ups place in the level editor, which shouldn't fall unlike obtained power-ups
	private List<Vector2I> frozenPowerUps = new List<Vector2I>();

	private int score = 0;
	public int Score { get { return score; } }
	private int virusLevel = -1;
	public int VirusLevel { get { return virusLevel; } set { virusLevel = value; } }
	public int SpeedLevel { get { return PlayerGameSettings.SpeedLevel; } }

	// viruses killed in one move
	private int virusCombo = 0;
	// lines cleared (columns and/or rows) in one move
	private int lineCombo = 0;
	// times a match is made in one move - this differs from line combos as if multiple lines are cleared at a time, lineCombo will go up two or more times, whereas comboMatches goes up once
	private int matchCombo = 0;

	private bool isPlayerOut = false;
	public bool IsPlayerOut { get { return isPlayerOut; } }

	// List of colours destroyed per line in a single turn/move
	private List<int> lineComboColours = new List<int>();
	// List of colour(s) destroyed on the first match in a single turn/move (before any pills fall to create another match)
	private List<int> initialLineComboColours = new List<int>();

	// List of groups of junk segments that will spawn at the top of the screen on the next turn (if list isn't empty)
	// Each turn will spawn the oldest group of junk segments in this list and remove it from the list
	// NOTE: The int IDs don't represent colour IDs, but rather the index of this player's specific colours (e.g. if player has yellow, green, purple, 0 = yellow, 1 = green, etc)
	private List<List<int>> queuedJunkSegments = new List<List<int>>();
	// List of power-ups that have been matched-up and activated
	private List<BasePowerUp> activePowerUps = new List<BasePowerUp>();

	// Whether or not junk segments are falling down
	private bool isJunkFalling = false;
	// The maximum no. of junk segments a player can send to another in one batch
	private const int maxJunkSegments = 4;

	// The X position for the popped state atlas of each tile type
	private int pillPoppedAtlasX;
	private int virusPoppedAtlasX;
	private int powerUpPoppedAtlasX;
	private Dictionary<Vector2I, JarTileData> customLevelTiles;
	public bool IsInEditorScene { get; set; } = false;
	public bool DisablePowerUpSpawning { get; set; } = false;

	// Jar-specific random number generator
	RandomNumberGenerator localRng = new RandomNumberGenerator();
	public RandomNumberGenerator LocalRng { get { return localRng; } }

	// Whether or not the player won or not on their last round
	private bool lastWinState = false;
	public bool LastWinState { get { return lastWinState; } }
	// No. of rounds won against other players in multiplayer
	private int roundWins = 0;
	// Whether or not this player has won enough rounds to be declared the winner of the overall game
	public bool HasWonEnoughRounds { get { return roundWins == CommonGameSettings.MultiplayerRequiredWinCount; } }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UpdatePoppedAtlasPositions();
		if (CommonGameSettings.PlayerCount == 1)
			uiMan.SetHighScoreLabel(HighScoreList.GetGameRuleHighScore());
		SetProcess(false);

		SetVirusTileAnimationState(IsInEditorScene ? false : CommonGameSettings.EnableVirusTileAnimation);
	}
 
	// Enables/disables the animation of the virus tiles AND animated object tiles
	public void SetVirusTileAnimationState(bool enabled)
	{
		TileSetAtlasSource virusSource = (TileSetAtlasSource)jarTiles.TileSet.GetSource(GameConstants.virusSourceID);
		TileSetAtlasSource objectSource = (TileSetAtlasSource)jarTiles.TileSet.GetSource(GameConstants.objectSourceID);
		
		for (int i = 0; i < GameConstants.noOfColours; i++)
		{
			virusSource.SetTileAnimationFramesCount(Vector2I.Zero + Vector2I.Down * i, enabled ? 2 : 1);
		}

		for (int i = 0; i < GameConstants.noOfObjects; i++)
		{
			Vector2I atlas = Vector2I.Zero + Vector2I.Down * i;

			if (objectSource.GetTileAnimationSpeed(atlas) > 1)
				objectSource.SetTileAnimationFramesCount(atlas, enabled ? 3 : 1);
		}
	}

	// Whether the tile at pos is the same as the target source and atlas (must be an exact match, NOT just the colour)
	public bool DoesTileMatch(Vector2I pos, int targetSourceID, Vector2I targetAtlas)
    {
        return jarTiles.GetCellSourceId(pos) == targetSourceID && jarTiles.GetCellAtlasCoords(pos) == targetAtlas;
    }

    public string ExportLevelToString()
    {
		string sectionDivider = "/";
		string itemDivider = ";";
        string subItemDivider = ",";

		string code = "";

		int lastDifferentSourceID = -2;
		Vector2I lastDifferentAtlas = -Vector2I.One;
		int sameStreak = -1;
		int savedStreaks = 0;

		// Basic settings ==================================================================================================
		// level format version
		code += GameConstants.levelCodeVer + itemDivider;
		code += CommonGameSettings.CustomLevelName + itemDivider;
		code += CommonGameSettings.CustomLevelTheme + itemDivider;
		code += CommonGameSettings.CustomLevelMusic + itemDivider;
		code += JarSize.X + subItemDivider + JarSize.Y + itemDivider;
		code += (CommonGameSettings.CustomLevelMusic == GameConstants.customMusicID ? CommonGameSettings.CustomLevelCustomMusicFile : "") + sectionDivider;

		// Jar tile data ==================================================================================================
		for (int y = 0; y < JarSize.Y; y++)
        {
            for (int x = 0; x < JarSize.X; x++)
            {
                Vector2I pos = new Vector2I(x + jarOrigin.X, y + jarOrigin.Y);
                
                int sourceID = jarTiles.GetCellSourceId(pos);
                Vector2I atlas = jarTiles.GetCellAtlasCoords(pos);

				// if matching previous...
				if (DoesTileMatch(pos, lastDifferentSourceID, lastDifferentAtlas))
				{
					sameStreak++;
				}
				// if NOT matching previous...
				else
				{
					if (sameStreak != -1)
					{
						if (savedStreaks != 0)
							code += itemDivider;
						
						code += lastDifferentSourceID + subItemDivider + ((lastDifferentSourceID == -1) ? sameStreak : (lastDifferentAtlas.X + subItemDivider + lastDifferentAtlas.Y + subItemDivider + sameStreak));
						savedStreaks++;
					}

					lastDifferentSourceID = sourceID;
					lastDifferentAtlas = atlas;
					sameStreak = 1;
				}
            }

        }

		if (lastDifferentSourceID != -1)
		{
			if (savedStreaks != 0)
				code += itemDivider;
			
			code += lastDifferentSourceID + subItemDivider + ((lastDifferentSourceID == -1) ? sameStreak : (lastDifferentAtlas.X + subItemDivider + lastDifferentAtlas.Y + subItemDivider + sameStreak));
			savedStreaks++;
		}

		// Player game settings ==================================================================================================
		code += sectionDivider + PlayerGameSettings.ExportToString();

		return code;
    }

	public bool ImportLevelFromString(string code)
    {
        string backUpCode = ExportLevelToString();

        try
        {
			string sectionDivider = "/";
			string itemDivider = ";";
			string subItemDivider = ",";

			// if v0 code detected, use : instead of ; (legacy code compatibility)
            if (code[0] == '0')
                itemDivider = ":";

            string[] codeSections = code.Split(sectionDivider);

			// Basic settings ==================================================================================================
            string[] basicSettingChunks = codeSections[0].Split(itemDivider);

			if (PlayerID == 0)
			{
				CommonGameSettings.CustomLevelName = basicSettingChunks[1];
				CommonGameSettings.CustomLevelTheme = int.Parse(basicSettingChunks[2]);
				CommonGameSettings.CustomLevelMusic = int.Parse(basicSettingChunks[3]);

				if (CommonGameSettings.CustomLevelMusic == GameConstants.customMusicID)
					CommonGameSettings.CustomLevelCustomMusicFile = basicSettingChunks[5];
			}

			string[] jarSizeData = basicSettingChunks[4].Split(subItemDivider);
			PlayerGameSettings.JarWidth = int.Parse(jarSizeData[0]);
			//PlayerGameSettings.JarSize.Y = int.Parse(jarSizeData[1]);
			UpdateJarSize();

			// Jar tile data ==================================================================================================
			jarTiles.Clear();
			foregroundTiles.Clear();

			if (codeSections[1] != "")
			{
            	string[] tileData = codeSections[1].Split(itemDivider);

				int streakSourceID = -2;
				Vector2I streakAtlas = -Vector2I.One;

				int streakNoToBuild = 0;
				int streakTilesRemaining = 0;
			
				bool breakLoop = false;
				for (int y = 0; y < JarSize.Y; y++)
				{
					for (int x = 0; x < JarSize.X; x++)
					{
						Vector2I pos = new Vector2I(x + jarOrigin.X, y + jarOrigin.Y);

						if (streakTilesRemaining == 0)
						{
							if (streakSourceID != -2)
								streakNoToBuild++;

							if (streakNoToBuild == tileData.Length)
							{
								breakLoop = true;
								break;
							}

							string[] individualTileData = tileData[streakNoToBuild].Split(subItemDivider);
							streakSourceID = int.Parse(individualTileData[0]);

							if (streakSourceID == -1)
							{
								streakTilesRemaining = int.Parse(individualTileData[1]);
							}
							else
							{
								streakAtlas.X = int.Parse(individualTileData[1]);
								streakAtlas.Y = int.Parse(individualTileData[2]);
								streakTilesRemaining = int.Parse(individualTileData[3]);
							}
						}

						if (streakSourceID != -1)
							jarTiles.SetCell(pos, streakSourceID, streakAtlas);
						streakTilesRemaining--;
					}

					if (breakLoop)
						break;
				}
			}

            // Player game settings ============================================================================
            PlayerGameSettings.ImportFromString(codeSections[2]);

			// Update all visuals
			GameThemer.UpdateAllVisualsAndSfx();
			
            // successful
            return true;
        }
        catch (Exception e)
        {
            ImportLevelFromString(backUpCode);
            GD.PrintErr("Level code caused an error, level was unchanged: " + e.Message);

            // unsuccessful
            return false;
        }
    }

	private void UpdatePoppedAtlasPositions()
	{
		pillPoppedAtlasX = ((TileSetAtlasSource)jarTiles.TileSet.GetSource(GameConstants.pillSourceID)).GetAtlasGridSize().X - 1;
		virusPoppedAtlasX =((TileSetAtlasSource)jarTiles.TileSet.GetSource(GameConstants.virusSourceID)).GetAtlasGridSize().X - 1;
		powerUpPoppedAtlasX = ((TileSetAtlasSource)jarTiles.TileSet.GetSource(GameConstants.powerUpSourceID)).GetAtlasGridSize().X - 1;
	}

	public bool IsActionJustPressed(string action)
	{
		if (CommonGameSettings.PlayerCount > 1)
		{
			string newAction = PlayerMultiInputSettings.MultiplayerInputPrefix + action;

			return InputMap.HasAction(newAction) ? Input.IsActionJustPressed(newAction) : false;
		}
		else
		{
			return (Input.IsActionJustPressed("KeyboardFull" + action) && !Input.IsActionPressed("ControllerSingle" + action))
			|| (Input.IsActionJustPressed("ControllerSingle" + action) && !Input.IsActionPressed("KeyboardFull" + action));
		}
	}

	public bool IsActionPressed(string action)
	{
		if (CommonGameSettings.PlayerCount > 1)
		{
			string newAction = PlayerMultiInputSettings.MultiplayerInputPrefix + action;
			return InputMap.HasAction(newAction) ? Input.IsActionPressed(newAction) : false;
		}
		else
		{
			return Input.IsActionPressed("KeyboardFull" + action) || Input.IsActionPressed("ControllerSingle" + action);
		}
	}

	private void Continue()
	{
		if (CommonGameSettings.IsCustomLevel || CommonGameSettings.IsMultiplayer)
			GameMan.ReplayLevel();
		else	
			GameMan.NextLevel();
	}

	private void QuitGame()
	{
		GameMan.QuitGame();
	}

	public void Win()
	{
		if (CommonGameSettings.IsMultiplayer)
		{
			roundWins++;
			uiMan.UpdateWinIconContainer(roundWins);
		}
		else
		{
			// only hide the next pill (aka the one mario is holding) in singleplayer, since he's not there in multiplayer
			pillMan.HideNextPill();
		}

		isPlayerOut = true;
		lastWinState = true;
		
		SetProcess(false);
		pillMan.SetProcess(false);
		
		if (!IsInEditorScene)
		{
			uiMan.ShowWinOverlay();

			if (!CommonGameSettings.IsMultiplayer)
				uiMan.Mario.PlayAnimation("Win");
		}
		

		if (pillMan.IsThrowingPill)
			pillMan.HideThrowingPills();

		previewTiles.Clear();
		
		if (IsInEditorScene)
			GameMan.EndPlayTest();

		GameMan.EndGame();

		DisablePowerUpSpawning = true;
		DeleteAllPowerUps();
	}

	public void GameOver()
	{		
		isPlayerOut = true;
		lastWinState = false;

		SetProcess(false);
		pillMan.SetProcess(false);
		
		if (!IsInEditorScene)
		{
			uiMan.ShowGameOverOverlay(CommonGameSettings.PlayerCount > 1);

			if (!CommonGameSettings.IsMultiplayer)
			{
				uiMan.Mario.PlayAnimation("Lose");
				VirusRing.VirusLaugh();
			}
		}

		if (pillMan.IsThrowingPill)
			pillMan.HideThrowingPills();

		previewTiles.Clear();

		if (IsInEditorScene)
			GameMan.EndPlayTest();
		else if (CommonGameSettings.PlayerCount > 1)
			GameMan.IndicatePlayerOut();
		else
		{
			GameMan.EndGame();
			// only hide the next pill (aka the one mario is holding) in singleplayer, since he's not there in multiplayer
			pillMan.HideNextPill();
		}

		DisablePowerUpSpawning = true;
		DeleteAllPowerUps();
	}

	public void ResetState(bool resetScore, bool resetWinCount)
	{
		if (resetWinCount)
		{
			roundWins = 0;
			uiMan.UpdateWinIconContainer(0);
		}

		if (resetScore)
		{
			ResetScore();
		}
		
		uiMan.HideOverlay();

		if (!CommonGameSettings.IsMultiplayer)
			uiMan.Mario.ResetFrame();

		uiMan.SetVirusLabel(0);

		tilesToDestroy.Clear();
		destroyedTiles.Clear();
		tilesToFall.Clear();
		uncheckedLandedTiles.Clear();

		destructionContainsVirus = false;

		destroyTimer = 0;
		autoFallTimer = 0;

		ResetCombo();
		isJunkFalling = false;

		isPlayerOut = false;
		
		jarTiles.Clear();
		foregroundTiles.Clear();

		previewTiles.Clear();

		frozenPowerUps.Clear();

		pillMan.ResetState();

		PowerUpMeter.ResetState();

		DeleteAllPowerUps();
	}

	public void ResetScore()
	{
		score = 0;
		if (CommonGameSettings.PlayerCount == 1)
			uiMan.SetScoreLabel(0);
	}
	public void ResetVirusCount()
	{
		uiMan.SetVirusLabel(0);
	}

	public void DeleteAllPowerUps()
	{
		if (activePowerUps.Count != 0)
		{
			foreach (BasePowerUp powerUp in activePowerUps)
			{
				powerUp.QueueFree();
			}
			activePowerUps.Clear();
		}
	}

	private void ResetCombo()
	{
		virusCombo = 0;
		lineCombo = 0;
		matchCombo = 0;

		lineComboColours.Clear();
		initialLineComboColours.Clear();
	}

	public void PrepareLevel(ulong seed)
	{
		localRng.Seed = seed;
		DisablePowerUpSpawning = false;
		PowerUpMeter.SetVisibility(PlayerGameSettings.IsUsingPowerUps);
		uiMan.SetSpeedLabel(PlayerGameSettings.SpeedLevel);
		
		if (!CommonGameSettings.IsMultiplayer)
			GameThemer.UpdateMarioSprite();

        pillMan.InitialiseNextPillVariables();

        if (customLevelTiles != null)
			GenerateCustomLevel();
		else
		{
            UpdateJarSize();
            GenerateViruses();
		}
	}

	// Scans all current jar tiles and adds them to the custom level tiles dictionary
	public void CurrentTilesToCustomLevelTiles()
	{
		if (customLevelTiles == null)
			customLevelTiles = new Dictionary<Vector2I, JarTileData>();

		customLevelTiles.Clear();

		// Scan all tiles
        for (int y = JarOrigin.Y; y < JarOrigin.Y + JarSize.Y; y++)
        {
            for (int x = JarOrigin.X; x < JarOrigin.X + JarSize.X; x++)
            {
                Vector2I pos = new Vector2I(x, y);

				if (jarTiles.GetCellSourceId(pos) != -1)
				{
					JarTileData tileData = new JarTileData(pos, jarTiles);
					customLevelTiles.Add(pos, tileData);
				}
            }

        }
	}

	public void RestoreCustomLevelTilesForEditor()
	{
		jarTiles.Clear();
		foregroundTiles.Clear();
		foreach (Vector2I pos in customLevelTiles.Keys)
		{
			jarTiles.SetCell(pos, customLevelTiles[pos].sourceID, customLevelTiles[pos].atlas);
		}
	}

	// Scan tiles for colours and update PlayerGameSettings' ChosenColours and ColourCount accordingly
	public void SavePresentColoursToGameSettings()
	{
		List<int> presentColours = new List<int>();

		for (int y = jarOrigin.Y; y < jarOrigin.Y + JarSize.Y; y++)
		{
			for (int x = jarOrigin.X; x < jarOrigin.X + JarSize.X; x++)
			{
				Vector2I pos = new Vector2I(x, y);

				int colour = GetTileColour(pos);

				if (colour > 0 && !presentColours.Contains(colour))
					presentColours.Add(colour);
			}
		}

		// Fallback colour if level is empty
		if (presentColours.Count == 0)
			presentColours.Add(1);

		PlayerGameSettings.ChosenColours = new Godot.Collections.Array<int>(presentColours);
		PlayerGameSettings.ColourCount = presentColours.Count;
	}

	// Replace possibleColours with newColours
	public void ReplacePossibleColours(Godot.Collections.Array<int> newColours)
	{
		// Backup original colours into originalPossibleColours
		if (originalPossibleColours.Count == 0)
			originalPossibleColours = new List<int>(possibleColours);

		// Indexes in originalPossibleColours/possibleColours to skip since they already exist in newColours
		List<int> segColIndexesToSkip = new List<int>();
		// Colours in newColours that don't need to be used as they're already in originalPossibleColours
		List<int> newColoursToSkip = new List<int>();
		
		// Fill in segColIndexesToSkip and newColoursToSkip
		foreach (var newCol in newColours)
		{
			if (originalPossibleColours.Contains(newCol))
			{
				//GD.Print("this col already exists:" + newCol);
				segColIndexesToSkip.Add(originalPossibleColours.IndexOf(newCol));
				newColoursToSkip.Add(newCol);
			}
		}

		// No. of colours to change (smallest of newColours and possibleColours count, minus any colours to be skipped)
		int remainingChanges = Mathf.Min(newColours.Count, possibleColours.Count) - newColoursToSkip.Count;

		int segColIndex = 0;
		int newColIndex = 0;

		// Replace colours in possibleColours with newColours, skipping possibleColours that don't need changed (as exist in newColours) and skipping newColours that don't need to be used (as exist in possibleColours)
		while (remainingChanges > 0)
		{
			if (!segColIndexesToSkip.Contains(segColIndex))
			{
				if (newColoursToSkip.Contains(newColours[newColIndex]))
				{
					//GD.Print("" + newColours[newColIndex] + " is being skipped");
					newColIndex++;
					continue;
				}
				else
				{
					//GD.Print("replacing " + possibleColours[segColIndex] + " with " + newColours[newColIndex]);
					possibleColours[segColIndex] = newColours[newColIndex];
					remainingChanges--;
					newColIndex++;
				}
			}

			segColIndex++;
		}
	}

	public async void GenerateCustomLevel()
	{
		virusesRemaining.Clear();
		possibleColours.Clear();
		originalPossibleColours.Clear();

		List<int> virusColours = new List<int>();
		
		// Find possible colours that pills/power-ups should come in
		foreach (JarTileData tile in customLevelTiles.Values)
		{
			int colour = tile.colour;
			
			// if any instance of this colour appears, add to possibleColours
			if (colour > 0 && !possibleColours.Contains(colour))
			{
				possibleColours.Add(colour);
			}

			// if a virus of this colour appears, add to virusColours
			if (tile.sourceID == GameConstants.virusSourceID && !virusColours.Contains(colour))
				virusColours.Add(colour);
		}

		// Fallback possible colour if level is empty
		if (possibleColours.Count == 0)
			possibleColours.Add(1);

		// Fallback virus colour if virusColours is empty
		if (virusColours.Count == 0)
		{
			if (possibleColours.Count == 0)
				virusColours.Add(1);
			else
				virusColours.Add(possibleColours[0]);
		}

		possibleColours.Sort();
		virusColours.Sort();

		// replace virusColours and possibleColours values with overrideCustomLevelColours if present
		if (CommonGameSettings.CurrentThemeHasOverrideCustomLevelColours)
		{
			Godot.Collections.Array<int> newColours = CommonGameSettings.CurrentThemeOverrideCustomLevelColours;

			ReplacePossibleColours(newColours);

			for (int i = 0; i < virusColours.Count; i++)
			{
				if (originalPossibleColours.Contains(virusColours[i]))
					virusColours[i] = possibleColours[originalPossibleColours.IndexOf(virusColours[i])];
			}
		}

		// randomise next pill colours based of possibleColours
		pillMan.RandomiseNextPillColours();
		if (PlayerGameSettings.IsUsingPowerUps)
			PowerUpMeter.QueueNewPowerUp();

		// If official level, use #-# (pack-lvl) number format for level label
		if (CommonGameSettings.IsOfficialCustomLevel)
		{
			int packNo = (CommonGameSettings.CustomLevelID / GameConstants.levelPackLevelCount) + 1;
            int lvlNo = (CommonGameSettings.CustomLevelID % GameConstants.levelPackLevelCount) + 1;
            uiMan.SetLevelLabel(packNo, lvlNo);
		}
		// Else, blank out level label number
		else
			uiMan.SetLevelLabel(-1);

		// Create viruses of each possible colour in the virus ring/magnifying glass
		if (VirusRing != null)
		{
			VirusRing.CreateViruses(virusColours);
		}
		
		// Create tiles based of customLevelTiles data in a random order
		List<Vector2I> remainingTilePositions = new List<Vector2I>(customLevelTiles.Keys);

		while (remainingTilePositions.Count > 0)
		{
			int index = localRng.RandiRange(0, remainingTilePositions.Count - 1);

			Vector2I pos = remainingTilePositions[index];

			int colour = customLevelTiles[pos].colour;
			Vector2I atlas = customLevelTiles[pos].atlas;

			// if possible colours have been replaced, change tile colour
			if (HaveColoursBeenReplaced && colour > 0)
			{
				int newColour = possibleColours[originalPossibleColours.IndexOf(colour)];
				atlas += Vector2I.Down * (newColour - colour);
				colour = newColour;
			}

			jarTiles.SetCell(pos, customLevelTiles[pos].sourceID, atlas);

			int sourceID = jarTiles.GetCellSourceId(pos);

			if (sourceID == GameConstants.virusSourceID)
			{
				virusesRemaining.Add(pos, colour);
				uiMan.SetVirusLabel(virusesRemaining.Count);
			}
			else if (sourceID == GameConstants.powerUpSourceID)
			{
				frozenPowerUps.Add(pos);
			}
			else if (sourceID == GameConstants.objectSourceID && ShouldTileBeForeground(pos))
			{
				jarTiles.SetCell(pos, -1);
				foregroundTiles.SetCell(pos, customLevelTiles[pos].sourceID, atlas);
			}

			remainingTilePositions.RemoveAt(index);
			
			if (!IsInEditorScene)
				await Task.Delay(1000 / virusGenerationSpeed);
		}

		uiMan.SetVirusLabel(virusesRemaining.Count);
		
		GameMan.IndicateFinishedFillingJar();
	}

	// updates the size of the jar based on the jar size set
	public void UpdateJarSize()
	{
        jarOrigin.X = -JarSize.X / 2;
        uiMan.UpdateJarSpriteSize();
    }

	public async void GenerateViruses()
	{
        if (virusLevel < 0)
			virusLevel = PlayerGameSettings.InitialVirusLevel;

		uiMan.SetLevelLabel(virusLevel);

		int virusCount = (virusLevel + 1) * 4;

		// adjust virus count based on jar size
		float virusCountModifier = JarSize.X / (float)baseJarSize.X;
        virusCount = Mathf.FloorToInt(virusCount * virusCountModifier);

        List<Vector2I> possibleCells = new List<Vector2I>();
		List<int> virusColours = new List<int>();

		virusesRemaining.Clear();
		possibleColours.Clear();

		int gap = 6;
		if (virusLevel > 18)
			gap = 3;
		else if (virusLevel > 16)
			gap = 4;
		else if (virusLevel > 14)
			gap = 5;
	

		// possible positions the viruses could spawn
		for (int i = gap; i < JarSize.Y; i++)
		{
			for (int j = 0; j < JarSize.X; j++)
			{
				possibleCells.Add(jarOrigin + new Vector2I(j, i));
			}
		}

		// create list of each virus colour to use
		int firstVirus = localRng.RandiRange(0, PlayerGameSettings.ColourCount - 1);
		for (int i = 0; i < virusCount; i++)
		{
			int colour = PlayerGameSettings.ChosenColours[(firstVirus + i) % PlayerGameSettings.ColourCount];
			virusColours.Add(colour);

			if (!possibleColours.Contains(colour))
				possibleColours.Add(colour);
		}

		// randomise next pill colours based of possibleColours
		pillMan.RandomiseNextPillColours();
		if (PlayerGameSettings.IsUsingPowerUps)
			PowerUpMeter.QueueNewPowerUp();

		// Create viruses of each possible colour in the virus ring/magnifying glass
		if (VirusRing != null)
			VirusRing.CreateViruses(possibleColours);

		for (int i = 0; i < virusCount; i++)
		{
			if (possibleCells.Count == 0)
				break;
			
			int posIndex = localRng.RandiRange(0, possibleCells.Count - 1);
			int virusIndex = localRng.RandiRange(0, virusColours.Count - 1);

			Vector2I pos = possibleCells[posIndex];
			int virusColour = virusColours[virusIndex];

			// if this pos isn't valid, find another valid pos in this row
			if (!ValidVirusPosition(pos, virusColour))
			{
				bool posValid = false;
				Vector2I potentialPos = pos;

				// search left
				while (potentialPos.X > jarOrigin.X)
				{
					potentialPos.X--;

					// skip if virus is already here
					if (virusesRemaining.ContainsKey(potentialPos))
						continue;

					posValid = ValidVirusPosition(potentialPos, virusColour);
					if (posValid)
						break;
				}

				// search right
				if (!posValid)
				{
					potentialPos = pos;

					while (potentialPos.X < jarOrigin.X + JarSize.X - 1)
					{
						potentialPos.X++;

						// skip if virus is already here
						if (virusesRemaining.ContainsKey(potentialPos))
							continue;

						posValid = ValidVirusPosition(potentialPos, virusColour);
						if (posValid)
							break;
					}
				}

				// if position found...
				if (posValid)
				{
					// set pos to potentialPos
					pos = potentialPos;
					// if the pos found was also in possibleCells, remove it from there
					if (possibleCells.Contains(pos))
						possibleCells.Remove(pos);
				}
				// if not found, remove pos from possibleCells and the virus colour
				else
				{
					possibleCells.RemoveAt(posIndex);
					virusColours.RemoveAt(virusIndex);
					continue;
				}
			}
			else
				possibleCells.RemoveAt(posIndex);


			virusColours.RemoveAt(virusIndex);
			
			jarTiles.SetCell(pos, GameConstants.virusSourceID, Vector2I.Down * (virusColour - 1));

			virusesRemaining.Add(pos, virusColour);

			uiMan.SetVirusLabel(virusesRemaining.Count);

			await Task.Delay(1000 / virusGenerationSpeed);
		}

		GameMan.IndicateFinishedFillingJar();
	}

	public void StartGame()
	{
		pillMan.ThrowNextPill();
	}

	// whether or not this is a valid positon a virus can generate in
	// if pos is in a matchung row/column of 3 or more viruses, its invalid
	private bool ValidVirusPosition(Vector2I pos, int colour)
	{
		int streakLeft = CheckForStreakFromPos(pos, Vector2I.Left, colour);
		int streakRight = CheckForStreakFromPos(pos, Vector2I.Right, colour);

		int streakUp = CheckForStreakFromPos(pos, Vector2I.Up, colour);
		int streakDown = CheckForStreakFromPos(pos, Vector2I.Down, colour);
		
		return streakLeft + streakRight + 1 < 3 && streakUp + streakDown + 1 < 3;
	}

	private bool IsTileSingle(Vector2I pos)
	{
		return jarTiles.GetCellSourceId(pos) != GameConstants.pillSourceID || jarTiles.GetCellAtlasCoords(pos).X == PillConstants.atlasSingle;
	}

	public bool IsTilePresent(Vector2I pos)
	{
		return jarTiles.GetCellSourceId(pos) != -1;
	}

	public int GetTileColour(Vector2I pos)
	{
		TileData data = jarTiles.GetCellTileData(pos);

		if (data == null)
			return -1;

		int colour = (int)data.GetCustomData("Colour");

		if (jarTiles.GetCellSourceId(pos) != GameConstants.powerUpSourceID && colour == 0)
			return -1;

		return colour;
	}

	public bool DoesTileCauseRebound(Vector2I pos) { return IsObjectWithAttribute(pos, "CauseRebound"); }
	public bool IsTileUnbreakable(Vector2I pos) { return IsObjectWithAttribute(pos, "Unbreakable"); }
	public bool ShouldTileBeForeground(Vector2I pos)  { return IsObjectWithAttribute(pos, "Foreground"); }
	public bool IsTileHazard(Vector2I pos)  { return IsObjectWithAttribute(pos, "Hazard"); }

	public bool IsObjectWithAttribute(Vector2I pos, string attribute)
	{
		if (foregroundTiles.GetCellSourceId(pos) == GameConstants.objectSourceID)
		{
			return (bool)foregroundTiles.GetCellTileData(pos).GetCustomData(attribute);
		}
		else if (jarTiles.GetCellSourceId(pos) == GameConstants.objectSourceID)
		{
			return (bool)jarTiles.GetCellTileData(pos).GetCustomData(attribute);
		}
		else
		{
			return false;
		}
	}

	private Vector2I GetConnectedSegment(Vector2I pos)
	{
		return GetConnectedSegment(pos, jarTiles.GetCellSourceId(pos), jarTiles.GetCellAtlasCoords(pos).X);
	}

	private Vector2I GetConnectedSegment(Vector2I pos, int sourceID, int atlasX)
	{
		if (sourceID != GameConstants.pillSourceID)
			return pos;
			
		Vector2I connectedPos = pos;
		
		if (atlasX == PillConstants.atlasSingle)
			return connectedPos;
		else if (atlasX == PillConstants.atlasLeft)
			connectedPos += Vector2I.Right;
		else if (atlasX == PillConstants.atlasRight)
			connectedPos += Vector2I.Left;
		else if (atlasX == PillConstants.atlasTop)
			connectedPos += Vector2I.Down;
		else if (atlasX == PillConstants.atlasBottom)
			connectedPos += Vector2I.Up;
		
		return connectedPos;
	}

	private void AddTileToFall(Vector2I pos)
	{
		if (!tilesToFall.ContainsKey(pos.Y))
			tilesToFall.Add(pos.Y, new List<int>());

		if (!tilesToFall[pos.Y].Contains(pos.X))
			tilesToFall[pos.Y].Add(pos.X);
	}

	// adds tile to tilesToDestroy - returns true or false depending on whether the position given was already present in tilesToDestroy or not
	// (true = was added, didn't already exist)
	// (false = already exists)
	private bool AddTileToDestroy(Vector2I pos)
	{
		if (!tilesToDestroy.ContainsKey(pos.Y))
			tilesToDestroy.Add(pos.Y, new List<int>());

		if (!tilesToDestroy[pos.Y].Contains(pos.X))
		{
			tilesToDestroy[pos.Y].Add(pos.X);

			if (virusesRemaining.ContainsKey(pos))
				destructionContainsVirus = true;
			
			return true;
		}

		return false;
	}

	// adds tile to destroyedTiles - returns true or false depending on whether the position given was already present in destroyedTiles or not
	// (true = was added, didn't already exist)
	// (false = already exists)
	private bool AddDestroyedTile(Vector2I pos)
	{
		if (!destroyedTiles.ContainsKey(pos.Y))
			destroyedTiles.Add(pos.Y, new List<int>());

		if (!destroyedTiles[pos.Y].Contains(pos.X))
		{
			destroyedTiles[pos.Y].Add(pos.X);

			if (virusesRemaining.ContainsKey(pos))
				destructionContainsVirus = true;
			
			return true;
		}

		return false;
	}

	private int CheckForStreakFromPos(Vector2I origPos, Vector2I dir, int origColour)
	{
		int checkColour = origColour;
		
		// if colour is 0, then origPos is a rainbow tile which can be matched with any colour, so set it to match the colour beside it in this streak check
		if (checkColour == 0)
		{
			int neighbourColour = GetTileColour(origPos + dir);
			if (neighbourColour != -1)
				checkColour = neighbourColour;
		}
		
		int streak = 0;
		Vector2I neighbourPos = origPos;

		while (true)
		{
			neighbourPos += dir;

			int neighbourColour = GetTileColour(neighbourPos);

			// If still checking for rainbow colour and neightbouring isn't rainbow, update checkColour to match it
			if (checkColour == 0 && neighbourColour > 0)
				checkColour = neighbourColour;

			if ((neighbourColour == checkColour || neighbourColour == 0) && (tilesToFall.Count == 0 || !TilesToFallContainsPos(neighbourPos)))
				streak++;
			else
				break;
		}

		return streak;
	}

	private void CheckForLinesToDestroy(Vector2I origPos)
	{
		int origColour = GetTileColour(origPos);
		
		int streakLeft = CheckForStreakFromPos(origPos, Vector2I.Left, origColour);
		int streakRight = CheckForStreakFromPos(origPos, Vector2I.Right, origColour);

		int streakUp = CheckForStreakFromPos(origPos, Vector2I.Up, origColour);
		int streakDown = CheckForStreakFromPos(origPos, Vector2I.Down, origColour);

		if (streakLeft + streakRight + 1 >= PlayerGameSettings.MinStreakLength)
		{
			bool isNewLine = false;

			if (!tilesToDestroy.ContainsKey(origPos.Y))
				tilesToDestroy.Add(origPos.Y, new List<int>());

			for (int i = -streakLeft; i <= streakRight; i++)
			{
				Vector2I pos = origPos + Vector2I.Right * i;

				if (!tilesToDestroy[pos.Y].Contains(pos.X))
				{
					tilesToDestroy[pos.Y].Add(pos.X);
					isNewLine = true;

					if (virusesRemaining.ContainsKey(pos))
						destructionContainsVirus = true;
				}
			}

			if (isNewLine)
				IncrementLineCombo(origColour);
		}

		if (streakUp + streakDown + 1 >= PlayerGameSettings.MinStreakLength)
		{
			bool isNewLine = false;

			for (int i = -streakUp; i <= streakDown; i++)
			{
				Vector2I pos = origPos + Vector2I.Down * i;

				if (AddTileToDestroy(pos))
					isNewLine = true;
			}

			if (isNewLine)
				IncrementLineCombo(origColour);
		}
	}

	public void AddPillToTilemap(Pill pill)
	{
		pill.Visible = false;
		destroyTimer = 0;

		Vector2I centrePos = pill.GridPos;

        List<Vector2I> positions;

        // If power-up, only place one tile
        if (pill.IsPowerUp)
		{
			// If rainbow and not on a hazard, auto-activate power-up
			if (pill.CentreSegmentColour == 0 && !IsTileHazard(centrePos))
			{
				SfxMan.Play("Land");
				ActivatePowerUp(pill.CurrentPowerUp, pill.CentreSegmentColour, centrePos);

				SetProcess(true);
				return;
			}

			jarTiles.SetCell(centrePos, GameConstants.powerUpSourceID, pill.CentreTextureCoords);

            positions = new List<Vector2I> { centrePos };
        }
		else
		{
            positions = new List<Vector2I>();
            Dictionary<Vector2I, JarTileData> tilesToAdd = new Dictionary<Vector2I, JarTileData>();

			// only add in-bounds tiles to tilesToAdd
            foreach (Vector2I localPos in pill.RotatedTiles.Keys)
            {
                Vector2I realPos = localPos + pill.GridPos;
                // if position is at or below the top of the jar...
                if (realPos.Y >= jarOrigin.Y)
                {
                    JarTileData tileData = pill.RotatedTiles[localPos];

					// if in the top row of jar AND bottom pill segment, change to single pill segment, since connected segment won't exist
					if (realPos.Y == jarOrigin.Y && tileData.atlas.X == PillConstants.atlasBottom)
					{
                        tileData.atlas.X = PillConstants.atlasSingle;
                    }

                    tilesToAdd.Add(realPos, tileData);
                    positions.Add(realPos);
                }
            }

			// place tiles from tilesToAdd
            foreach (Vector2I pos in tilesToAdd.Keys)
            {
				jarTiles.SetCell(pos, tilesToAdd[pos].sourceID, tilesToAdd[pos].atlas);
            }
		}
		
		tilesToDestroy.Clear();
		destroyedTiles.Clear();
		tilesToFall.Clear();

		// check for hazards and destroy segment(s) if needed
		for (int i = positions.Count - 1; i >= 0; i--)
		{
			if (IsTileHazard(positions[i]))
			{
				SfxMan.Play("VirusStunLand");
				DestroyTile(positions[i], true);
                positions.RemoveAt(i);
            }
		}

		// check for lines
		foreach (Vector2I pos in positions)
		{
			if (jarTiles.GetCellTileData(pos) != null)
				CheckForLinesToDestroy(pos);
		}

		// check for segments which should fall (ignoring ones to destroy)
		foreach (Vector2I pos in positions)
		{
			// continue if cell below is solid OR tile already going to fall/destroy
			if (!IsCellFree(pos + Vector2I.Down) || TilesToFallContainsPos(pos) || TilesToDestroyContainsPos(pos))
                continue;

			// get connected segment pos (same as current pos is single)
            Vector2I connectedPos = GetConnectedSegment(pos);

			// will always fall if its a single
			if (pos == connectedPos)
			{
				AddTileToFall(pos);
			}
			// if its a double horizontal segment, only fall if not grounded on the connected segment and the segment below isn't itself
			else
			{
				Vector2I conDownPos = connectedPos + Vector2I.Down;

				if (conDownPos == pos || IsCellFree(connectedPos + Vector2I.Down))
				{
					AddTileToFall(pos);
					
					if (!TilesToDestroyContainsPos(connectedPos))
						AddTileToFall(connectedPos);
				}
			}
		}

		if (tilesToDestroy.Count == 0 && tilesToFall.Count == 0)
		{
			SfxMan.Play("Land");

			if (queuedJunkSegments.Count > 0)
			{
				CreateJunkSegments();
				SetProcess(true);
			}
			else
			{
				pillMan.ThrowNextPill();
			}
		}
		else
		{
			if (tilesToDestroy.Count != 0)
			{
				destroyRow = tilesToDestroy.Keys.Max();
				
				if (destructionContainsVirus)
					SfxMan.Play("VirusMatch");
				else
					SfxMan.Play("PillMatch");
				
				IncrementMatchCombo();
			}
			else
				autoFallTimer = 1;

			SetProcess(true);
		}
	}

	private bool TilesToDestroyContainsPos(Vector2I pos)
	{
		if (tilesToDestroy.ContainsKey(pos.Y) && tilesToDestroy[pos.Y].Contains(pos.X))
			return true;
		else
			return false;
	}

	private bool DestroyedTilesContainsPos(Vector2I pos)
	{
		if (destroyedTiles.ContainsKey(pos.Y) && destroyedTiles[pos.Y].Contains(pos.X))
			return true;
		else
			return false;
	}

	private bool TilesToFallContainsPos(Vector2I pos)
	{
		return tilesToFall.ContainsKey(pos.Y) && tilesToFall[pos.Y].Contains(pos.X);
	}

	// whether the position given is empty, or will be updated (e.g. will fall, be destroyed or is already destroyed/popped) 
	private bool IsPosEmptyOrWillUpdate(Vector2I pos)
	{
		return IsCellFree(pos) || TilesToDestroyContainsPos(pos) || DestroyedTilesContainsPos(pos) || TilesToFallContainsPos(pos);
	}
	
	// recursive function to make the segement at pos fall, as well as any connecting segments (aka is double) and any on top of the segment(s)
	// doesn't affect viruses
	private void RecursiveFall(Vector2I pos)
	{
		int sourceID = jarTiles.GetCellSourceId(pos);

		// if tile is not nothing, not updating (falling, destroyed, to-be-destroyed) and either a pill segment or non-frozen power-up, check if it can fall and make it fall is so
		if (!IsPosEmptyOrWillUpdate(pos) && (sourceID == GameConstants.pillSourceID || (sourceID == GameConstants.powerUpSourceID && !frozenPowerUps.Contains(pos))))
		{
			Vector2I connectedPos = GetConnectedSegment(pos);

			// will always fall if its a single or vertical segment
			if (IsTileSingle(pos) || pos.X == connectedPos.X)
			{
				AddTileToFall(pos);
				RecursiveFall(pos + Vector2I.Up);
			}
			// if its a double horizontal segment, only fall if not grounded on the connected segment
			else
			{
				Vector2I conDownPos = connectedPos + Vector2I.Down;

				if (pos.X != connectedPos.X && IsPosEmptyOrWillUpdate(conDownPos))
				{
					AddTileToFall(pos);
					RecursiveFall(pos + Vector2I.Up);
					AddTileToFall(connectedPos);
					RecursiveFall(connectedPos + Vector2I.Up);
				}
			}
		}
	}

	private void IncrementVirusCombo()
	{
		if (!isJunkFalling)
			virusCombo++;
		score += Math.Clamp(virusCombo, 1, 6) * (PlayerGameSettings.SpeedLevel + 1) * 100;
	}
	private void IncrementMatchCombo()
	{
		if (isJunkFalling)
			return;

		matchCombo++;

		if (PlayerGameSettings.IsUsingPowerUps)
			PowerUpMeter.IncrementLevel(2);
	}

	private void IncrementLineCombo(int colour)
	{
		if (isJunkFalling)
			return;
			
		lineCombo++;

		if (matchCombo == 0)
			initialLineComboColours.Add(colour);

		lineComboColours.Add(colour);
	}

	private void CompleteAutoFallingAndDestruction()
	{
		if (lineCombo > 3)
			SfxMan.Play("ChainX3");
		else if (lineCombo > 2)
			SfxMan.Play("ChainX3");
		else if (lineCombo > 1)
			SfxMan.Play("ChainX2");
	
		if (CommonGameSettings.IsMultiplayer && CommonGameSettings.MultiplayerUseJunkPills && lineCombo > 1)
		{
			// Get random player which isn't this player
			int receivingPlayer = GD.RandRange(0, CommonGameSettings.PlayerCount - 2);

			if (receivingPlayer == PlayerID)
			{
				receivingPlayer = (receivingPlayer + 1) % CommonGameSettings.PlayerCount;
			}

			// convert combo colours to colour indexes (since colours could vary between players)

			List<int> junkColourIndexes = new List<int>();
			
			for (int i = 0; i < Mathf.Min(lineComboColours.Count, maxJunkSegments); i++)
			{
				int colour = lineComboColours[i];
				junkColourIndexes.Add(possibleColours.IndexOf(colour));
			}

			// Send junk colours to receiving player
			GameMan.SendJunkSegments(receivingPlayer, junkColourIndexes);
		}

		autoFallTimer = 0;

		ResetCombo();

		if (queuedJunkSegments.Count > 0)
		{
			isJunkFalling = true;
			CreateJunkSegments();
		}
		else
		{
			isJunkFalling = false;
			SetProcess(false);
			pillMan.ThrowNextPill();
		}
	}

	private void ActivatePowerUp(PowerUp pwr, int colour, Vector2I pos)
	{
		if (DisablePowerUpSpawning)
			return;
		
		BasePowerUp powerUp = powerUpPrefabs.GetPowerUpPrefab(pwr).Instantiate<BasePowerUp>();

		powerUp.InitialGridPos = pos;
		powerUp.Colour = colour;
		powerUp.JarMan = this;
		powerUp.SfxMan = SfxMan;
		powerUp.Texture = ((TileSetAtlasSource)jarTiles.TileSet.GetSource(GameConstants.powerUpSourceID)).Texture;

		jarTiles.GetParent().AddChild(powerUp);
		powerUp.GetParent().MoveChild(powerUp, previewTiles.GetIndex());

		powerUp.GlobalPosition = TilemapGlobalPos + pos * JarCellSize;
		powerUp.GlobalPosition += JarCellSize / 2;

		activePowerUps.Add(powerUp);
	}

	public void SetActivePowerUpsPauseState(bool pause)
	{
		foreach (BasePowerUp powerUp in activePowerUps)
		{
			powerUp.ProcessMode = pause ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit;
			powerUp.Visible = !pause;
		}
	}

	public void SetVirusRingPauseState(bool pause)
	{
		if (VirusRing != null)
			VirusRing.SetPauseState(pause);
	}

	public void FinishPowerUp(BasePowerUp pwr)
	{
		activePowerUps.Remove(pwr);
	}

	private void CreateJunkSegments()
	{
		isJunkFalling = true;

		autoFallTimer = 0;

		// Get possible spawn locations for junk segments
		List<int> possiblePositions = new List<int>();

		for (int i = 0; i < JarSize.X; i++)
		{
			Vector2I pos = jarOrigin + Vector2I.Right * i;

			if (!IsTilePresent(pos))
				possiblePositions.Add(pos.X);

		}

		// Get list of segments to create
		List<int> segmentsToCreate = queuedJunkSegments[0];

		// Set the amount of segments to create to the smallest between the segmentsToCreate and possiblePositions counts
		int createCount = Mathf.Min(segmentsToCreate.Count, possiblePositions.Count);

		// Create junk segments using randomised positions in possiblePositions 
		for (int i = 0; i < createCount; i++)
		{
			// Get position and remove from segmentsToCreate/queuedJunkSegments
			int posIndex = GD.RandRange(0, possiblePositions.Count - 1);
			Vector2I pos = new Vector2I(possiblePositions[posIndex], jarOrigin.Y);

			possiblePositions.Remove(pos.X);

			// Get index of colour (not the colour itself as colours could vary between players)
			int colourIndex = segmentsToCreate[i];

			// if the colour index is out of range of this player's possibleColours, fix it to it is
			if (colourIndex > possibleColours.Count - 1)
			{
				colourIndex = colourIndex % possibleColours.Count;
			}

			// Set cell at pos to single pill segment of colour based on colourIndex
			jarTiles.SetCell(pos, GameConstants.pillSourceID, new Vector2I(PillConstants.atlasSingle, possibleColours[colourIndex] - 1));

			// Add segment to fall list
			AddTileToFall(pos);
		}

		// Remove group of junk segments just created from queuedJunkSegments
		queuedJunkSegments.RemoveAt(0);
	}

	// Destroys tile at given position - returns true if pos is not empty, aka something gets destroyed
	public bool DestroyTile(Vector2I pos, bool forceInsta = false)
	{
		// Add pos to destroyedTiles (if not forceInsta) - return if pos is already in destroyedTiles or pos is empty
		if (jarTiles.GetCellSourceId(pos) == -1 || (forceInsta ? false : !AddDestroyedTile(pos)))
			return false;

		Vector2I atlas = jarTiles.GetCellAtlasCoords(pos);
		int sourceID = jarTiles.GetCellSourceId(pos);
		int colour = GetTileColour(pos);
		Vector2I poppedAtlas;

		// If tile falling, remove it from fall list
		if (TilesToFallContainsPos(pos))
		{
			tilesToFall[pos.Y].Remove(pos.X);
		}

		// if forceInsta, just delete cell right away if object, don't bother with "popping" the tile
		if (forceInsta)
		{
			jarTiles.SetCell(pos, -1);

			// if this a connect pill segment, change the connected segment to a non-connected one
			if (sourceID == GameConstants.pillSourceID)
			{
				SeparateConnectedSegment(pos, sourceID, atlas);
			}
			
			return true;
		}
		else if (sourceID == GameConstants.pillSourceID)
			poppedAtlas.X = pillPoppedAtlasX;
		else if (sourceID == GameConstants.virusSourceID)
			poppedAtlas.X = virusPoppedAtlasX;
		else if (sourceID == GameConstants.powerUpSourceID)
			poppedAtlas.X = powerUpPoppedAtlasX;
		// if other sourceID, just delete cell right away if object, don't bother with "popping" the tile
		else
		{
			jarTiles.SetCell(pos, -1);
			return true;
		}
			
		poppedAtlas.Y = sourceID == GameConstants.powerUpSourceID ? colour : colour - 1;

		if (sourceID == GameConstants.virusSourceID)
		{
			virusesRemaining.Remove(pos);
			uiMan.SetVirusLabel(virusesRemaining.Count);
			if (VirusRing != null)
				VirusRing.StunVirus(colour, !virusesRemaining.ContainsValue(colour));

			IncrementVirusCombo();
		}
		else if (sourceID == GameConstants.powerUpSourceID)
		{
			if (frozenPowerUps.Contains(pos))
				frozenPowerUps.Remove(pos);
				
			ActivatePowerUp((PowerUp)atlas.X, colour, pos);
		}
		
		jarTiles.SetCell(pos, sourceID, poppedAtlas);

		// if this a connect pill segment, change the connected segment to a non-connected one
		if (sourceID == GameConstants.pillSourceID)
			SeparateConnectedSegment(pos, sourceID, atlas);

		return true;
	}

	private void SeparateConnectedSegment(Vector2I pos, int sourceID, Vector2I atlas)
	{
		Vector2I connectedPos = GetConnectedSegment(pos, sourceID, atlas.X);

		if (connectedPos != pos)
		{
			jarTiles.SetCell(connectedPos, GameConstants.pillSourceID, new Vector2I(PillConstants.atlasSingle, GetTileColour(connectedPos) - 1));
			
			// if pos below is empty or will update, do RecursiveFall for the connected pos
			if (IsPosEmptyOrWillUpdate(connectedPos + Vector2I.Down))
			{
				RecursiveFall(connectedPos);
			}
		}
	}

	public void DestroyAllPillSegments(int colour)
	{
		bool destroyAll = colour == 0;

		for (int y = jarOrigin.Y; y < jarOrigin.Y + JarSize.Y; y++)
		{
			for (int x = jarOrigin.X; x < jarOrigin.X + JarSize.X; x++)
			{
				Vector2I pos = new Vector2I(x, y);

				if (jarTiles.GetCellSourceId(pos) == GameConstants.pillSourceID && (destroyAll || GetTileColour(pos) == colour))
				{
					DestroyTile(pos);
				}
			}
		}
	}

	public void DestroyAllViruses(int colour)
	{
		// If rainbow, destroy half of the total viruses
		if (colour == 0)
		{
			List<Vector2I> viruses = new List<Vector2I>(virusesRemaining.Keys);

			if (viruses.Count == 0)
				return;

			int halfCount = viruses.Count - viruses.Count / 2;

			if (halfCount == 0)
				halfCount = viruses.Count;

			for (int i = 0; i < halfCount; i++)
			{
				DestroyTile(viruses[i]);
			}
		}
		// Else, destroy all viruses of given colour
		else
		{
			List<Vector2I> viruses = new List<Vector2I>(virusesRemaining.Keys);

			foreach (Vector2I pos in viruses)
			{
				if (virusesRemaining[pos] == colour)
				{
					DestroyTile(pos);
				}
			}
		}
	}

	private void UpdateDestruction(double delta)
	{
		// if there were no tiles to be destroyed or they have all been destroyed, remove all the "popped" tiles
		if (tilesToDestroy.Count == 0 || destroyRow < tilesToDestroy.Keys.Min())
		{
			if (activePowerUps.Count != 0 && virusesRemaining.Count != 0)
			{
				if (destroyTimer > 0)
					destroyTimer -= delta * destroyDisappearSpeed;
				return;
			}
			
			// reset destructionContainsVirus to default/false in prep for next destruction
			destructionContainsVirus = false;

			// Only update score label if in single player
			if (CommonGameSettings.PlayerCount == 1)
			{
				uiMan.SetScoreLabel(score);
				if (score > HighScoreList.GetGameRuleHighScore())
					uiMan.SetHighScoreLabel(score);
			}

			if (virusesRemaining.Count() == 0 && virusCombo > 0)
			{
				Win();
				return;
			}

			destroyTimer -= delta * destroyDisappearSpeed;

			if (destroyTimer <= 0 && activePowerUps.Count == 0)
			{
				int minY = destroyedTiles.Keys.Min();
				int maxY = destroyedTiles.Keys.Max();

				for (int y = maxY; y >= minY; y--)
				{
					if (!destroyedTiles.ContainsKey(y))
						continue;

					for (int i = 0; i < destroyedTiles[y].Count; i++)
					{
						Vector2I pos = new Vector2I(destroyedTiles[y][i], y);
						jarTiles.SetCell(pos, -1);
						
						// if pos is in tilesToFall, remove it from there - otherwise an empty tile will fall
						if (TilesToFallContainsPos(pos))
						{
							tilesToFall[pos.Y].Remove(pos.X);

							if (tilesToFall[pos.Y].Count == 0)
								tilesToFall.Remove(pos.Y);
						}

						Vector2I abovePos = pos + Vector2I.Up;
						
						// do RecursiveFall for the position above if isn't destroyed
						if (!DestroyedTilesContainsPos(abovePos))
							RecursiveFall(abovePos);
					}
				}
				tilesToDestroy.Clear();
				destroyedTiles.Clear();
				autoFallTimer = 0;
				
				if (tilesToFall.Count == 0)
					CompleteAutoFallingAndDestruction();
			}
			return;
		}
		// wait if destroy timer hasn't finished
		if (destroyTimer > 0)
		{
			destroyTimer -= delta * destroyRowSpeed;
			return;
		}
		// destroy row specified by destroyRow
		for (int i = 0; i < tilesToDestroy[destroyRow].Count; i++)
		{
			Vector2I pos = new Vector2I(tilesToDestroy[destroyRow][i], destroyRow);
			DestroyTile(pos);
		}
		destroyRow--;
		if (!tilesToDestroy.ContainsKey(destroyRow) && destroyRow >= tilesToDestroy.Keys.Min())
		{
			do
			{
				destroyRow--;
			} while (!tilesToDestroy.ContainsKey(destroyRow)); 
		}
		destroyTimer = 1;
	}

	private bool IsCellFullAndNotFalling(Vector2I pos)
	{
		return !IsCellFree(pos) && !TilesToFallContainsPos(pos);
	}

	private void UpdateTilesToFall(double delta)
	{
		// wait if autoFallTimer hasn't finished
		if (autoFallTimer > 0)
		{
			autoFallTimer -= delta * ((IsActionPressed("SoftDrop") || !CommonGameSettings.ManualAutoFallSpeedUp) ? pillMan.SoftDropSpeed : PlayerGameSettings.AutoFallSpeed);
			return;
		}

		int minY = tilesToFall.Keys.Min();
		int maxY = tilesToFall.Keys.Max();
		
		// tiles to get insta destroyed, any tiles touching a hazard get put here
		List<Vector2I> tilesToInstaDestroy = new List<Vector2I>();

		// find landed tiles AND tiles about to hit hazard (aka to be insta destroyed)
		for (int y = maxY; y >= minY; y--)
		{
			if (!tilesToFall.ContainsKey(y))
				continue;
			
			for (int i = tilesToFall[y].Count - 1; i >= 0; i--)
			{
				Vector2I pos = new Vector2I(tilesToFall[y][i], y);
				Vector2I connectedPos = GetConnectedSegment(pos);
				bool landed = false;
				
				if (connectedPos == pos + Vector2I.Left || connectedPos == pos + Vector2I.Right)
				{
					if (IsCellFullAndNotFalling(pos + Vector2I.Down) || IsCellFullAndNotFalling(connectedPos + Vector2I.Down))
					{
						tilesToFall[y].RemoveAt(i);
						uncheckedLandedTiles.Add(pos);
						landed = true;
					}
				}
				else if (IsCellFullAndNotFalling(pos + Vector2I.Down))
				{
					tilesToFall[y].RemoveAt(i);
					uncheckedLandedTiles.Add(pos);
					landed = true;
				}

				// if tile would still fall but would hit a hazard, insta-destroy it
				if (!landed)
				{
					// insta-destroy tile if hazard
					if (IsTileHazard(pos + Vector2I.Down))
					{
						tilesToInstaDestroy.Add(pos);
					}
				}
			}

			if (tilesToFall[y].Count == 0)
				tilesToFall.Remove(y);
		}

		// insta destroy any tiles hit by hazard
		if (tilesToInstaDestroy.Count > 0)
		{
			SfxMan.Play("VirusStunLand");
			for (int i = 0; i < tilesToInstaDestroy.Count; i++)
			{
				DestroyTile(tilesToInstaDestroy[i], true);
			}
		}

		// do line checks for landed tiles
		if (uncheckedLandedTiles.Count > 0 && PlayerGameSettings.ImpatientMatching || tilesToFall.Count == 0)
		{
			tilesToDestroy.Clear();
			destroyedTiles.Clear();

			foreach (Vector2I pos in uncheckedLandedTiles)
			{
				if (jarTiles.GetCellTileData(pos) != null)
					CheckForLinesToDestroy(pos);
			}

			uncheckedLandedTiles.Clear();

			if (tilesToDestroy.Count > 0)
			{
				autoFallTimer = 0;
				destroyTimer = 0;
				destroyRow = tilesToDestroy.Keys.Max();

				if (destructionContainsVirus)
					SfxMan.Play("VirusMatch");
				else
					SfxMan.Play("PillMatch");
				
				IncrementMatchCombo();

				return;
			}
			else if (tilesToFall.Count == 0)
			{
				CompleteAutoFallingAndDestruction();
				return;
			}

		}

		// do CompleteAutoFallingAndDestruction if no falling/popped tiles present
		if (tilesToDestroy.Count == 0 && tilesToFall.Count == 0)
		{
			CompleteAutoFallingAndDestruction();
			return;
		}

		if (IsActionPressed("SoftDrop") && CommonGameSettings.ManualAutoFallSpeedUp)
			SfxMan.Play("FastFall");
		else
			SfxMan.Play("Fall");

		// move mid-air tiles down
		for (int y = maxY; y >= minY; y--)
		{
			if (!tilesToFall.ContainsKey(y))
				continue;

			for (int i = tilesToFall[y].Count - 1; i >= 0; i--)
			{
				Vector2I pos = new Vector2I(tilesToFall[y][i], y);
				Vector2I atlas = jarTiles.GetCellAtlasCoords(pos);
				int sourceID = jarTiles.GetCellSourceId(pos);

				jarTiles.SetCell(pos, -1);
				jarTiles.SetCell(pos + Vector2I.Down, sourceID, atlas);
			}
		}

		// shift dictonary y positions
		for (int y = maxY; y >= minY; y--)
		{
			if (!tilesToFall.ContainsKey(y))
				continue;

			tilesToFall.Add(y + 1, tilesToFall[y]);
			tilesToFall.Remove(y);
		}
		
		autoFallTimer = 1;
	}

	public void ClearPreviewTiles()
	{
		previewTiles.Clear();
	}

	public void CreateGhostPill(Pill pill, Vector2I centrePos)
	{
		previewTiles.Clear();

		if (pill.IsPowerUp)
		{
			previewTiles.SetCell(centrePos, GameConstants.powerUpSourceID, pill.CentreTextureCoords);
		}
		else
		{
			// place tiles from tilesToAdd
            foreach (Vector2I pos in pill.RotatedTiles.Keys)
            {
				previewTiles.SetCell(pos + centrePos, pill.RotatedTiles[pos].sourceID, pill.RotatedTiles[pos].atlas);
            }
		}
	}

	// Adds the list of junk segment colour indexes to queuedJunkSegments 
	public void AddJunkSegmentsToQueue(List<int> colourIndexes)
	{
		queuedJunkSegments.Add(colourIndexes);
	}

	public override void _Process(double delta)
	{
		if (tilesToDestroy.Count > 0 || destroyedTiles.Count > 0)
			UpdateDestruction(delta);
		else if (tilesToFall.Count > 0)
			UpdateTilesToFall(delta);
		else if (activePowerUps.Count == 0)
			CompleteAutoFallingAndDestruction();
	}

	// Returns whether a cell is empty AND is not out-of-bounds
	public bool IsCellFree(Vector2I pos)
	{
		bool validX = pos.X - jarOrigin.X == Mathf.Clamp(pos.X - jarOrigin.X, 0, JarSize.X - 1);
		bool validY = pos.Y - jarOrigin.Y < JarSize.Y;
		return validX && validY && jarTiles.GetCellTileData(pos) == null;
	}

	public void SetTileVisibity(bool b)
	{
		jarTiles.Visible = b;
		previewTiles.Visible = b;
	}
}
