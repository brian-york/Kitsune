using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int scoreThreshold = 500;      // set in Inspector
    

    public List<Relic> activeRelics = new List<Relic>();
    
    public string lastTriggeredNarrative;
    public CellController.NarrativeCellType lastTriggeredCellType;
    public string currentPuzzleId;
   public void CheckForLevelComplete(int currentScore)
{
    if (currentScore >= scoreThreshold)
    {
        Debug.Log("🎉 YOU WIN!");

        ProgressManager progress = ProgressManager.Instance;
        if (progress != null && !string.IsNullOrEmpty(currentPuzzleId))
        {
            progress.MarkPuzzleComplete(currentPuzzleId);
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

