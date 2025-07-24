using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;
    public string currentPuzzleId;
    public int playerCurrency = 0;
public int TotalCurrency => playerCurrency;

public List<Relic> collectedRelics = new List<Relic>();
    public IReadOnlyList<Relic> CollectedRelics => collectedRelics;
public int totalTwosPlaced = 0;


    public void AddCurrency(int amount)
{
    playerCurrency += amount;

    UIManager ui = FindFirstObjectByType<UIManager>();
    if (ui != null)
    {
        ui.UpdateCurrencyDisplay(TotalCurrency); // Will return playerCurrency
        ui.ShowCurrencyPopup($"+{amount} Mon");
    }
}


public bool SpendCurrency(int amount)
{
    if (playerCurrency >= amount)
    {
        playerCurrency -= amount;
        Debug.Log($"🧾 Spent {amount} currency. Remaining: {playerCurrency}");
        return true;
    }
    Debug.Log("❌ Not enough currency.");
    return false;
}


    public HashSet<string> completedPuzzles = new HashSet<string>();

    private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject); // ✅ Keep it across scenes
        Debug.Log("✅ ProgressManager initialized and set to persist.");
    }
    else
    {
        Destroy(gameObject); // Avoid duplicates
    }
}

public void AcquireRelic(Relic relic)
{
    if (!collectedRelics.Contains(relic))
    {
        collectedRelics.Add(relic);
        Debug.Log($"[ProgressManager] Acquired new relic: {relic.name}");
    }
}

    public void MarkPuzzleComplete(string puzzleId)
    {
        completedPuzzles.Add(puzzleId);
        Debug.Log($"✅ ProgressManager: Marked puzzle {puzzleId} as complete.");
    }

    public bool IsPuzzleComplete(string puzzleId)
    {
        return completedPuzzles.Contains(puzzleId);
    }
}
