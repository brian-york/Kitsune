using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject tileButton;
    public RectTransform tilePoolTransform;
    public TilePoolManager poolManager;

    void Start()
    {
        // Find puzzle pool manager if not assigned
        if (poolManager == null)
        {
            poolManager = FindFirstObjectByType<TilePoolManager>();
            if (poolManager == null)
            {
                Debug.LogError("TilePoolManager not found in scene!");
                return;
            }
        }

        poolManager.InitializePool();
        poolManager.ShufflePool();

        // Spawn initial hand of 3 tiles
        for (int i = 0; i < 3; i++)
{
    TileData drawnTile = poolManager.DrawTile();

    if (drawnTile != null)
    {
        CreateTile(drawnTile);
    }
    else
    {
        Debug.Log("No more tiles left in the pool!");
        break;
    }
}

        // Load puzzle grid as before
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
    }

    public void CreateTile(TileData tileData)
{
    GameObject tileObj = Instantiate(tileButton, tilePoolTransform);
    TileDragHandler handler = tileObj.GetComponent<TileDragHandler>();

    if (handler != null)
    {
        handler.SetTileData(tileData);
    }
    else
    {
        Debug.LogError("TileDragHandler missing from tileButton prefab!");
    }
}
public void RefillTileHand()
{
    int currentTiles = tilePoolTransform.childCount;

    while (currentTiles < 3)
    {
        TileData newTile = poolManager.DrawTile();

        if (newTile != null)
        {
            CreateTile(newTile);
            currentTiles++;
        }
        else
        {
            Debug.Log("Tile pool is empty — cannot refill hand further.");
            break;
        }
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
                cellController.SetInteractable(false);
            }
        }
    }
}