using System.Collections.Generic;
using UnityEngine;

public static class PuzzlePool
{
    private static List<string> easyPuzzles = new List<string>();
    private static List<string> mediumPuzzles = new List<string>();

    public static void Initialize()
    {
        easyPuzzles.Clear();
        mediumPuzzles.Clear();

        TextAsset[] allPuzzles = Resources.LoadAll<TextAsset>("Puzzles");
        foreach (TextAsset puzzle in allPuzzles)
        {
            string name = puzzle.name.ToLower();
            if (name.StartsWith("puzzle001") || name.StartsWith("puzzle002"))
                easyPuzzles.Add(puzzle.name);
            else
                mediumPuzzles.Add(puzzle.name);
        }
    }

    public static string GetRandomPuzzle(string difficulty)
    {
        if (difficulty.ToLower() == "easy" && easyPuzzles.Count > 0)
            return easyPuzzles[Random.Range(0, easyPuzzles.Count)];

        if (difficulty.ToLower() == "medium" && mediumPuzzles.Count > 0)
            return mediumPuzzles[Random.Range(0, mediumPuzzles.Count)];

        Debug.LogWarning("⚠️ No puzzles found for difficulty: " + difficulty);
        return null;
    }
}

