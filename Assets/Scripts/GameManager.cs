using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int scoreThreshold = 500;
    
    public List<Relic> activeRelics = new List<Relic>();
    
    public string lastTriggeredNarrative;
    public CellController.NarrativeCellType lastTriggeredCellType;

    void Start()
    {
        if (ProgressManager.Instance != null)
        {
            activeRelics.Clear();
            activeRelics.AddRange(ProgressManager.Instance.collectedRelics);
            Debug.Log($"✅ Loaded {activeRelics.Count} relics into GameManager");
        }
    }

    public void CheckForLevelComplete(int currentScore)
    {
        if (currentScore >= scoreThreshold)
        {
            Debug.Log("🎉 YOU WIN!");
            Debug.Log($"🔍 Attempting to mark node completed: '{ProgressManager.Instance.currentNodeName}'");

            ProgressManager progress = ProgressManager.Instance;
            if (progress != null && !string.IsNullOrEmpty(progress.currentPuzzleId))
            {
                ProgressManager.Instance.MarkNodeCompleted(ProgressManager.Instance.currentNodeName);
            }

            UIManager ui = FindFirstObjectByType<UIManager>();
            if (ui != null)
                ui.ShowWinPanel(currentScore);
        }
    }

    public void TriggerGameOver()
    {
        Debug.Log("💀 GAME OVER!");
        UIManager ui = FindFirstObjectByType<UIManager>();
        ScoreManager sm = FindFirstObjectByType<ScoreManager>();
        if (ui != null && sm != null)
            ui.ShowGameOverPanel(sm.currentScore);
    }
}
