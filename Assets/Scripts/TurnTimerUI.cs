using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnTimerUI : MonoBehaviour
{
    public static TurnTimerUI Instance;

    [Header("UI References")]
    public Slider timerSlider;
    public TextMeshProUGUI timerText;
    public Image fillImage;

    [Header("Visual Feedback")]
    public Color normalColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;

    private bool isTimerActive = false;
    private float currentTime = 0f;
    private float maxTime = 0f;
    private float warningThreshold = 5f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        HideTimer();
    }

    public void StartTimer(float duration, float warningTime)
    {
        maxTime = duration;
        currentTime = duration;
        warningThreshold = warningTime;
        isTimerActive = true;

        if (timerSlider != null)
        {
            timerSlider.maxValue = duration;
            timerSlider.value = duration;
        }

        gameObject.SetActive(true);
        UpdateTimerDisplay();
    }

    public void StopTimer()
    {
        isTimerActive = false;
        HideTimer();
    }

    public void HideTimer()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isTimerActive) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isTimerActive = false;
        }

        UpdateTimerDisplay();
    }

    void UpdateTimerDisplay()
    {
        if (timerSlider != null)
        {
            timerSlider.value = currentTime;
        }

        if (timerText != null)
        {
            timerText.text = $"{Mathf.CeilToInt(currentTime)}s";
        }

        if (fillImage != null)
        {
            float percentage = currentTime / maxTime;

            if (currentTime <= warningThreshold * 0.4f)
            {
                fillImage.color = criticalColor;
            }
            else if (currentTime <= warningThreshold)
            {
                fillImage.color = warningColor;
            }
            else
            {
                fillImage.color = normalColor;
            }

            if (currentTime <= 3f)
            {
                float pulseSpeed = 5f;
                float pulse = Mathf.PingPong(Time.time * pulseSpeed, 0.3f);
                fillImage.color = Color.Lerp(criticalColor, Color.white, pulse);
            }
        }
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public bool IsTimerActive()
    {
        return isTimerActive;
    }
}
