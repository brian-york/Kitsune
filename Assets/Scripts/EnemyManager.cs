using UnityEngine;
using TMPro;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    
    [Header("Enemy Data")]
    public EnemyData currentEnemy;
    
    [Header("Combat State")]
    public int enemyCurrentHP;
    public int turnCount = 0;
    
    [Header("UI References")]
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI enemyAbilityText;
    public GameObject abilityNotificationPanel;
    public TextMeshProUGUI abilityNotificationText;
    public EnemyHPUI enemyHPUI;
    
    private PuzzleManager puzzleManager;
    private UIManager uiManager;
    private bool enemyLoaded = false;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        puzzleManager = FindFirstObjectByType<PuzzleManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        
        if (enemyHPUI == null)
            enemyHPUI = FindFirstObjectByType<EnemyHPUI>();
        
        if (abilityNotificationPanel != null)
            abilityNotificationPanel.SetActive(false);
        
        if (currentEnemy != null)
        {
            StartCoroutine(LoadEnemyAfterPuzzle());
        }
    }
    
    IEnumerator LoadEnemyAfterPuzzle()
    {
        yield return new WaitForSeconds(0.5f);
        
        while (puzzleManager == null || puzzleManager.blockedCells == null)
        {
            puzzleManager = FindFirstObjectByType<PuzzleManager>();
            yield return null;
        }
        
        LoadEnemy(currentEnemy);
    }

    public void LoadEnemy(EnemyData enemy)
    {
        currentEnemy = enemy;
        enemyCurrentHP = enemy.maxHP;
        turnCount = 0;
        enemyLoaded = true;

        Debug.Log($"⚔️ Enemy loaded: {enemy.enemyName} (HP: {enemy.maxHP})");

        ApplyStartingBlockedCells();
        UpdateEnemyUI();

        if (enemyHPUI != null)
        {
            enemyHPUI.Initialize(currentEnemy.enemyName, enemyCurrentHP, currentEnemy.maxHP);
        }
        
        ConfigureTurnTimer(); 
    }
    
    
    
   void ApplyStartingBlockedCells()
{
    if (puzzleManager == null || currentEnemy == null) 
    {
        Debug.LogError("❌ PuzzleManager or Enemy is null!");
        return;
    }
    
    if (puzzleManager.blockedCells == null)
    {
        Debug.LogError("❌ blockedCells array is null!");
        return;
    }
    
    int cellsBlocked = 0;
    int maxAttempts = currentEnemy.startingBlockedCells * 3;
    int attempts = 0;
    
    while (cellsBlocked < currentEnemy.startingBlockedCells && attempts < maxAttempts)
    {
        int row = Random.Range(0, 9);
        int col = Random.Range(0, 9);
        
        attempts++;
        
        if (!puzzleManager.blockedCells[row, col] &&
            puzzleManager.playerGrid[row, col] == 0)
        {
            puzzleManager.blockedCells[row, col] = true;
            int index = row * 9 + col;
            puzzleManager.cellStates[index] = CellState.Blocked;
            
            CellController[] allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            foreach (var cell in allCells)
            {
                if (cell.row == row && cell.column == col)
                {
                    cell.locked = true;
                    cell.isBlocked = true;
                    cell.SetBlocked(true);
                    Debug.Log($"🚫 Starting blocked cell [{row},{col}]");
                    break;
                }
            }
            
            cellsBlocked++;
        }
    }
    
    Debug.Log($"🚫 Enemy started with {cellsBlocked} blocked cells");
}

void BlockRandomCells(int count)
{
    if (puzzleManager == null)
    {
        Debug.LogError("❌ PuzzleManager is null!");
        return;
    }
    
    int cellsBlocked = 0;
    int maxAttempts = count * 5;
    int attempts = 0;
    
    while (cellsBlocked < count && attempts < maxAttempts)
    {
        int row = Random.Range(0, 9);
        int col = Random.Range(0, 9);
        
        attempts++;
        
        if (!puzzleManager.blockedCells[row, col] &&
            puzzleManager.playerGrid[row, col] == 0)
        {
            puzzleManager.blockedCells[row, col] = true;
            int index = row * 9 + col;
            puzzleManager.cellStates[index] = CellState.Blocked;
            
            CellController[] allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
            foreach (var cell in allCells)
            {
                if (cell.row == row && cell.column == col)
                {
                    cell.locked = true;
                    cell.isBlocked = true;
                    cell.SetBlocked(true);
                    Debug.Log($"🚫 Blocked cell [{row},{col}] via enemy ability");
                    break;
                }
            }
            
            cellsBlocked++;
        }
    }
    
    if (cellsBlocked < count)
    {
        Debug.LogWarning($"⚠️ Only blocked {cellsBlocked}/{count} cells (ran out of valid cells)");
    }
    else
    {
        Debug.Log($"🚫 Enemy blocked {cellsBlocked} cells!");
    }
}

