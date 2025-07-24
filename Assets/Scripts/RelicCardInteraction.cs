using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RelicCardInteraction : MonoBehaviour
{
    [Header("Setup")]

    public RelicData relicData;

    private bool isSelected = false;

    private Coroutine scaleCoroutine;

    private void Start()
    {


        DeselectCard(); // Ensure border starts off
    }

    public void OnCardClicked()
    {
        if (!isSelected)
        {
            SelectCard();
        }
        else
        {
            bool success = RelicShopManager.Instance.TryBuyRelic(relicData);
            if (success)
            {
                // Instead of destroying immediately, remove after a slight delay
                StartCoroutine(DestroyAfterFrame());
            }
            else
            {
                DeselectCard(); // Not enough Mon
            }
        }
    }
private IEnumerator DestroyAfterFrame()
{
    yield return null; // Let Unity finish the deselection loop
    Destroy(gameObject);
}
    public void SelectCard()
{
    isSelected = true;

    if (scaleCoroutine != null)
        StopCoroutine(scaleCoroutine);

    scaleCoroutine = StartCoroutine(ScaleTo(Vector3.one * 1.1f, 0.1f));

    RelicShopManager.Instance.DeselectAllCardsExcept(this);
}


   public void DeselectCard()
{
    isSelected = false;

    // Scale down safely
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
