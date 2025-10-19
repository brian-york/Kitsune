using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class RelicShopManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform relicContainer;
    public Transform tileContainer;
    public Button tricksterButton;
    public TextMeshProUGUI tricksterUsesText;
    public GameObject relicCardPrefab;
    public TextMeshProUGUI currencyText;
    public Button returnButton;
    
    [Header("Trickster Settings")]
    public int tricksterCost = 15;
    
    private List<RelicData> availableRelics;
    private List<RelicData> availableTiles;
    private int tricksterUsesRemaining = 1;
    private bool tricksterSelected = false;
    public static RelicShopManager Instance;
    private List<RelicCardInteraction> allCards = new List<RelicCardInteraction>();

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        Debug.Log("🟡 RelicShopManager.Start() called");
        yield return new WaitUntil(() => ProgressManager.Instance != null);
        Debug.Log("🟢 ProgressManager is ready");

        InitializeTricksterUses();
        LoadItems();
        DisplayRelics();
        DisplayTiles();
        SetupTricksterButton();
        UpdateCurrencyUI();
    }

    void InitializeTricksterUses()
    {
        tricksterUsesRemaining = 1;
        Debug.Log($"🦊 Trickster uses available: {tricksterUsesRemaining}");
    }

    public void ReturnToMapScene()
    {
        Debug.Log("🧭 Returning to MapScene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene");
    }

    void LoadItems()
    {
        availableRelics = new List<RelicData>()
        {
            new RelicData { id = "adzuki_beans", name = "Adzuki Beans", cost = 2, icon = LoadIcon("adzuki_beans"), itemType = ShopItemType.Relic },
            new RelicData { id = "bamboo_shute", name = "Bamboo Shute", cost = 3, icon = LoadIcon("bamboo_shute"), itemType = ShopItemType.Relic },
            new RelicData { id = "oni_mask", name = "Oni Mask", cost = 4, icon = LoadIcon("oni_mask"), itemType = ShopItemType.Relic },
            new RelicData { id = "lost_onamori", name = "Lost Onamori", cost = 2, icon = LoadIcon("lost_onamori"), itemType = ShopItemType.Relic },
            new RelicData { id = "yakos_clover", name = "Yako's Clover", cost = 3, icon = LoadIcon("yakos_clover"), itemType = ShopItemType.Relic }
        };

        availableTiles = GenerateRandomTiles(5);

        Debug.Log($"[Shop] Loaded {availableRelics.Count} relics and {availableTiles.Count} tiles.");
    }

    List<RelicData> GenerateRandomTiles(int count)
    {
        List<RelicData> tiles = new List<RelicData>();
        TileEffect[] possibleEffects = new TileEffect[]
        {
            TileEffect.None, TileEffect.None, TileEffect.None,
            TileEffect.Booned, TileEffect.Leaf, TileEffect.Flame, 
            TileEffect.Lunar, TileEffect.Solar, TileEffect.Baned
        };

        for (int i = 0; i < count; i++)
        {
            int number = Random.Range(1, 10);
            TileEffect effect = possibleEffects[Random.Range(0, possibleEffects.Length)];
            int bonus = effect == TileEffect.Booned ? Random.Range(10, 25) : 0;
            Color color = GetColorForEffect(effect);
            
            int baseCost = 20;
            if (effect != TileEffect.None) baseCost += 15;
            if (number >= 7) baseCost += 10;
            
            string effectSymbol = ProgressManager.GetSymbolForEffect(effect);
            string tileName = effect == TileEffect.None ? $"Tile {number}" : $"{effectSymbol} {number}";
            
            TileData tileData = new TileData(number, color, effect, bonus, false, "");
            
            tiles.Add(new RelicData 
            { 
                id = $"tile_{i}", 
                name = tileName, 
                cost = baseCost, 
                icon = null,
                itemType = ShopItemType.Tile,
                purchasableTile = tileData
            });
        }

        return tiles;
    }

    void SetupTricksterButton()
    {
        if (tricksterButton != null)
        {
            tricksterButton.onClick.AddListener(OnTricksterButtonClicked);
            UpdateTricksterButtonState();
        }
    }

    void UpdateTricksterButtonState()
    {
        if (tricksterButton != null)
        {
            bool canUse = tricksterUsesRemaining > 0 && ProgressManager.Instance.TotalCurrency >= tricksterCost;
            tricksterButton.interactable = canUse;
            
            if (tricksterSelected)
            {
                tricksterButton.transform.localScale = Vector3.one * 1.1f;
            }
            else
            {
                tricksterButton.transform.localScale = Vector3.one;
            }
        }

        if (tricksterUsesText != null)
        {
            tricksterUsesText.text = $"Uses: {tricksterUsesRemaining}";
        }
    }

   public void OnTricksterButtonClicked()
{
    if (tricksterUsesRemaining <= 0)
    {
        Debug.Log("⚠️ Trickster's Bag: No uses remaining!");
        return;
    }

    if (ProgressManager.Instance.TotalCurrency < tricksterCost)
    {
        Debug.Log($"⚠️ Not enough Mon! Need {tricksterCost} Mon.");
        return;
    }

    if (!tricksterSelected)
    {
        tricksterSelected = true;
        DeselectAllCards();
        UpdateTricksterButtonState();
        Debug.Log("🦊 Trickster's Bag selected! Click again to confirm.");
        return;
    }

    ProgressManager.Instance.SpendCurrency(tricksterCost);
    tricksterUsesRemaining--;
    tricksterSelected = false;

    float luckBonus = 0f;
    float addChance = 0.5f + luckBonus;
    bool addTile = Random.value < addChance;

    if (addTile)
    {
        TileEffect[] allEffects = new TileEffect[]
        {
            TileEffect.None, TileEffect.Booned, TileEffect.Leaf, TileEffect.Flame, 
            TileEffect.Lunar, TileEffect.Solar, TileEffect.Baned, TileEffect.Portent, TileEffect.Wild
        };

        TileEffect randomEffect = allEffects[Random.Range(0, allEffects.Length)];
        int randomNumber = Random.Range(1, 10);
        int bonus = randomEffect == TileEffect.Booned ? Random.Range(5, 20) : 0;
        
        TileData newTile = new TileData(randomNumber, GetColorForEffect(randomEffect), randomEffect, bonus, false, "");
        ProgressManager.Instance.AddTileToCollection(newTile);
        Debug.Log($"🦊✅ Trickster's Bag: Added {ProgressManager.GetSymbolForEffect(randomEffect)} {randomNumber}");
    }
    else
    {
        var collection = ProgressManager.Instance.playerTileCollection;
        if (collection.Count <= 5)
        {
            Debug.Log("⚠️ Collection too small! No tile removed.");
        }
        else
        {
            int randomIndex = Random.Range(0, collection.Count);
            var removedTile = collection[randomIndex];
            collection.RemoveAt(randomIndex);
            Debug.Log($"🦊❌ Trickster's Bag: Removed {ProgressManager.GetSymbolForEffect(removedTile.tileEffect)} {removedTile.number}");
        }
    }

    UpdateCurrencyUI();
    UpdateTricksterButtonState();
}

