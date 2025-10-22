using UnityEngine;

public class HarmonyManager : MonoBehaviour
{
    public static HarmonyManager Instance;

    [Header("Harmony Settings")]
    public int currentHarmony = 0;
    public int minHarmony = -10;
    public int maxHarmony = 10;

    [Header("Optional UI")]
    public TMPro.TextMeshProUGUI harmonyDisplayText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentHarmony = 0;
        UpdateHarmonyDisplay();
        Debug.Log("🎴 HarmonyManager initialized. Starting Harmony: 0");
    }

    public void AddHarmony(int amount, string reason = "")
    {
        currentHarmony += amount;
        currentHarmony = Mathf.Clamp(currentHarmony, minHarmony, maxHarmony);
        
        Debug.Log($"⚖️ Harmony changed by {amount:+#;-#;0} → Now: {currentHarmony} (Reason: {reason})");
        UpdateHarmonyDisplay();
    }

    public void SetHarmony(int value)
    {
        currentHarmony = Mathf.Clamp(value, minHarmony, maxHarmony);
        UpdateHarmonyDisplay();
    }

    void UpdateHarmonyDisplay()
    {
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.UpdateHarmonyDisplay(currentHarmony);
        }

        HarmonyMeterUI meterUI = FindFirstObjectByType<HarmonyMeterUI>();
        if (meterUI != null)
        {
            meterUI.UpdateHarmonyMeter(currentHarmony);
        }

        if (harmonyDisplayText != null)
        {
            harmonyDisplayText.text = $"⚖️ Harmony: {currentHarmony}";
        }
    }
}
