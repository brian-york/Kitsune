using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CellController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_InputField inputField;
    private int row;
    private int column;

    public string cellState = "Playable";
    public string narrativeDescription;
    private PuzzleManager puzzleManager;

    public bool isBlocked = false;
    public bool narrativeTriggered = false;
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

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.characterLimit = 1;
        inputField.onValueChanged.AddListener(OnCellValueChanged);
        inputField.readOnly = true;

        // Find the PuzzleManager in the scene
        puzzleManager = FindFirstObjectByType<PuzzleManager>();
    }

    public void SetupCell(int r, int c)
    {
        row = r;
        column = c;
    }

    void OnCellValueChanged(string text)
    {
        if (isBlocked)
            return;

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
}


    }

    public void SetValue(int value, bool locked)
    {
        if (value > 0)
            inputField.SetTextWithoutNotify(value.ToString());
        else
            inputField.text = "";

        inputField.interactable = !locked;

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

    }

    public void SetBlocked(bool blocked)
    {
        isBlocked = blocked;

        var textComponent = inputField.transform.Find("Text Area/Text")?.GetComponent<TextMeshProUGUI>();

        if (inputField != null)
        {
            if (blocked)
            {
                inputField.image.color = Color.black;
                inputField.interactable = false;
                inputField.text = "";

                if (textComponent != null)
                    textComponent.color = Color.white;
            }
            else
            {
                inputField.image.color = Color.white;
                inputField.interactable = true;

                if (textComponent != null)
                    textComponent.color = KitsuneColors.DriedInkBrown;
            }
        }
    }

    public void SetInteractable(bool state)
    {
        if (!isBlocked)
        {
            inputField.interactable = state;
        }
    }

    /*public void SetNarrative(bool isNarrative)
    {
        
        if (inputField != null)
        {
            inputField.image.color = isNarrative
                ? KitsuneColors.AonibiBlue
                : Color.white;
        }
    
}*/

public void SetNarrativeCellColor()
{
    if (narrativeCellType == NarrativeCellType.None)
        return;

    Color cellColor = Color.white;

        switch (narrativeCellType)
        {
            case NarrativeCellType.Shop:
                cellColor = KitsuneColors.ShopCell;
                break;
            case NarrativeCellType.Event:
                cellColor = KitsuneColors.EventCell;
                break;
            case NarrativeCellType.Boss:
                cellColor = KitsuneColors.BossCell;
                break;
            case NarrativeCellType.RelicReward:
                cellColor = KitsuneColors.RelicRewardCell;
                break;
            case NarrativeCellType.Currency:
            cellColor = KitsuneColors.CurrencyCell;
            break;
    }

    GetComponent<Image>().color = cellColor;
    
    if (currencyOverlay != null)
    {
        currencyOverlay.SetActive(narrativeCellType == NarrativeCellType.Currency);
    }

    if (inputField != null)
        {
            var colors = inputField.colors;
            colors.normalColor = cellColor;
            inputField.colors = colors;

            Debug.Log($"[SetNarrativeCellColor] Assigned color {cellColor} to cell [{row},{column}] via ColorBlock.");
        }
}


   public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager ui = FindFirstObjectByType<UIManager>();

        Debug.Log($"[ENTER] Cell [{row},{column}] narrativeDescription: {narrativeDescription}");

        if (ui != null && !string.IsNullOrEmpty(narrativeDescription))
        {
            if (ui.currentlyHoveredCell != this)
            {
                Debug.Log("[ENTER] Showing tooltip.");
                ui.currentlyHoveredCell = this;
                ui.ShowNarrativeTooltip(narrativeDescription, Input.mousePosition);
            }
            else
            {
                Debug.Log("[ENTER] Skipped show, already hovering this cell.");
            }
        }
    }

   public void OnPointerExit(PointerEventData eventData)
{
    UIManager ui = FindFirstObjectByType<UIManager>();

    Debug.Log($"[EXIT] Cell [{row},{column}] narrativeDescription: {narrativeDescription}");

    if (ui != null && ui.currentlyHoveredCell == this)
    {
        Debug.Log("[EXIT] Hiding tooltip.");
        ui.currentlyHoveredCell = null;
        ui.HideNarrativeTooltip();
    }
    else
    {
        Debug.Log("[EXIT] No action taken on exit.");
    }
}
}
