using UnityEngine;
using System.Collections.Generic;

public class GridSpawner : MonoBehaviour
{
    [Header("Grid System Toggle")]
    public bool useNewGridSystem = false;
    
    [Header("UI Grid (Old System)")]
    public GameObject cellPrefab;
    
    [Header("GameObject Grid (New System)")]
    public GameObject cellPrefabV2;
    public Transform gridContainer;
    
    [Header("Grid Layout Settings")]
    public float cellSize = 0.8f;
    public float cellGap = 0.08f;
    public float boxDividerGap = 0.12f;
    
    [Header("Tile System")]
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

        PuzzleManager puzzleManager = FindFirstObjectByType<PuzzleManager>();
        if (puzzleManager != null)
        {
            puzzleManager.isLoading = true;
            puzzleManager.LoadPuzzle(puzzleData);
            puzzleManager.isLoading = false;
        }
        else
        {
            Debug.LogError("PuzzleManager not found!");
        }

        int[,] puzzleGrid = new int[9, 9];
        string[,] cellStateGrid = new string[9, 9];
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int index = row * 9 + col;
                puzzleGrid[row, col] = puzzleData.grid[index];
                cellStateGrid[row, col] = (puzzleData.cellStates != null && puzzleData.cellStates.Count > index)
                    ? puzzleData.cellStates[index]
                    : "Playable";
            }
        }

        GenerateGrid(puzzleGrid, cellStateGrid, narrativeDescriptions, puzzleData);
    }

    private void ApplyNarrativeRequirementOverride(CellController cellController, int row, int col, PuzzleData puzzleData)
    {
        if (puzzleData == null || puzzleData.narrativeRequirements == null)
            return;

        foreach (var requirement in puzzleData.narrativeRequirements)
        {
            if (requirement.row == row && requirement.col == col)
            {
                cellController.narrativeCondition = new NarrativeCondition
                {
                    requiresSpecificTile = requirement.requiresSpecificTile,
                    requiredTileNumber = requirement.requiredTileNumber,
                    requiredTileEffect = NarrativeCondition.ParseTileEffect(requirement.requiredTileEffect)
                };

                Debug.Log($"🎯 Applied custom requirement to [{row},{col}]: " +
                          $"RequireSpecific={requirement.requiresSpecificTile}, " +
                          $"Number={requirement.requiredTileNumber}, " +
                          $"Effect={requirement.requiredTileEffect}");
                
                return;
            }
        }
    }

    public void CreateTile(TileData tileData)
    {
        GameObject tileObj = Instantiate(tileButton, tilePoolTransform);
        TileDragHandler handler = tileObj.GetComponent<TileDragHandler>();
        
        if (handler != null)
        {
            handler.SetTileData(tileData);
            
            if (EnemyManager.Instance != null && EnemyManager.Instance.currentEnemy != null)
            {
                handler.EnableTimer(EnemyManager.Instance.currentEnemy.hasTurnTimer);
            }
        }
        else
        {
            Debug.LogError("TileDragHandler missing from tileButton prefab!");
        }
    }

    public void RefillTileHand()
    {
        int baseTileCount = 3;
        int bonusTiles = 0;

        if (CharacterManager.Instance != null)
        {
            bonusTiles = CharacterManager.Instance.GetPassiveValue(PassiveAbilityType.ExtraTilesPerDraw);
        }
        int enemyReduction = 0;
        if (EnemyStatusManager.Instance != null)
        {
            enemyReduction = EnemyStatusManager.Instance.handSizeReduction;
        }
        
        int targetTileCount = baseTileCount + bonusTiles;
        
        int currentTiles = tilePoolTransform.childCount;
        
        while (currentTiles < targetTileCount)
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
        
        if (bonusTiles > 0)
        {
            Debug.Log($"🎴 Hand refilled to {currentTiles} tiles (base: {baseTileCount}, bonus: {bonusTiles})");
        }
    }

    void GenerateGrid(int[,] puzzle, string[,] cellStates, string[,] narrativeDescriptions, PuzzleData puzzleData)
    {
        var narrativeCellMap = new Dictionary<(int, int), string>();
        List<NarrativeCellEntry> narrativeCells = puzzleData.narrativeCells;
        if (narrativeCells != null)
        {
            foreach (var entry in narrativeCells)
                narrativeCellMap[(entry.row, entry.col)] = entry.narrativeCellType;
        }

        GameObject prefabToUse = useNewGridSystem ? cellPrefabV2 : cellPrefab;
        Transform parentTransform = useNewGridSystem ? gridContainer : transform;

        if (useNewGridSystem)
        {
            Debug.Log("🆕 Using NEW GameObject Grid System");
        }
        else
        {
            Debug.Log("📱 Using OLD UI Grid System");
        }

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int value = puzzle[row, col];
                bool isBlocked = cellStates[row, col] == "Blocked";
                bool locked = value != 0;

                Vector3 cellPosition = Vector3.zero;
                if (useNewGridSystem)
                {
                    float xPos = CalculateCellPosition(col);
                    float yPos = -CalculateCellPosition(row);
                    cellPosition = new Vector3(xPos, yPos, 0);
                }

                GameObject newCell = Instantiate(prefabToUse, parentTransform);
                newCell.name = $"Cell_{row}_{col}";
                
                if (useNewGridSystem)
                {
                    newCell.transform.localPosition = cellPosition;
                }

                if (useNewGridSystem)
                {
                    SetupNewGridCell(newCell, row, col, value, locked, isBlocked);
                }
                else
                {
                    SetupOldGridCell(newCell, row, col, value, locked, isBlocked, narrativeCells, narrativeDescriptions, narrativeCellMap, puzzleData);
                }
            }
        }
    }

    void SetupNewGridCell(GameObject newCell, int row, int col, int value, bool locked, bool isBlocked)
    {
        var cellControllerV2 = newCell.GetComponent<CellControllerV2>();
        if (cellControllerV2 != null)
        {
            cellControllerV2.SetupCell(row, col);
            cellControllerV2.SetValue(value, locked);
            if (isBlocked)
            {
                cellControllerV2.SetBlocked(true);
            }
        }
        else
        {
            Debug.LogError($"CellControllerV2 missing on cell prefab at [{row},{col}]!");
        }

        var dropHandler = newCell.GetComponent<CellDropHandler>();
        if (dropHandler != null)
        {
            dropHandler.row = row;
            dropHandler.col = col;
        }
    }

    void SetupOldGridCell(GameObject newCell, int row, int col, int value, bool locked, bool isBlocked, 
                          List<NarrativeCellEntry> narrativeCells, string[,] narrativeDescriptions,
                          Dictionary<(int, int), string> narrativeCellMap, PuzzleData puzzleData)
    {
        var cellController = newCell.GetComponent<CellController>();
        cellController.SetupCell(row, col);
        
        string description = "";
        if (narrativeCells != null)
        {
            foreach (var entry in narrativeCells)
            {
                if (entry.row == row && entry.col == col)
                {
                    description = entry.description;
                    break;
                }
            }
        }
        cellController.narrativeDescription = description;

        if (!string.IsNullOrEmpty(cellController.narrativeDescription))
        {
            Debug.Log($"[GridSpawner] NarrativeDesc at [{row},{col}] = {cellController.narrativeDescription}");
        }
        else
        {
            Debug.LogWarning($"[GridSpawner] No narrativeDesc at [{row},{col}] — narrativeCellType might not work!");
        }

        if (narrativeCellMap.TryGetValue((row, col), out string typeStr))
        {
            if (System.Enum.TryParse(typeStr, out CellController.NarrativeCellType parsed))
            {
                cellController.narrativeCellType = parsed;
                Debug.Log($"✅ Assigned narrative type {parsed} to [{row},{col}] from JSON.");
            }

            if (parsed == CellController.NarrativeCellType.Currency)
            {
                Debug.Log($"💰 Confirmed: Currency cell at [{row},{col}] from JSON.");
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
            {
                cellController.narrativeCellType = CellController.NarrativeCellType.Currency;
                Debug.Log($"💰 Assigned fallback Currency type to [{row},{col}] based on narrative description: {desc}");
            }
            else if (desc.Contains("shop"))
            {
                cellController.narrativeCellType = CellController.NarrativeCellType.Shop;
                Debug.Log($"🛒 Assigned fallback Shop type to [{row},{col}] based on narrative description: {desc}");
            }
            else if (desc.Contains("relic"))
            {
                cellController.narrativeCellType = CellController.NarrativeCellType.RelicReward;
                Debug.Log($"✨ Assigned fallback RelicReward type to [{row},{col}] based on narrative description: {desc}");
            }
            else
            {
                cellController.narrativeCellType = CellController.NarrativeCellType.Event;
                Debug.Log($"📜 Assigned fallback Event type to [{row},{col}] based on narrative description: {desc}");
            }
        }

        if (cellController.narrativeCellType != CellController.NarrativeCellType.None)
        {
            cellController.narrativeCondition = new NarrativeRules().GetCondition(cellController.narrativeCellType);
            
            ApplyNarrativeRequirementOverride(cellController, row, col, puzzleData);
            
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

    float CalculateCellPosition(int index)
    {
        float pos = 0f;
        
        for (int i = 0; i < index; i++)
        {
            pos += cellSize;
            
            if ((i + 1) % 3 == 0 && i < 8)
                pos += boxDividerGap;
            else
                pos += cellGap;
        }
        
        float totalSize = (cellSize * 9) + (cellGap * 6) + (boxDividerGap * 2);
        return pos - (totalSize / 2f);
    }
}