public void TakeTileDamage(int tileValue, Vector3 damageSourcePosition)
{
    if (!enemyLoaded || currentEnemy == null) return;

    int damage = tileValue;
    
    enemyCurrentHP -= damage;
    if (enemyCurrentHP < 0) enemyCurrentHP = 0;
    
    Debug.Log($"💥 Enemy took {damage} tile damage! HP: {enemyCurrentHP}/{currentEnemy.maxHP}");
    
    ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
    if (scoreManager != null)
    {
        scoreManager.ShowDamagePopup(damage, damageSourcePosition, false);
    }
    
    if (enemyHPUI != null)
    {
        enemyHPUI.UpdateHP(enemyCurrentHP);
    }
    
    if (enemyCurrentHP <= 0)
    {
        OnEnemyDefeated();
    }
}


    
   public void OnTilePlaced(TileData tile, bool wasValid)
{
    if (!enemyLoaded || currentEnemy == null) return;
    
    turnCount++;
    
    Debug.Log($"🎴 Turn {turnCount}: Tile {tile.number} placed (valid: {wasValid})");
    
    if (currentEnemy.ability != null)
    {
        int currentHarmony = 0;
        int cellsFilled = CountFilledCells();
        
        if (currentEnemy.ability.trigger == AbilityTrigger.OnTurnStart &&
            currentEnemy.ability.triggerEveryNTurns > 0)
        {
            int baseInterval = currentEnemy.ability.triggerEveryNTurns;
            int delay = 0;
            
            if (CharacterManager.Instance != null)
            {
                delay = CharacterManager.Instance.GetPassiveValue(PassiveAbilityType.EnemyAbilityDelay);
            }
            
            int adjustedInterval = baseInterval + delay;
            
            Debug.Log($"🔍 Ability check: Turn {turnCount} % {adjustedInterval} (base: {baseInterval}, delay: {delay})");
            
            if (turnCount % adjustedInterval == 0)
            {
                Debug.Log($"⚡ ABILITY TRIGGERED! (Turn {turnCount}, interval: {adjustedInterval})");
                ExecuteAbility();
            }
        }
        else if (currentEnemy.ability.ShouldTrigger(
            AbilityTrigger.OnTilePlaced, 
            tile, 
            turnCount, 
            cellsFilled, 
            currentHarmony))
        {
            ExecuteAbility();
        }
        
        if (wasValid && currentEnemy.ability.ShouldTrigger(
            AbilityTrigger.OnCorrectPlacement, 
            tile, 
            turnCount, 
            cellsFilled, 
            currentHarmony))
        {
            ExecuteAbility();
        }
        
        if (!wasValid && currentEnemy.ability.ShouldTrigger(
            AbilityTrigger.OnInvalidPlacement, 
            tile, 
            turnCount, 
            cellsFilled, 
            currentHarmony))
        {
            ExecuteAbility();
        }
    }
}

    
public void OnRegionDamaged(int damage, Vector3 cellPosition)
{
    Debug.Log($"🔥 EnemyManager.OnRegionDamaged called with damage: {damage}");
    
    if (!enemyLoaded || currentEnemy == null)
    {
        Debug.LogWarning("⚠️ No enemy loaded! Cannot apply region damage.");
        return;
    }
    
    enemyCurrentHP -= damage;
    
    if (enemyCurrentHP < 0)
        enemyCurrentHP = 0;
    
    Debug.Log($"💥 Enemy took {damage} region damage! HP: {enemyCurrentHP}/{currentEnemy.maxHP}");
    
    ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
    Debug.Log($"📍 ScoreManager found: {scoreManager != null}");

    if (scoreManager != null && damage > 0)
    {
        StartCoroutine(ShowDelayedRegionPopup(scoreManager, damage, cellPosition));
    }
    else
    {
        Debug.LogWarning($"⚠️ Popup NOT shown! ScoreManager: {scoreManager != null}, Damage: {damage}");
    }
    
    UpdateEnemyUI();

    if (enemyCurrentHP <= 0)
    {
        OnEnemyDefeated();
    }
}
private IEnumerator ShowDelayedRegionPopup(ScoreManager scoreManager, int damage, Vector3 cellPosition)
{
    yield return new WaitForSeconds(0.5f);
    
    Debug.Log($"📍 Showing delayed region damage popup at cell position: {cellPosition}");
    scoreManager.ShowDamagePopup(damage, cellPosition, true);
}

