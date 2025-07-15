using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NarrativeRules
{
    public Dictionary<CellController.NarrativeCellType, NarrativeCondition> narrativeConditions;

    public NarrativeRules()
    {
        narrativeConditions = new Dictionary<CellController.NarrativeCellType, NarrativeCondition>();

        // Example rules:
        narrativeConditions[CellController.NarrativeCellType.Shop] = new NarrativeCondition
        {
            requiresSpecificTile = true,
            requiredTileNumber = 3,
            requiredTileEffect = TileEffect.None
        };

        narrativeConditions[CellController.NarrativeCellType.Boss] = new NarrativeCondition
        {
            requiresSpecificTile = true,
            requiredTileEffect = TileEffect.Flame
        };

        narrativeConditions[CellController.NarrativeCellType.RelicReward] = new NarrativeCondition
        {
            requiresSpecificTile = false
        };

        narrativeConditions[CellController.NarrativeCellType.Event] = new NarrativeCondition
        {
            requiresSpecificTile = false
        };
    }

    public NarrativeCondition GetCondition(CellController.NarrativeCellType type)
    {
        if (narrativeConditions.ContainsKey(type))
            return narrativeConditions[type];
        else
            return new NarrativeCondition(); // Default = no requirements
    }
}
