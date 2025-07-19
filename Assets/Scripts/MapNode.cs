using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MapNode : MonoBehaviour
{
    public string puzzleId;  // Set in Inspector
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();

        RefreshLockout();

        // ✅ Only assign listener if still interactable after lockout check
        if (button.interactable)
        {
            button.onClick.AddListener(OnNodeClicked);
        }
    }

    void OnEnable()
    {
        RefreshLockout();
    }

    private void RefreshLockout()
    {
        if (ProgressManager.Instance != null && ProgressManager.Instance.IsPuzzleComplete(puzzleId))
        {
            Debug.Log($"[MapNode] Locking puzzle {puzzleId}");

            if (button == null)
                button = GetComponent<Button>();

            button.interactable = false; // 💡 This now disables the button BEFORE listener is added
            GetComponent<Image>().color = Color.gray;
        }
    }

    private void OnNodeClicked()
    {
        StartCoroutine(DelayedSceneLoad());
    }

    private IEnumerator DelayedSceneLoad()
    {
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.currentPuzzleId = puzzleId;
            Debug.Log("[MapNode] Selected puzzle: " + puzzleId);
        }

        yield return null;
        SceneManager.LoadScene("SudokuScene");
    }
}
