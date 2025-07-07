using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TileDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int tileValue;
    public TileEffect tileEffect;
    public int scoreBonus;
    public TextMeshProUGUI numberText;
    public TileData tileData;
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

    public void SetTileData(TileData tileData)
{
    this.tileData = tileData;            // Save the entire TileData object here

    tileValue = tileData.number;
    tileEffect = tileData.tileEffect;
    scoreBonus = tileData.scoreBonus;

    if (numberText != null)
    {
        numberText.text = tileData.number.ToString();
    }

    // Optional: Apply tile color
    UnityEngine.UI.Image img = GetComponent<UnityEngine.UI.Image>();
    if (img != null)
    {
        img.color = tileData.tileColor;
    }
}
}