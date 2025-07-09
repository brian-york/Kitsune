using UnityEngine;

[CreateAssetMenu(menuName = "Relics/Adzuki Beans")]
public class AdzukiBeansRelic : Relic
{
    public int bonusPoints = 5;

    public override int ModifyScore(int basePoints, TileData tile)
    {
        if (tile.tileEffect == TileEffect.Booned)
        {
            Debug.Log("Adzuki Beans relic triggered!");
            return basePoints + bonusPoints;
        }
        return basePoints;
    }
}
