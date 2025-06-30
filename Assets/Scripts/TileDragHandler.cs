using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TileDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int tileValue;
    public TextMeshProUGUI numberText;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;

        // Move tile to root Canvas so it appears above everything
        transform.SetParent(transform.root);

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / transform.root.localScale.x;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Snap back to original spot if not dropped on target
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;
    }

    public void SetValue(int value)
    {
        tileValue = value;
        if (numberText != null)
        {
            numberText.text = value.ToString();
        }
    }
}