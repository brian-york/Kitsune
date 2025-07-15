using UnityEngine;

public class ScoringManager : MonoBehaviour
{
    public GameObject popupScorePrefab;

    public TileScoreBreakdown CalculateTileScore(int row, int col, TileData tile, int[,] playerGrid)
    {
        TileScoreBreakdown breakdown = new TileScoreBreakdown();

        breakdown.basePoints = tile.number;
        breakdown.boxSum = SumBox(row, col, playerGrid);
        breakdown.rowSum = SumRow(row, playerGrid);
        breakdown.colSum = SumColumn(col, playerGrid);

        int subtotal = breakdown.basePoints + breakdown.boxSum + breakdown.rowSum + breakdown.colSum;

        breakdown.multiplier = CalculateCompletionMultiplier(row, col, playerGrid);
        subtotal = Mathf.RoundToInt(subtotal * breakdown.multiplier);

        breakdown.tileEffectBonus = 0;
        breakdown.relicBonus = 0;

        if (tile.tileEffect == TileEffect.Booned)
        {
            breakdown.tileEffectBonus += tile.scoreBonus;
            subtotal += tile.scoreBonus;
        }
        else if (tile.tileEffect == TileEffect.Leaf)
        {
            float leafMultiplier = 2f;

            GameManager gm = FindFirstObjectByType<GameManager>();
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

            breakdown.tileEffectBonus = subtotal;
            subtotal = Mathf.RoundToInt(subtotal * leafMultiplier);
        }

        GameManager gm2 = FindFirstObjectByType<GameManager>();

        if (gm2 != null && gm2.activeRelics != null)
        {
            foreach (var relic in gm2.activeRelics)
            {
                int newPoints = relic.ModifyScore(subtotal, tile);

                if (newPoints != subtotal)
                {
                    breakdown.relicBonus += newPoints - subtotal;
                }

                subtotal = newPoints;
            }
        }

        bool colComplete = IsColumnComplete(col, playerGrid);

        if (colComplete && gm2 != null && gm2.activeRelics != null)
        {
            foreach (var relic in gm2.activeRelics)
            {
                if (relic is BambooShuteRelic bambooRelic)
                {
                    Debug.Log("🌿 Bamboo Shute relic triggered! +100 points for column completion.");
                    breakdown.relicBonus += bambooRelic.BonusForColumnCompletion;
                    subtotal += bambooRelic.BonusForColumnCompletion;
                }
            }
        }

        breakdown.totalPoints = subtotal;

        Debug.Log(
            $"Score breakdown for tile {tile.number}: " +
            $"Base={breakdown.basePoints}, Box={breakdown.boxSum}, Row={breakdown.rowSum}, Col={breakdown.colSum}, " +
            $"Multiplier={breakdown.multiplier}, TileEffectBonus={breakdown.tileEffectBonus}, RelicBonus={breakdown.relicBonus} → Total={breakdown.totalPoints}"
        );

        return breakdown;
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
