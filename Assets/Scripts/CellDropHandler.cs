using UnityEngine;
using UnityEngine.EventSystems;


public class CellDropHandler : MonoBehaviour, IDropHandler
{
    public int row;
    public int col;


    public void OnDrop(PointerEventData eventData)
{
    TileDragHandler tile = eventData.pointerDrag?.GetComponent<TileDragHandler>();

    if (tile != null)
    {
        // ✅ BLOCKED CELL CHECK:
        CellController cellController = GetComponent<CellController>();
        if (cellController != null && cellController.isBlocked)
        {
            Debug.Log($"Cell [{row},{col}] is blocked. Rejecting drop.");

            Destroy(tile.gameObject);

            // Draw a new tile to replace it
            GridSpawner gridSpawner = FindFirstObjectByType<GridSpawner>();
            if (gridSpawner != null)
            {
                gridSpawner.RefillTileHand();
            }

            return;
        }

        Debug.Log($"Tile dropped on cell [{row},{col}] with value {tile.tileValue}");

        PuzzleManager puzzleManager = FindFirstObjectByType<PuzzleManager>();

        if (puzzleManager != null && !puzzleManager.IsValid(row, col, tile.tileValue))
        {
            // INVALID PLACEMENT
            Debug.Log("❌ Invalid placement. Tile destroyed, no score given.");

            Destroy(tile.gameObject);

            GridSpawner gridSpawner = FindFirstObjectByType<GridSpawner>();
            if (gridSpawner != null)
            {
                gridSpawner.RefillTileHand();
            }

            return; // abort the rest of OnDrop
        }

        puzzleManager.UpdateCell(row, col, tile.tileValue);

        if (cellController != null)
        {
            cellController.SetValue(tile.tileValue, true);
        }

        ScoringManager scoringManager = FindFirstObjectByType<ScoringManager>();
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();

        if (scoringManager != null && scoreManager != null)
        {
            int points = scoringManager.CalculateTileScore(row, col, tile.tileData, puzzleManager.playerGrid);
            scoreManager.AddScore(points);

            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.CheckForLevelComplete(scoreManager.currentScore);
            }
        }

        Destroy(tile.gameObject);

        GridSpawner gs = FindFirstObjectByType<GridSpawner>();
        if (gs != null)
        {
            gs.RefillTileHand();
        }
    }
}



}