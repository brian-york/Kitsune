using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public GameObject cellPrefab;

    void Start()
    {
        Debug.Log("GridSpawner is running!");
        GenerateGrid();
    }

   void GenerateGrid()
{
    int[,] puzzle = new int[,]
    {
        {5,3,0, 0,7,0, 0,0,0},
        {6,0,0, 1,9,5, 0,0,0},
        {0,9,8, 0,0,0, 0,6,0},
        {8,0,0, 0,6,0, 0,0,3},
        {4,0,0, 8,0,3, 0,0,1},
        {7,0,0, 0,2,0, 0,0,6},
        {0,6,0, 0,0,0, 2,8,0},
        {0,0,0, 4,1,9, 0,0,5},
        {0,0,0, 0,8,0, 0,7,9}
    };

    for (int row = 0; row < 9; row++)
    {
        for (int col = 0; col < 9; col++)
        {
            GameObject newCell = Instantiate(cellPrefab, transform);
            newCell.name = $"Cell_{row}_{col}";

            var cellController = newCell.GetComponent<CellController>();

            int value = puzzle[row, col];
            bool locked = value != 0;

            cellController.SetupCell(row, col);
            cellController.SetValue(value, locked);
        }
    }
}

}