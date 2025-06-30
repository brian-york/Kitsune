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
            Debug.Log($"Tile dropped on cell [{row},{col}] with value {tile.tileValue}");

            // Update the puzzle grid
            PuzzleManager puzzleManager = FindFirstObjectByType<PuzzleManager>();
            if (puzzleManager != null)
            {
                puzzleManager.UpdateCell(row, col, tile.tileValue);
            }

            // Update the visible cell value
            CellController cellController = GetComponent<CellController>();
            if (cellController != null)
            {
                cellController.SetValue(tile.tileValue, false);
            }

            // Destroy the placed tile
            Destroy(tile.gameObject);

            // Spawn a new tile of the same value back into the pool
            GridSpawner gridSpawner = FindFirstObjectByType<GridSpawner>();
            if (gridSpawner != null)
            {
                gridSpawner.CreateTile(tile.tileValue);
            }
        }
    }
}
