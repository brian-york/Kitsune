using UnityEngine;

[System.Serializable]
public class NarrativeCondition
{
    public bool requiresSpecificTile = false;
    public int requiredTileNumber;
    public TileEffect requiredTileEffect;

    public NarrativeCondition()
    {
        requiresSpecificTile = false;
        requiredTileNumber = 0;
        requiredTileEffect = TileEffect.None;
    }

    public static TileEffect ParseTileEffect(string effectString)
    {
        if (string.IsNullOrEmpty(effectString))
            return TileEffect.None;

        switch (effectString.ToLower())
        {
            case "flame": return TileEffect.Flame;
            case "leaf": return TileEffect.Leaf;
            case "booned": return TileEffect.Booned;
            case "none":
            default:
                return TileEffect.None;
        }
    }
}
