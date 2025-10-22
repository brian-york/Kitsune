using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CellController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_InputField inputField;
    public int row;
    public int column;

    public string cellState = "Playable";
    public string narrativeDescription;
    private PuzzleManager puzzleManager;

    public bool isBlocked = false;
    public bool narrativeTriggered = false;
    public bool locked = false;

    public NarrativeCondition narrativeCondition;
    public enum NarrativeCellType
    {
        None,
        Shop,
        Event,
        Boss,
        RelicReward,
        Currency
    }

    [SerializeField] private GameObject currencyOverlay;

    public NarrativeCellType narrativeCellType = NarrativeCellType.None;
    
    private Color originalColor;
    private bool isHovering = false;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.characterLimit = 1;
        inputField.onValueChanged.AddListener(OnCellValueChanged);
        inputField.readOnly = true;

        puzzleManager = FindFirstObjectByType<PuzzleManager>();
    }

    public void SetupCell(int r, int c)
    {
        row = r;
        column = c;
    }

    void OnCellValueChanged(string text)
    {
        Debug.Log($"[CurrencyCheck] Cell [{row},{column}] Type: {narrativeCellType}, Triggered: {narrativeTriggered}");

        if (isBlocked)
            return;
        
        Debug.Log($"[CurrencyTrigger] Triggering Currency logic for cell [{row},{column}].");

        int value = 0;

        if (!string.IsNullOrEmpty(text))
        {
            char digit = text[0];

            if (digit >= '1' && digit <= '9')
            {
                value = digit - '0';
                inputField.text = digit.ToString();
            }
            else
            {
                inputField.text = "";
            }
        }
        else
        {
            value = 0;
        }

        puzzleManager.UpdateCell(row, column, value);

        var textComponent = inputField.transform.Find("Text Area/Text")?.GetComponent<TextMeshProUGUI>();

        if (puzzleManager.IsValid(row, column))
        {
            if (narrativeCellType == NarrativeCellType.None)
            {
                Debug.Log($"[OnCellValueChanged] Cell [{row},{column}] setting color to WHITE.");
                inputField.image.color = Color.white;
                originalColor = Color.white;
            }
            else
            {
                Debug.Log($"[OnCellValueChanged] Cell [{row},{column}] preserving narrative color: {narrativeCellType}");
            }
        }
        else
        {
            Debug.Log($"[OnCellValueChanged] Cell [{row},{column}] setting color to RED.");
            inputField.image.color = Color.red;
            originalColor = Color.red;
            
            HighlightConflictingCells(value);
        }

        if (narrativeCellType != NarrativeCellType.None && !narrativeTriggered && puzzleManager.IsValid(row, column))
        {
            switch (narrativeCellType)
            {
                case NarrativeCellType.Currency:
                    Debug.Log($"[💰 CurrencyCell] Triggered at [{row},{column}]");
                    
                    int currencyAmount = 1;
                    var relics = ProgressManager.Instance.collectedRelics;
                    
                    if (relics != null)
                    {
                        foreach (var relic in relics)
                        {
                            relic.OnCurrencyGain(ref currencyAmount, this);
                        }
                    }
                    
                    ProgressManager.Instance.AddCurrency(currencyAmount);
                    
                    UIManager ui = FindFirstObjectByType<UIManager>();
                    ui?.UpdateCurrencyDisplay(ProgressManager.Instance.TotalCurrency);
                    break;
                    
                case NarrativeCellType.Shop:
                case NarrativeCellType.Event:
                case NarrativeCellType.Boss:
                case NarrativeCellType.RelicReward:
                    Debug.Log($"[🎭 Narrative] {narrativeCellType} triggered at [{row},{column}]");
                    break;
            }
            
            TriggerNarrative();
        }
    }

    void HighlightConflictingCells(int value)
    {
        if (value == 0 || puzzleManager == null) return;

        List<Vector2Int> conflictingCells = new List<Vector2Int>();

        for (int c = 0; c < 9; c++)
        {
            if (c != column && 
                puzzleManager.playerGrid[row, c] == value && 
                puzzleManager.cellStates[row * 9 + c] != CellState.Blocked)
            {
                conflictingCells.Add(new Vector2Int(row, c));
            }
        }

        for (int r = 0; r < 9; r++)
        {
            if (r != row && 
                puzzleManager.playerGrid[r, column] == value && 
                puzzleManager.cellStates[r * 9 + column] != CellState.Blocked)
            {
                conflictingCells.Add(new Vector2Int(r, column));
            }
        }

        int boxRowStart = (row / 3) * 3;
        int boxColStart = (column / 3) * 3;

        for (int r = boxRowStart; r < boxRowStart + 3; r++)
        {
            for (int c = boxColStart; c < boxColStart + 3; c++)
            {
                if ((r != row || c != column) &&
                    puzzleManager.playerGrid[r, c] == value &&
                    puzzleManager.cellStates[r * 9 + c] != CellState.Blocked)
                {
                    conflictingCells.Add(new Vector2Int(r, c));
                }
            }
        }

        CellController[] allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        foreach (var cell in allCells)
        {
            foreach (var conflict in conflictingCells)
            {
                if (cell.row == conflict.x && cell.column == conflict.y)
                {
                    cell.FlashConflict();
                }
            }
        }
    }

    public void FlashConflict()
    {
        StartCoroutine(FlashConflictCoroutine());
    }

    IEnumerator FlashConflictCoroutine()
    {
        Color flashColor = new Color(1f, 0.6f, 0f, 1f);
        Image img = inputField.image;
        Color startColor = img.color;
        
        float flashDuration = 0.3f;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            img.color = Color.Lerp(flashColor, startColor, t);
            yield return null;
        }

        img.color = startColor;
    }

    public void SetValue(int value, bool locked)
    {
        this.locked = locked;

        if (value > 0)
        {
            inputField.SetTextWithoutNotify(value.ToString());
            inputField.interactable = false;
            inputField.image.color = new Color(0.95f, 0.92f, 0.88f);
            originalColor = new Color(0.95f, 0.92f, 0.88f);
        }
        else
        {
            inputField.text = "";
            inputField.interactable = true;
            originalColor = Color.white;
        }

        var textComponent = inputField.transform.Find("Text Area/Text")?.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.color = KitsuneColors.DriedInkBrown;
        }

        if (narrativeCellType != NarrativeCellType.None)
        {
            Debug.Log($"[SetValue] Re-applying narrative color for cell [{row},{column}]");
            SetNarrativeCellColor();
        }

        Debug.Log($"[SetValue] Cell [{row},{column}] => value: {value}, locked: {locked}, interactable: {inputField.interactable}");
    }

   public void SetBlocked(bool blocked)
{
    isBlocked = blocked;
    locked = blocked;

    var textComponent = inputField.transform.Find("Text Area/Text")?.GetComponent<TextMeshProUGUI>();

    if (inputField != null)
    {
        if (blocked)
        {
            inputField.image.color = Color.black;
            originalColor = Color.black;
            inputField.interactable = false;
            inputField.text = "";

            if (textComponent != null)
                textComponent.text = "";
            
            Debug.Log($"🚫 Cell [{row},{column}] is now BLOCKED (black, non-interactable)");
        }
        else
        {
            inputField.image.color = Color.white;
            originalColor = Color.white;
            inputField.interactable = true;
        }
    }
}


  void TriggerNarrative()
{
    narrativeTriggered = true;

    GameManager gameManager = FindFirstObjectByType<GameManager>();
    if (gameManager != null)
    {
        gameManager.lastTriggeredNarrative = narrativeDescription;
        gameManager.lastTriggeredCellType = narrativeCellType;
    }

    switch (narrativeCellType)
    {
        case NarrativeCellType.Currency:
            Debug.Log($"💰 Currency narrative triggered at [{row},{column}]");
            break;
            
        case NarrativeCellType.RelicReward:
            Debug.Log($"🔮 Relic reward narrative triggered at [{row},{column}]");
            break;

        default:
            Debug.Log($"🎭 Triggered narrative: {narrativeDescription}");
            break;
    }

    puzzleManager.DisableOtherNarrativeCells(this);
}


   public void SetNarrativeCellColor()
{
    Color cellColor = Color.white;

    switch (narrativeCellType)
    {
        case NarrativeCellType.Shop:
            cellColor = new Color(0.2f, 0.6f, 0.8f);
            break;
        case NarrativeCellType.Event:
            cellColor = new Color(0.9f, 0.7f, 0.2f);
            break;
        case NarrativeCellType.Boss:
            cellColor = new Color(0.7f, 0.1f, 0.1f);
            break;
        case NarrativeCellType.RelicReward:
            cellColor = new Color(0.5f, 0.2f, 0.7f);
            break;
        case NarrativeCellType.Currency:
            cellColor = new Color(0.1f, 0.7f, 0.3f);
            break;
    }

    ColorBlock colorBlock = inputField.colors;
    colorBlock.normalColor = cellColor;
    colorBlock.highlightedColor = cellColor * 1.3f;
    colorBlock.pressedColor = cellColor * 0.8f;
    colorBlock.selectedColor = cellColor;
    colorBlock.disabledColor = cellColor * 0.5f;
    inputField.colors = colorBlock;

    inputField.image.color = cellColor;
    originalColor = cellColor;

    Debug.Log($"[SetNarrativeCellColor] Assigned color {cellColor} to cell [{row},{column}] via ColorBlock.");
}

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isBlocked || locked || !inputField.interactable) return;

        isHovering = true;
        
        Color hoverColor = new Color(0.4f, 0.8f, 1f, 1f);
        inputField.image.color = hoverColor;

        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null && !string.IsNullOrEmpty(narrativeDescription))
        {
            ui.ShowNarrativeTooltip(narrativeDescription, Input.mousePosition);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isBlocked || locked) return;

        isHovering = false;
        inputField.image.color = originalColor;

        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null && !string.IsNullOrEmpty(narrativeDescription))
        {
            ui.HideNarrativeTooltip();
        }
    }
}
