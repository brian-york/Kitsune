using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RelicShopManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform relicContainer; // Holds instantiated relics
    public GameObject relicCardPrefab; // RelicCard.prefab with icon/name/cost/buy button
    public TextMeshProUGUI currencyText;
    public Button returnButton;
    private List<RelicData> availableRelics;

    void Start()
    {
        // Hook up button (manual routing for now)
        returnButton.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene");
        });

        LoadRelics();
        DisplayRelics();
        UpdateCurrencyUI();
    }

    void LoadRelics()
    {
        // Temporary stub list
        availableRelics = new List<RelicData>()
        {
            new RelicData { id = "bamboo_shute", name = "Bamboo Shute", cost = 2, icon = null },
            new RelicData { id = "lucky_coin", name = "Lucky Coin", cost = 3, icon = null },
            new RelicData { id = "mirror_leaf", name = "Mirror Leaf", cost = 2, icon = null },
        };
    }

    void DisplayRelics()
{
    foreach (var relic in availableRelics)
    {
        GameObject relicGO = Instantiate(relicCardPrefab, relicContainer);
        relicGO.name = relic.name;

        // Set visuals
        relicGO.transform.Find("RelicName").GetComponent<TextMeshProUGUI>().text = relic.name;
        relicGO.transform.Find("RelicCost").GetComponent<TextMeshProUGUI>().text = $"{relic.cost} Mon";
        Image iconImage = relicGO.transform.Find("RelicIcon").GetComponent<Image>();
        if (relic.icon != null)
            iconImage.sprite = relic.icon;

        // Set up interaction logic
        RelicCardInteraction interaction = relicGO.GetComponent<RelicCardInteraction>();
        interaction.relicData = relic;
        RegisterCard(interaction); // Track it for deselection logic
    }
}


    Relic GetRelicById(string id)
{
    Relic[] allRelics = Resources.LoadAll<Relic>("Relics");
    foreach (var relic in allRelics)
    {
        if (relic.name.ToLower().Replace(" ", "_") == id.ToLower())
            return relic;
    }

    Debug.LogWarning($"[Shop] Relic not found for id: {id}");
    return null;
}
public static RelicShopManager Instance;

    private List<RelicCardInteraction> allCards = new List<RelicCardInteraction>();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterCard(RelicCardInteraction card)
    {
        allCards.Add(card);
    }

    public void DeselectAllCardsExcept(RelicCardInteraction selectedCard)
    {
        foreach (var card in allCards)
        {
            if (card != selectedCard)
                card.DeselectCard();
        }
    }

    public bool TryBuyRelic(RelicData relic)
{
    if (ProgressManager.Instance.TotalCurrency >= relic.cost)
    {
        ProgressManager.Instance.SpendCurrency(relic.cost);

        Relic relicAsset = GetRelicById(relic.id);
        if (relicAsset != null)
            ProgressManager.Instance.AcquireRelic(relicAsset);

        UpdateCurrencyUI();
        Debug.Log($"[Shop] Bought relic: {relic.name}");
        return true;
    }
    else
    {
        Debug.Log($"[Shop] Not enough Mon to buy {relic.name}");
        return false;
    }
}

    void UpdateCurrencyUI()
    {
        if (currencyText != null)
            currencyText.text = $"{ProgressManager.Instance.TotalCurrency} Mon";
    }
}

[System.Serializable]
public class RelicData
{
    public string id;
    public string name;
    public int cost;
    public Sprite icon;
}
