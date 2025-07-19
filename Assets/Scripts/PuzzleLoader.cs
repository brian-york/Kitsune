using UnityEngine;
public class PuzzleLoader : MonoBehaviour
{
    public string[] puzzleFileNames = {
        "Puzzles/puzzle001",
        "Puzzles/puzzle002",
        "Puzzles/puzzle003",
        "Puzzles/puzzle004",
        "Puzzles/puzzle005",
        "Puzzles/puzzle006",
        "Puzzles/puzzle007"
    };

    public PuzzleData LoadPuzzle()
    {
        string chosenFile;




        ProgressManager progress = FindFirstObjectByType<ProgressManager>();
        if (progress != null && !string.IsNullOrEmpty(progress.currentPuzzleId))
        {
            chosenFile = "Puzzles/" + progress.currentPuzzleId;
        }
        else
        {
            Debug.LogWarning("No puzzle ID set. Defaulting to puzzle005.");
            chosenFile = "Puzzles/puzzle005";
        }

        TextAsset jsonText = Resources.Load<TextAsset>(chosenFile);

        if (jsonText == null)
        {
            Debug.LogError("Could not find puzzle file: " + chosenFile);
            return null;
        }

        Debug.Log("PuzzleLoader attempting to load — puzzle ID is: " + progress?.currentPuzzleId);

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