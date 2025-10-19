using UnityEngine;
using System.Linq;
using System;
public class PuzzleManager : MonoBehaviour
{
    public int[,] playerGrid = new int[9, 9];
    public GameObject winText;
    public bool[,] blockedCells;
    public CellState[] cellStates = new CellState[81];  // Ensure it's always initialized

    public bool isLoading = false;

    void Start()
    {
        if (winText != null)
            winText.SetActive(false);

        if (ProgressManager.Instance != null)
        {
            UIManager ui = FindFirstObjectByType<UIManager>();
            ui?.UpdateCurrencyDisplay(ProgressManager.Instance.TotalCurrency);
        }
    }

    public void UpdateCell(int row, int col, int value)
    {
        if (isLoading)
            return;

        if (blockedCells != null && blockedCells[row, col])
        {
            Debug.Log($"⚠️ Attempt to update BLOCKED cell [{row},{col}]. Skipping grid update.");
            return;
        }

        if (value == 2)
        {
            ProgressManager.Instance.totalTwosPlaced++;
            Debug.Log($"🍡 totalTwosPlaced incremented to: {ProgressManager.Instance.totalTwosPlaced}");
        }

        playerGrid[row, col] = value;

        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();

        if (!IsValid(row, col))
        {
            Debug.Log($"Invalid value at [{row},{col}]");
        }
        else
        {
            Debug.Log("So far, valid!");
        }

        if (IsComplete())
        {
            Debug.Log("🎉 Puzzle solved!");
            if (winText != null)
                winText.SetActive(true);
        }
    }

    public bool IsValid(int row, int col, int value)
    {
        if (value == 0) return true;

        // Row check
        for (int c = 0; c < 9; c++)
        {
            if (c != col && playerGrid[row, c] == value && cellStates[row * 9 + c] != CellState.Blocked)
                return false;
        }

        // Column check
        for (int r = 0; r < 9; r++)
        {
            if (r != row && playerGrid[r, col] == value && cellStates[r * 9 + col] != CellState.Blocked)
                return false;
        }

        // 3x3 Box check
        int boxRowStart = (row / 3) * 3;
        int boxColStart = (col / 3) * 3;

        for (int r = boxRowStart; r < boxRowStart + 3; r++)
        {
            for (int c = boxColStart; c < boxColStart + 3; c++)
            {
                if ((r != row || c != col) &&
                    playerGrid[r, c] == value &&
                    cellStates[r * 9 + c] != CellState.Blocked)
                    return false;
            }
        }

        return true;
    }

    public bool IsValid(int row, int col)
    {
        int value = playerGrid[row, col];
        return IsValid(row, col, value);
    }

    public bool IsComplete()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (blockedCells != null && blockedCells[r, c])
                    continue;

                if (playerGrid[r, c] == 0)
                    return false;

                if (!IsValid(r, c))
                    return false;
            }
        }
        return true;
    }

    // ✅ New unified loader to handle grid + cellStates
    public void LoadPuzzle(PuzzleData puzzleData)
    {
        if (puzzleData.grid == null || puzzleData.grid.Count != 81)
        {
            Debug.LogError("❌ Invalid puzzle grid.");
            return;
        }

        // Fill playerGrid
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int index = row * 9 + col;
                playerGrid[row, col] = puzzleData.grid[index];
            }
        }

        // Populate cellStates from strings to enums
        if (puzzleData.cellStates == null || puzzleData.cellStates.Count != 81)
        {
            Debug.LogWarning("⚠️ Missing or invalid cellStates. Defaulting to all Playable.");
            cellStates = Enumerable.Repeat(CellState.Playable, 81).ToArray();
        }
        else
        {
            cellStates = puzzleData.cellStates.Select(s => Enum.Parse<CellState>(s)).ToArray();
        }

        // Populate blockedCells based on cellStates
        blockedCells = new bool[9, 9];
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int index = row * 9 + col;
                blockedCells[row, col] = cellStates[index] == CellState.Blocked;
            }
        }

        Debug.Log("✅ Puzzle loaded with grid, cellStates, and blockedCells.");
    }
public void DisableOtherNarrativeCells(CellController triggeredCell)
{
    CellController[] allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
    
    foreach (var cell in allCells)
    {
        if (cell != triggeredCell && 
            cell.narrativeCellType != CellController.NarrativeCellType.None && 
            !cell.narrativeTriggered)
        {
            cell.SetBlocked(true);
            
            var image = cell.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                Color fadedColor = image.color;
                fadedColor.a = 0.3f;
                image.color = fadedColor;
            }
            
            Debug.Log($"🚫 Disabled narrative cell: {cell.narrativeCellType}");
        }
    }
}


}
