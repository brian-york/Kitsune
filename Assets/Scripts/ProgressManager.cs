using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;
    public string currentPuzzleId;
    public int playerCurrency = 0;

    public void AddCurrency(int amount)
{
    playerCurrency += amount;
    Debug.Log($"💰 Currency increased by {amount}. Total: {playerCurrency}");
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
