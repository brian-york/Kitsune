using UnityEngine;

[CreateAssetMenu(menuName = "Relics/Kinko's Kibidongo")]
public class KinkoKibidangoRelic : Relic
{
    public override int ModifyScore(int baseScore, TileData tile)
    {
        if (tile.number != 2)
            return baseScore;

        int count = ProgressManager.Instance?.totalTwosPlaced ?? 0;
        float multiplier = 1f + 0.25f * count;

        int finalScore = Mathf.RoundToInt(baseScore * multiplier);

        Debug.Log($"🍡 Kinko's Kibidongo triggered! totalTwosPlaced={count}, multiplier={multiplier} → finalScore={finalScore}");

        return finalScore;
    }
}
