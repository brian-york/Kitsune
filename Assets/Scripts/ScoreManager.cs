using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public int currentScore = 0;
    public TextMeshProUGUI scoreText;
    public GameObject popupScorePrefab;
    public Transform popupCanvasLayer;

    void Start()
    {
        UpdateScoreUI();

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            AddThreshold(gm.scoreThreshold);
        }
    }

    public void AddThreshold(int threshold)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {currentScore} / {threshold}";
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    public void ShowPopup(int amount, string label, Vector3 worldPosition)
    {
        if (popupScorePrefab != null)
        {
            var popupGO = Instantiate(popupScorePrefab, transform.root);
            var popup = popupGO.GetComponent<PopupScore>();

            Color color = KitsuneColors.DriedInkBrown;
            if (amount >= 50) color = KitsuneColors.KokeGreen;

            string text = $"+{amount} ({label})";
            popup.Initialize(amount, color, worldPosition);
        }
    }

    private void UpdateScoreUI()
    {
        GameManager gm = FindFirstObjectByType<GameManager>();

        if (scoreText != null)
        {
            int threshold = gm != null ? gm.scoreThreshold : 0;
            scoreText.text = $"Score: {currentScore} / {threshold}";
        }
    }

    public void SubtractPoints(int amount)
    {
        currentScore -= amount;
        if (currentScore < 0) currentScore = 0;
        UpdateScoreUI();
    }
}
