using Godot;
using System;
using System.Collections.Generic;
using static PowerUpEnums;

public partial class EditorCursor : Node2D
{
    [ExportGroup("Textures")]
    [Export] private Texture2D virusTexture;
    [Export] private Texture2D powerUpTexture;
    public Texture2D VirusTexture
    {
        set
        {
            if (sprite.Texture == virusTexture)
                sprite.Texture = value;

            virusTexture = value;
        }
    }
    public Texture2D PowerUpTexture
    {
        set
        {
            if (sprite.Texture == powerUpTexture)
                sprite.Texture = value;

            powerUpTexture = value;
        }
    }
    [Export] private Texture2D toolPreviewTexture;

    [ExportGroup("Internal References")]
    [Export] private Sprite2D sprite;

    [ExportGroup("External References")]
    [Export] private GameManager gameMan;
    [Export] private EditorManager editorMan;
    [Export] private EditorUndoRedoManager undoRedoMan;
    [Export] private EditorBaseSelector toolSelector;
    [Export] private EditorBaseSelector styleSelector;

    private JarManager jarMan;
    private Vector2I gridPos;
    private Vector2I lastGridPos;
    private Vector2I drawStartGridPos;
    private bool outOfBounds = false;
    private TileMapLayer jarTiles;

    // Variables representing current tile type
    private int currentColour = 1;
    public int CurrentColour { get { return currentColour; } }
    private bool isPowerUp = false;
    public bool IsPowerUp { get { return isPowerUp; } }
    private PowerUp currentPowerUp = PowerUp.Thwomp;
    public PowerUp CurrentPowerUp { get { return currentPowerUp; } }

    // Variables used to place current tile
    private int currentSourceID;
    private Vector2I currentAtlas = Vector2I.Zero;

    // Drawing-related
    private enum DrawingTool { Place, Delete, Select, Duplicate, ColourPicker }
    private enum DrawingAction { Place, Delete, Select }
    private enum DrawingStyle  { Free, Box, Line, Fill, Replace }
    private bool isDrawing = false;
    private bool isDragging = false;
    public bool IsBusy { get { return isDrawing || isDragging; } }
    // The tool chosen by the user which determines what happens when clicking on the jar grid
    private DrawingTool drawingTool = DrawingTool.Place;
    // The action that occurs when drawing
    private DrawingAction drawingAction = DrawingAction.Place;

    // The style of drawing chosen by the user (free draw, boxes, etc) for each tool
    private Dictionary<DrawingTool, DrawingStyle> toolDrawingStyles = new Dictionary<DrawingTool, DrawingStyle>();
    private DrawingStyle CurrentToolDrawingStyle
    {
        get
        {
            if (toolDrawingStyles.ContainsKey(drawingTool))
                return toolDrawingStyles[drawingTool];
            else
                return DrawingStyle.Free;
        }
        set { toolDrawingStyles[drawingTool] = value; }
    }
    // The drawing style automatically used, influenced by selectedDrawingStyle but does change in specific scenarios (e.g. if deleting via right-clickng, manually use the delete tool's style even if currently on a different tool (e.g. place))
    private DrawingStyle autoDrawingStyle = DrawingStyle.Free;
    private bool ToolHasDrawingStyles { get { return toolDrawingStyles.ContainsKey(drawingTool); } }

    // Function corrisponding to drawingAction
    private Action<Vector2I> DrawActionFunc
    {
        get
        {
            if (drawingAction == DrawingAction.Place)
                return PlaceTile;
            else if (drawingAction == DrawingAction.Delete)
                return DeleteTile;
            else
                return SelectTile;
        }
    }

    private Vector2I virusAtlasSize;
    public Vector2I VirusAtlasSize { get { return virusAtlasSize; } }
    private Vector2I powerUpAtlasSize;
    public Vector2I PowerUpAtlasSize { get { return powerUpAtlasSize; } }
    private Vector2I toolPreviewAtlasSize;

    private int virusSourceID;
    private int powerUpSourceID;

    private Vector2 initialDragMousePos;
    private StringName drawStartInput;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        jarMan = gameMan.Jars[0];
        jarTiles = jarMan.JarTiles;

        virusSourceID = jarMan.VirusSourceID;
        powerUpSourceID = jarMan.PowerUpSourceID;
        
        currentSourceID = virusSourceID;

