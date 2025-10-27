using UnityEngine;

public enum AbilityTrigger
{
   OnTilePlaced,
    OnCorrectPlacement,
    OnInvalidPlacement,
    OnTurnStart,
    OnSpecificNumber,
    OnSpecificEffect,
    OnRegionCompleted,
    OnHarmonyThreshold,
    OnCellsFilled
}

public enum AbilityEffect
{
    BlockRandomCell,
    ModifyHarmony,
    StealMon,
    ShuffleHand,
    DestroyTile,
    AddBlockedCells,
    ForceDiscard,
    
    // NEW EFFECTS
    CorruptTileInHand,
    TransformHandToNumber,
    RestrictToBox,
    DestroyRegion,
    AddBlankTiles,
    ShrinkHandSize,
    DamageImmunity,
    DamageShield
}

[System.Serializable]
public class EnemyAbility
{
    [Header("Ability Identity")]
    public string abilityName;
    [TextArea(2, 4)]
    public string description;
    
    [Header("Trigger Conditions")]
    public AbilityTrigger trigger;
    public int triggerEveryNTurns = 0;
    public int requiredTileNumber = 0;
    public TileEffect requiredTileEffect = TileEffect.None;
    public int harmonyThreshold = 0;
    public int cellsFilledThreshold = 0;
    public bool checkTileNumberLessThan = false;
    
    [Header("Effect")]
    public AbilityEffect effect;
    public int effectValue = 1;
    
    public bool ShouldTrigger(AbilityTrigger currentTrigger, TileData tile, int turnCount, int cellsFilled, int currentHarmony)
    {
        if (trigger != currentTrigger) return false;
        
        switch (trigger)
        {
            case AbilityTrigger.OnTurnStart:
                return triggerEveryNTurns > 0 && turnCount % triggerEveryNTurns == 0;
                
            case AbilityTrigger.OnSpecificNumber:
                if (checkTileNumberLessThan)
                    return tile.number > 0 && tile.number <= requiredTileNumber;
                else
                    return tile.number == requiredTileNumber;
                    
            case AbilityTrigger.OnSpecificEffect:
                return tile.tileEffect == requiredTileEffect;
                
            case AbilityTrigger.OnHarmonyThreshold:
                if (harmonyThreshold > 0)
                    return currentHarmony >= harmonyThreshold;
                else if (harmonyThreshold < 0)
                    return currentHarmony <= harmonyThreshold;
                return false;
                
            case AbilityTrigger.OnCellsFilled:
                return cellsFilled == cellsFilledThreshold;
                
            default:
                return true;
        }
    }
}
