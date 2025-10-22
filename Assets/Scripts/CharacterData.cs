using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Kitsune/Character")]
public class CharacterData : ScriptableObject
{
    [Header("Character Identity")]
    public string characterName;
    [TextArea(3, 5)]
    public string description;
    public Sprite portrait;
    
    [Header("Passive Abilities")]
    public PassiveAbility[] passives;
}

[System.Serializable]
public class PassiveAbility
{
    public string abilityName;
    [TextArea(2, 3)]
    public string description;
    public PassiveAbilityType type;
    public int value;
}

public enum PassiveAbilityType
{
    None,
    ExtraTilesPerDraw,
    PartialRegionDamageBonus,
    EnemyAbilityDelay,
    StartingMon,
    BonusMonPerRegion,
    BlockedCellImmunity,
    DoubleRegionDamage
}
