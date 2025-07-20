using UnityEngine;
using System.Collections.Generic;

public class GridSpawner : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject tileButton;
    public RectTransform tilePoolTransform;
    public TilePoolManager poolManager;

    void Start()
    {
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

        for (int i = 0; i < 3; i++)
        {
            TileData drawnTile = poolManager.DrawTile();
            if (drawnTile != null)
                CreateTile(drawnTile);
            else
            {
                Debug.Log("No more tiles left in the pool!");
                break;
            }
        }

        PuzzleLoader loader = FindFirstObjectByType<PuzzleLoader>();
        PuzzleData puzzleData = loader.LoadPuzzle();

        if (puzzleData == null || puzzleData.grid == null)
        {
            Debug.LogError("PuzzleData or its grid is null!");
            return;
        }

        Debug.Log("Grid loaded with " + puzzleData.grid.Count + " cells.");

        string[,] narrativeDescriptions = new string[9, 9];
        for (int row = 0; row < 9; row++)
            for (int col = 0; col < 9; col++)
            {
                int index = row * 9 + col;
                narrativeDescriptions[row, col] = (puzzleData.narrativeCellDescriptions != null && puzzleData.narrativeCellDescriptions.Count > index)
                    ? puzzleData.narrativeCellDescriptions[index]
                    : "";
            }

        int[,] puzzleGrid = new int[9, 9];
        string[,] cellStates = new string[9, 9];
        for (int row = 0; row < 9; row++)
            for (int col = 0; col < 9; col++)
            {
                int index = row * 9 + col;
                puzzleGrid[row, col] = puzzleData.grid[index];
                cellStates[row, col] = (puzzleData.cellStates != null && puzzleData.cellStates.Count > index)
                    ? puzzleData.cellStates[index]
                    : "Playable";
            }

        bool[,] blockedCells = new bool[9, 9];
        for (int row = 0; row < 9; row++)
            for (int col = 0; col < 9; col++)
                blockedCells[row, col] = cellStates[row, col] == "Blocked";

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

        GenerateGrid(puzzleGrid, cellStates, narrativeDescriptions, puzzleData.narrativeCells);
    }

    public void CreateTile(TileData tileData)
    {
        GameObject tileObj = Instantiate(tileButton, tilePoolTransform);
        TileDragHandler handler = tileObj.GetComponent<TileDragHandler>();
        if (handler != null) handler.SetTileData(tileData);
        else Debug.LogError("TileDragHandler missing from tileButton prefab!");
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
                    gm?.TriggerGameOver();
                }
                break;
            }
        }
    }

    void GenerateGrid(int[,] puzzle, string[,] cellStates, string[,] narrativeDescriptions, List<NarrativeCellEntry> narrativeCells)
    {
        var narrativeCellMap = new Dictionary<(int, int), string>();
        if (narrativeCells != null)
        {
            foreach (var entry in narrativeCells)
                narrativeCellMap[(entry.row, entry.col)] = entry.narrativeCellType;
        }

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int value = puzzle[row, col];
                bool isBlocked = cellStates[row, col] == "Blocked";
                bool locked = value != 0;

                GameObject newCell = Instantiate(cellPrefab, transform);
                newCell.name = $"Cell_{row}_{col}";

                var cellController = newCell.GetComponent<CellController>();
                cellController.SetupCell(row, col);
                cellController.narrativeDescription = narrativeDescriptions[row, col];

                // Assign narrative type from JSON first
                if (narrativeCellMap.TryGetValue((row, col), out string typeStr))
                {
                    if (System.Enum.TryParse(typeStr, out CellController.NarrativeCellType parsed))
                    {
                        cellController.narrativeCellType = parsed;
                        Debug.Log($"✅ Assigned narrative type {parsed} to [{row},{col}] from JSON.");
                    }
                    else
                    {
                        Debug.LogWarning($"❌ Could not parse narrativeCellType: '{typeStr}' at [{row},{col}]");
                    }
                }
                else if (!string.IsNullOrEmpty(narrativeDescriptions[row, col]))
                {
                    string desc = narrativeDescriptions[row, col].ToLower();
                    if (desc.Contains("currency"))
                        cellController.narrativeCellType = CellController.NarrativeCellType.Currency;
                    else if (desc.Contains("shop"))
                        cellController.narrativeCellType = CellController.NarrativeCellType.Shop;
                    else if (desc.Contains("relic"))
                        cellController.narrativeCellType = CellController.NarrativeCellType.RelicReward;
                    else
                        cellController.narrativeCellType = CellController.NarrativeCellType.Event;
                }

                if (cellController.narrativeCellType != CellController.NarrativeCellType.None)
                {
                    cellController.narrativeCondition = new NarrativeRules().GetCondition(cellController.narrativeCellType);
                    cellController.SetNarrativeCellColor();
                }

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
