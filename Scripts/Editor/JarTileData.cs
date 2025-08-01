using Godot;
using System;

public struct JarTileData
{
    public JarTileData(Vector2I pos, TileMapLayer tileMap)
    {
        sourceID = tileMap.GetCellSourceId(pos);
        atlas = tileMap.GetCellAtlasCoords(pos);

        TileData data = tileMap.GetCellTileData(pos);
        colour = 1;

		if (data == null)
			colour = 0;
        else
            colour = (int)data.GetCustomData("Colour");
    }
    
    public int sourceID;
    public Vector2I atlas;
    public int colour;
}
