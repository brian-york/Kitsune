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

        tile.row = row;
        tile.col = col;
        RelicContext.grid = playerGrid;

        breakdown.tileEffectBonus = ProcessTileEffect(tile, subtotal, row, col, playerGrid, out int modifiedSubtotal);
        subtotal = modifiedSubtotal;

        breakdown.relicBonus = 0;
        GameManager gm = FindFirstObjectByType<GameManager>();
        
        if (gm != null && gm.activeRelics != null)
        {
            foreach (var relic in gm.activeRelics)
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
        
        if (colComplete && gm != null && gm.activeRelics != null)
        {
            foreach (var relic in gm.activeRelics)
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
            $"Score breakdown for tile {tile.number} ({tile.tileEffect}): " +
            $"Base={breakdown.basePoints}, Box={breakdown.boxSum}, Row={breakdown.rowSum}, Col={breakdown.colSum}, " +
            $"Multiplier={breakdown.multiplier}, TileEffectBonus={breakdown.tileEffectBonus}, RelicBonus={breakdown.relicBonus} → Total={breakdown.totalPoints}"
        );

        return breakdown;
    }

    private int ProcessTileEffect(TileData tile, int currentScore, int row, int col, int[,] grid, out int modifiedScore)
    {
        int bonus = 0;
        modifiedScore = currentScore;

        switch (tile.tileEffect)
        {
            case TileEffect.None:
                break;

            case TileEffect.Booned:
                bonus = tile.scoreBonus;
                modifiedScore += bonus;
                Debug.Log($"✨ Booned tile: +{bonus} flat bonus");
                break;

            case TileEffect.Leaf:
                float leafMultiplier = 2f;
                
                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null && gm.activeRelics != null)
                {
                    foreach (var relic in gm.activeRelics)
                    {
                        if (relic is YakoCloverRelic)
                        {
                            Debug.Log("🍀 Yako's Clover relic triggered! Using triple multiplier.");
                            leafMultiplier = 3f;
                            break;
                        }
                    }
                }
                
                bonus = currentScore;
                modifiedScore = Mathf.RoundToInt(currentScore * leafMultiplier);
                Debug.Log($"🍃 Leaf tile: {currentScore} × {leafMultiplier} = {modifiedScore}");
                break;

            case TileEffect.Flame:
                int flameBonus = CalculateAdjacentBonus(row, col, grid);
                bonus = flameBonus;
                modifiedScore += flameBonus;
                Debug.Log($"🔥 Flame tile: +{flameBonus} from adjacent tiles");
                break;

            case TileEffect.Baned:
                int penalty = Mathf.RoundToInt(currentScore * 0.5f);
                bonus = -penalty;
                modifiedScore -= penalty;
                Debug.Log($"💀 Baned tile: -{penalty} score penalty");
                break;

            case TileEffect.Lunar:
                int rowSum = SumRow(row, grid);
                bonus = rowSum * 2;
                modifiedScore += bonus;
                Debug.Log($"🌙 Lunar tile: Row sum {rowSum} × 2 = +{bonus}");
                break;

            case TileEffect.Solar:
                int colSum = SumColumn(col, grid);
                bonus = colSum * 2;
                modifiedScore += bonus;
                Debug.Log($"☀️ Solar tile: Col sum {colSum} × 2 = +{bonus}");
                break;

            case TileEffect.Portent:
                bonus = 50;
                modifiedScore += bonus;
                Debug.Log($"🔮 Portent tile: +{bonus} bonus (future: free next tile)");
                break;

            case TileEffect.Wild:
                bonus = 100;
                modifiedScore += bonus;
                Debug.Log($"🌟 Wild tile: +{bonus} flexibility bonus");
                break;
        }

        return bonus;
    }

    private int CalculateAdjacentBonus(int row, int col, int[,] grid)
    {
        int bonus = 0;
        int[] dr = { -1, 1, 0, 0 };
        int[] dc = { 0, 0, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int newRow = row + dr[i];
            int newCol = col + dc[i];

            if (newRow >= 0 && newRow < 9 && newCol >= 0 && newCol < 9)
            {
                if (grid[newRow, newCol] != 0)
                {
                    bonus += Mathf.RoundToInt(grid[newRow, newCol] * 0.5f);
                }
            }
        }

        return bonus;
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

        for (int c = 0; c < 9; c++)
        {
            if (grid[row, c] == 0)
            {
                rowComplete = false;
                break;
            }
        }

        for (int r = 0; r < 9; r++)
        {
            if (grid[r, col] == 0)
            {
                colComplete = false;
                break;
            }
        }

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
