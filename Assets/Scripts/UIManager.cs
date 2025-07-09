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
    
}
