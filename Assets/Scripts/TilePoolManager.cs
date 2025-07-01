using System.Collections.Generic;
using UnityEngine;

public class TilePoolManager : MonoBehaviour
{
    public List<TileData> availableTiles = new List<TileData>();
    private List<TileData> tilePool = new List<TileData>();

    void Start()
    {
        InitializePool();
        ShufflePool();
    }

    public void InitializePool()
{
    tilePool.Clear();

    foreach (TileData tile in availableTiles)
    {
        for (int i = 0; i < tile.copies; i++)
        {
            tilePool.Add(new TileData(
                tile.number,
                tile.tileColor,
                tile.specialEffect,
                tile.scoreBonus,
                tile.isWild,
                tile.narrativeTag
            ));
        }
    }

    Debug.Log("Tile pool initialized with " + tilePool.Count + " tiles.");
}

    public void ShufflePool()
    {
        for (int i = 0; i < tilePool.Count; i++)
{
    int randomIndex = Random.Range(i, tilePool.Count);

    TileData temp = tilePool[i];
    tilePool[i] = tilePool[randomIndex];
    tilePool[randomIndex] = temp;
}

        Debug.Log("Tile pool shuffled.");
    }

    public TileData DrawTile()
{
    if (tilePool.Count > 0)
    {
        TileData drawnTile = tilePool[0];
        tilePool.RemoveAt(0);
        Debug.Log("Drew tile: " + drawnTile.number);
        return drawnTile;
    }
    else
    {
        Debug.Log("Tile pool is empty!");
        return null;
    }
}

    public bool IsPoolEmpty()
    {
        return tilePool.Count == 0;
    }
}
