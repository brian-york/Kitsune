using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject winPanel;
    public GameObject gameOverPanel;

    [Header("Text Fields")]
    public TextMeshProUGUI winScoreText;
    public TextMeshProUGUI gameOverScoreText;
    public GameObject narrativeTooltipPanel;
    public TextMeshProUGUI narrativeTooltipText;
    public CellController currentlyHoveredCell;


    public void ShowWinPanel(int score)
    {
        if (winScoreText != null)
            winScoreText.text = $"Score: {score}";

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void ShowGameOverPanel(int score)
    {
        if (gameOverScoreText != null)
            gameOverScoreText.text = $"Score: {score}";

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowNarrativeTooltip(string description, Vector2 position)
{
    narrativeTooltipPanel.SetActive(true);
    narrativeTooltipText.text = description;

    Vector2 offset = new Vector2(40f, -40f);
    narrativeTooltipPanel.transform.position = (Vector3)(position + offset);
}



    public void HideNarrativeTooltip()
    {
        narrativeTooltipPanel.SetActive(false);
    }
    
}
