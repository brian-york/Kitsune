using UnityEngine;
using TMPro;

public class CellControllerV2 : MonoBehaviour
{
    [Header("Cell References")]
    public SpriteRenderer backgroundRenderer;
    public TextMeshPro numberText;
    
    [Header("Cell Background Variants (For Later)")]
    public Sprite[] cellBackgroundVariants;
    
    [Header("Cell Data")]
    public int row;
    public int column;
    public bool isBlocked = false;
    public bool locked = false;
    
    [Header("Visual Colors")]
    public Color lockedColor = new Color(0.95f, 0.92f, 0.88f);
    public Color playableColor = Color.white;
    public Color blockedColor = Color.black;

    [Header("Hover Effect Settings")]
    public float hoverLiftAmount = 0.15f;
    public float hoverVibrateSpeed = 15f;
    public float hoverVibrateAmount = 0.02f;
    
    private int currentValue = 0;
    private Vector3 originalLocalPosition;
    private bool isHovered = false;
    private float vibrateTimer = 0f;
    private bool positionInitialized = false;

    void Update()
    {
        if (isHovered)
        {
            vibrateTimer += Time.deltaTime * hoverVibrateSpeed;
            
            float vibrateX = Mathf.Sin(vibrateTimer) * hoverVibrateAmount;
            float vibrateY = Mathf.Cos(vibrateTimer * 1.3f) * hoverVibrateAmount;
            
            Vector3 targetPos = originalLocalPosition + new Vector3(vibrateX, hoverLiftAmount + vibrateY, -0.1f);
            transform.localPosition = targetPos;
        }
    }

    public void SetupCell(int r, int c)
    {
        row = r;
        column = c;
        gameObject.name = $"Cell_{r}_{c}";
        
        originalLocalPosition = transform.localPosition;
        positionInitialized = true;
        
        if (cellBackgroundVariants != null && cellBackgroundVariants.Length > 0 && backgroundRenderer != null)
        {
            int variantIndex = (r + c) % cellBackgroundVariants.Length;
            backgroundRenderer.sprite = cellBackgroundVariants[variantIndex];
        }
    }

    public void SetValue(int value, bool isLocked)
    {
        currentValue = value;
        locked = isLocked;

        if (numberText != null)
        {
            if (value > 0)
            {
                numberText.text = value.ToString();
                numberText.color = Color.black;
            }
            else
            {
                numberText.text = "";
            }
        }

        if (backgroundRenderer != null)
        {
            if (isLocked)
            {
                backgroundRenderer.color = lockedColor;
            }
            else
            {
                backgroundRenderer.color = playableColor;
            }
        }
    }

    public void SetBlocked(bool blocked)
    {
        isBlocked = blocked;
        locked = blocked;

        if (backgroundRenderer != null)
        {
            if (blocked)
            {
                backgroundRenderer.color = blockedColor;
                
                if (numberText != null)
                    numberText.text = "";
            }
            else
            {
                backgroundRenderer.color = playableColor;
            }
        }
    }

    public void UpdateValue(int value)
    {
        currentValue = value;
        
        if (numberText != null)
        {
            if (value > 0)
                numberText.text = value.ToString();
            else
                numberText.text = "";
        }
    }

    public void OnHoverEnter()
    {
        if (!positionInitialized || locked || isBlocked) return;

        isHovered = true;
        vibrateTimer = 0f;
    }

    public void OnHoverExit()
    {
        if (!positionInitialized) return;
        
        isHovered = false;
        transform.localPosition = originalLocalPosition;
    }
}
