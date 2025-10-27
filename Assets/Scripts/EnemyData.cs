using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Kitsune/Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy Identity")]
    public string enemyName;
    public Sprite enemySprite;
    [TextArea(2, 4)]
    public string flavorText;
    
    [Header("Combat Stats")]
    public int maxHP = 200;
    public int startingBlockedCells = 3;
    
    [Header("Ability")]
    public EnemyAbility ability;

    [Header("Rewards")]
    public int baseMonReward = 30;
   [Header("Advanced Ability Settings")]
    public RegionType immuneToRegionType = RegionType.Row;
    public int shieldAmount = 50;
    public int restrictedBoxIndex = 0;

    [Header("Turn Timer Ability")]
public bool hasTurnTimer = false;
[Tooltip("Time in seconds before tile is destroyed if not placed")]
public float turnTimerDuration = 20f;
[Tooltip("Warning threshold in seconds (visual feedback intensifies)")]
public float turnTimerWarningThreshold = 5f;

}
