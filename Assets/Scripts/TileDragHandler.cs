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
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;
    }

    public void SetTileData(TileData tileData)
    {
        this.tileData = tileData;
        tileValue = tileData.number;
        tileEffect = tileData.tileEffect;
        scoreBonus = tileData.scoreBonus;

        if (numberText != null)
        {
            string symbol = ProgressManager.GetSymbolForEffect(tileEffect);
            numberText.text = string.IsNullOrEmpty(symbol) 
                ? tileData.number.ToString() 
                : $"{symbol} {tileData.number}";
        }

        var img = GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            img.color = tileData.tileColor;
        }
    }
}