public void DeselectAllCards()
{
    List<RelicCardInteraction> stillValid = new List<RelicCardInteraction>();

    foreach (var card in allCards)
    {
        if (card == null)
            continue;

        card.DeselectCard();
        stillValid.Add(card);
    }

    allCards = stillValid;
}


    public void DeselectTrickster()
    {
        tricksterSelected = false;
        UpdateTricksterButtonState();
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

            Transform nameT = relicGO.transform.Find("RelicName");
            Transform costT = relicGO.transform.Find("RelicCost");
            Transform iconT = relicGO.transform.Find("RelicIcon");

            if (nameT != null) nameT.GetComponent<TextMeshProUGUI>().text = relic.name;
            if (costT != null) costT.GetComponent<TextMeshProUGUI>().text = $"{relic.cost} Mon";
            if (iconT != null && relic.icon != null) 
                iconT.GetComponent<Image>().sprite = relic.icon;

            RelicCardInteraction interaction = relicGO.GetComponent<RelicCardInteraction>();
            interaction.relicData = relic;
            RegisterCard(interaction);
        }
        
        Debug.Log($"[Shop] Displayed {availableRelics.Count} relics.");
    }

    void DisplayTiles()
    {
        foreach (var tile in availableTiles)
        {
            GameObject tileGO = Instantiate(relicCardPrefab, tileContainer);
            tileGO.name = tile.name;

            Transform nameT = tileGO.transform.Find("RelicName");
            Transform costT = tileGO.transform.Find("RelicCost");
            Transform iconT = tileGO.transform.Find("RelicIcon");

            if (nameT != null) nameT.GetComponent<TextMeshProUGUI>().text = tile.name;
            if (costT != null) costT.GetComponent<TextMeshProUGUI>().text = $"{tile.cost} Mon";
            if (iconT != null)
            {
                Image iconImage = iconT.GetComponent<Image>();
                iconImage.color = tile.purchasableTile.tileColor;
            }

            RelicCardInteraction interaction = tileGO.GetComponent<RelicCardInteraction>();
            interaction.relicData = tile;
            RegisterCard(interaction);
        }
        
        Debug.Log($"[Shop] Displayed {availableTiles.Count} tiles.");
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

    public void RegisterCard(RelicCardInteraction card)
    {
        allCards.Add(card);
    }

    public void DeselectAllCardsExcept(RelicCardInteraction selectedCard)
    {
        List<RelicCardInteraction> stillValid = new List<RelicCardInteraction>();

        foreach (var card in allCards)
        {
            if (card == null)
                continue;

            if (card != selectedCard)
                card.DeselectCard();

            stillValid.Add(card);
        }

        allCards = stillValid;
        DeselectTrickster();
    }

    public bool TryBuyRelic(RelicData item)
    {
        if (ProgressManager.Instance.TotalCurrency >= item.cost)
        {
            ProgressManager.Instance.SpendCurrency(item.cost);

            switch (item.itemType)
            {
                case ShopItemType.Relic:
                    Relic relicAsset = GetRelicById(item.id);
                    if (relicAsset != null)
                        ProgressManager.Instance.AcquireRelic(relicAsset);
                    break;

                case ShopItemType.Tile:
                    ProgressManager.Instance.AddTileToCollection(item.purchasableTile);
                    Debug.Log($"✅ Purchased {item.name}");
                    break;
            }

            UpdateCurrencyUI();
            UpdateTricksterButtonState();
            Debug.Log($"[Shop] Bought: {item.name}");
            return true;
        }
        else
        {
            Debug.Log($"[Shop] Not enough Mon to buy {item.name}");
            return false;
        }
    }

    public void UpdateCurrencyUI()
    {
        if (ProgressManager.Instance == null)
        {
            Debug.LogError("❌ ProgressManager.Instance is null!");
            return;
        }

        if (currencyText != null)
            currencyText.text = $"{ProgressManager.Instance.TotalCurrency} Mon";
            
        UpdateTricksterButtonState();
    }

    Color GetColorForEffect(TileEffect effect)
    {
        switch (effect)
        {
            case TileEffect.Booned: return new Color(0.9f, 0.9f, 1f);
            case TileEffect.Leaf: return new Color(0.6f, 0.9f, 0.6f);
            case TileEffect.Flame: return new Color(1f, 0.6f, 0.4f);
            case TileEffect.Lunar: return new Color(0.7f, 0.8f, 1f);
            case TileEffect.Solar: return new Color(1f, 0.95f, 0.6f);
            case TileEffect.Baned: return new Color(0.5f, 0.5f, 0.5f);
            case TileEffect.Portent: return new Color(0.8f, 0.7f, 1f);
            case TileEffect.Wild: return new Color(1f, 1f, 0.8f);
            default: return Color.white;
        }
    }
}

[System.Serializable]
public class RelicData
{
    public string id;
    public string name;
    public int cost;
    public Sprite icon;
    
    public ShopItemType itemType;
    public TileData purchasableTile;
}

public enum ShopItemType
{
    Relic,
    Tile
}
