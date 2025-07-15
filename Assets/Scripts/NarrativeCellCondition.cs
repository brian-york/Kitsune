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
}

