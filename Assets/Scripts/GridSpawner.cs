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

        Debug.Log("First value in puzzle grid: " + puzzleData.grid[0]);

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
        puzzleManager.LoadPuzzle(puzzleGrid);
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
                cellController.SetValue(value, locked);
            }
        }
    }
}