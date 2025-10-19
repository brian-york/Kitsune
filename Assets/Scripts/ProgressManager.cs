using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;
    public string currentPuzzleId;
    public int playerCurrency = 0;
    public int TotalCurrency => playerCurrency;

    public List<Relic> collectedRelics = new List<Relic>();
    public IReadOnlyList<Relic> CollectedRelics => collectedRelics;
    public int totalTwosPlaced = 0;

    [Header("Narrative Routing")]
    public bool narrativeTriggeredThisPuzzle = false;
    public CellController.NarrativeCellType narrativeTypeTriggered = CellController.NarrativeCellType.None;
    private HashSet<string> completedNodes = new HashSet<string>();
    public string currentNodeName;

    [Header("Tile Collection")]
    public List<TileData> playerTileCollection = new List<TileData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ ProgressManager initialized and set to persist.");
            
            if (playerTileCollection.Count == 0)
            {
                InitializeStartingCollection();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeStartingCollection()
    {
        Debug.Log("🎴 Creating starting tile collection...");
        
        int startingTileCount = 30;
        
        for (int i = 0; i < startingTileCount; i++)
        {
            int number = Random.Range(1, 10);
            TileEffect effect = TileEffect.None;
            
            if (i >= 25)
            {
                effect = Random.value > 0.5f ? TileEffect.Booned : TileEffect.Leaf;
            }
            
            Color color = GetColorForEffect(effect);
            int bonus = effect == TileEffect.Booned ? Random.Range(10, 20) : 0;
            
            playerTileCollection.Add(new TileData(
                number,
                color,
                effect,
                bonus,
                false,
                ""
            ));
        }
        
        Debug.Log($"✅ Starting collection created with {playerTileCollection.Count} tiles");
        LogCollectionDistribution();
    }

    public void AddTileToCollection(TileData newTile)
    {
        playerTileCollection.Add(newTile);
        Debug.Log($"➕ Added {newTile.tileEffect} {newTile.number} to collection (Total: {playerTileCollection.Count})");
    }

    public bool RemoveTileFromCollection(TileData tileToRemove)
    {
        for (int i = 0; i < playerTileCollection.Count; i++)
        {
            var tile = playerTileCollection[i];
            if (tile.number == tileToRemove.number && tile.tileEffect == tileToRemove.tileEffect)
            {
                playerTileCollection.RemoveAt(i);
                Debug.Log($"➖ Removed {tile.tileEffect} {tile.number} from collection (Total: {playerTileCollection.Count})");
                return true;
            }
        }
        return false;
    }

    public void UpgradeTileInCollection(int index, TileEffect newEffect, int newBonus = 0)
    {
        if (index >= 0 && index < playerTileCollection.Count)
        {
            var tile = playerTileCollection[index];
            tile.tileEffect = newEffect;
            tile.scoreBonus = newBonus;
            tile.tileColor = GetColorForEffect(newEffect);
            
            Debug.Log($"⬆️ Upgraded tile {index} to {newEffect}");
        }
    }

    private Color GetColorForEffect(TileEffect effect)
    {
        switch (effect)
        {
            case TileEffect.None: return KitsuneColors.WashedRicePaper;
            case TileEffect.Booned: return new Color(0.9f, 0.9f, 1f);
            case TileEffect.Leaf: return new Color(0.6f, 0.9f, 0.6f);
            case TileEffect.Flame: return new Color(1f, 0.6f, 0.4f);
            case TileEffect.Baned: return new Color(0.5f, 0.3f, 0.5f);
            case TileEffect.Lunar: return new Color(0.7f, 0.8f, 1f);
            case TileEffect.Solar: return new Color(1f, 0.95f, 0.6f);
            case TileEffect.Portent: return new Color(0.8f, 0.6f, 1f);
            case TileEffect.Wild: return new Color(1f, 1f, 0.8f);
            default: return KitsuneColors.WashedRicePaper;
        }
    }
public static string GetSymbolForEffect(TileEffect effect)
{
    switch (effect)
    {
        case TileEffect.Booned: return "[+]";
        case TileEffect.Leaf: return "[x2]";
        case TileEffect.Flame: return "[F]";
        case TileEffect.Baned: return "[!]";
        case TileEffect.Lunar: return "[M]";
        case TileEffect.Solar: return "[S]";
        case TileEffect.Portent: return "[?]";
        case TileEffect.Wild: return "[*]";
        default: return "";
    }
}




    private void LogCollectionDistribution()
    {
        Dictionary<TileEffect, int> counts = new Dictionary<TileEffect, int>();

        foreach (var tile in playerTileCollection)
        {
            if (!counts.ContainsKey(tile.tileEffect))
                counts[tile.tileEffect] = 0;
            
            counts[tile.tileEffect]++;
        }

        Debug.Log("=== Player Collection ===");
        foreach (var entry in counts)
        {
            float percentage = (entry.Value / (float)playerTileCollection.Count) * 100f;
            Debug.Log($"{entry.Key}: {entry.Value} tiles ({percentage:F1}%)");
        }
    }

    [ContextMenu("View Tile Collection")]
    public void ViewTileCollection()
    {
        LogCollectionDistribution();
        Debug.Log($"Total tiles in collection: {playerTileCollection.Count}");
    }

    public void AddCurrency(int amount)
    {
        playerCurrency += amount;

        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null)
        {
            ui.UpdateCurrencyDisplay(TotalCurrency);
            ui.ShowCurrencyPopup($"+{amount} Mon");
        }
    }

    public void SetNarrativeTrigger(CellController.NarrativeCellType type)
    {
        narrativeTriggeredThisPuzzle = true;
        narrativeTypeTriggered = type;
    }

    public bool SpendCurrency(int amount)
    {
        if (playerCurrency >= amount)
        {
            playerCurrency -= amount;
            Debug.Log($"🧾 Spent {amount} currency. Remaining: {playerCurrency}");
            return true;
        }
        Debug.Log("❌ Not enough currency.");
        return false;
    }

    public HashSet<string> completedPuzzles = new HashSet<string>();

    public void AcquireRelic(Relic relic)
    {
        if (!collectedRelics.Contains(relic))
        {
            collectedRelics.Add(relic);
            Debug.Log($"[ProgressManager] Acquired new relic: {relic.name}");
        }
    }
    

    public void MarkNodeCompleted(string nodeName)
    {
        if (!completedNodes.Contains(nodeName))
        {
            completedNodes.Add(nodeName);
            Debug.Log($"✅ ProgressManager: Marked node '{nodeName}' as completed.");
        }
    }

    public bool IsNodeCompleted(string nodeName)
    {
        return completedNodes.Contains(nodeName);
    }

    public Dictionary<string, string> puzzleAssignments = new Dictionary<string, string>();

    public bool HasPuzzleAssignment(string nodeName)
    {
        return puzzleAssignments.ContainsKey(nodeName);
    }

    public string GetPuzzleForNode(string nodeName)
    {
        return puzzleAssignments.TryGetValue(nodeName, out var puzzleId) ? puzzleId : null;
    }

    public void SetPuzzleForNode(string nodeName, string puzzleId)
    {
        if (!puzzleAssignments.ContainsKey(nodeName))
        {
            puzzleAssignments[nodeName] = puzzleId;
            Debug.Log($"💾 Saved puzzle assignment: {nodeName} → {puzzleId}");
        }
    }
}
