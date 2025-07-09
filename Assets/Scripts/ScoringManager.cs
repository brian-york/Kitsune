using UnityEngine;

public class ScoringManager : MonoBehaviour
{
    public int CalculateTileScore(int row, int col, TileData tile, int[,] playerGrid)
{
    int basePoints = tile.number;

    int boxSum = SumBox(row, col, playerGrid);
    int rowSum = SumRow(row, playerGrid);
    int colSum = SumColumn(col, playerGrid);

    int points = basePoints + boxSum + rowSum + colSum;

    float multiplier = CalculateCompletionMultiplier(row, col, playerGrid);
    points = Mathf.RoundToInt(points * multiplier);

        GameManager gm = FindFirstObjectByType<GameManager>();


        int tileEffectBonus = 0;

        if (tile.tileEffect == TileEffect.Booned)
{
    tileEffectBonus += tile.scoreBonus;
    points += tile.scoreBonus;
}
else if (tile.tileEffect == TileEffect.Leaf)
{
    float leafMultiplier = 2f;

    if (gm != null && gm.activeRelics != null)
    {
        foreach (var relic in gm.activeRelics)
        {
            if (relic is YakoCloverRelic)
            {
                Debug.Log("Yako’s Clover relic triggered! Using triple multiplier.");
                leafMultiplier = 3f;
                break;
            }
        }
    }


    tileEffectBonus = points;
    points = Mathf.RoundToInt(points * leafMultiplier);
}


    int relicBonus = 0;
    int beforeRelics = points;

   

    if (gm != null && gm.activeRelics != null)
    {
        foreach (var relic in gm.activeRelics)
        {
            int newPoints = relic.ModifyScore(points, tile);

            if (newPoints != points)
            {
                relicBonus += newPoints - points;
            }

            points = newPoints;
        }
    }

// Check for column completion relics like Bamboo Shute
bool colComplete = IsColumnComplete(col, playerGrid);

if (colComplete && gm != null && gm.activeRelics != null)
{
    foreach (var relic in gm.activeRelics)
    {
        if (relic is BambooShuteRelic bambooRelic)
        {
            Debug.Log("🌿 Bamboo Shute relic triggered! +100 points for column completion.");
            relicBonus += bambooRelic.BonusForColumnCompletion;
            points += bambooRelic.BonusForColumnCompletion;
        }
    }
}


        Debug.Log(
        $"Score breakdown for tile {tile.number}: " +
        $"Base={basePoints}, Box={boxSum}, Row={rowSum}, Col={colSum}, Multiplier={multiplier}, " +
        $"TileEffectBonus={tileEffectBonus}, RelicBonus={relicBonus} → Total={points}"
    );

    return points;
}
private bool IsColumnComplete(int col, int[,] grid)
{
    for (int row = 0; row < 9; row++)
    {
        if (grid[row, col] == 0)
        {
            return false;
        }
    }
    return true;
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
