using UnityEngine;
using System.Collections.Generic;

public class EnemyStatusManager : MonoBehaviour
{
    public static EnemyStatusManager Instance;
    
    [Header("Active Status Effects")]
    public bool hasCorruptedTile = false;
    public TileDragHandler corruptedTile = null;
    
    public bool isRestrictedToBox = false;
    public int restrictedBoxIndex = 0;
    public int turnsUntilBoxChange = 0;
    
    public int handSizeReduction = 0;
    
    public RegionType immuneRegionType = RegionType.Row;
    public bool hasRegionImmunity = false;
    
    public int damageShieldRemaining = 0;
    
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
    
    public void CorruptRandomTileInHand()
    {
        GridSpawner spawner = FindFirstObjectByType<GridSpawner>();
        if (spawner == null) return;
        
        TileDragHandler[] tilesInHand = spawner.tilePoolTransform.GetComponentsInChildren<TileDragHandler>();
        
        if (tilesInHand.Length == 0) return;
        
        corruptedTile = tilesInHand[Random.Range(0, tilesInHand.Length)];
        hasCorruptedTile = true;
        
        var img = corruptedTile.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            img.color = new Color(0.5f, 0f, 0.5f);
        }
        
        Debug.Log($"💀 Corrupted tile: {corruptedTile.tileValue}! Must be played first.");
    }
    
    public void TransformHandToNumber(int targetNumber)
    {
        GridSpawner spawner = FindFirstObjectByType<GridSpawner>();
        if (spawner == null) return;
        
        TileDragHandler[] tilesInHand = spawner.tilePoolTransform.GetComponentsInChildren<TileDragHandler>();
        
        foreach (var tile in tilesInHand)
        {
            tile.tileValue = targetNumber;
            if (tile.numberText != null)
            {
                string symbol = ProgressManager.GetSymbolForEffect(tile.tileEffect);
                tile.numberText.text = string.IsNullOrEmpty(symbol) 
                    ? targetNumber.ToString() 
                    : $"{symbol} {targetNumber}";
            }
        }
        
        Debug.Log($"🔮 All tiles in hand transformed to {targetNumber}!");
    }
    
    public void RestrictToBox(int boxIndex, int duration)
    {
        isRestrictedToBox = true;
        restrictedBoxIndex = boxIndex;
        turnsUntilBoxChange = duration;
        
        Debug.Log($"🔒 Restricted to box {boxIndex} for {duration} turns!");
    }
    
    public void DestroyRandomRegion()
    {
        PuzzleManager pm = FindFirstObjectByType<PuzzleManager>();
        if (pm == null) return;
        
        int regionType = Random.Range(0, 3);
        
        if (regionType == 0)
        {
            int row = Random.Range(0, 9);
            DestroyRow(row, pm);
        }
        else if (regionType == 1)
        {
            int col = Random.Range(0, 9);
            DestroyColumn(col, pm);
        }
        else
        {
            int box = Random.Range(0, 9);
            DestroyBox(box, pm);
        }
    }
    
    private void DestroyRow(int row, PuzzleManager pm)
    {
        for (int col = 0; col < 9; col++)
        {
            if (!pm.blockedCells[row, col] && pm.playerGrid[row, col] != 0)
            {
                pm.playerGrid[row, col] = 0;
                CellController cell = GetCellController(row, col);
                if (cell != null && !cell.locked)
                {
                    cell.SetValue(0, false);
                }
            }
        }
        Debug.Log($"💥 Destroyed all tiles in row {row}!");
    }
    
    private void DestroyColumn(int col, PuzzleManager pm)
    {
        for (int row = 0; row < 9; row++)
        {
            if (!pm.blockedCells[row, col] && pm.playerGrid[row, col] != 0)
            {
                pm.playerGrid[row, col] = 0;
                CellController cell = GetCellController(row, col);
                if (cell != null && !cell.locked)
                {
                    cell.SetValue(0, false);
                }
            }
        }
        Debug.Log($"💥 Destroyed all tiles in column {col}!");
    }
    
    private void DestroyBox(int boxIndex, PuzzleManager pm)
    {
        int startRow = (boxIndex / 3) * 3;
        int startCol = (boxIndex % 3) * 3;
        
        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                if (!pm.blockedCells[r, c] && pm.playerGrid[r, c] != 0)
                {
                    pm.playerGrid[r, c] = 0;
                    CellController cell = GetCellController(r, c);
                    if (cell != null && !cell.locked)
                    {
                        cell.SetValue(0, false);
                    }
                }
            }
        }
        Debug.Log($"💥 Destroyed all tiles in box {boxIndex}!");
    }
    
    public void AddBlankTilesToHand(int count)
    {
        GridSpawner spawner = FindFirstObjectByType<GridSpawner>();
        if (spawner == null) return;
        
        for (int i = 0; i < count; i++)
        {
            TileData blankTile = new TileData(0, Color.gray, TileEffect.None, 0, false, "");
            spawner.CreateTile(blankTile);
        }
        
        Debug.Log($"👻 Added {count} blank tiles to hand!");
    }
    
    public void ReduceHandSize(int reduction, int duration)
    {
        handSizeReduction = reduction;
        Debug.Log($"✂️ Hand size reduced by {reduction} for {duration} turns!");
    }
    
    public void SetDamageImmunity(RegionType regionType)
    {
        hasRegionImmunity = true;
        immuneRegionType = regionType;
        Debug.Log($"🛡️ Enemy is immune to {regionType} damage!");
    }
    
    public void ActivateDamageShield(int amount)
    {
        damageShieldRemaining = amount;
        Debug.Log($"🛡️ Enemy gained {amount} shield!");
    }
    
    public int FilterDamage(RegionType damageSource, int damage)
    {
        if (damageShieldRemaining > 0)
        {
            int blocked = Mathf.Min(damage, damageShieldRemaining);
            damageShieldRemaining -= blocked;
            damage -= blocked;
            Debug.Log($"🛡️ Shield blocked {blocked} damage! ({damageShieldRemaining} remaining)");
        }
        
        if (hasRegionImmunity && damageSource != immuneRegionType)
        {
            Debug.Log($"🛡️ Enemy is immune to {damageSource} damage!");
            return 0;
        }
        
        return damage;
    }
    
    public void OnTilePlaced()
    {
        if (turnsUntilBoxChange > 0)
        {
            turnsUntilBoxChange--;
            if (turnsUntilBoxChange <= 0)
            {
                isRestrictedToBox = false;
                Debug.Log("🔓 Box restriction lifted!");
            }
        }
    }
    
    public void ClearCorruptedTile()
    {
        hasCorruptedTile = false;
        corruptedTile = null;
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
    
    public void ResetAllStatus()
    {
        hasCorruptedTile = false;
        corruptedTile = null;
        isRestrictedToBox = false;
        handSizeReduction = 0;
        hasRegionImmunity = false;
        damageShieldRemaining = 0;
        Debug.Log("🔄 All enemy status effects cleared");
    }
}
