using UnityEngine;
public class PuzzleLoader : MonoBehaviour
{
    public string[] puzzleFileNames = {
        "Puzzles/puzzle001",
        "Puzzles/puzzle002",
        "Puzzles/puzzle003"
    };

    public PuzzleData LoadPuzzle()
    {
        int index = Random.Range(0, puzzleFileNames.Length);
        string chosenFile = puzzleFileNames[index];

        TextAsset jsonText = Resources.Load<TextAsset>(chosenFile);

        if (jsonText == null)
        {
            Debug.LogError("Could not find puzzle file: " + chosenFile);
            return null;
        }

        Debug.Log("Loaded JSON text: " + jsonText.text);

        PuzzleData puzzle = JsonUtility.FromJson<PuzzleData>(jsonText.text);
        return puzzle;

        
    }

    
}