        virusAtlasSize = ((TileSetAtlasSource)jarTiles.TileSet.GetSource(virusSourceID)).GetAtlasGridSize();
        powerUpAtlasSize = ((TileSetAtlasSource)jarTiles.TileSet.GetSource(powerUpSourceID)).GetAtlasGridSize();
        toolPreviewAtlasSize = ((TileSetAtlasSource)jarTiles.TileSet.GetSource(editorMan.ToolPreviewSourceID)).GetAtlasGridSize();

        toolDrawingStyles.Add(DrawingTool.Place, DrawingStyle.Free);
        toolDrawingStyles.Add(DrawingTool.Delete, DrawingStyle.Free);
        toolDrawingStyles.Add(DrawingTool.Select, DrawingStyle.Box);
	}

    private Vector2I WorldPosToGridPos(Vector2 worldPos)
    {
        Vector2 scaledPos = (worldPos - jarMan.TilemapGlobalPos) / jarMan.JarCellSize - Vector2.One / 2.0f;
        Vector2I gridPos = new Vector2I(Mathf.RoundToInt(scaledPos.X), Mathf.RoundToInt(scaledPos.Y));
        
        return gridPos;
    }

    private Vector2 GridPosToWorldPos(Vector2I gridPos)
    {
        Vector2 worldPos = gridPos * jarMan.JarCellSize + jarMan.TilemapGlobalPos + jarMan.JarCellSize / 2;
        
        return worldPos;
    }

    public void SetCursorColour(int colour)
    {
        if (IsBusy || colour == currentColour)
            return;
        
        currentColour = colour;
        UpdateCursorSprite();
    }
    public void SetCursorToVirus()
    {
        if (IsBusy || !isPowerUp)
            return;

        isPowerUp = false;
        UpdateCursorSprite();
    }
    public void SetCursorToPowerUp(PowerUp powerUp)
    {
        if (IsBusy || (powerUp == currentPowerUp && isPowerUp))
            return;

        isPowerUp = true;
        currentPowerUp = powerUp;
        UpdateCursorSprite();
    }
    public void SetDrawingTool(int tool)
    {
        if (IsBusy || (DrawingTool)tool == drawingTool)
            return;

        drawingTool = (DrawingTool)tool;
        if (ToolHasDrawingStyles)
        {
            styleSelector.PressButton((int)CurrentToolDrawingStyle);
            styleSelector.Visible = true;
        }
        else
        {
            styleSelector.Visible = false;
        }
        UpdateCursorSprite();
    }

    public void SetDrawingStyle(int style)
    {
        if (IsBusy || !ToolHasDrawingStyles || (DrawingStyle)style == CurrentToolDrawingStyle)
            return;
        
        CurrentToolDrawingStyle = (DrawingStyle)style;
    }

    private void UpdateSourceIDAndAtlas()
    {
        if (sprite.Texture == toolPreviewTexture)
            currentSourceID = editorMan.ToolPreviewSourceID;
        else if (sprite.Texture == powerUpTexture)
            currentSourceID = powerUpSourceID;
        else
            currentSourceID = virusSourceID;

        currentAtlas = new Vector2I(sprite.Frame % sprite.Hframes, sprite.Frame / sprite.Hframes);
    }
    private void UpdateCursorSprite()
    {
        if (isDrawing && (drawingAction == DrawingAction.Delete || drawingAction == DrawingAction.Select))
        {
            if (sprite.Texture != toolPreviewTexture)
            {
                sprite.Frame = 0;
                sprite.Texture = toolPreviewTexture;

                sprite.Hframes = toolPreviewAtlasSize.X;
                sprite.Vframes = toolPreviewAtlasSize.Y;
            }

            sprite.Frame = (int)drawingAction - 1;
        }
        else if (drawingTool != DrawingTool.Place)
        {
            if (sprite.Texture != toolPreviewTexture)
            {
                sprite.Frame = 0;
                sprite.Texture = toolPreviewTexture;

                sprite.Hframes = toolPreviewAtlasSize.X;
                sprite.Vframes = toolPreviewAtlasSize.Y;
            }

            sprite.Frame = (int)drawingTool - 1;
        }
        else if (isPowerUp)
        {
            if (sprite.Texture != powerUpTexture)
            {
                sprite.Frame = 0;
                sprite.Texture = powerUpTexture;

                sprite.Hframes = powerUpAtlasSize.X;
                sprite.Vframes = powerUpAtlasSize.Y;
            }

            sprite.Frame = sprite.Hframes * currentColour + (int)currentPowerUp;
        }
        else
        {
            if (sprite.Texture != virusTexture)
            {
                sprite.Frame = 0;
                sprite.Texture = virusTexture;

                sprite.Hframes = virusAtlasSize.X;
                sprite.Vframes = virusAtlasSize.Y;
            }

            sprite.Frame = sprite.Hframes * (currentColour - 1);
        }

        UpdateSourceIDAndAtlas();
    }

    private bool IsPosOutOfBounds(Vector2I pos)
    {
        return editorMan.IsPosOutOfBounds(pos);
    }

    private void PlaceTile(Vector2I pos)
    {
        if (!IsPosOutOfBounds(pos))
        {
            undoRedoMan.ActiveUndoRedoStep.AddOldTile(pos, jarTiles);
            jarTiles.SetCell(pos, currentSourceID, currentAtlas);
            undoRedoMan.ActiveUndoRedoStep.AddNewTile(pos, jarTiles);
        }
    }

    private void DeleteTile(Vector2I pos)
    {
        if (!IsPosOutOfBounds(pos))
        {
            if (editorMan.IsSelectionPresent && editorMan.IsPositionSelected(pos))
            {
                editorMan.RemoveSelectedTile(pos, true);
            }

            undoRedoMan.ActiveUndoRedoStep.AddOldTile(pos, jarTiles);
            jarTiles.SetCell(pos, -1);
            undoRedoMan.ActiveUndoRedoStep.AddNewTile(pos, jarTiles);
        }
    }

    // Same as above but if deleting a selected tile, the whole selection gets deleted
    private void DeleteTileAndJoinedSelection(Vector2I pos)
    {
        if (!IsPosOutOfBounds(pos))
        {
            if (editorMan.IsSelectionPresent && editorMan.IsPositionSelected(pos))
            {
                for (int i = 0; i < editorMan.SelectedTiles.Count; i++)
                {
                    Vector2I selectedPos = editorMan.SelectedTiles[i];
                    undoRedoMan.ActiveUndoRedoStep.AddOldTile(selectedPos, jarTiles);
                    jarTiles.SetCell(selectedPos, -1);
                    undoRedoMan.ActiveUndoRedoStep.AddNewTile(selectedPos, jarTiles);
                }

                editorMan.ClearSelectedTiles(true);
                return;
            }

            undoRedoMan.ActiveUndoRedoStep.AddOldTile(pos, jarTiles);
            jarTiles.SetCell(pos, -1);
            undoRedoMan.ActiveUndoRedoStep.AddNewTile(pos, jarTiles);
        }
    }

    private void SelectTile(Vector2I pos)
    {
        if (!IsPosOutOfBounds(pos) && jarTiles.GetCellSourceId(pos) != -1)
        {
            editorMan.AddSelectedTile(pos, true);
        }
    }

    private void PlacePreviewTile(Vector2I pos)
    {
        if (!IsPosOutOfBounds(pos))
        {
            if (drawingAction == DrawingAction.Place)
                jarMan.PreviewTiles.SetCell(pos, currentSourceID, currentAtlas);
            else
                editorMan.PreviewToolTiles.SetCell(pos, currentSourceID, currentAtlas);
        }
    }

    private void ClearPreview()
    {
        jarMan.PreviewTiles.Clear();
        editorMan.PreviewToolTiles.Clear();
    }

    protected void DrawLine(Vector2I startPos, Vector2I endPos, bool skipFirstTile, Action<Vector2I> drawFunc)
    {
        Vector2I diff = endPos - startPos;

        if (startPos == endPos)
        {
            if (!skipFirstTile)
                drawFunc(startPos);
        }
        else if (diff.Length() == 1)
        {
            if (!skipFirstTile)
                drawFunc(startPos);
            drawFunc(endPos);
        }
        else
        {
            // create array for storing positions
            List<Vector2I> positions = new List<Vector2I>();

            // size of x and y box area
            Vector2I areaSize;
            areaSize.X = Mathf.Abs(diff.X) + 1;
            areaSize.Y = Mathf.Abs(diff.Y) + 1;

            // min and max lengths (one is x length, the other is y)
            int maxLength = Mathf.Max(areaSize.X, areaSize.Y);

            // draw line based on line to pixel interpolation (skip first tile, i = 1, if skipFirstTile is true)
            for (int i = skipFirstTile ? 1 : 0; i < maxLength; i++)
            {
                Vector2I interpolatedPos;
                interpolatedPos.X = startPos.X + Mathf.RoundToInt(diff.X * (i / (float)(maxLength - 1)));
                interpolatedPos.Y = startPos.Y + Mathf.RoundToInt(diff.Y * (i / (float)(maxLength - 1)));

                positions.Add(interpolatedPos);
            }

            // do drawFunc for each position
            foreach (Vector2I pos in positions)
            {
                drawFunc(pos);
            }
        }
    }

    private void DrawBox(Vector2I posA, Vector2I posB, Action<Vector2I> drawFunc)
    {
        if (posA == posB)
        {
            drawFunc(posA);
        }
        else
        {
            // create array for storing positions
            List<Vector2I> positions = new List<Vector2I>();

            // box area size between both positions given
            Vector2I boxArea = (posA - posB);
            boxArea.X = Mathf.Abs(boxArea.X) + 1;
            boxArea.Y = Mathf.Abs(boxArea.Y) + 1;

            // top left position of box area
            Vector2I topLeft;
            topLeft.X = Mathf.Min(posA.X, posB.X);
            topLeft.Y = Mathf.Min(posA.Y, posB.Y);

            // add position values to array
            for (int i = 0; i < boxArea.Y; i++)
            {
                for (int j = 0; j < boxArea.X; j++)
                {
                    positions.Add(topLeft + new Vector2I(j,i));
                }
            }

            // do drawFunc for each position
            foreach (Vector2I pos in positions)
            {
                drawFunc(pos);
            }
        }
    }

    private void DrawFill(Vector2I startPos, Action<Vector2I> drawFunc)
    {
        int targetSourceID = jarTiles.GetCellSourceId(startPos);
        Vector2I targetAtlas = jarTiles.GetCellAtlasCoords(startPos);

        // Create array for storing filled positions
        List<Vector2I> filledPositions = new List<Vector2I>();
        filledPositions.Add(startPos);

        // Positions that will be checked if they match target
        List<Vector2I> positionsToCheck = new List<Vector2I>();

        // Add neighbours of startPos to check
        positionsToCheck.Add(startPos + Vector2I.Up);
        positionsToCheck.Add(startPos + Vector2I.Down);
        positionsToCheck.Add(startPos + Vector2I.Left);
        positionsToCheck.Add(startPos + Vector2I.Right);

        while (positionsToCheck.Count != 0)
        {
            // Create duplicate of positionsToCheck so only the positions currently within positionsToCheck are checked and positionsToCheck can be changed without breaking the loop
            List<Vector2I> tempPositionsToCheck = new List<Vector2I>(positionsToCheck);

            for (int i = 0; i < tempPositionsToCheck.Count; i++)
            {
                Vector2I pos = tempPositionsToCheck[i];
                if (jarMan.DoesTileMatch(pos, targetSourceID, targetAtlas) && !IsPosOutOfBounds(pos) && !filledPositions.Contains(pos))
                {
                    filledPositions.Add(pos);
                    
                    // Add neighbours of pos
                    positionsToCheck.Add(pos + Vector2I.Up);
                    positionsToCheck.Add(pos + Vector2I.Down);
                    positionsToCheck.Add(pos + Vector2I.Left);
                    positionsToCheck.Add(pos + Vector2I.Right);
                }

                positionsToCheck.Remove(pos);
            }
        }

        // Do drawFunc for each position
        foreach (Vector2I pos in filledPositions)
        {
            drawFunc(pos);
        }
    }

    private void DrawReplace(Vector2I startPos, Action<Vector2I> drawFunc)
    {
        int targetSourceID = jarTiles.GetCellSourceId(startPos);
        Vector2I targetAtlas = jarTiles.GetCellAtlasCoords(startPos);

        // Create array for storing matching tile positions
        List<Vector2I> matchingPositions = new List<Vector2I>();
        matchingPositions.Add(startPos);

        // Fill matchingPositions list
        for (int y = jarMan.JarOrigin.Y; y < jarMan.JarOrigin.Y + jarMan.JarSize.Y; y++)
        {
            for (int x = jarMan.JarOrigin.X; x < jarMan.JarOrigin.X + jarMan.JarSize.X; x++)
            {
                Vector2I pos = new Vector2I(x, y);
                
                if (jarMan.DoesTileMatch(pos, targetSourceID, targetAtlas))
                {
                    matchingPositions.Add(pos);
                }
            }
        }

        // Do drawFunc for each position
        foreach (Vector2I pos in matchingPositions)
        {
            drawFunc(pos);
        }
    }

    // First frame of draw
    private void StartDraw(DrawingAction action, string startInput = "EditorDraw")
    {
        lastGridPos = gridPos;
        drawStartGridPos = gridPos;
        drawingAction = action;

        drawStartInput = startInput;

        autoDrawingStyle = CurrentToolDrawingStyle;

        // If doing an action different to the selected tool (e.g. middle-clicking to select while selected another tool like place), manually change autoDrawingStyle to match the action
        // Do not do this if the current action (regardless of selected tool) is deleting, so deleting via right-clicking keeps the current drawing style since it feels more natural
        if (startInput != "EditorDraw" && action != DrawingAction.Delete)
        {
            if (drawingAction == DrawingAction.Select)
                autoDrawingStyle = toolDrawingStyles[DrawingTool.Select];
            else if (drawingAction == DrawingAction.Delete)
                autoDrawingStyle = toolDrawingStyles[DrawingTool.Delete];
        }

        undoRedoMan.StartUndoRedoStep();

        // only clear selected tiles if NOT deleting and NOT selecting while holding shift
        if (action != DrawingAction.Delete && !(action == DrawingAction.Select && Input.IsKeyPressed(Key.Shift)))
            editorMan.ClearSelectedTiles(true);

        if (autoDrawingStyle == DrawingStyle.Free)
        {
            isDrawing = true;
            UpdateCursorSprite();

            if (drawingAction == DrawingAction.Delete)
                DeleteTileAndJoinedSelection(gridPos);
            else
                DrawActionFunc(gridPos);
        }
        else if (autoDrawingStyle == DrawingStyle.Fill)
        {
            DrawFill(gridPos, DrawActionFunc);
            undoRedoMan.EndUndoRedoStep();
        }
        else if (autoDrawingStyle == DrawingStyle.Replace)
        {
            DrawReplace(gridPos, DrawActionFunc);
            undoRedoMan.EndUndoRedoStep();
        }
        else
        {
            isDrawing = true;
            UpdateCursorSprite();
            ClearPreview();
            PlacePreviewTile(gridPos);
        }
    }

    // During draw after first frame
    private void UpdateDraw()
    {
        // Box draw
        if (autoDrawingStyle == DrawingStyle.Box)
        {
            ClearPreview();
            DrawBox(drawStartGridPos, gridPos, PlacePreviewTile);
        }
        // Line draw
        else if (autoDrawingStyle == DrawingStyle.Line)
        {
            ClearPreview();
            DrawLine(drawStartGridPos, gridPos, false, PlacePreviewTile);
        }
        // Free draw
        else
        {
            DrawLine(lastGridPos, gridPos, true, drawingAction == DrawingAction.Delete ? DeleteTileAndJoinedSelection : DrawActionFunc);
        }
    }

    // Last frame of drawing / when drawing button is released
    private void EndDraw()
    {
        isDrawing = false;
        ClearPreview();

        // Box draw
        if (autoDrawingStyle == DrawingStyle.Box)
        {
            DrawBox(drawStartGridPos, gridPos, DrawActionFunc);
        }
        // Line draw
        else if (autoDrawingStyle == DrawingStyle.Line)
        {
            DrawLine(drawStartGridPos, gridPos, false, DrawActionFunc);
        }

        undoRedoMan.EndUndoRedoStep();

        if (drawingAction != DrawingAction.Place)
        {
            UpdateCursorSprite();
        }

        autoDrawingStyle = CurrentToolDrawingStyle;
    }

    private void SetCursorToMatchTile(Vector2I pos)
    {
        int sourceID = jarTiles.GetCellSourceId(pos);
        if (sourceID == -1)
            return;

        SetCursorColour(jarMan.GetSegmentColour(pos));
        editorMan.ColourSelector.PressButton(currentColour - 1);
        
        if (sourceID == powerUpSourceID)
        {
            SetCursorToPowerUp((PowerUp)jarTiles.GetCellAtlasCoords(pos).X);
            editorMan.TileTypeSelector.PressButton((int)currentPowerUp + 1);
        }
        else
        {
            SetCursorToVirus();
            editorMan.TileTypeSelector.PressButton(0);
        }

        SetDrawingTool(0);
        toolSelector.PressButton(0);
    }

    private void HandleInput()
    {
        if (!isDrawing && !isDragging)
        {
            if (Input.IsActionJustPressed("EditorSelectAll"))
            {
                undoRedoMan.StartUndoRedoStep();

                // Select all tiles
                for (int y = jarMan.JarOrigin.Y; y < jarMan.JarOrigin.Y + jarMan.JarSize.Y; y++)
                {
                    for (int x = jarMan.JarOrigin.X; x < jarMan.JarOrigin.X + jarMan.JarSize.X; x++)
                    {
                        Vector2I pos = new Vector2I(x, y);
                        
                        SelectTile(pos);
                    }

                }

                undoRedoMan.EndUndoRedoStep();
            }
            else if (Input.IsActionJustPressed("EditorDeselect"))
            {
                undoRedoMan.StartUndoRedoStep();

                editorMan.ClearSelectedTiles(true);

                undoRedoMan.EndUndoRedoStep();
            }

            if (editorMan.IsSelectionPresent)
            {
                if (Input.IsActionJustPressed("EditorSelectionDeselect"))
                {
                    undoRedoMan.StartUndoRedoStep();
                    editorMan.ClearSelectedTiles(true);
                    undoRedoMan.EndUndoRedoStep();
                }
                else if (Input.IsActionJustPressed("EditorSelectionDelete"))
                {
                    undoRedoMan.StartUndoRedoStep();

                    for (int i = 0; i < editorMan.SelectedTiles.Count; i++)
                    {
                        Vector2I selectedPos = editorMan.SelectedTiles[i];
                        undoRedoMan.ActiveUndoRedoStep.AddOldTile(selectedPos, jarTiles);
                        jarTiles.SetCell(selectedPos, -1);
                        undoRedoMan.ActiveUndoRedoStep.AddNewTile(selectedPos, jarTiles);
                    }

                    editorMan.ClearSelectedTiles(true);
                    
                    undoRedoMan.EndUndoRedoStep();
                }
            }

            if (!outOfBounds)
            {
                if (Input.IsActionJustPressed("EditorDraw"))
                {
                    GetViewport().GuiReleaseFocus();
                    
                    if (drawingTool == DrawingTool.ColourPicker)
                    {
                        SetCursorToMatchTile(gridPos);
                        return;
                    }

                    // dragging stuff
                    if (jarMan.IsSegmentPresent(gridPos) && drawingTool != DrawingTool.Delete)
                    {
                        if (drawingTool == DrawingTool.Duplicate)
                            StartDrag(true);
                        else if (drawingTool != DrawingTool.Delete && editorMan.IsPositionSelected(gridPos))
                            StartDrag(false);
                    }
                    
                    // drawing stuff
                    if (!isDragging)
                    {
                        if (drawingTool == DrawingTool.Delete)
                            StartDraw(DrawingAction.Delete);
                        else if (drawingTool == DrawingTool.Place)
                            StartDraw(DrawingAction.Place);
                        else if (drawingTool == DrawingTool.Select)
                            StartDraw(DrawingAction.Select);
                    }
                }
                else if (Input.IsActionJustPressed("EditorDrawDelete"))
                {
                    GetViewport().GuiReleaseFocus();
                    StartDraw(DrawingAction.Delete, "EditorDrawDelete");
                }
                else if (Input.IsActionJustPressed("EditorDrawSelect"))
                {
                    GetViewport().GuiReleaseFocus();
                    StartDraw(DrawingAction.Select, "EditorDrawSelect");
                }
            }
        }

        if (isDragging)
        {
            UpdateDrag();

            if (!Input.IsActionPressed("EditorDraw"))
            {
                EndDrag();
            }
        }
        else if (isDrawing)
        {
            if (gridPos != lastGridPos)
            {
                UpdateDraw();
            }

            if (!Input.IsActionPressed(drawStartInput))
                EndDraw();
        }
    }

    private void StartDrag(bool duplicate)
    {
        isDragging = true;

        undoRedoMan.StartUndoRedoStep();
        
        initialDragMousePos = GetGlobalMousePosition();

        if (!editorMan.IsPositionSelected(gridPos))
        {
            editorMan.ClearSelectedTiles(true);
            editorMan.AddSelectedTile(gridPos, true);
        }

        editorMan.GrabSelection(duplicate);
    }

    private void UpdateDrag()
    {
        editorMan.OffsetGrabbedSelectionPos(GetGlobalMousePosition() - initialDragMousePos);
    }

    private void EndDrag()
    {
        editorMan.DropSelection(GetGlobalMousePosition() - initialDragMousePos);
        undoRedoMan.EndUndoRedoStep();

        isDragging = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        gridPos = WorldPosToGridPos(GetGlobalMousePosition());

        outOfBounds = IsPosOutOfBounds(gridPos);
        if (!outOfBounds)
            GlobalPosition = GridPosToWorldPos(gridPos);

        HandleInput();

        Visible = !outOfBounds && !isDragging && !editorMan.IsPositionSelected(gridPos);
        
        lastGridPos = gridPos;
    }
}
