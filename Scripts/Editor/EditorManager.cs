using Godot;
using System;
using System.Collections.Generic;

public partial class EditorManager : Node
{
    [ExportGroup("Editor-related References")]
    [Export] private PauseManager editorPauseMan;
    [Export] private EditorUndoRedoManager undoRedoMan;
    [Export] private EditorCursor cursor;
    [Export] private EditorBaseSelector drawingToolSelector;
    [Export] private CanvasLayer editorUILayer;
    public EditorBaseSelector DrawingToolSelector { get { return drawingToolSelector; } }
    [Export] private EditorColourSelector colourSelector;
    public EditorColourSelector ColourSelector { get { return colourSelector; } }
    [Export] private EditorTileTypeSelector tileTypeSelector;
    public EditorTileTypeSelector TileTypeSelector { get { return tileTypeSelector; } }

    [ExportGroup("Other References & Resources")]
    [Export] private GameManager gameMan;
    [Export] private GameThemer gameThemer;
    [Export] private SfxManager sfxMan;
    [Export] private MusicManager musicMan;
    [Export] private TouchControlsManager touchControlsMan;
    [Export] private CommonGameSettings commonGameSettings;
    [Export] private UserCustomLevelList userCustomLevelList;
    [Export] private MusicPreviewPlayer musicPreviewPlayer;

    private JarManager jarMan;
    private TileMapLayer jarTiles;
    private TileMapLayer previewTiles;
    private TileMapLayer previewToolTiles;
    public TileMapLayer PreviewToolTiles { get { return previewToolTiles; } }
    private TileMapLayer selectionTileMap;

    private List<Vector2I> selectedTiles = new List<Vector2I>();
    public List<Vector2I> SelectedTiles { get { return selectedTiles; }}
    public bool IsSelectionPresent { get { return selectedTiles.Count != 0; } }
    private Vector2 origGrabbedSelectionPos;
    private Vector2I grabbedSelectionGridOffset;
    private Vector2I lastGrabbedSelectionGridOffset;

    public bool CanPressButtons { get { return !isPaused && !cursor.IsBusy && editorUILayer.Visible; } }
    private bool isPaused = false;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        jarMan = gameMan.Jars[0];

        jarTiles = jarMan.JarTiles;
        previewTiles = jarMan.PreviewTiles;

        previewToolTiles = jarMan.JarTiles.Duplicate() as TileMapLayer;
        jarTiles.GetParent().AddChild(previewToolTiles);
        previewToolTiles.GlobalPosition = jarTiles.GlobalPosition;
        previewToolTiles.Clear();
        
        selectionTileMap = jarMan.JarTiles.Duplicate() as TileMapLayer;
        jarTiles.GetParent().AddChild(selectionTileMap);
        selectionTileMap.Position = jarTiles.Position;
        selectionTileMap.Clear();

        origGrabbedSelectionPos = selectionTileMap.Position;

        jarMan.UIMan.SetHUDVisibility(false);
        jarMan.UIMan.PowerUpMeter.SetVisibility(false);

