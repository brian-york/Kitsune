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
}
