using UnityEngine;
using UnityEngine.UI;

public class MapNodeIconHandler : MonoBehaviour
{
    public Image iconImage;
    public Sprite puzzleIcon;
    public Sprite shopIcon;
    public Sprite eventIcon;
    public Sprite bossIcon;

    public void SetIcon(MapNodeType type)
    {

        Debug.Log($"🖼 Setting icon for type: {type}");

        if (iconImage == null)
    {
        Debug.LogError("❌ iconImage is null in MapNodeIconHandler");
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
    }

    public class TestIconSetter : MonoBehaviour
{
    public MapNodeIconHandler handler;

    void Start()
    {
        handler.SetIcon(MapNodeType.Shop);
    }
}

}
