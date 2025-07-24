using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    public GameObject tooltipObject;
    public TextMeshProUGUI tooltipText;

    private void Awake()
    {
        Instance = this;
        HideTooltip();
    }

    public void ShowTooltip(string text)
    {
        tooltipObject.SetActive(true);
        tooltipText.text = text;
    }

    public void HideTooltip()
    {
        tooltipObject.SetActive(false);
    }
}
