using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public struct TileData
{
    public TileBase tile;
    public bool pathable;
    public bool plantable;
    public bool plottable;
    public bool occupied;
    public bool fertilized;
}

[CreateAssetMenu(fileName = "TileStore", menuName = "GrandmaGreen/TileStore", order = 0)]
public class TileStore : ScriptableObject
{
    public List<TileData> tileDataSet;

    public TileData this[int i]
    {
        get { return tileDataSet[i]; }
        set { tileDataSet[i] = value; }
    }

    public TileData this[TileBase tile]
    {
        get
        {
            foreach (TileData tileData in tileDataSet)
            {
                if(tileData.tile == tile)
                    return tileData;
            }

            return default;
        }

    }

}
