using Godot;
using System;
using System.Collections.Generic;

public partial class EditorUndoRedoManager : Node
{
    [Export] private GameManager gameMan;
    [Export] private EditorManager editorMan;
    [Export] private EditorCursor cursor;
    private JarManager jarMan;
    private List<UndoRedoStep> undoRedoSteps = new List<UndoRedoStep>();
    private int currentUndoRedoStep = 0;
    private const int maxUndoRedoSteps = 50;
    private bool isRecordingUndoRedoStep = false;
    // Gets the currently recording undo redo step
    public UndoRedoStep ActiveUndoRedoStep
    {
        get
        {
            return isRecordingUndoRedoStep ? undoRedoSteps[currentUndoRedoStep] : null;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        jarMan = gameMan.Jars[0];
        undoRedoSteps.Add(new UndoRedoStep());
	}

    private void CreateTiles(Dictionary<Vector2I, JarTileData> tiles)
    {
        GD.Print("no. of tiles to change: " + tiles.Count);
        foreach (Vector2I pos in tiles.Keys)
        {
            jarMan.JarTiles.SetCell(pos, tiles[pos].sourceID, tiles[pos].atlas);
        }
    }

    private void UpdateSelection(Dictionary<Vector2I, bool> selectedTiles)
    {
        foreach (Vector2I pos in selectedTiles.Keys)
        {
            if (selectedTiles[pos])
                editorMan.AddSelectedTile(pos, false);
            else
                editorMan.RemoveSelectedTile(pos, false);
        }
    }

    public void Undo()
    {
        if (!editorMan.CanPressButtons || currentUndoRedoStep < 1 || isRecordingUndoRedoStep)
            return;

        CreateTiles(undoRedoSteps[currentUndoRedoStep - 1].OldTiles);
        UpdateSelection(undoRedoSteps[currentUndoRedoStep - 1].OldSelectedTiles);

        currentUndoRedoStep--;
        GD.Print("undid");
    }

    public void Redo()
    {
        if (!editorMan.CanPressButtons || undoRedoSteps.Count - 1 <= currentUndoRedoStep || isRecordingUndoRedoStep)
            return;

        CreateTiles(undoRedoSteps[currentUndoRedoStep].NewTiles);
        UpdateSelection(undoRedoSteps[currentUndoRedoStep].NewSelectedTiles);


        currentUndoRedoStep++;
        GD.Print("redid");
    }

    public void StartUndoRedoStep()
    {
        if (isRecordingUndoRedoStep)
        {
            GD.Print("NOTICE: Attempted to begin logging while already logging!");
            return;
        }
        
        isRecordingUndoRedoStep = true;

        if (undoRedoSteps.Count - 1 > currentUndoRedoStep)
            undoRedoSteps.Insert(currentUndoRedoStep, new UndoRedoStep());

        GD.Print("START STEP");
    }

     // removes every undo/redo step after the current one
    private void RemoveAllUndoRedoStepsPastCurrent()
    {
        int times = undoRedoSteps.Count - 1 - currentUndoRedoStep;
        for (int i = 0; i < times; i++)
            undoRedoSteps.RemoveAt(undoRedoSteps.Count - 1);
    }

    private void CancelUndoRedoStep()
    {
        // return if not recording
        if (!isRecordingUndoRedoStep)
        {
            GD.PrintErr("this shouldn't occur");
            return;
        }

        GD.Print("CANCEL STEP");

        // if there are undo/redo states after the current one, remove the current empty undo/redo state
        if (undoRedoSteps.Count - 1 > currentUndoRedoStep)
            undoRedoSteps.RemoveAt(currentUndoRedoStep);
        // otherwise, clear the current one (aka the latest one)
        else
            undoRedoSteps[currentUndoRedoStep].Clear();

        isRecordingUndoRedoStep = false;
    }

    public void EndUndoRedoStep()
    {
        if (!isRecordingUndoRedoStep)
        {
            GD.PrintErr("NOTICE: Attempted to stop logging while not logging!");
            return;
        }

        // if nothing has changed, cancel undo/redo step and don't increase count
        if (!undoRedoSteps[currentUndoRedoStep].ChangesPresent)
        {
            CancelUndoRedoStep();
            return;
        }

        // if there are undo/redo states after the current one, remove them all
        if (undoRedoSteps.Count - 1 > currentUndoRedoStep)
            RemoveAllUndoRedoStepsPastCurrent();

        if (currentUndoRedoStep >= maxUndoRedoSteps)
            undoRedoSteps.RemoveAt(0);
        else
            currentUndoRedoStep++;

        // create new, empty undo/redo state
        undoRedoSteps.Add(new UndoRedoStep());

        isRecordingUndoRedoStep = false;

        GD.Print("END STEP");
    }
}
