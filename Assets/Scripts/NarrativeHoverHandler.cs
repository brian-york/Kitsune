using UnityEngine;
using UnityEngine.EventSystems;

public class NarrativeHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CellController parentCell;  // Manually assign in Inspector
    
    void Start()
{
    Debug.Log("[NarrativeHoverHandler] Start() called.");
}


   public void OnPointerEnter(PointerEventData eventData)
{
    Debug.Log("[Hover] Pointer entered hover detector.");

    if (parentCell == null)
    {
        Debug.LogWarning("[Hover] parentCell is null!");
        return;
    }

    Debug.Log($"[Hover] narrativeDescription = {parentCell.narrativeDescription}");

    UIManager ui = FindFirstObjectByType<UIManager>();
    if (ui != null && !string.IsNullOrEmpty(parentCell.narrativeDescription))
    {
        Debug.Log("[Hover] Triggering ShowNarrativeTooltip...");
        ui.currentlyHoveredCell = parentCell;
        ui.ShowNarrativeTooltip(parentCell.narrativeDescription, Input.mousePosition);
    }
    else
    {
        Debug.Log("[Hover] UIManager not found or narrativeDescription is empty.");
    }
}

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null && ui.currentlyHoveredCell == parentCell)
        {
            ui.currentlyHoveredCell = null;
            ui.HideNarrativeTooltip();
        }
    }
}
