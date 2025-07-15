using UnityEngine;
using TMPro;

public class PopupScore : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float lifetime = 1.0f;
    public float moveSpeed = 50f;

    private float timer = 0f;
    private CanvasGroup canvasGroup;
    private Vector3 startPos;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        startPos = transform.position;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Move upward
        transform.position = startPos + new Vector3(0, timer * moveSpeed, 0);

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

    public void Initialize(int amount, Color color, Vector3 worldPosition)
    {
        text.text = $"+{amount}";
        text.color = color;

        // Convert world → screen
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        transform.position = screenPos;

        startPos = transform.position;
    }
}
