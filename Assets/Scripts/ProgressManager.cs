using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;

    public HashSet<string> completedPuzzles = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MarkPuzzleComplete(string puzzleId)
    {
        completedPuzzles.Add(puzzleId);
        Debug.Log($"✅ ProgressManager: Marked puzzle {puzzleId} as complete.");
    }

    public bool IsPuzzleComplete(string puzzleId)
    {
        return completedPuzzles.Contains(puzzleId);
    }
}
