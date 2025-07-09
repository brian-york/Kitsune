using UnityEngine;

[CreateAssetMenu(fileName = "YakoCloverRelic", menuName = "Relics/YakoCloverRelic")]
public class YakoCloverRelic : Relic
{
    public override int ModifyScore(int basePoints, TileData tile)
    {
        if (tile.tileEffect == TileEffect.Leaf)
        {
            Debug.Log("Yako's Clover relic triggered! Leaf tiles triple instead of double.");
            return basePoints * 3 / 2; 
            // triples instead of doubles = multiply by 1.5 relative to default doubling
        }

        return basePoints;
    }
}

