using UnityEngine;
using UnityEngine.UI;

public class RelicCardInteraction : MonoBehaviour
{
    public Image highlightBorder;
    public RelicData relicData;

    private bool isSelected = false;

    public void OnCardClicked()
    {
        if (!isSelected)
        {
            SelectCard();
        }
        else
        {
            BuyCard();
        }
    }

    public void SelectCard()
    {
        isSelected = true;
        highlightBorder.gameObject.SetActive(true);
        RelicShopManager.Instance.DeselectAllCardsExcept(this);
    }

    public void DeselectCard()
    {
        isSelected = false;
        highlightBorder.gameObject.SetActive(false);
    }

    private void BuyCard()
    {
        RelicShopManager.Instance.TryBuyRelic(relicData);
        DeselectCard();
    
    if (RelicShopManager.Instance.TryBuyRelic(relicData))
    {
        // Only destroy if purchase succeeded
        Destroy(gameObject); // This destroys the card from the UI
    }
    else
    {
        DeselectCard(); // Optional: fallback if not enough Mon
    }
}
}
