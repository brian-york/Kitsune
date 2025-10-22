using UnityEngine;
using System.Collections.Generic;

public class TileEffectHandler : MonoBehaviour
{
    public static TileEffectHandler Instance;

    [Header("Effect Tracking")]
    private List<TileData> flameTilesOnBoard = new List<TileData>();
    private Dictionary<int, List<TileData>> tilesInRegions = new Dictionary<int, List<TileData>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ProcessOnPlacementEffect(TileData tile, int row, int col)
    {
        Debug.Log($"🎴 Processing {tile.tileEffect} effect at [{row},{col}]");

        switch (tile.tileEffect)
        {
            case TileEffect.Flame:
                ProcessFlameEffect(tile);
                break;

            case TileEffect.Solar:
                ProcessSolarEffect(row, col);
                break;

            case TileEffect.Lunar:
                ProcessLunarEffect(row, col);
                break;

            case TileEffect.Baned:
                ProcessBanedEffect(row, col);
                break;

            case TileEffect.Portent:
                ProcessPortentEffect();
                break;
        }

        TrackTileInRegion(tile, row, col);
    }

  private void ProcessFlameEffect(TileData tile)
{
    flameTilesOnBoard.Add(tile);
    int flameCount = flameTilesOnBoard.Count;
    int damage = flameCount;

    Debug.Log($"🔥 Flame tile placed! Total Flames: {flameCount}, Dealing {damage} damage");
    
    if (EnemyManager.Instance != null)
    {
        EnemyManager.Instance.OnRegionDamaged(damage);  // ✅ Use existing method
    }
}

    private void ProcessSolarEffect(int row, int col)
    {
        PuzzleManager puzzleManager = FindFirstObjectByType<PuzzleManager>();
        if (puzzleManager == null) return;

        int boxIndex = GetBoxIndex(row, col);
        List<Vector2Int> emptyCells = GetEmptyCellsInBox(boxIndex, puzzleManager);

        if (emptyCells.Count == 0)
        {
            Debug.Log("☀️ Solar: No empty cells in box to fill");
            return;
        }

        Vector2Int targetCell = emptyCells[Random.Range(0, emptyCells.Count)];
        int validNumber = FindValidNumberForCell(targetCell.x, targetCell.y, puzzleManager);

        if (validNumber > 0)
        {
            puzzleManager.UpdateCell(targetCell.x, targetCell.y, validNumber);
            
            CellController cell = GetCellController(targetCell.x, targetCell.y);
            if (cell != null)
            {
                cell.SetValue(validNumber, true);
            }

            Debug.Log($"☀️ Solar: Filled [{targetCell.x},{targetCell.y}] with {validNumber}");
        }
    }

    private void ProcessLunarEffect(int row, int col)
    {
        PuzzleManager puzzleManager = FindFirstObjectByType<PuzzleManager>();
        if (puzzleManager == null) return;

        int boxIndex = GetBoxIndex(row, col);
        List<Vector2Int> emptyCells = GetEmptyCellsInBox(boxIndex, puzzleManager);

        int cellsToBlock = Mathf.Min(2, emptyCells.Count);

        for (int i = 0; i < cellsToBlock; i++)
        {
            if (emptyCells.Count == 0) break;

            Vector2Int targetCell = emptyCells[Random.Range(0, emptyCells.Count)];
            emptyCells.Remove(targetCell);

            puzzleManager.blockedCells[targetCell.x, targetCell.y] = true;
            puzzleManager.cellStates[targetCell.x * 9 + targetCell.y] = CellState.Blocked;

            CellController cell = GetCellController(targetCell.x, targetCell.y);
            if (cell != null)
            {
                cell.SetBlocked(true);
            }

            Debug.Log($"🌙 Lunar: Blocked [{targetCell.x},{targetCell.y}]");
        }
    }

