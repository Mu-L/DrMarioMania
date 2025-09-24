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

    public JarTileData(int sid, Vector2I att)
    {
        sourceID = sid;

        atlas = att;

        colour = att.Y;

        if (sid != GameConstants.powerUpSourceID)
            colour += 1;
    }
    
    public int sourceID;
    public Vector2I atlas;
    public int colour;
}
