using UnityEngine;
using TMPro;

public class PopupScore : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float lifetime = 1.0f;
    public float moveSpeed = 50f;

    private float timer = 0f;
    private CanvasGroup canvasGroup;
    private Vector2 startPos;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        startPos = transform.position;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Move upward
       RectTransform rt = GetComponent<RectTransform>();
rt.anchoredPosition = startPos + new Vector2(0, timer * moveSpeed);

        // Fade out
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, timer / lifetime);
        }

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(string textString, Color color, Vector3 worldPosition)
{
    Debug.Log($"🎨 PopupScore.Initialize | Text: '{textString}' | Color: {color} | World: {worldPosition}");
    
    if (text == null)
        text = GetComponent<TextMeshProUGUI>();

    text.text = textString;
    text.color = color;

    if (Camera.main == null)
    {
        Debug.LogError("❌ Camera.main is NULL!");
        return;
    }

    Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
    Debug.Log($"📍 World→Screen: {worldPosition} → {screenPos}");

    RectTransform parentRect = transform.parent as RectTransform;
    if (parentRect == null)
    {
        Debug.LogError("❌ Parent is not a RectTransform!");
        return;
    }

    bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
        parentRect,
        screenPos,
        Camera.main,
        out Vector2 localPoint
    );

    Debug.Log($"📍 Screen→Local: {screenPos} → {localPoint} | Success: {success}");

    RectTransform rt = GetComponent<RectTransform>();
    rt.anchoredPosition = localPoint;

    startPos = localPoint;
    
    Debug.Log($"✅ Final anchored position: {rt.anchoredPosition} | Visible: {localPoint.x} x {localPoint.y}");
}


}
