using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public int[,] playerGrid = new int[9, 9];
    public GameObject winText;

    void Start()
    {
        if (winText != null)
            winText.SetActive(false);
    }

    // Called whenever a cell changes
    public void UpdateCell(int row, int col, int value)
    {
        playerGrid[row, col] = value;

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

    // Check rules for this cell
    public bool IsValid(int row, int col)
    {
        int value = playerGrid[row, col];
        if (value == 0) return true;

        // Check row
        for (int c = 0; c < 9; c++)
        {
            if (c != col && playerGrid[row, c] == value)
                return false;
        }

        // Check column
        for (int r = 0; r < 9; r++)
        {
            if (r != row && playerGrid[r, col] == value)
                return false;
        }

        // Check 3×3 box
        int boxRowStart = (row / 3) * 3;
        int boxColStart = (col / 3) * 3;

        for (int r = boxRowStart; r < boxRowStart + 3; r++)
        {
            for (int c = boxColStart; c < boxColStart + 3; c++)
            {
                if ((r != row || c != col) && playerGrid[r, c] == value)
                    return false;
            }
        }

        return true;
    }

    // Check if the entire grid is filled and valid
    public bool IsComplete()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (playerGrid[r, c] == 0)
                    return false;

                if (!IsValid(r, c))
                    return false;
            }
        }

        return true;
    }

    // Called when loading a new puzzle
    public void LoadPuzzle(int[,] puzzle)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                playerGrid[row, col] = puzzle[row, col];
            }
        }
    }
}