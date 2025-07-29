using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string description;

    public void OnPointerEnter(PointerEventData eventData)
    {
        var uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowNarrativeTooltip(description, Input.mousePosition);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.HideNarrativeTooltip();
        }
    }
}
