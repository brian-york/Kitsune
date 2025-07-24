using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;


public class RelicShopManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform relicContainer; // Holds instantiated relics
    public GameObject relicCardPrefab; // RelicCard.prefab with icon/name/cost/buy button
    public TextMeshProUGUI currencyText;
    public Button returnButton;
    private List<RelicData> availableRelics;

    private IEnumerator Start()
{
     Debug.Log("🟡 RelicShopManager.Start() called");

    yield return new WaitUntil(() => ProgressManager.Instance != null);

    Debug.Log("🟢 ProgressManager is ready");

    LoadRelics();          // ✅ Now it's safe
    DisplayRelics();
    UpdateCurrencyUI();
}


public void ReturnToMapScene()
{
    Debug.Log("🧭 Returning to MapScene...");
    UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene");
}

    void LoadRelics()
    {
        // Temporary stub list
        availableRelics = new List<RelicData>()
{
new RelicData { id = "adzuki_beans", name = "Adzuki Beans", cost = 2, icon = LoadIcon("adzuki_beans") },
    new RelicData { id = "bamboo_shute", name = "Bamboo Shute", cost = 3, icon = LoadIcon("bamboo_shute") },
    new RelicData { id = "oni_mask", name = "Oni Mask", cost = 4, icon = LoadIcon("oni_mask") },
    new RelicData { id = "lost_onamori", name = "Lost Onamori", cost = 2, icon = LoadIcon("lost_onamori") },
    new RelicData { id = "yakos_clover", name = "Yako's Clover", cost = 3, icon = LoadIcon("yakos_clover") }
};

           Debug.Log($"[Shop] Loaded {availableRelics.Count} relics.");
    }

    private Sprite LoadIcon(string iconName)
{
    Sprite icon = Resources.Load<Sprite>($"RelicIcons/{iconName}");
    if (icon == null)
    {
        Debug.LogWarning($"[Shop] Icon not found for: {iconName}");
    }
    return icon;
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

            Transform nameT = relicGO.transform.Find("RelicName");
            if (nameT == null) Debug.LogError($"RelicCard prefab is missing 'RelicName' object");

            Transform costT = relicGO.transform.Find("RelicCost");
            if (costT == null) Debug.LogError($"RelicCard prefab is missing 'RelicCost' object");

            Transform iconT = relicGO.transform.Find("RelicIcon");
            if (iconT == null) Debug.LogError($"RelicCard prefab is missing 'RelicIcon' object");


            // Set up interaction logic
            RelicCardInteraction interaction = relicGO.GetComponent<RelicCardInteraction>();
            interaction.relicData = relic;
            RegisterCard(interaction); // Track it for deselection logic
        
        Debug.Log($"[Shop] Displaying {availableRelics.Count} relics...");

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
    // Create a temporary list to hold valid cards
    List<RelicCardInteraction> stillValid = new List<RelicCardInteraction>();

    foreach (var card in allCards)
    {
        if (card == null)
            continue; // 🛑 Skip destroyed or null references

        if (card != selectedCard)
            card.DeselectCard();

        stillValid.Add(card); // ✅ Keep only active cards
    }

    allCards = stillValid; // 🔄 Replace with clean list
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
        if (ProgressManager.Instance == null)
{
    Debug.LogError("❌ ProgressManager.Instance is null! It's not initialized before UpdateCurrencyUI() is called.");
    return;
}

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
