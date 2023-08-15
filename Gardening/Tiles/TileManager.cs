using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Core.Input;

public class TileManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap tiles;

    [SerializeField]
    private PointerState pstate;

    private void Update()
    {
        if (pstate.phase == PointerState.Phase.DOWN)
        {
            Vector3 pos = pstate.position;
            Debug.Log("pos " + pos);
            Vector3Int gridpos = tiles.WorldToCell(pos);
            TileBase clickedTile = tiles.GetTile(gridpos);
            // Incorrect tile mapping; need to translate world coordinates to
            // tilemap coordinates.
            Debug.Log("Tile clicked is " + clickedTile.name);
        }
    }
}