    private void ProcessBanedEffect(int row, int col)
    {
        PuzzleManager puzzleManager = FindFirstObjectByType<PuzzleManager>();
        if (puzzleManager == null) return;

        List<Vector2Int> allEmptyCells = new List<Vector2Int>();

        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (!puzzleManager.blockedCells[r, c] && puzzleManager.playerGrid[r, c] == 0)
                {
                    allEmptyCells.Add(new Vector2Int(r, c));
                }
            }
        }

        if (allEmptyCells.Count == 0)
        {
            Debug.Log("💀 Baned: No empty cells to block");
            return;
        }

        Vector2Int targetCell = allEmptyCells[Random.Range(0, allEmptyCells.Count)];

        puzzleManager.blockedCells[targetCell.x, targetCell.y] = true;
        puzzleManager.cellStates[targetCell.x * 9 + targetCell.y] = CellState.Blocked;

        CellController cell = GetCellController(targetCell.x, targetCell.y);
        if (cell != null)
        {
            cell.SetBlocked(true);
        }

        Debug.Log($"💀 Baned: Blocked random cell [{targetCell.x},{targetCell.y}]");
    }

    private void ProcessPortentEffect()
    {
        TilePoolManager poolManager = FindFirstObjectByType<TilePoolManager>();
        if (poolManager == null) return;

        Debug.Log("🔮 Portent: Revealing next 3 tiles...");
    }

    private void TrackTileInRegion(TileData tile, int row, int col)
    {
        int rowKey = row;
        int colKey = 100 + col;
        int boxKey = 200 + GetBoxIndex(row, col);

        AddToRegion(rowKey, tile);
        AddToRegion(colKey, tile);
        AddToRegion(boxKey, tile);
    }

    private void AddToRegion(int regionKey, TileData tile)
    {
        if (!tilesInRegions.ContainsKey(regionKey))
        {
            tilesInRegions[regionKey] = new List<TileData>();
        }
        tilesInRegions[regionKey].Add(tile);
    }

    public int CalculateRegionDamageModifier(int regionKey, int baseDamage)
    {
        if (!tilesInRegions.ContainsKey(regionKey))
            return baseDamage;

        List<TileData> tiles = tilesInRegions[regionKey];
        int bonusDamage = 0;
        bool hasBaned = false;

        foreach (var tile in tiles)
        {
            if (tile.tileEffect == TileEffect.Booned)
            {
                bonusDamage += tile.number;
            }

            if (tile.tileEffect == TileEffect.Baned)
            {
                hasBaned = true;
            }
        }

        int totalDamage = baseDamage + bonusDamage;

        if (hasBaned)
        {
            totalDamage *= 2;
            Debug.Log($"💀 Baned multiplier applied! {baseDamage + bonusDamage} × 2 = {totalDamage}");
        }

        if (bonusDamage > 0)
        {
            Debug.Log($"✨ Booned bonus: +{bonusDamage} damage");
        }

        return totalDamage;
    }

    private int GetBoxIndex(int row, int col)
    {
        return (row / 3) * 3 + (col / 3);
    }

    private List<Vector2Int> GetEmptyCellsInBox(int boxIndex, PuzzleManager puzzleManager)
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        int startRow = (boxIndex / 3) * 3;
        int startCol = (boxIndex % 3) * 3;

        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                if (!puzzleManager.blockedCells[r, c] && puzzleManager.playerGrid[r, c] == 0)
                {
                    emptyCells.Add(new Vector2Int(r, c));
                }
            }
        }

        return emptyCells;
    }

    private int FindValidNumberForCell(int row, int col, PuzzleManager puzzleManager)
    {
        for (int num = 1; num <= 9; num++)
        {
            if (puzzleManager.IsValid(row, col, num))
            {
                return num;
            }
        }
        return 0;
    }

    private CellController GetCellController(int row, int col)
    {
        CellController[] allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        foreach (var cell in allCells)
        {
            if (cell.row == row && cell.column == col)
            {
                return cell;
            }
        }
        return null;
    }

    public void ResetForNewPuzzle()
    {
        flameTilesOnBoard.Clear();
        tilesInRegions.Clear();
        Debug.Log("🔄 TileEffectHandler reset for new puzzle");
    }
}
