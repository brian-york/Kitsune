using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour
{
    public TextMeshProUGUI tooltipText;
    public Vector2 offset = new Vector2(20f, -20f);

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        HideTooltip();
    }

    private void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        rectTransform.position = mousePos + offset;
    }

    public void ShowTooltip(string description)
    {
        tooltipText.text = description;
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = false;
    }

    public void HideTooltip()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }
}
