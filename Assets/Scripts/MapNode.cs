using UnityEngine;
using UnityEngine.UI;

public enum MapNodeType
{
    Puzzle,
    Shop, 
    Event,
    Boss
}

public class MapNode : MonoBehaviour
{
    [Header("Node Configuration")]
    public string nodeName;
    public string puzzleId;
    public MapNodeType nodeType = MapNodeType.Puzzle;
    
    [Header("Visual Components")]
    public Image iconImage;
    
    [Header("Icons")]
    public Sprite puzzleIcon;
    public Sprite shopIcon;
    public Sprite eventIcon;
    public Sprite bossIcon;

   public void UpdateVisuals()
{
    try
    {
        Debug.Log($"🔍 MapNode UpdateVisuals ENTRY for '{nodeName}'");
        
        if (iconImage == null)
        {
            Debug.LogError($"❌ iconImage is null for {nodeName}");
            return;
        }
        
        SetIcon(nodeType);
        Debug.Log($"🔍 MapNode UpdateVisuals COMPLETED for '{nodeName}'");
    }
    catch (System.Exception e)
    {
        Debug.LogError($"❌ Exception in MapNode.UpdateVisuals: {e.Message}");
        Debug.LogException(e);
    }
}


    public void SetIcon(MapNodeType type)
    {
        Debug.Log($"🖼 Setting icon for type: {type}");

        if (iconImage == null)
        {
            Debug.LogError($"❌ iconImage is null in MapNode {nodeName}");
            return;
        }

        switch (type)
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

        Debug.Log($"🖼 Icon set to: {iconImage.sprite?.name ?? "null"}");
         Debug.Log($"🖼 iconImage color: {iconImage.color}");
    }
}
