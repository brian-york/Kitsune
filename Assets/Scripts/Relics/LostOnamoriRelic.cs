using UnityEngine;

[CreateAssetMenu(fileName = "LostOnamoriRelic", menuName = "Relics/LostOnamoriRelic")]
public class LostOnamoriRelic : Relic
{
    public override void OnCurrencyGain(ref int amount, CellController cell)
    {
        
        if (cell.narrativeCellType == CellController.NarrativeCellType.Currency)
        {
            Debug.Log("💰 Lost Onamori relic triggered! Gaining +1 extra Mon.");
            amount += 1;
        }
    }
}
