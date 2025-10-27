using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BoxBorderVisualizer : MonoBehaviour
{
    [Header("Border Settings")]
    public Color borderColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public float borderThickness = 4f;
    
    void Start()
    {
        Invoke(nameof(DrawBoxBorders), 0.2f);
    }
    
    void DrawBoxBorders()
    {
        CellController[] allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        
        if (allCells.Length == 0)
        {
            Debug.LogWarning("⚠️ No cells found to draw borders around!");
            return;
        }
        
        for (int boxRow = 0; boxRow < 3; boxRow++)
        {
            for (int boxCol = 0; boxCol < 3; boxCol++)
            {
                DrawBoxBorder(boxRow, boxCol, allCells);
            }
        }
        
        Debug.Log($"✅ Drew borders around 9 boxes");
    }
    
    void DrawBoxBorder(int boxRow, int boxCol, CellController[] allCells)
    {
        int startRow = boxRow * 3;
        int startCol = boxCol * 3;
        
        CellController topLeftCell = GetCell(startRow, startCol, allCells);
        CellController bottomRightCell = GetCell(startRow + 2, startCol + 2, allCells);
        
        if (topLeftCell == null || bottomRightCell == null)
        {
            Debug.LogWarning($"⚠️ Could not find corner cells for box [{boxRow},{boxCol}]");
            return;
        }
        
        RectTransform topLeftRect = topLeftCell.GetComponent<RectTransform>();
        RectTransform bottomRightRect = bottomRightCell.GetComponent<RectTransform>();
        
        Vector2 topLeftPos = topLeftRect.anchoredPosition;
        Vector2 bottomRightPos = bottomRightRect.anchoredPosition;
        
        float cellWidth = topLeftRect.rect.width;
        float cellHeight = topLeftRect.rect.height;
        
        float left = topLeftPos.x;
        float right = bottomRightPos.x + cellWidth;
        float top = topLeftPos.y;
        float bottom = bottomRightPos.y - cellHeight;
        
        float boxWidth = right - left;
        float boxHeight = top - bottom;
        
        Vector2 centerPos = new Vector2((left + right) / 2f, (top + bottom) / 2f);
        
        GameObject borderContainer = new GameObject($"BoxBorder_{boxRow}_{boxCol}");
        borderContainer.transform.SetParent(transform, false);
        
        RectTransform containerRect = borderContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = centerPos;
        containerRect.sizeDelta = new Vector2(boxWidth, boxHeight);
        
        CreateBorderLine(borderContainer.transform, "Top", 0, boxHeight / 2f, boxWidth + borderThickness, borderThickness);
        CreateBorderLine(borderContainer.transform, "Bottom", 0, -boxHeight / 2f, boxWidth + borderThickness, borderThickness);
        CreateBorderLine(borderContainer.transform, "Left", -boxWidth / 2f, 0, borderThickness, boxHeight + borderThickness);
        CreateBorderLine(borderContainer.transform, "Right", boxWidth / 2f, 0, borderThickness, boxHeight + borderThickness);
    }
    
    CellController GetCell(int row, int col, CellController[] allCells)
    {
        foreach (var cell in allCells)
        {
            if (cell.row == row && cell.column == col)
                return cell;
        }
        return null;
    }
    
    void CreateBorderLine(Transform parent, string name, float x, float y, float width, float height)
    {
        GameObject line = new GameObject(name);
        line.transform.SetParent(parent, false);
        
        RectTransform lineRect = line.AddComponent<RectTransform>();
        Image lineImage = line.AddComponent<Image>();
        
        lineImage.color = borderColor;
        lineImage.raycastTarget = false;
        
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.sizeDelta = new Vector2(width, height);
        lineRect.anchoredPosition = new Vector2(x, y);
    }
}
