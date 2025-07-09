using System.Collections.Generic;

[System.Serializable]
public class PuzzleData
{
    public string id;
    public string difficulty;
    public string theme;
    public List<int> grid;
    public List<string> cellStates;

}