using UnityEngine;
using UnityEngine.EventSystems;

public class CellDropHandler : MonoBehaviour, IDropHandler
{
    public int row;
    public int col;

    public void OnDrop(PointerEventData eventData)
    {
        TileDragHandler tile = eventData.pointerDrag?.GetComponent<TileDragHandler>();
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        PuzzleManager puzzleManager = FindFirstObjectByType<PuzzleManager>();
        GameManager gm = FindFirstObjectByType<GameManager>();

        if (tile == null) return;

        CellController cellController = GetComponent<CellController>();
        if (cellController != null && cellController.locked)
        {
            Debug.Log($"Cell [{row},{col}] is locked. Rejecting drop.");
            Destroy(tile.gameObject);
            FindFirstObjectByType<GridSpawner>()?.RefillTileHand();
            return;
        }

        Debug.Log($"Tile dropped on cell [{row},{col}] with value {tile.tileValue}");

        if (puzzleManager != null && !puzzleManager.IsValid(row, col, tile.tileValue))
        {
            Debug.Log("❌ Invalid placement. Tile destroyed, no score given.");
            Destroy(tile.gameObject);
            FindFirstObjectByType<GridSpawner>()?.RefillTileHand();
            return;
        }


        if (cellController != null && cellController.narrativeCellType != CellController.NarrativeCellType.None && !cellController.narrativeTriggered)
        {
            bool passesCondition = true;

            if (cellController.narrativeCondition != null && cellController.narrativeCondition.requiresSpecificTile)
            {
                passesCondition = tile.tileValue == cellController.narrativeCondition.requiredTileNumber;
                if (!passesCondition)
                {
                    Debug.Log($"❌ Narrative condition failed at [{row},{col}]. Required: {cellController.narrativeCondition.requiredTileNumber}, Got: {tile.tileValue}");
                    Destroy(tile.gameObject);
                    FindFirstObjectByType<GridSpawner>()?.RefillTileHand();
                    return;
                }
            }

            if (passesCondition)
            {
                Debug.Log($"✅ Narrative triggered at [{row},{col}]: {cellController.narrativeDescription}");

                // Store last triggered narrative
                if (gm != null)
                {
                    gm.lastTriggeredNarrative = cellController.narrativeDescription;
                    gm.lastTriggeredCellType = cellController.narrativeCellType;
                }

                // 💰 Currency Logic with Relic Evaluation
                if (cellController.narrativeCellType == CellController.NarrativeCellType.Currency)
                {
                    int currencyAmount = 1;

                    var relics = ProgressManager.Instance?.collectedRelics;
                    if (relics != null && relics.Count > 0)
                    {
                        foreach (var relic in relics)
                        {
                            Debug.Log($"[🔍 Relic Evaluation] Calling {relic.name}.OnCurrencyGain()...");
                            relic.OnCurrencyGain(ref currencyAmount, cellController);
                        }
                    }

                    ProgressManager.Instance?.AddCurrency(currencyAmount);
                    Debug.Log($"💰 Currency awarded for playing on Currency cell. Final amount: {currencyAmount}");

                    if (scoreManager != null)
                    {
                        Vector3 popupPosition = transform.position + new Vector3(0, 50, 0);
                        scoreManager.ShowCurrencyPopup(currencyAmount, popupPosition);
                    }
                }

                cellController.narrativeTriggered = true;
            
            }
        }

        // ✅ Update puzzle grid and UI
        puzzleManager?.UpdateCell(row, col, tile.tileValue);
        cellController?.SetValue(tile.tileValue, true);

        // 🧮 Scoring
        ScoringManager scoringManager = FindFirstObjectByType<ScoringManager>();
        if (scoringManager != null && scoreManager != null)
        {
            TileScoreBreakdown breakdown = scoringManager.CalculateTileScore(row, col, tile.tileData, puzzleManager.playerGrid);
            Vector3 cellWorldPos = transform.position;

            float delay = 0f;
            scoreManager.ShowPopupDelayed(breakdown.basePoints, "Tile", cellWorldPos + Vector3.zero, delay += 0f);
            if (breakdown.boxSum > 0) scoreManager.ShowPopupDelayed(breakdown.boxSum, "Box Sum", cellWorldPos + new Vector3(0, 50, 0), delay += 0.5f);
            if (breakdown.rowSum > 0) scoreManager.ShowPopupDelayed(breakdown.rowSum, "Row Sum", cellWorldPos + new Vector3(-50, 0, 0), delay += 0.5f);
            if (breakdown.colSum > 0) scoreManager.ShowPopupDelayed(breakdown.colSum, "Col Sum", cellWorldPos + new Vector3(0, -50, 0), delay += 0.5f);
            if (breakdown.tileEffectBonus > 0) scoreManager.ShowPopupDelayed(breakdown.tileEffectBonus, "Tile Bonus", cellWorldPos + new Vector3(50, 0, 0), delay += 0.5f);
            if (breakdown.relicBonus > 0) scoreManager.ShowPopupDelayed(breakdown.relicBonus, "Relic Bonus", cellWorldPos + new Vector3(0, 100, 0), delay += 0.5f);

            scoreManager.AddScore(breakdown.totalPoints);
            gm?.CheckForLevelComplete(scoreManager.currentScore);
        }

        Destroy(tile.gameObject);
        FindFirstObjectByType<GridSpawner>()?.RefillTileHand();
    }
}
