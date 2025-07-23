using UnityEngine;

[CreateAssetMenu(menuName = "Relics/Oni Mask")]
public class OniMaskRelic : Relic
{
    public override int ModifyScore(int baseScore, TileData tile)
{
    Debug.Log("👺 Oni Mask ModifyScore called.");

    if (RelicContext.grid == null)
    {
        Debug.LogWarning("👺 Oni Mask failed: RelicContext.grid is null.");
        return baseScore;
    }

    int[,] grid = RelicContext.grid;

    int startRow = (tile.row / 3) * 3;
    int startCol = (tile.col / 3) * 3;

    int emptyCount = 0;

   for (int r = startRow; r < startRow + 3; r++)
{
    for (int c = startCol; c < startCol + 3; c++)
    {
        if (r == tile.row && c == tile.col) continue;

        Debug.Log($"👁️ Checking cell [{r},{c}] = {grid[r, c]}");

        if (grid[r, c] == 0)
            emptyCount++;
    }
}

    int bonus = emptyCount * 10;
    Debug.Log($"👺 Oni Mask triggered! Empty cells in 3x3: {emptyCount}, bonus: {bonus}");

    return baseScore + bonus;
}
}