using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;
    
    [Header("Current Character")]
    public CharacterData currentCharacter;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetCharacter(CharacterData character)
    {
        currentCharacter = character;
        Debug.Log($"🎭 Character set to: {character.characterName}");
    }
    
    public bool HasPassive(PassiveAbilityType type)
    {
        if (currentCharacter == null) return false;
        
        foreach (var passive in currentCharacter.passives)
        {
            if (passive.type == type)
                return true;
        }
        return false;
    }
    
    public int GetPassiveValue(PassiveAbilityType type)
    {
        if (currentCharacter == null) return 0;
        
        foreach (var passive in currentCharacter.passives)
        {
            if (passive.type == type)
                return passive.value;
        }
        return 0;
    }
    
    public PassiveAbility GetPassive(PassiveAbilityType type)
    {
        if (currentCharacter == null) return null;
        
        foreach (var passive in currentCharacter.passives)
        {
            if (passive.type == type)
                return passive;
        }
        return null;
    }
    
    public void ApplyStartingBonuses()
    {
        if (currentCharacter == null) return;
        
        int startingMon = GetPassiveValue(PassiveAbilityType.StartingMon);
        if (startingMon > 0)
        {
            ProgressManager.Instance?.AddCurrency(startingMon);
            Debug.Log($"💰 Starting bonus: +{startingMon} Mon");
        }
    }
}
