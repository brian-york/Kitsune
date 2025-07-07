using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public int currentScore = 0;
    public TextMeshProUGUI scoreText;

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    void Start()
    {
        UpdateScoreUI();
    }

    public void SubtractPoints(int amount)
    {
        currentScore -= amount;
        if (currentScore < 0) currentScore = 0;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";
    }
}