using UnityEngine;
using TMPro;
using System.Collections;
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

        if (popupCanvasLayer == null)
        {
            var found = GameObject.Find("PopupCanvasLayer");
            if (found != null)
                popupCanvasLayer = found.transform;
            else
                Debug.LogError("PopupCanvasLayer not found!");
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

    public void ShowPopup(int amount, string label, Vector3 worldPos)
    {

        if (popupScorePrefab == null || popupCanvasLayer == null)
        {
            Debug.LogError("Popup prefab or canvas layer missing!");
            return;
        }

        var popupGO = Instantiate(popupScorePrefab, popupCanvasLayer.transform);
        var popup = popupGO.GetComponent<PopupScore>();

        Color color = KitsuneColors.DriedInkBrown;
        if (amount >= 50)
            color = KitsuneColors.KokeGreen;

        string display = $"+{amount} ({label})";
        popup.Initialize(display, color, worldPos);
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
    
    public void ShowPopupDelayed(int amount, string label, Vector3 worldPos, float delaySeconds)
{
    StartCoroutine(ShowPopupCoroutine(amount, label, worldPos, delaySeconds));
}

private IEnumerator ShowPopupCoroutine(int amount, string label, Vector3 worldPos, float delay)
{
    yield return new WaitForSeconds(delay);
    ShowPopup(amount, label, worldPos);
}

}
