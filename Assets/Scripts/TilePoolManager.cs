using System.Collections.Generic;
using UnityEngine;

public class TilePoolManager : MonoBehaviour
{
    private List<TileData> drawPile = new List<TileData>();
    private List<TileData> discardPile = new List<TileData>();

    void Start()
    {
        PrepareDrawPile();
    }

    private void PrepareDrawPile()
    {
        drawPile.Clear();
        discardPile.Clear();

        if (ProgressManager.Instance == null)
        {
            Debug.LogError("⚠️ ProgressManager not found! Cannot create tile pool.");
            return;
        }

        foreach (var tile in ProgressManager.Instance.playerTileCollection)
        {
            drawPile.Add(new TileData(
                tile.number,
                tile.tileColor,
                tile.tileEffect,
                tile.scoreBonus,
                tile.isWild,
                tile.narrativeTag
            ));
        }

        ShuffleDrawPile();
        Debug.Log($"🔀 Draw pile prepared with {drawPile.Count} tiles from ProgressManager");
    }

    private void ShuffleDrawPile()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            int randomIndex = Random.Range(i, drawPile.Count);
            TileData temp = drawPile[i];
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
    }

    public TileData DrawTile()
    {
        if (drawPile.Count == 0)
        {
            if (discardPile.Count > 0)
            {
                Debug.Log("♻️ Reshuffling discard pile into draw pile");
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDrawPile();
            }
            else
            {
                Debug.LogWarning("⚠️ No tiles left in collection!");
                return null;
            }
        }

        TileData drawnTile = drawPile[0];
        drawPile.RemoveAt(0);
        Debug.Log($"Drew tile: {drawnTile.number} ({drawnTile.tileEffect})");
        return drawnTile;
    }

    public void DiscardTile(TileData tile)
    {
        discardPile.Add(tile);
    }

    public bool IsPoolEmpty()
    {
        return drawPile.Count == 0 && discardPile.Count == 0;
    }

    public void ResetForNewPuzzle()
    {
        PrepareDrawPile();
        Debug.Log("🔄 Tile pool reset for new puzzle");
    }

    [ContextMenu("View Pool State")]
    public void ViewPoolState()
    {
        Debug.Log($"📊 Draw pile: {drawPile.Count} | Discard pile: {discardPile.Count}");
    }
}
