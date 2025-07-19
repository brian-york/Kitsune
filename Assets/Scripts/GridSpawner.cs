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

        if (puzzleData.cellStates != null)
        {
            Debug.Log("cellStates found! Rows: " + puzzleData.cellStates.Count);
            Debug.Log("First row states: " + string.Join(",", puzzleData.cellStates[0]));
        }
        else
        {
            Debug.Log("cellStates is NULL!");
        }
string[,] narrativeDescriptions = new string[9, 9];

if (puzzleData.narrativeCellDescriptions != null && puzzleData.narrativeCellDescriptions.Count > 0)
{
    for (int row = 0; row < 9; row++)
    {
        for (int col = 0; col < 9; col++)
        {
            int index = row * 9 + col;
            narrativeDescriptions[row, col] = puzzleData.narrativeCellDescriptions[index];
        }
    }

    Debug.Log("Narrative cell descriptions loaded.");
}
else
{
    for (int row = 0; row < 9; row++)
    {
        for (int col = 0; col < 9; col++)
        {
            narrativeDescriptions[row, col] = "";
        }
    }
}



    int[,] puzzleGrid = new int[9, 9];

    for (int row = 0; row < 9; row++)
    {
        for (int col = 0; col < 9; col++)
        {
            int index = row * 9 + col;
            puzzleGrid[row, col] = puzzleData.grid[index];
        }
    }

    // Load cell states if available
string[,] cellStates = new string[9, 9];

if (puzzleData.cellStates != null && puzzleData.cellStates.Count > 0)
{
    for (int row = 0; row < 9; row++)
    {
        for (int col = 0; col < 9; col++)
        {
            int index = row * 9 + col;
            cellStates[row, col] = puzzleData.cellStates[index];
        }
    }

    Debug.Log("Loaded cellStates for puzzle: " + puzzleData.id);
}
else
{
    for (int row = 0; row < 9; row++)
    {
        for (int col = 0; col < 9; col++)
        {
            cellStates[row, col] = "Playable";
        }
    }

    Debug.Log("No cellStates block found. Defaulting to Playable for all cells.");
}


    // Prepare the blocked cells array
    bool[,] blockedCells = new bool[9, 9];

    for (int row = 0; row < 9; row++)
    {
        for (int col = 0; col < 9; col++)
        {
            blockedCells[row, col] = cellStates[row, col] == "Blocked";
        }
    }

    // Pass blockedCells to PuzzleManager
    PuzzleManager puzzleManager = FindFirstObjectByType<PuzzleManager>();
    if (puzzleManager != null)
    {
        puzzleManager.isLoading = true;
        puzzleManager.LoadPuzzle(puzzleGrid);
        puzzleManager.blockedCells = blockedCells;
        puzzleManager.isLoading = false;
    }
    else
    {
        Debug.LogError("PuzzleManager not found!");
    }

    // Pass cellStates into GenerateGrid
    GenerateGrid(puzzleGrid, cellStates, narrativeDescriptions);

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

                if (currentTiles == 0)
            {
                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.TriggerGameOver();
                }
            }
                break;
            }
        }
    }
void GenerateGrid(int[,] puzzle, string[,] cellStates, string[,] narrativeDescriptions)


    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int value = puzzle[row, col];
string state = cellStates[row, col];
bool isBlocked = state == "Blocked";
bool locked = value != 0;




GameObject newCell = Instantiate(cellPrefab, transform);
newCell.name = $"Cell_{row}_{col}";


                var cellController = newCell.GetComponent<CellController>();
cellController.narrativeDescription = narrativeDescriptions[row, col];

                if (!string.IsNullOrEmpty(narrativeDescriptions[row, col]))
                {
                    cellController.narrativeDescription = narrativeDescriptions[row, col];

                    // Example: Assign type based on description keyword

                    if (narrativeDescriptions[row, col].Contains("currency"))
                    {
                        cellController.narrativeCellType = CellController.NarrativeCellType.Currency;
                    }

                    if (narrativeDescriptions[row, col].Contains("shop"))
                    {
                        cellController.narrativeCellType = CellController.NarrativeCellType.Shop;
                    }
                    else if (narrativeDescriptions[row, col].Contains("relic"))
                    {
                        cellController.narrativeCellType = CellController.NarrativeCellType.RelicReward;
                    }
                    else
                    {
                        cellController.narrativeCellType = CellController.NarrativeCellType.Event;
                    }
                    cellController.narrativeCondition = new NarrativeCondition();

                    NarrativeRules rules = new NarrativeRules();

                    if (cellController.narrativeCellType != CellController.NarrativeCellType.None)
                    {
                        cellController.narrativeCondition = rules.GetCondition(cellController.narrativeCellType);
                        Debug.Log($"Assigned narrative condition for cell [{row},{col}]: requiresSpecificTile={cellController.narrativeCondition.requiresSpecificTile} number={cellController.narrativeCondition.requiredTileNumber} effect={cellController.narrativeCondition.requiredTileEffect}");
                    }
                    cellController.SetNarrativeCellColor();
                }


cellController.SetupCell(row, col);

CellDropHandler dropHandler = newCell.GetComponent<CellDropHandler>();
if (dropHandler != null)
{
    dropHandler.row = row;
    dropHandler.col = col;
}

cellController.SetValue(value, locked);
cellController.SetBlocked(isBlocked);


            }
        }
    }
}