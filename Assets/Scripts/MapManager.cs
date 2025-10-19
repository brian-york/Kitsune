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
    Debug.Log($"🔍 MapManager starting with {nodes.Count} nodes, prefab: {mapNodeButtonPrefab?.name}, panel: {mapPanel?.name}");

    PuzzlePool.Initialize();

    foreach (var node in nodes)
    {
        nodeLookup[node.nodeName] = node;
    }

    foreach (var node in nodes)
    {
        Debug.Log($"🔍 Processing node: {node.nodeName}, spawnPoint: {node.spawnPoint?.name}");
        
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

            node.isCompleted = ProgressManager.Instance.IsNodeCompleted(node.nodeName);
        }

        Debug.Log($"🔍 About to instantiate for node: {node.nodeName}");
        GameObject newNodeGO = Instantiate(mapNodeButtonPrefab, node.spawnPoint.position, Quaternion.identity, mapPanel);
        Debug.Log($"🔍 Instantiated: {newNodeGO.name}");

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
            Debug.Log($"🔍 Found MapNode component on {newNodeGO.name}");
            mapNode.nodeName = node.nodeName;
            mapNode.puzzleId = node.puzzleId;
            mapNode.nodeType = node.nodeType;

            Debug.Log($"🔍 Calling UpdateVisuals on {mapNode.nodeName}");

            try
            {
                mapNode.UpdateVisuals();
                Debug.Log($"🔍 UpdateVisuals completed successfully for {mapNode.nodeName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ CRITICAL: UpdateVisuals failed for {mapNode.nodeName}: {e.Message}");
                Debug.LogException(e);
            }

            Debug.Log($"🔍 Continuing MapManager.Start() after UpdateVisuals");
        }
        else
        {
            Debug.LogError($"❌ No MapNode component found on instantiated object: {newNodeGO.name}");
        }
    }

    UpdateNodeInteractivity();
    DrawLinesBetweenNodes();
    ApplyNarrativeOverrides();
    StretchLineParentToMatchPanel();
}

    
    public void UpdateNextNodeTypeFromNarrative()
{
    if (!ProgressManager.Instance.narrativeTriggeredThisPuzzle)
    {
        Debug.Log("📍 No narrative triggered this puzzle, skipping node update");
        return;
    }
    
    var triggeredType = ProgressManager.Instance.narrativeTypeTriggered;
    string currentNode = ProgressManager.Instance.currentNodeName;
    
    Debug.Log($"📍 Narrative triggered: {triggeredType} from node {currentNode}");
    
    MapNodeData nextNode = FindNextUncompletedNode(currentNode);
    
    if (nextNode != null)
    {
        MapNodeType newType = ConvertNarrativeToMapType(triggeredType);
        nextNode.nodeType = newType;
        
        if (spawnedNodeButtons.TryGetValue(nextNode.nodeName, out GameObject nodeGO))
        {
            MapNode mapNode = nodeGO.GetComponent<MapNode>();
            if (mapNode != null)
            {
                mapNode.nodeType = newType;
                mapNode.UpdateVisuals();
                Debug.Log($"✅ Changed {nextNode.nodeName} to {newType}");
            }
        }
        
        ProgressManager.Instance.narrativeTriggeredThisPuzzle = false;
    }
}

private MapNodeData FindNextUncompletedNode(string fromNodeName)
{
    if (!nodeLookup.TryGetValue(fromNodeName, out MapNodeData currentNode))
        return null;
    
    foreach (string connectedName in currentNode.connectedNodeNames)
    {
        if (nodeLookup.TryGetValue(connectedName, out MapNodeData connected))
        {
            if (!connected.isCompleted)
            {
                Debug.Log($"📍 Found next node: {connectedName}");
                return connected;
            }
        }
    }
    
    return null;
}

