using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class MapManager : MonoBehaviour
{
    [System.Serializable]
    public class MapNodeData
    {
        public string nodeName;
        public string sceneToLoad;
        public bool isUnlocked;
        public bool isCompleted;
        public Transform spawnPoint;
        public MapNodeType nodeType = MapNodeType.Puzzle;
        public List<string> connectedNodeNames = new List<string>();
        public MapNodeData prerequisiteNode;
        
    }

    public List<MapNodeData> nodes = new List<MapNodeData>();
    public Transform mapPanel;
    public GameObject defaultNodeButtonPrefab;

    [Header("Line Drawing")]
    public GameObject linePrefab;
    public Transform lineParent;
    private Dictionary<string, GameObject> spawnedNodeButtons = new Dictionary<string, GameObject>();
    private Dictionary<string, RectTransform> nodePositionLookup = new Dictionary<string, RectTransform>();

    private void Start()
    {
        Debug.Log("🟢 MapManager.Start() has run");

        foreach (var node in nodes)
        {
            GameObject newNodeGO = Instantiate(defaultNodeButtonPrefab, node.spawnPoint.position, Quaternion.identity, mapPanel);
            spawnedNodeButtons[node.nodeName] = newNodeGO;

            RectTransform rect = newNodeGO.GetComponent<RectTransform>();
            if (rect != null)
            {
                nodePositionLookup[node.nodeName] = rect;
            }

            var button = newNodeGO.GetComponent<Button>();
            if (button != null)
            {
                MapNodeData localNode = node; // ensure correct closure reference
                button.onClick.AddListener(() => OnNodeClicked(localNode));
            }

            var mapNode = newNodeGO.GetComponent<MapNode>();
            if (mapNode != null)
            {
                mapNode.puzzleId = node.nodeName;
                mapNode.nodeType = node.nodeType;
                mapNode.UpdateVisuals();
            }

            var iconHandler = newNodeGO.GetComponent<MapNodeIconHandler>();
            if (iconHandler != null)
            {
                Debug.Log($"🧩 Setting icon for node '{node.nodeName}' of type {node.nodeType}");
                iconHandler.SetIcon(node.nodeType);
            }
            else
            {
                Debug.LogWarning($"⚠️ No MapNodeIconHandler found on node '{node.nodeName}'");
            }
            
        }

        DrawLinesBetweenNodes();

        // Apply narrative override
        ProgressManager progress = ProgressManager.Instance;
        if (progress != null && progress.narrativeTriggeredThisPuzzle)
        {
            foreach (var node in nodes)
            {
                if (node.isUnlocked && !node.isCompleted)
                {
                    if (spawnedNodeButtons.TryGetValue(node.nodeName, out GameObject go))
                    {
                        MapNode mapNodeComponent = go.GetComponent<MapNode>();
                        if (mapNodeComponent != null)
                        {
                            MapNodeType newType = ConvertNarrativeToNodeType(progress.narrativeTypeTriggered);
                            mapNodeComponent.nodeType = newType;
                            mapNodeComponent.UpdateVisuals();
                            Debug.Log($"🌀 Narrative Routing: Setting node '{node.nodeName}' to type {newType}");
                        }
                    }
                    break;
                }
            }

            progress.narrativeTriggeredThisPuzzle = false;
            progress.narrativeTypeTriggered = CellController.NarrativeCellType.None;
        }

        UpdateNodeInteractivity();
        DrawLinesBetweenNodes();
    }


    private void DrawLinesBetweenNodes()
    {
        foreach (var node in nodes)
        {
            if (!nodePositionLookup.ContainsKey(node.nodeName)) continue;
            Vector2 start = nodePositionLookup[node.nodeName].anchoredPosition;

            foreach (var targetName in node.connectedNodeNames)
            {
                if (!nodePositionLookup.ContainsKey(targetName)) continue;
                Vector2 end = nodePositionLookup[targetName].anchoredPosition;

                GameObject lineGO = Instantiate(linePrefab, lineParent);
                UILineRenderer line = lineGO.GetComponent<UILineRenderer>();

                if (line != null)
                {
                    line.Points = new Vector2[] { start, end };
                }
            }
        }
    }

    private void OnNodeClicked(MapNodeData node)
    {
        Debug.Log($"🖱️ Clicked on node: {node.nodeName}");

        if (node.nodeType == MapNodeType.Puzzle)
        {
            if (!string.IsNullOrEmpty(node.sceneToLoad))
            {
                ProgressManager.Instance.currentPuzzleId = node.nodeName;
                SceneManager.LoadScene(node.sceneToLoad);
            }
        }
        else if (node.nodeType == MapNodeType.Shop)
        {
            SceneManager.LoadScene("RelicShopScene");
        }
        else if (node.nodeType == MapNodeType.Event)
        {
            SceneManager.LoadScene("EventScene");
        }
        else if (node.nodeType == MapNodeType.Boss)
        {
            SceneManager.LoadScene("BossScene");
        }
    }

   private void UpdateNodeInteractivity()
{
    foreach (var node in nodes)
    {
        bool shouldUnlock = false;

        if (node.prerequisiteNode == null)
        {
            shouldUnlock = true;
        }
        else if (node.prerequisiteNode.isCompleted)
        {
            shouldUnlock = true;
        }

        if (shouldUnlock && !node.isUnlocked)
        {
            node.isUnlocked = true;
            Debug.Log($"🔓 Unlocking node: {node.nodeName}");
        }

        if (spawnedNodeButtons.TryGetValue(node.nodeName, out GameObject go))
        {
            var button = go.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = node.isUnlocked && !node.isCompleted;
            }
        }
    }
}


    public void MarkNodeComplete(string nodeName)
    {
        var node = nodes.Find(n => n.nodeName == nodeName);
        if (node != null)
        {
            node.isCompleted = true;
            Debug.Log($"✅ Node completed: {node.nodeName}");
        }

        UpdateNodeInteractivity();
    }

    public void UnlockNode(string nodeName)
    {
        var node = nodes.Find(n => n.nodeName == nodeName);
        if (node != null)
        {
            node.isUnlocked = true;
            Debug.Log($"🟢 Node manually unlocked: {node.nodeName}");
        }

        if (spawnedNodeButtons.TryGetValue(nodeName, out GameObject go))
        {
            var button = go.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = true;
            }
        }
    }

    private MapNodeType ConvertNarrativeToNodeType(CellController.NarrativeCellType narrativeType)
    {
        switch (narrativeType)
        {
            case CellController.NarrativeCellType.Shop:
                return MapNodeType.Shop;
            case CellController.NarrativeCellType.Boss:
                return MapNodeType.Boss;
            case CellController.NarrativeCellType.Event:
                return MapNodeType.Event;
            default:
                return MapNodeType.Puzzle;
        }
    }
}
