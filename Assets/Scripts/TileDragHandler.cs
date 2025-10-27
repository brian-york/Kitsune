using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

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

    private bool timerEnabled = false;
    private float timerDuration = 0f;
    private float warningThreshold = 0f;
    private Coroutine timerCoroutine;

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

        if (timerEnabled && EnemyManager.Instance != null)
        {
            EnemyData currentEnemy = EnemyManager.Instance.currentEnemy;
            if (currentEnemy != null && currentEnemy.hasTurnTimer)
            {
                timerDuration = currentEnemy.turnTimerDuration;
                warningThreshold = currentEnemy.turnTimerWarningThreshold;
                
                if (TurnTimerUI.Instance != null)
                {
                    TurnTimerUI.Instance.StartTimer(timerDuration, warningThreshold);
                }

                timerCoroutine = StartCoroutine(TurnTimerCoroutine());
            }
        }
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

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        if (TurnTimerUI.Instance != null)
        {
            TurnTimerUI.Instance.StopTimer();
        }
    }

    private IEnumerator TurnTimerCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < timerDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"⏱️ Turn timer expired! Tile {tileValue} destroyed.");
        
        if (TurnTimerUI.Instance != null)
        {
            TurnTimerUI.Instance.StopTimer();
        }

        Destroy(gameObject);

        GridSpawner gridSpawner = FindFirstObjectByType<GridSpawner>();
        if (gridSpawner != null)
        {
            gridSpawner.RefillTileHand();
        }
    }

    public void EnableTimer(bool enable)
    {
        timerEnabled = enable;
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
