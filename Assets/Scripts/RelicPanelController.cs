using UnityEngine;

public class RelicPanelController : MonoBehaviour
{
    public Transform iconContainer; // This should be Relic Icons under RelicPanel
    public GameObject relicIconPrefab;

    private void Start()
    {
        if (ProgressManager.Instance == null || ProgressManager.Instance.CollectedRelics == null)
        {
            Debug.LogWarning("No relics to display.");
            return;
        }

        foreach (Relic relic in ProgressManager.Instance.CollectedRelics)
        {
            GameObject iconGO = Instantiate(relicIconPrefab, iconContainer);
            iconGO.GetComponent<UnityEngine.UI.Image>().sprite = relic.icon;

            TooltipTrigger tooltip = iconGO.GetComponent<TooltipTrigger>();
            if (tooltip != null)
                tooltip.description = relic.description;
        }
    }
}
