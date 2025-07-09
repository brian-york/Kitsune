using UnityEngine;

[CreateAssetMenu(menuName = "Relics/Bamboo Shute")]
public class BambooShuteRelic : Relic
{
    public override int ModifyScore(int baseScore, TileData tile)
    {
        return baseScore;
    }

    public int BonusForColumnCompletion => 100;
}
