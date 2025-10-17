using Godot;
using System;
using System.Collections.Generic;

public class UndoRedoStep
{
    private Dictionary<Vector2I, JarTileData> oldTiles = new Dictionary<Vector2I, JarTileData>();
    public Dictionary<Vector2I, JarTileData> OldTiles { get { return oldTiles; } }
    private Dictionary<Vector2I, JarTileData> newTiles = new Dictionary<Vector2I, JarTileData>();
    public Dictionary<Vector2I, JarTileData> NewTiles { get { return newTiles; } }

    private Dictionary<Vector2I, bool> oldSelectedTiles = new Dictionary<Vector2I, bool>();
    public Dictionary<Vector2I, bool> OldSelectedTiles { get { return oldSelectedTiles; } }
    private Dictionary<Vector2I, bool> newSelectedTiles = new Dictionary<Vector2I, bool>();
    public Dictionary<Vector2I, bool> NewSelectedTiles { get { return newSelectedTiles; } }

    public int OldJarWidth { get; set; } = 0;
    public int NewJarWidth { get; set; } = 0;

    public bool ChangesPresent
    {
        get
        {
            if (OldJarWidth != NewJarWidth)
                return true;
            
            if (oldTiles.Count == 0 && oldSelectedTiles.Count == 0)
                return false;
            
            foreach (Vector2I pos in oldTiles.Keys)
            {
                if (!DoTilesMatch(oldTiles[pos], newTiles[pos]))
                    return true;
            }

            foreach (Vector2I pos in oldSelectedTiles.Keys)
            {
                if (oldSelectedTiles[pos] != newSelectedTiles[pos])
                    return true;
            }
            
            return false;
        }
    }

    private bool DoTilesMatch(JarTileData tileA, JarTileData tileB)
    {
        return tileA.sourceID == tileB.sourceID && tileA.atlas == tileB.atlas;
    }

    public void AddOldTile(Vector2I pos, TileMapLayer tileMap)
    {
        if (!oldTiles.ContainsKey(pos))
        {
            JarTileData JarTileData = new JarTileData(pos, tileMap);
            oldTiles.Add(pos, JarTileData);
        }
    }

    public void AddNewTile(Vector2I pos, TileMapLayer tileMap)
    {
        JarTileData JarTileData = new JarTileData(pos, tileMap);

        if (newTiles.ContainsKey(pos))
        {
            // Replace existing new tile entry
            newTiles[pos] = JarTileData;
        }
        else
        {
            // Add new tile entry
            newTiles.Add(pos, JarTileData);
        }
    }

    public void AddOldSelectedTile(Vector2I pos, bool isSelected)
    {
        if (!oldSelectedTiles.ContainsKey(pos))
        {
            oldSelectedTiles.Add(pos, isSelected);
        }
    }

    public void AddNewSelectedTile(Vector2I pos, bool isSelected)
    {
        if (newSelectedTiles.ContainsKey(pos))
        {
            // Replace existing new selected tile entry
            newSelectedTiles[pos] = isSelected;
        }
        else
        {
            // Add new selected tile entry
            newSelectedTiles.Add(pos, isSelected);
        }
    }

    public void Clear()
    {
        oldTiles.Clear();
        newTiles.Clear();
        oldSelectedTiles.Clear();
        newSelectedTiles.Clear();
        OldJarWidth = 0;
        NewJarWidth = 0;
    }
}
