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
    public TextMeshProUGUI currencyText;


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

public void ReturnToMap()
{
    SceneManager.LoadScene("MapScene");
}
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowNarrativeTooltip(string description, Vector2 position)
{

    Debug.Log($"[UIManager] Showing tooltip at {position} with text: {description}");

    narrativeTooltipPanel.SetActive(true);
    narrativeTooltipText.text = description;

    Vector2 offset = new Vector2(40f, -40f);
    narrativeTooltipPanel.transform.position = (Vector3)(position + offset);
}

[Header("Currency Popup")]
public GameObject currencyPopupPrefab;
public Transform popupSpawnRoot;

public void ShowCurrencyPopup(string message)
{
    if (currencyPopupPrefab != null && popupSpawnRoot != null)
    {
        GameObject popup = Instantiate(currencyPopupPrefab, popupSpawnRoot);
        var tmp = popup.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = message;

        // Optional: Animate or float up
        Destroy(popup, 1.5f); // Adjust as needed
    }
}

    public void UpdateCurrencyDisplay(int amount)
    {
        if (currencyText != null)
            currencyText.text = $"{amount} Mon";
    }


    public void HideNarrativeTooltip()
    {
        narrativeTooltipPanel.SetActive(false);
    }
    
}
