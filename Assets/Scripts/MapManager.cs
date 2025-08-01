using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Collections;
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
        public string prerequisiteNodeName;
        public string puzzleId;
        public string difficulty = "Medium";


    }

    public List<MapNodeData> nodes = new List<MapNodeData>();
    public Transform mapPanel;

    public GameObject mapNodeButtonPrefab;

    [Header("Line Drawing")]
    public GameObject linePrefab;
    public Transform lineParent;

    private Dictionary<string, GameObject> spawnedNodeButtons = new Dictionary<string, GameObject>();
    private Dictionary<string, RectTransform> nodePositionLookup = new Dictionary<string, RectTransform>();
    private Dictionary<string, MapNodeData> nodeLookup = new Dictionary<string, MapNodeData>();

    private void Start()
    {
        Debug.Log("🟢 MapManager.Start() has run");

        PuzzlePool.Initialize();

        // Build node lookup for prerequisites
        foreach (var node in nodes)
        {
            nodeLookup[node.nodeName] = node;
        }

        foreach (var node in nodes)
        {
            // Assign a puzzle ID if not already set
            if (node.nodeType == MapNodeType.Puzzle)
            {
                if (ProgressManager.Instance.HasPuzzleAssignment(node.nodeName))
                {
                    node.puzzleId = ProgressManager.Instance.GetPuzzleForNode(node.nodeName);
                    Debug.Log($"📂 Loaded saved puzzle '{node.puzzleId}' for node '{node.nodeName}'");
                }
                else
                {
                    node.puzzleId = PuzzlePool.GetRandomPuzzle(node.difficulty);
                    ProgressManager.Instance.SetPuzzleForNode(node.nodeName, node.puzzleId);
                    Debug.Log($"🎲 Assigned new puzzle '{node.puzzleId}' to node '{node.nodeName}'");
                }
            }

            GameObject newNodeGO = Instantiate(mapNodeButtonPrefab, node.spawnPoint.position, Quaternion.identity, mapPanel);
            spawnedNodeButtons[node.nodeName] = newNodeGO;

            RectTransform rect = newNodeGO.GetComponent<RectTransform>();
            if (rect != null)
            {
                nodePositionLookup[node.nodeName] = rect;
            }

            var button = newNodeGO.GetComponent<Button>();
            if (button != null)
            {
                MapNodeData localNode = node;
                button.onClick.AddListener(() => OnNodeClicked(localNode));
            }

            var mapNode = newNodeGO.GetComponent<MapNode>();
            if (mapNode != null)
            {
                mapNode.puzzleId = node.puzzleId;
                mapNode.nodeType = node.nodeType;
                mapNode.UpdateVisuals();
            }

            var iconHandler = newNodeGO.GetComponent<MapNodeIconHandler>();
            if (iconHandler != null)
            {
                iconHandler.SetIcon(node.nodeType);
            }
        }

        UpdateNodeInteractivity();
        DrawLinesBetweenNodes();
        ApplyNarrativeOverrides();
        StretchLineParentToMatchPanel();
    }
private void StretchLineParentToMatchPanel()
{
    RectTransform panelRect = mapPanel.GetComponent<RectTransform>();
    RectTransform lineParentRect = lineParent.GetComponent<RectTransform>();

    // Stretch to match parent anchors (fill)
    lineParentRect.anchorMin = Vector2.zero;
    lineParentRect.anchorMax = Vector2.one;
    lineParentRect.offsetMin = Vector2.zero;
    lineParentRect.offsetMax = Vector2.zero;

    // Match scale/position if needed
    lineParentRect.localScale = Vector3.one;
    lineParentRect.localPosition = Vector3.zero;
}

    private void DrawLinesBetweenNodes()
    {
        foreach (var node in nodes)
        {
            if (!nodePositionLookup.ContainsKey(node.nodeName))
                continue;

            RectTransform startRect = nodePositionLookup[node.nodeName];

            foreach (var targetName in node.connectedNodeNames)
            {
                if (!nodePositionLookup.ContainsKey(targetName))
                    continue;

                RectTransform endRect = nodePositionLookup[targetName];

                // 🌐 Convert world positions to local relative to lineParent
                Vector2 startWorld = startRect.position;
                Vector2 endWorld = endRect.position;

                Vector2 startLocal = lineParent.InverseTransformPoint(startWorld);
                Vector2 endLocal = lineParent.InverseTransformPoint(endWorld);

                Debug.Log($"🌐 World Start: {startWorld} → Local: {startLocal}");
                Debug.Log($"🌐 World End: {endWorld} → Local: {endLocal}");

                GameObject lineGO = Instantiate(linePrefab, lineParent);

                RectTransform lineRect = lineGO.GetComponent<RectTransform>();
                lineRect.anchorMin = Vector2.zero;
                lineRect.anchorMax = Vector2.one;
                lineRect.offsetMin = Vector2.zero;
                lineRect.offsetMax = Vector2.zero;
                lineRect.localScale = Vector3.one;

                lineGO.transform.SetAsFirstSibling(); // push behind nodes

                UILineRenderer line = lineGO.GetComponent<UILineRenderer>();
                if (line != null)
                {
                    line.Points = new Vector2[] { startLocal, endLocal };
                    line.SetAllDirty();

                    Debug.Log($"🧮 Line Points: {startLocal} → {endLocal}");
                    Debug.Log($"✅ Line drawn from {node.nodeName} → {targetName}");
                }
                else
                {
                    Debug.LogError("❌ UILineRenderer missing on line prefab!");
                }
            }
        }
    }

    private void OnNodeClicked(MapNodeData node)
    {
        Debug.Log($"🖱️ Clicked on node: {node.nodeName}");
        if (node.nodeType == MapNodeType.Puzzle)
        {
            if (!string.IsNullOrEmpty(node.puzzleId))
            {
                ProgressManager.Instance.currentPuzzleId = node.puzzleId;
                SceneManager.LoadScene("SudokuScene");
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
            bool shouldUnlock = true;

            if (!string.IsNullOrEmpty(node.prerequisiteNodeName))
            {
                if (nodeLookup.TryGetValue(node.prerequisiteNodeName, out MapNodeData prereq))
                {
                    shouldUnlock = prereq.isCompleted;
                }
                else
                {
                    Debug.LogWarning($"❓ Prerequisite node '{node.prerequisiteNodeName}' not found for '{node.nodeName}'");
                    shouldUnlock = false;
                }
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
        if (nodeLookup.TryGetValue(nodeName, out var node))
        {
            node.isCompleted = true;
            Debug.Log($"✅ Node completed: {node.nodeName}");
            UpdateNodeInteractivity();
        }
    }

    public void UnlockNode(string nodeName)
    {
        if (nodeLookup.TryGetValue(nodeName, out var node))
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

    private void ApplyNarrativeOverrides()
    {
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
    }

    private MapNodeType ConvertNarrativeToNodeType(CellController.NarrativeCellType narrativeType)
    {
        switch (narrativeType)
        {
            case CellController.NarrativeCellType.Shop: return MapNodeType.Shop;
            case CellController.NarrativeCellType.Boss: return MapNodeType.Boss;
            case CellController.NarrativeCellType.Event: return MapNodeType.Event;
            default: return MapNodeType.Puzzle;
        }
    }
    
    private IEnumerator DelayedDrawLines()
{
    yield return new WaitForEndOfFrame(); // Wait for layout to finish

    // Now run DrawLinesBetweenNodes
    DrawLinesBetweenNodes();
}

}