        if (commonGameSettings.EnableEditorMusic)
            musicPreviewPlayer.SetPreviewMusicToCurrent();
	}

    public void PlayTest()
    {
        if (!CanPressButtons)
            return;

        jarMan.SavePresentColoursToGameSettings();
        
        Input.MouseMode = Input.MouseModeEnum.Hidden;
        cursor.Visible = false;
        cursor.SetProcess(false);
        SetProcess(false);

        GetViewport().GuiReleaseFocus();
        editorUILayer.Visible = false;
        selectionTileMap.Visible = false;
        jarMan.DisablePowerUpSpawning = false;

        jarMan.UIMan.SetHUDVisibility(true);
        gameThemer.UpdateHoldGroup();

        jarMan.CurrentTilesToCustomLevelTiles();
        gameMan.UpdateMusic();
        gameMan.ReplayLevel();

        touchControlsMan.ShowTouchControlsIfAvailable(true);

        GetViewport().GetCamera2D().Zoom = Vector2.One * (commonGameSettings.EnableLargerView ? GameConstants.largerViewZoom : 1);
        jarMan.SetVirusTileAnimationState(commonGameSettings.EnableVirusTileAnimation);
    }

    public void EndPlayTest()
    {
        GetViewport().GetCamera2D().Zoom = Vector2.One;

        jarMan.DisablePowerUpSpawning = true;
        jarMan.DeleteAllPowerUps();
        jarMan.ResetScore();
        jarMan.ResetVirusCount();
        jarMan.PillMan.HideAllPills();
        jarMan.RestoreCustomLevelTilesForEditor();

        jarMan.UIMan.SetHUDVisibility(false);
        jarMan.UIMan.PowerUpMeter.SetVisibility(false);
        jarMan.SetVirusTileAnimationState(false);
        
        musicMan.Stop();

        SetProcess(true);
        cursor.SetProcess(true);

        GetViewport().GuiReleaseFocus();
        editorUILayer.Visible = true;
        selectionTileMap.Visible = true;

        touchControlsMan.ShowTouchControlsIfAvailable(false);
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public void SaveLevel()
    {
        jarMan.SavePresentColoursToGameSettings();

        UserCustomLevelEntry customLevelEntry = new UserCustomLevelEntry();

        customLevelEntry.name = commonGameSettings.CustomLevelName;
        customLevelEntry.date = new Date();
        customLevelEntry.code = jarMan.ExportLevelToString();
        customLevelEntry.highScore = 0;

        // if -1, make new entry
        if (commonGameSettings.CustomLevelID == -1)
        {
            userCustomLevelList.CustomLevels.Insert(0, customLevelEntry);
            commonGameSettings.CustomLevelID = 0;
        }
        // else, replace existing entry
        else
        {
            userCustomLevelList.CustomLevels[commonGameSettings.CustomLevelID] = customLevelEntry;
        }

        userCustomLevelList.SaveToFile();
    }

    public void SaveLevelAndQuit()
    {
        SaveLevel();
        gameMan.QuitGame();
    }

    public bool IsPosOutOfBounds(Vector2I pos)
    {
        return pos.X < jarMan.JarOrigin.X || pos.X > jarMan.JarOrigin.X + jarMan.JarSize.X - 1
        || pos.Y < jarMan.JarOrigin.Y || pos.Y > jarMan.JarOrigin.Y + jarMan.JarSize.Y - 1;
    }

    public bool IsPositionSelected(Vector2I pos)
    {
        return selectedTiles.Contains(pos);
    }

    public void AddSelectedTile(Vector2I pos, bool logChanges)
    {
        if (selectedTiles.Contains(pos))
        {
            return;
        }

        if (logChanges)
        {
            undoRedoMan.ActiveUndoRedoStep.AddOldSelectedTile(pos, false);
            undoRedoMan.ActiveUndoRedoStep.AddNewSelectedTile(pos, true);
        }

        selectedTiles.Add(pos);
        selectionTileMap.SetCell(pos, GameConstants.toolPreviewSourceID, new Vector2I(1, 0));
    }

    public void RemoveSelectedTile(Vector2I pos, bool logChanges)
    {
        if (!selectedTiles.Contains(pos))
        {
            return;
        }

        if (logChanges)
        {
            undoRedoMan.ActiveUndoRedoStep.AddOldSelectedTile(pos, true);
            undoRedoMan.ActiveUndoRedoStep.AddNewSelectedTile(pos, false);
        }

        selectedTiles.Remove(pos);
        selectionTileMap.SetCell(pos, -1);
    }

    public void ClearSelectedTiles(bool logChanges)
    {
        if (selectedTiles.Count == 0)
            return;
        
        if (logChanges)
        {
            foreach (Vector2I pos in selectedTiles)
            {
                undoRedoMan.ActiveUndoRedoStep.AddOldSelectedTile(pos, true);
                undoRedoMan.ActiveUndoRedoStep.AddNewSelectedTile(pos, false);
            }
        }

        selectionTileMap.Clear();
        selectedTiles.Clear();
    }

    public void EraseTile(Vector2I pos, bool logChanges)
    {
        if (jarTiles.GetCellSourceId(pos) != -1)
        {
            if (logChanges)
                undoRedoMan.ActiveUndoRedoStep.AddOldTile(pos, jarTiles);

            jarTiles.SetCell(pos, -1);

            if (logChanges)
                undoRedoMan.ActiveUndoRedoStep.AddNewTile(pos, jarTiles);
        }
    }

    public void ClearAllTiles(bool logChanges)
    {
        for (int y = jarMan.JarOrigin.Y; y < jarMan.JarOrigin.Y + jarMan.JarSize.Y; y++)
        {
            for (int x = jarMan.JarOrigin.X; x < jarMan.JarOrigin.X + jarMan.JarSize.X; x++)
            {
                Vector2I pos = new Vector2I(x, y);
                EraseTile(pos, logChanges);
            }
        }

        ClearSelectedTiles(logChanges);
    }

    public void GrabSelection(bool duplicate)
    {
        foreach (Vector2I pos in selectedTiles)
        {
            if (duplicate)
            {
                selectionTileMap.SetCell(pos, jarTiles.GetCellSourceId(pos), jarTiles.GetCellAtlasCoords(pos));
            }
            else
            {
                undoRedoMan.ActiveUndoRedoStep.AddOldTile(pos, jarTiles);
                selectionTileMap.SetCell(pos, jarTiles.GetCellSourceId(pos), jarTiles.GetCellAtlasCoords(pos));
                jarTiles.SetCell(pos, -1);
                undoRedoMan.ActiveUndoRedoStep.AddNewTile(pos, jarTiles);
            }
        }

        grabbedSelectionGridOffset = Vector2I.Zero;
        lastGrabbedSelectionGridOffset = grabbedSelectionGridOffset;
    }

    public void OffsetGrabbedSelectionPos(Vector2 offset)
    {
        selectionTileMap.Position = origGrabbedSelectionPos + offset;

        grabbedSelectionGridOffset = new Vector2I(Mathf.RoundToInt(offset.X / jarMan.JarCellSize.X), Mathf.RoundToInt(offset.Y / jarMan.JarCellSize.Y));

        if (lastGrabbedSelectionGridOffset != grabbedSelectionGridOffset)
            UpdateGrabbedSelectionPreview();

        lastGrabbedSelectionGridOffset = grabbedSelectionGridOffset;
    }

    private void UpdateGrabbedSelectionPreview()
    {
        previewTiles.Clear();

        foreach (Vector2I pos in selectedTiles)
        {
            Vector2I offsetPos = pos + grabbedSelectionGridOffset;

            if (!IsPosOutOfBounds(offsetPos))
                previewTiles.SetCell(offsetPos, GameConstants.toolPreviewSourceID, new Vector2I(1, 0));
        }
    }

    public void DropSelection(Vector2 offset)
    {
        OffsetGrabbedSelectionPos(offset);
        previewTiles.Clear();

        List<Vector2I> selectedPositionsToRemove = new List<Vector2I>();
        List<Vector2I> newPositions = new List<Vector2I>(selectedTiles);

        // Selected tiles -> jar tiles
        for (int i = 0; i < newPositions.Count; i++)
        {
            Vector2I pos = newPositions[i];
            Vector2I offsetPos = pos + grabbedSelectionGridOffset;

            if (IsPosOutOfBounds(offsetPos))
            {
                selectedPositionsToRemove.Add(offsetPos);
            }
            else
            {
                undoRedoMan.ActiveUndoRedoStep.AddOldTile(offsetPos, jarTiles);
                jarTiles.SetCell(offsetPos, selectionTileMap.GetCellSourceId(pos), selectionTileMap.GetCellAtlasCoords(pos));
                undoRedoMan.ActiveUndoRedoStep.AddNewTile(offsetPos, jarTiles);
            }

            newPositions[i] = offsetPos;
        }

        // Clear selection tiles
        ClearSelectedTiles(true);
        
        // Remove out-of-bounds positions
        foreach (Vector2I pos in selectedPositionsToRemove)
        {
            newPositions.Remove(pos);
        }

        // Re-add selection outline tiles with thw newly shifted selected positions
        foreach (Vector2I pos in newPositions)
        {
            AddSelectedTile(pos, true);
        }
        
        // Reset selectionTileMap position
        selectionTileMap.Position = origGrabbedSelectionPos;
    }

    private void ChangeJarWidth(int amount)
    {
        if (cursor.IsBusy)
            return;

        int newWidth = jarMan.PlayerGameSettings.JarWidth + amount;

        // size limits
        if (newWidth > GameConstants.jarWidthMax || newWidth < GameConstants.jarWidthMin)
            return;

        undoRedoMan.StartUndoRedoStep();

        jarMan.PlayerGameSettings.JarWidth = newWidth;
        jarMan.UpdateJarSize();

        // if shrinking jar size, delete any tiles that would be out-of-bounds
        if (amount < 0)
        {
            Vector2I orig = jarMan.JarOrigin;
            int halfAmount = amount / 2;

            for (int i = 0; i < -halfAmount; i++)
            {
                int xl = orig.X + newWidth + i;
                int xr = orig.X - (i + 1);

                for (int y = orig.Y; y < orig.Y + jarMan.JarSize.Y; y++)
                {
                    Vector2I pos = new Vector2I(xl, y);

                    RemoveSelectedTile(pos, true);
                    EraseTile(pos, true);
                    pos.X = xr;
                    RemoveSelectedTile(pos, true);
                    EraseTile(pos, true);
                }
            }

        }

        undoRedoMan.EndUndoRedoStep();
    }

    public void IncreaseJarWidth()
    {
        ChangeJarWidth(2);
    }

    public void DecreaseJarWidth()
    {
        ChangeJarWidth(-2);
    }

    public void OpenPauseScreen(int screen)
    {
        if (!isPaused)
            SetIsPaused(true);

        editorPauseMan.SetScreen(screen);
    }

    public void SetIsPaused(bool b)
    {
        isPaused = b;

        cursor.SetProcess(!b);
        editorPauseMan.SetPauseMenuVisibility(b);

        if (b)
            sfxMan.Play("Pause");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (cursor.IsBusy)
            return;

        if (isPaused && Input.IsActionJustPressed("ui_cancel"))
            editorPauseMan.GoBack();
		else if (Input.IsActionJustPressed("Pause") && !IsSelectionPresent)
        {
            if (!isPaused)
                editorPauseMan.SetScreen(0);
            SetIsPaused(!isPaused);
        }
        
        if (isPaused)
            return;
        
        if (Input.IsActionJustPressed("EditorPlayTest"))
        {
            PlayTest();
        }
        else if (Input.IsActionJustPressed("Redo"))
        {
            undoRedoMan.Redo();
        }
        else if (Input.IsActionJustPressed("Undo"))
        {
            undoRedoMan.Undo();
        }
        else if (Input.IsActionJustPressed("EditorIncreaseJarWidth"))
        {
            IncreaseJarWidth();
        }
        else if (Input.IsActionJustPressed("EditorDecreaseJarWidth"))
        {
            DecreaseJarWidth();
        }
    }
}