private MapNodeType ConvertNarrativeToMapType(CellController.NarrativeCellType narrativeType)
{
    switch (narrativeType)
    {
        case CellController.NarrativeCellType.Shop:
            return MapNodeType.Shop;
        case CellController.NarrativeCellType.Event:
            return MapNodeType.Event;
        case CellController.NarrativeCellType.Boss:
            return MapNodeType.Boss;
        default:
            return MapNodeType.Puzzle;
    }
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
                ProgressManager.Instance.currentNodeName = node.nodeName;
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
    Debug.Log($"🚨 UpdateNodeInteractivity ENTRY - nodes.Count: {nodes?.Count ?? 0}, spawnedNodeButtons.Count: {spawnedNodeButtons?.Count ?? 0}");
    foreach (var node in nodes)
        {
            bool shouldUnlock = true;

            // ✅ Split comma-separated prerequisites
            if (!string.IsNullOrEmpty(node.prerequisiteNodeName))
            {
                string[] prereqNames = node.prerequisiteNodeName.Split(',');

                foreach (string rawName in prereqNames)
                {
                    string prereqName = rawName.Trim();

                    if (nodeLookup.TryGetValue(prereqName, out MapNodeData prereq))
                    {
                        if (!prereq.isCompleted)
                        {
                            shouldUnlock = false;
                            break;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"❓ Prerequisite node '{prereqName}' not found for '{node.nodeName}'");
                        shouldUnlock = false;
                        break;
                    }
                }
            }

            node.isUnlocked = shouldUnlock;

            if (spawnedNodeButtons.TryGetValue(node.nodeName, out GameObject go))
            {
                Debug.Log($"🧭 {node.nodeName} - unlocked: {node.isUnlocked}, completed: {node.isCompleted}");

                var button = go.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = node.isUnlocked && !node.isCompleted;
                    Debug.Log($"🔘 {node.nodeName} button interactable: {button.interactable}");
                }

                // Visual effects
                Transform nodeTransform = go.transform;
                nodeTransform.localScale = node.isUnlocked ? Vector3.one * 1.2f : Vector3.one;

                // ✅ FIXED: Color setting with proper scoping
                var backgroundImage = go.GetComponent<Image>(); // Main background image
                if (backgroundImage != null)
                {
                    Color targetColor;
                    if (node.isCompleted)
                    {
                        targetColor = Color.gray;
                        Debug.Log($"🎨 Setting {node.nodeName} to GREY (completed)");
                    }
                    else if (node.isUnlocked)
                    {
                        targetColor = Color.white;
                        Debug.Log($"🎨 Setting {node.nodeName} to WHITE (available)");
                    }
                    else
                    {
                        targetColor = Color.gray;
                        Debug.Log($"🎨 Setting {node.nodeName} to GRAY (locked)");
                    }

                    backgroundImage.color = targetColor;
                    Debug.Log($"🎨 {node.nodeName} background color set to: {backgroundImage.color}");
                }
                else
                {
                    Debug.LogError($"❌ No background Image found on {node.nodeName}");
                }
            }
            else
            {
                Debug.LogWarning($"❓ No spawned button found for node: {node.nodeName}");
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
        MapNodeType newType = ConvertNarrativeToNodeType(progress.narrativeTypeTriggered);
        int nodesUpdated = 0;

        foreach (var node in nodes)
        {
            if (node.isUnlocked && !node.isCompleted)
            {
                if (spawnedNodeButtons.TryGetValue(node.nodeName, out GameObject go))
                {
                    MapNode mapNodeComponent = go.GetComponent<MapNode>();
                    if (mapNodeComponent != null)
                    {
                        node.nodeType = newType;
                        mapNodeComponent.nodeType = newType;
                        mapNodeComponent.UpdateVisuals();
                        nodesUpdated++;
                        Debug.Log($"🌀 Narrative Routing: Changed node '{node.nodeName}' to {newType}");
                    }
                }
            }
        }

        Debug.Log($"✅ Applied narrative override ({newType}) to {nodesUpdated} interactable node(s)");

        progress.narrativeTriggeredThisPuzzle = false;
        progress.narrativeTypeTriggered = CellController.NarrativeCellType.None;
    }
    else
    {
        Debug.Log("📍 No narrative trigger to apply in ApplyNarrativeOverrides");
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
