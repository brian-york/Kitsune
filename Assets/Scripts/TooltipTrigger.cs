using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string description;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipController tooltip = Object.FindFirstObjectByType<TooltipController>();


        if (tooltip != null)
            tooltip.ShowTooltip(description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController tooltip = Object.FindFirstObjectByType<TooltipController>();
        if (tooltip != null)
            tooltip.HideTooltip();
    }
}
