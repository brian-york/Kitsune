using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyStatusVisualizer : MonoBehaviour
{
    public static EnemyStatusVisualizer Instance;
    
    [Header("Corrupted Tile Effect")]
    public Color corruptedGlowColor = new Color(0.8f, 0f, 0.8f, 1f);
    public float glowSpeed = 2f;
    
    [Header("Box Restriction Effect")]
    public Color allowedBoxColor = new Color(0.2f, 1f, 0.2f, 0.3f);
    public Color restrictedBoxColor = new Color(1f, 0.2f, 0.2f, 0.2f);
    
    private GameObject[] boxHighlights = new GameObject[9];
    private Coroutine corruptedGlowCoroutine;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        UpdateCorruptedTileVisuals();
        UpdateBoxRestrictionVisuals();
    }
    
    void UpdateCorruptedTileVisuals()
    {
        EnemyStatusManager statusMgr = EnemyStatusManager.Instance;
        if (statusMgr == null) return;
        
        if (statusMgr.hasCorruptedTile && statusMgr.corruptedTile != null)
        {
            if (corruptedGlowCoroutine == null)
            {
                corruptedGlowCoroutine = StartCoroutine(GlowCorruptedTile(statusMgr.corruptedTile));
            }
        }
        else
        {
            if (corruptedGlowCoroutine != null)
            {
                StopCoroutine(corruptedGlowCoroutine);
                corruptedGlowCoroutine = null;
            }
        }
    }
    
    IEnumerator GlowCorruptedTile(TileDragHandler tile)
    {
        Image tileImage = tile.GetComponent<Image>();
        if (tileImage == null) yield break;
        
        Color originalColor = new Color(0.5f, 0f, 0.5f);
        
        while (true)
        {
            float glow = Mathf.PingPong(Time.time * glowSpeed, 1f);
            tileImage.color = Color.Lerp(originalColor, corruptedGlowColor, glow);
            yield return null;
        }
    }
    
    void UpdateBoxRestrictionVisuals()
    {
        EnemyStatusManager statusMgr = EnemyStatusManager.Instance;
        if (statusMgr == null) return;
        
        if (statusMgr.isRestrictedToBox)
        {
            ShowBoxRestriction(statusMgr.restrictedBoxIndex);
        }
        else
        {
            ClearBoxHighlights();
        }
    }
    
    public void ShowBoxRestriction(int allowedBoxIndex)
    {
        ClearBoxHighlights();
        
        CellController[] allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        
        for (int i = 0; i < 9; i++)
        {
            GameObject highlight = CreateBoxHighlight(i);
            boxHighlights[i] = highlight;
            
            Image img = highlight.GetComponent<Image>();
            if (img != null)
            {
                img.color = (i == allowedBoxIndex) ? allowedBoxColor : restrictedBoxColor;
            }
        }
    }
    
    GameObject CreateBoxHighlight(int boxIndex)
    {
        int startRow = (boxIndex / 3) * 3;
        int startCol = (boxIndex % 3) * 3;
        
        CellController firstCell = GetCellController(startRow, startCol);
        if (firstCell == null) return null;
        
        GameObject highlight = new GameObject($"BoxHighlight_{boxIndex}");
        RectTransform highlightRect = highlight.AddComponent<RectTransform>();
        Image highlightImage = highlight.AddComponent<Image>();
        
        highlightImage.raycastTarget = false;
        
        RectTransform cellRect = firstCell.GetComponent<RectTransform>();
        if (cellRect != null)
        {
            highlight.transform.SetParent(firstCell.transform.parent, false);
            
            float cellWidth = cellRect.rect.width;
            float cellHeight = cellRect.rect.height;
            
            highlightRect.sizeDelta = new Vector2(cellWidth * 3, cellHeight * 3);
            highlightRect.anchoredPosition = cellRect.anchoredPosition;
            
            highlight.transform.SetSiblingIndex(0);
        }
        
        return highlight;
    }
    
    void ClearBoxHighlights()
    {
        for (int i = 0; i < boxHighlights.Length; i++)
        {
            if (boxHighlights[i] != null)
            {
                Destroy(boxHighlights[i]);
                boxHighlights[i] = null;
            }
        }
    }
    
    private CellController GetCellController(int row, int col)
    {
        CellController[] allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        foreach (var cell in allCells)
        {
            if (cell.row == row && cell.column == col)
            {
                return cell;
            }
        }
        return null;
    }
    
    public void ShowFloatingText(string message, Vector3 worldPosition, Color color)
    {
        GameObject textObj = new GameObject("FloatingText");
        textObj.transform.position = worldPosition;
        
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            textObj.transform.SetParent(canvas.transform, false);
        }
        
        TMPro.TextMeshProUGUI text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = message;
        text.fontSize = 24;
        text.color = color;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);
        
        StartCoroutine(FloatAndFade(textObj, text));
    }
    
    IEnumerator FloatAndFade(GameObject obj, TMPro.TextMeshProUGUI text)
    {
        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 startPos = obj.transform.position;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            obj.transform.position = startPos + Vector3.up * (t * 50f);
            
            Color c = text.color;
            c.a = 1f - t;
            text.color = c;
            
            yield return null;
        }
        
        Destroy(obj);
    }
}
