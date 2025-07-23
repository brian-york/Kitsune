using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int scoreThreshold = 500;      // set in Inspector
    

    public List<Relic> activeRelics = new List<Relic>();
    
    public string lastTriggeredNarrative;
    public CellController.NarrativeCellType lastTriggeredCellType;
    public Relic testRelic;

void Start()
{
    if (!ProgressManager.Instance.collectedRelics.Contains(testRelic))
    {
        ProgressManager.Instance.AcquireRelic(testRelic);
        Debug.Log("✅ Test relic manually added to ProgressManager for testing.");
    }
}

    public void CheckForLevelComplete(int currentScore)
    {
        if (currentScore >= scoreThreshold)
        {
            Debug.Log("🎉 YOU WIN!");

            ProgressManager progress = ProgressManager.Instance;
            if (progress != null && !string.IsNullOrEmpty(progress.currentPuzzleId))
            {
                progress.MarkPuzzleComplete(progress.currentPuzzleId);
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

