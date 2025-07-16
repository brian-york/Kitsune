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
    if (text == null)
        text = GetComponent<TextMeshProUGUI>();

    text.text = textString;
    text.color = color;

    // Convert world position → screen position
    Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

    // Convert screen → local position inside the popup canvas
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        transform.parent as RectTransform,
        screenPos,
        Camera.main,
        out Vector2 localPoint
    );

    RectTransform rt = GetComponent<RectTransform>();
    rt.anchoredPosition = localPoint;

    startPos = localPoint;
}


}
