using UnityEngine;
public class PuzzleLoader : MonoBehaviour
{
    public string[] puzzleFileNames = {
        "Puzzles/puzzle001",
        "Puzzles/puzzle002",
        "Puzzles/puzzle003",
        "Puzzles/puzzle004"
    };

    public PuzzleData LoadPuzzle()
{


    string chosenFile = "Puzzles/puzzle004";

    TextAsset jsonText = Resources.Load<TextAsset>(chosenFile);

    if (jsonText == null)
    {
        Debug.LogError("Could not find puzzle file: " + chosenFile);
        return null;
    }

    Debug.Log("Loaded JSON text: " + jsonText.text);

    PuzzleData puzzle = JsonUtility.FromJson<PuzzleData>(jsonText.text);

    if (puzzle.cellStates == null)
    {
        Debug.Log($"No cell states found in puzzle: {puzzle.id}");
    }
    else
    {
        Debug.Log($"cellStates loaded from puzzle: {puzzle.id}");
    }

    return puzzle;
}


    
}