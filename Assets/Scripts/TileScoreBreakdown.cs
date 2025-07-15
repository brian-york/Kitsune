using UnityEngine;

[System.Serializable]
public class TileScoreBreakdown
{
    public int basePoints;
    public int boxSum;
    public int rowSum;
    public int colSum;
    public float multiplier;
    public int tileEffectBonus;
    public int relicBonus;

    public int totalPoints;

    public TileScoreBreakdown()
    {
        basePoints = 0;
        boxSum = 0;
        rowSum = 0;
        colSum = 0;
        multiplier = 1f;
        tileEffectBonus = 0;
        relicBonus = 0;
        totalPoints = 0;
    }
}
