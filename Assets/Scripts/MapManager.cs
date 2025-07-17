using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    [System.Serializable]
    public class MapNode
    {
        public string nodeName;
        public GameObject nodeButton;
        public string sceneToLoad;
        public bool isUnlocked;
        public bool isCompleted;
    }

    public List<MapNode> nodes = new List<MapNode>();

    private void Start()
    {
        foreach (var node in nodes)
        {
            if (node.nodeButton != null)
            {
                var button = node.nodeButton.GetComponent<UnityEngine.UI.Button>();

                if (button != null)
                {
                    button.onClick.AddListener(() => OnNodeClicked(node));
                    button.interactable = node.isUnlocked;
                }
            }
        }
    }

    private void OnNodeClicked(MapNode node)
    {
        Debug.Log($"Clicked on node: {node.nodeName}");

        if (!string.IsNullOrEmpty(node.sceneToLoad))
        {
            SceneManager.LoadScene(node.sceneToLoad);
        }
    }

    public void UnlockNode(string nodeName)
    {
        foreach (var node in nodes)
        {
            if (node.nodeName == nodeName)
            {
                node.isUnlocked = true;

                var button = node.nodeButton.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                {
                    button.interactable = true;
                }

                break;
            }
        }
    }
}
