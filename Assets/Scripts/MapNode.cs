using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public enum MapNodeType
{
    Puzzle,
    Shop,
    Event,
    Boss
}

public class MapNode : MonoBehaviour
{
    public string puzzleId; // Used for Puzzle-type nodes
    public MapNodeType nodeType = MapNodeType.Puzzle;
    private Button button;
    public Image iconImage; // Drag in from the inspector
public Sprite puzzleIcon;
public Sprite shopIcon;
public Sprite eventIcon;
    public Sprite bossIcon;


    void Start()
    {
        button = GetComponent<Button>();
        RefreshLockout();

        if (button != null && button.interactable)
        {
            button.onClick.AddListener(OnNodeClicked);

        }
    }

    void OnEnable()
    {
        RefreshLockout();
    }

public void UpdateVisuals()
{
    switch (nodeType)
    {
        case MapNodeType.Puzzle:
            iconImage.sprite = puzzleIcon;
            break;
        case MapNodeType.Shop:
            iconImage.sprite = shopIcon;
            break;
        case MapNodeType.Event:
            iconImage.sprite = eventIcon;
            break;
        case MapNodeType.Boss:
            iconImage.sprite = bossIcon;
            break;
    }
}

    private void RefreshLockout()
    {
        if (nodeType == MapNodeType.Puzzle &&
            ProgressManager.Instance != null &&
            ProgressManager.Instance.IsPuzzleComplete(puzzleId))
        {
            Debug.Log($"[MapNode] Locking puzzle {puzzleId}");

            if (button == null)
                button = GetComponent<Button>();

            button.interactable = false;
            GetComponent<Image>().color = Color.gray;
        }
    }

    private void OnNodeClicked()
    {
        if (ProgressManager.Instance != null && nodeType == MapNodeType.Puzzle)
        {
            ProgressManager.Instance.currentPuzzleId = puzzleId;
        }

        switch (nodeType)
        {
            case MapNodeType.Shop:
                SceneManager.LoadScene("RelicShopScene");
                break;

            case MapNodeType.Event:
                SceneManager.LoadScene("EventScene"); // placeholder
                break;

            case MapNodeType.Boss:
                SceneManager.LoadScene("BossScene"); // placeholder
                break;

            case MapNodeType.Puzzle:
            default:
                SceneManager.LoadScene("SudokuScene");
                break;
        }
    }
}
