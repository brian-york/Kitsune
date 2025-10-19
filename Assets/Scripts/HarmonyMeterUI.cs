using UnityEngine;
using UnityEngine.UI;

public class HarmonyMeterUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform harmonyBarBackground;
    public RectTransform harmonyIndicator;
    public Image glowImage;

    [Header("Settings")]
    public int minHarmony = -100;
    public int maxHarmony = 100;
    public Color yinGlowColor = new Color(0.3f, 0.1f, 0.5f, 0.8f);
    public Color yangGlowColor = new Color(1f, 0.7f, 0.3f, 0.8f);
    public Color neutralGlowColor = new Color(1f, 1f, 1f, 0.3f);

    public void UpdateHarmonyMeter(int currentHarmony)
    {
        if (harmonyBarBackground == null || harmonyIndicator == null) return;

        float barWidth = harmonyBarBackground.rect.width;
        float normalizedValue = Mathf.InverseLerp(minHarmony, maxHarmony, currentHarmony);
        
        float xPosition = Mathf.Lerp(-barWidth / 2f, barWidth / 2f, normalizedValue);
        
        harmonyIndicator.anchoredPosition = new Vector2(xPosition, harmonyIndicator.anchoredPosition.y);

        UpdateGlowColor(currentHarmony);
    }

    void UpdateGlowColor(int harmony)
    {
        if (glowImage == null) return;

        if (harmony < 0)
        {
            float t = Mathf.InverseLerp(minHarmony, 0, harmony);
            glowImage.color = Color.Lerp(yinGlowColor, neutralGlowColor, t);
        }
        else if (harmony > 0)
        {
            float t = Mathf.InverseLerp(0, maxHarmony, harmony);
            glowImage.color = Color.Lerp(neutralGlowColor, yangGlowColor, t);
        }
        else
        {
            glowImage.color = neutralGlowColor;
        }
    }
}
