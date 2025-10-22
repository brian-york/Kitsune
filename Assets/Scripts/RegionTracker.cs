using UnityEngine;

public enum RegionType
{
    Row,
    Column,
    Box
}

public class RegionTracker : MonoBehaviour
{
    public static RegionTracker Instance;
    
    private PuzzleManager puzzleManager;
    private ScoreManager scoreManager;
    
    public bool[] completedRows = new bool[9];
    public bool[] completedCols = new bool[9];
    public bool[] completedBoxes = new bool[9];
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        puzzleManager = FindFirstObjectByType<PuzzleManager>();
        scoreManager = FindFirstObjectByType<ScoreManager>();
    }
    
    public void CheckRegionCompletion(int row, int col)
    {
        if (puzzleManager == null) return;
        
        if (!completedRows[row] && IsRowCompleteOrBlocked(row))
        {
            completedRows[row] = true;
            int damage = CalculateRowDamage(row);
            OnRegionCompleted(RegionType.Row, row, damage);
        }
        
        if (!completedCols[col] && IsColCompleteOrBlocked(col))
        {
            completedCols[col] = true;
            int damage = CalculateColumnDamage(col);
            OnRegionCompleted(RegionType.Column, col, damage);
        }
        
        int boxIndex = GetBoxIndex(row, col);
        if (!completedBoxes[boxIndex] && IsBoxCompleteOrBlocked(boxIndex))
        {
            completedBoxes[boxIndex] = true;
            int damage = CalculateBoxDamage(boxIndex);
            OnRegionCompleted(RegionType.Box, boxIndex, damage);
        }
    }
    
    bool IsRowCompleteOrBlocked(int row)
    {
        for (int col = 0; col < 9; col++)
        {
            bool isBlocked = puzzleManager.blockedCells[row, col];
            bool isFilled = puzzleManager.playerGrid[row, col] != 0;
            
            if (!isBlocked && !isFilled)
                return false;
        }
        return true;
    }
    
    bool IsColCompleteOrBlocked(int col)
    {
        for (int row = 0; row < 9; row++)
        {
            bool isBlocked = puzzleManager.blockedCells[row, col];
            bool isFilled = puzzleManager.playerGrid[row, col] != 0;
            
            if (!isBlocked && !isFilled)
                return false;
        }
        return true;
    }
    
    bool IsBoxCompleteOrBlocked(int boxIndex)
    {
        int startRow = (boxIndex / 3) * 3;
        int startCol = (boxIndex % 3) * 3;
        
        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                bool isBlocked = puzzleManager.blockedCells[r, c];
                bool isFilled = puzzleManager.playerGrid[r, c] != 0;
                
                if (!isBlocked && !isFilled)
                    return false;
            }
        }
        return true;
    }
    int CalculateRowDamage(int row)
{
    int filledCells = 0;
    for (int col = 0; col < 9; col++)
    {
        if (puzzleManager.playerGrid[row, col] != 0)
            filledCells++;
    }

    int baseDamage = Mathf.RoundToInt((filledCells / 9f) * 100f);
    
    if (TileEffectHandler.Instance != null)
    {
        int regionKey = row;
        return TileEffectHandler.Instance.CalculateRegionDamageModifier(regionKey, baseDamage);
    }
    
    return baseDamage;
}

int CalculateColumnDamage(int col)
{
    int filledCells = 0;
    for (int row = 0; row < 9; row++)
    {
        if (puzzleManager.playerGrid[row, col] != 0)
            filledCells++;
    }

    int baseDamage = Mathf.RoundToInt((filledCells / 9f) * 100f);
    
    if (TileEffectHandler.Instance != null)
    {
        int regionKey = 100 + col;
        return TileEffectHandler.Instance.CalculateRegionDamageModifier(regionKey, baseDamage);
    }
    
    return baseDamage;
}

int CalculateBoxDamage(int box)
{
    int startRow = (box / 3) * 3;
    int startCol = (box % 3) * 3;
    int filledCells = 0;

    for (int r = startRow; r < startRow + 3; r++)
    {
        for (int c = startCol; c < startCol + 3; c++)
        {
            if (puzzleManager.playerGrid[r, c] != 0)
                filledCells++;
        }
    }

    int baseDamage = Mathf.RoundToInt((filledCells / 9f) * 100f);
    
    if (TileEffectHandler.Instance != null)
    {
        int regionKey = 200 + box;
        return TileEffectHandler.Instance.CalculateRegionDamageModifier(regionKey, baseDamage);
    }
    
    return baseDamage;
}

    
    void OnRegionCompleted(RegionType type, int index, int damage)
    {
        string regionName = type == RegionType.Row ? $"Row {index}" :
                           type == RegionType.Column ? $"Column {index}" :
                           $"Box {index}";
        
        Debug.Log($"✅ {regionName} completed! Damage dealt: {damage}");
        
        if (scoreManager != null)
        {
            Vector3 popupPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.7f, 10f));
            scoreManager.ShowPopupDelayed(damage, $"{regionName}!", popupPos, 0f);
        }
        
        EnemyManager.Instance?.OnRegionDamaged(damage);
    }
    
    int GetBoxIndex(int row, int col)
    {
        return (row / 3) * 3 + (col / 3);
    }
}
