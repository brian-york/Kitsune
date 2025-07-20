using System.Collections.Generic;

[System.Serializable]
public class PuzzleData
{
    public string id;
    public string difficulty;
    public string theme;
    public List<int> grid;
    public List<string> cellStates;
    public List<string> narrativeCellDescriptions;
    public List<NarrativeCellEntry> narrativeCells;

}
[System.Serializable]
public class NarrativeCellEntry
{
    public int row;
    public int col;
    public string narrativeCellType;
    public string description;
}
