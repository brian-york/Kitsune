using UnityEngine;

public class ScoringManager : MonoBehaviour
{
    public int CalculateTileScore(int row, int col, TileData tile, int[,] playerGrid)
    {
        int basePoints = tile.number;

        // Sum values in the same 3x3 box
        int boxSum = SumBox(row, col, playerGrid);

        // Sum values in the same row
        int rowSum = SumRow(row, playerGrid);

        // Sum values in the same column
        int colSum = SumColumn(col, playerGrid);

        int points = basePoints + boxSum + rowSum + colSum;

        // Apply tile-specific effects
        if (tile.tileEffect == TileEffect.Booned)
        {
            points += tile.scoreBonus;
        }
        else if (tile.tileEffect == TileEffect.Leaf)
        {
            points *= 2;
        }

        // Check for completion multipliers
        float multiplier = CalculateCompletionMultiplier(row, col, playerGrid);
        points = Mathf.RoundToInt(points * multiplier);

        Debug.Log($"Score breakdown for placed tile {tile.number}: Base={basePoints}, Box={boxSum}, Row={rowSum}, Col={colSum}, Multiplier={multiplier} → Total={points}");

        return points;
    }

    int SumBox(int row, int col, int[,] grid)
    {
        int sum = 0;
        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;

        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                if (grid[r, c] != 0)
                    sum += grid[r, c];
            }
        }

        return sum;
    }

    int SumRow(int row, int[,] grid)
    {
        int sum = 0;
        for (int c = 0; c < 9; c++)
        {
            if (grid[row, c] != 0)
                sum += grid[row, c];
        }
        return sum;
    }

    int SumColumn(int col, int[,] grid)
    {
        int sum = 0;
        for (int r = 0; r < 9; r++)
        {
            if (grid[r, col] != 0)
                sum += grid[r, col];
        }
        return sum;
    }

    float CalculateCompletionMultiplier(int row, int col, int[,] grid)
    {
        bool rowComplete = true;
        bool colComplete = true;
        bool boxComplete = true;

        // Check row
        for (int c = 0; c < 9; c++)
        {
            if (grid[row, c] == 0)
            {
                rowComplete = false;
                break;
            }
        }

        // Check column
        for (int r = 0; r < 9; r++)
        {
            if (grid[r, col] == 0)
            {
                colComplete = false;
                break;
            }
        }

        // Check 3×3 box
        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;

        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                if (grid[r, c] == 0)
                {
                    boxComplete = false;
                    break;
                }
            }
        }

        float multiplier = 1f;
        if (rowComplete) multiplier *= 2f;
        if (colComplete) multiplier *= 2f;
        if (boxComplete) multiplier *= 3f;

        return multiplier;
    }
}
