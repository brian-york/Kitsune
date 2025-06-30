using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public GameObject cellPrefab;

    void Start()
    {
        PuzzleLoader loader = FindFirstObjectByType<PuzzleLoader>();
        PuzzleData puzzleData = loader.LoadPuzzle();

        if (puzzleData == null)
        {
            Debug.LogError("puzzleData is null!");
            return;
        }

        if (puzzleData.grid == null)
        {
            Debug.LogError("puzzleData.grid is null!");
            return;
        }

        Debug.Log("Grid loaded with " + puzzleData.grid.Count + " cells.");

        int[,] puzzleGrid = new int[9, 9];

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int index = row * 9 + col;
                puzzleGrid[row, col] = puzzleData.grid[index];
            }
        }

        GenerateGrid(puzzleGrid);

        PuzzleManager puzzleManager = FindFirstObjectByType<PuzzleManager>();
        puzzleManager.isLoading = true;
        puzzleManager.LoadPuzzle(puzzleGrid);
        puzzleManager.isLoading = false;

        GenerateTilePool();
    }

    public GameObject tileButton;
public RectTransform tilePoolTransform;
    void GenerateTilePool()
    {
        Debug.Log("tileButton: " + tileButton);
        Debug.Log("tilePoolTransform: " + tilePoolTransform);

        for (int i = 1; i <= 9; i++)
        {
            GameObject tileObj = Instantiate(tileButton, tilePoolTransform);
            TileDragHandler handler = tileObj.GetComponent<TileDragHandler>();
            handler.SetValue(i);
        }
    }
public void CreateTile(int value)
{
    GameObject tileObj = Instantiate(tileButton, tilePoolTransform);
    TileDragHandler handler = tileObj.GetComponent<TileDragHandler>();

    if (handler != null)
    {
        handler.SetValue(value);
    }
    else
    {
        Debug.LogError("TileDragHandler missing from tileButton prefab!");
    }
}
    void GenerateGrid(int[,] puzzle)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                GameObject newCell = Instantiate(cellPrefab, transform);
                newCell.name = $"Cell_{row}_{col}";

                var cellController = newCell.GetComponent<CellController>();

                int value = puzzle[row, col];
                bool locked = value != 0;

                cellController.SetupCell(row, col);

CellDropHandler dropHandler = newCell.GetComponent<CellDropHandler>();
if (dropHandler != null)
{
    dropHandler.row = row;
    dropHandler.col = col;
}
                cellController.SetValue(value, locked);
            }
        }
    }
}