private void ConfigureTurnTimer()
{
    if (currentEnemy == null) return;

    TileDragHandler[] allTiles = FindObjectsByType<TileDragHandler>(FindObjectsSortMode.None);
    foreach (var tile in allTiles)
    {
        tile.EnableTimer(currentEnemy.hasTurnTimer);
    }

    Debug.Log($"⏱️ Turn timer {(currentEnemy.hasTurnTimer ? "ENABLED" : "DISABLED")} for {currentEnemy.enemyName}");
}

    
    void OnEnemyDefeated()
    {
        Debug.Log($"🎉 {currentEnemy.enemyName} defeated!");
        CalculateRewards();
    }
    
   void ExecuteAbility()
{
    if (currentEnemy == null || currentEnemy.ability == null) return;
    
    Debug.Log($"⚡ Enemy ability triggered: {currentEnemy.ability.abilityName} ({currentEnemy.ability.effect})");
    
    ShowAbilityNotification(currentEnemy.ability.abilityName);
    
    EnemyStatusManager statusMgr = EnemyStatusManager.Instance;
    
    switch (currentEnemy.ability.effect)
    {
        case AbilityEffect.BlockRandomCell:
            BlockRandomCells(currentEnemy.ability.effectValue);
            break;
            
        case AbilityEffect.StealMon:
            StealCurrency(currentEnemy.ability.effectValue);
            break;
            
        case AbilityEffect.AddBlockedCells:
            BlockRandomCells(currentEnemy.ability.effectValue);
            break;
            
        case AbilityEffect.CorruptTileInHand:
            if (statusMgr != null) statusMgr.CorruptRandomTileInHand();
            break;
            
        case AbilityEffect.TransformHandToNumber:
            if (statusMgr != null) statusMgr.TransformHandToNumber(currentEnemy.ability.effectValue);
            break;
            
        case AbilityEffect.RestrictToBox:
            if (statusMgr != null) statusMgr.RestrictToBox(currentEnemy.restrictedBoxIndex, currentEnemy.ability.effectValue);
            break;
            
        case AbilityEffect.DestroyRegion:
            if (statusMgr != null) statusMgr.DestroyRandomRegion();
            break;
            
        case AbilityEffect.AddBlankTiles:
            if (statusMgr != null) statusMgr.AddBlankTilesToHand(currentEnemy.ability.effectValue);
            break;
            
        case AbilityEffect.ShrinkHandSize:
            if (statusMgr != null) statusMgr.ReduceHandSize(currentEnemy.ability.effectValue, 5);
            break;
            
        case AbilityEffect.DamageImmunity:
            if (statusMgr != null) statusMgr.SetDamageImmunity(currentEnemy.immuneToRegionType);
            break;
            
        case AbilityEffect.DamageShield:
            if (statusMgr != null) statusMgr.ActivateDamageShield(currentEnemy.shieldAmount);
            break;
    }
}

    
    void StealCurrency(int amount)
    {
        ProgressManager progress = ProgressManager.Instance;
        if (progress != null)
        {
            int stolen = Mathf.Min(amount, progress.TotalCurrency);
            progress.AddCurrency(-stolen);
            Debug.Log($"💰 Enemy stole {stolen} Mon");
        }
    }
    
    int CountFilledCells()
    {
        if (puzzleManager == null) return 0;
        
        int count = 0;
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (puzzleManager.playerGrid[r, c] != 0)
                    count++;
            }
        }
        return count;
    }
    
    void UpdateEnemyUI()
    {
        if (currentEnemy == null) return;
        
        if (enemyNameText != null)
            enemyNameText.text = currentEnemy.enemyName;
        
        if (enemyHPText != null)
            enemyHPText.text = $"HP: {enemyCurrentHP} / {currentEnemy.maxHP}";
        
        if (enemyAbilityText != null && currentEnemy.ability != null)
            enemyAbilityText.text = $"• {currentEnemy.ability.description}";
    }
    
    void ShowAbilityNotification(string abilityName)
    {
        if (abilityNotificationPanel != null && abilityNotificationText != null)
        {
            abilityNotificationText.text = abilityName;
            abilityNotificationPanel.SetActive(true);
            Invoke(nameof(HideAbilityNotification), 2f);
        }
    }
    
    void HideAbilityNotification()
    {
        if (abilityNotificationPanel != null)
            abilityNotificationPanel.SetActive(false);
    }
    
    void CalculateRewards()
    {
        if (currentEnemy == null) return;
        
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        int finalScore = scoreManager != null ? scoreManager.currentScore : 0;
        
        float multiplier = 1f;
        if (finalScore >= 3000) multiplier = 3f;
        else if (finalScore >= 2000) multiplier = 2f;
        else if (finalScore >= 1000) multiplier = 1.5f;
        
        int monReward = Mathf.RoundToInt(currentEnemy.baseMonReward * multiplier);
        
        ProgressManager.Instance?.AddCurrency(monReward);
        
        Debug.Log($"💰 Rewards: {monReward} Mon (base: {currentEnemy.baseMonReward}, multiplier: {multiplier}x)");
        
        if (uiManager != null)
        {
            uiManager.ShowWinPanel(finalScore);
        }
    }
}
