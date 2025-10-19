using System.Collections.Generic;
using UnityEngine;

public static class PuzzlePool
{
    private static List<string> easyPuzzles = new List<string>();
    private static List<string> mediumPuzzles = new List<string>();
    private static List<string> hardPuzzles = new List<string>();

    public static void Initialize()
    {
        easyPuzzles.Clear();
        mediumPuzzles.Clear();
        hardPuzzles.Clear();

        TextAsset[] allPuzzles = Resources.LoadAll<TextAsset>("Puzzles");
        
        foreach (TextAsset puzzleAsset in allPuzzles)
        {
            try
            {
                PuzzleData puzzleData = JsonUtility.FromJson<PuzzleData>(puzzleAsset.text);
                
                string difficulty = puzzleData.difficulty.ToLower();
                
                if (difficulty.Contains("easy"))
                {
                    easyPuzzles.Add(puzzleAsset.name);
                    Debug.Log($"📘 Loaded EASY puzzle: {puzzleAsset.name}");
                }
                else if (difficulty.Contains("hard"))
                {
                    hardPuzzles.Add(puzzleAsset.name);
                    Debug.Log($"📕 Loaded HARD puzzle: {puzzleAsset.name}");
                }
                else
                {
                    mediumPuzzles.Add(puzzleAsset.name);
                    Debug.Log($"📗 Loaded MEDIUM puzzle: {puzzleAsset.name}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to parse puzzle {puzzleAsset.name}: {e.Message}");
            }
        }

        Debug.Log($"🎲 PuzzlePool initialized: {easyPuzzles.Count} Easy, {mediumPuzzles.Count} Medium, {hardPuzzles.Count} Hard");
    }

    public static string GetRandomPuzzle(string difficulty)
    {
        List<string> pool = null;
        
        string difficultyLower = difficulty.ToLower();
        
        if (difficultyLower.Contains("easy") && easyPuzzles.Count > 0)
            pool = easyPuzzles;
        else if (difficultyLower.Contains("hard") && hardPuzzles.Count > 0)
            pool = hardPuzzles;
        else if (mediumPuzzles.Count > 0)
            pool = mediumPuzzles;
        
        if (pool != null && pool.Count > 0)
        {
            string selected = pool[Random.Range(0, pool.Count)];
            Debug.Log($"🎲 Selected puzzle: {selected} (difficulty: {difficulty})");
            return selected;
        }

        Debug.LogWarning($"⚠️ No puzzles found for difficulty: {difficulty}");
        return null;
    }
}
