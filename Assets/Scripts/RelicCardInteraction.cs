using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RelicCardInteraction : MonoBehaviour
{
    [Header("Setup")]
    public RelicData relicData;

    private bool isSelected = false;
    private bool isPurchased = false;
    private Coroutine scaleCoroutine;

    private void Start()
    {
        DeselectCard();
    }

    public void OnCardClicked()
    {
        if (isPurchased) return;

        if (!isSelected)
        {
            SelectCard();
        }
        else
        {
            bool success = RelicShopManager.Instance.TryBuyRelic(relicData);
            if (success)
            {
                MarkAsPurchased();
            }
            else
            {
                DeselectCard();
            }
        }
    }

    void MarkAsPurchased()
    {
        isPurchased = true;
        
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        Debug.Log($"[Card] {relicData.name} marked as purchased (invisible placeholder)");
    }

    public void SelectCard()
    {
        if (isPurchased) return;

        isSelected = true;

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleTo(Vector3.one * 1.1f, 0.1f));

        RelicShopManager.Instance.DeselectAllCardsExcept(this);
    }

    public void DeselectCard()
    {
        if (isPurchased) return;

        isSelected = false;

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleTo(Vector3.one, 0.1f));
    }
    
    private IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 initial = transform.localScale;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initial, targetScale, timer / duration);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
