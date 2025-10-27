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
        Debug.Log($"🔍 RegionTracker initialized | PuzzleManager: {puzzleManager != null} | ScoreManager: {scoreManager != null}");
    }

    public void CheckRegionCompletion(int row, int col)
{
    if (puzzleManager == null) return;

    Vector3 cellPosition = GetCellWorldPosition(row, col);

    if (!completedRows[row] && IsRowCompleteOrBlocked(row))
    {
        completedRows[row] = true;
        int damage = CalculateRowDamage(row);
        Debug.Log($"🎯 Row {row} completed! Calculated damage: {damage}");
        OnRegionCompleted(RegionType.Row, row, damage, cellPosition);
    }

    if (!completedCols[col] && IsColCompleteOrBlocked(col))
    {
        completedCols[col] = true;
        int damage = CalculateColumnDamage(col);
        Debug.Log($"🎯 Column {col} completed! Calculated damage: {damage}");
        OnRegionCompleted(RegionType.Column, col, damage, cellPosition);
    }

    int boxIndex = GetBoxIndex(row, col);
    if (!completedBoxes[boxIndex] && IsBoxCompleteOrBlocked(boxIndex))
    {
        completedBoxes[boxIndex] = true;
        int damage = CalculateBoxDamage(boxIndex);
        Debug.Log($"🎯 Box {boxIndex} completed! Calculated damage: {damage}");
        OnRegionCompleted(RegionType.Box, boxIndex, damage, cellPosition);
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

   private void OnRegionCompleted(RegionType type, int index, int damage, Vector3 cellPosition)
    {
        string regionName = type == RegionType.Row ? $"Row {index}" :
                           type == RegionType.Column ? $"Column {index}" :
                           $"Box {index}";

        Debug.Log($"✅ {regionName} completed! Initial damage: {damage}");

        if (EnemyStatusManager.Instance != null)
        {
            int originalDamage = damage;
            damage = EnemyStatusManager.Instance.FilterDamage(type, damage);

            if (damage < originalDamage)
            {
                Debug.Log($"🛡️ Damage reduced from {originalDamage} to {damage}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ EnemyStatusManager.Instance is NULL!");
        }

        Debug.Log($"💥 Calling EnemyManager.OnRegionDamaged with damage: {damage} | EnemyManager exists: {EnemyManager.Instance != null}");
        
        if (EnemyManager.Instance != null)
{
    EnemyManager.Instance.OnRegionDamaged(damage, cellPosition);
}

        else
        {
            Debug.LogError("❌ EnemyManager.Instance is NULL! Region damage popup cannot be shown!");
        }

        if (scoreManager != null)
        {
            scoreManager.AddScore(damage);
        }
    }
private Vector3 GetCellWorldPosition(int row, int col)
{
    GameObject sudokuGrid = GameObject.Find("SudokuGrid");
    if (sudokuGrid == null)
    {
        Debug.LogWarning("⚠️ SudokuGrid not found! Using screen center for popup.");
        return new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
    }

    Transform cellTransform = sudokuGrid.transform.Find($"Cell_{row}_{col}");
    if (cellTransform != null)
    {
        return cellTransform.position;
    }

    Debug.LogWarning($"⚠️ Cell_{row}_{col} not found! Using screen center for popup.");
    return new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
}

    private int GetBoxIndex(int row, int col)
    {
        return (row / 3) * 3 + (col / 3);
    }
}
