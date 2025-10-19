using UnityEngine;
using UnityEngine.EventSystems;

public class CellDropHandler : MonoBehaviour, IDropHandler
{
    public int row;
    public int col;

    public void OnDrop(PointerEventData eventData)
    {
        TileDragHandler tile = eventData.pointerDrag?.GetComponent<TileDragHandler>();
        if (tile == null) return;

        CellController cellController = GetComponent<CellController>();
        PuzzleManager puzzleManager = FindFirstObjectByType<PuzzleManager>();
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        GameManager gm = FindFirstObjectByType<GameManager>();

        if (!ValidateBasicDrop(tile, cellController, puzzleManager))
        {
            DestroyTileAndRefill(tile);
            return;
        }

        ProcessNarrativeTriggers(tile, cellController, scoreManager, gm);

        UpdatePuzzleState(tile, cellController, puzzleManager);

        CalculateAndAwardScore(tile, puzzleManager, scoreManager, gm);

        DestroyTileAndRefill(tile);
    }

    private bool ValidateBasicDrop(TileDragHandler tile, CellController cellController, PuzzleManager puzzleManager)
    {
        if (cellController != null && cellController.locked)
        {
            Debug.Log($"Cell [{row},{col}] is locked. Rejecting drop.");
            return false;
        }

        Debug.Log($"Tile dropped on cell [{row},{col}] with value {tile.tileValue}");

        if (puzzleManager != null && !puzzleManager.IsValid(row, col, tile.tileValue))
        {
            Debug.Log("❌ Invalid placement. Tile destroyed, no score given.");
            return false;
        }

        return true;
    }

    private void ProcessNarrativeTriggers(TileDragHandler tile, CellController cellController, ScoreManager scoreManager, GameManager gm)
    {
        if (cellController == null) return;
        if (cellController.narrativeCellType == CellController.NarrativeCellType.None) return;
        if (cellController.narrativeTriggered) return;

        if (!ValidateNarrativeCondition(tile, cellController))
        {
            return;
        }

        Debug.Log($"✅ Narrative triggered at [{row},{col}]: {cellController.narrativeDescription}");

        if (gm != null)
        {
            gm.lastTriggeredNarrative = cellController.narrativeDescription;
            gm.lastTriggeredCellType = cellController.narrativeCellType;
        }

        ProgressManager progress = ProgressManager.Instance;
        if (progress != null)
        {
            progress.SetNarrativeTrigger(cellController.narrativeCellType);
            Debug.Log($"🧠 ProgressManager updated: Triggered narrative type = {cellController.narrativeCellType}");
        }

        if (cellController.narrativeCellType == CellController.NarrativeCellType.Currency)
        {
            ProcessCurrencyCell(cellController, scoreManager);
        }

        cellController.narrativeTriggered = true;
    }

   private bool ValidateNarrativeCondition(TileDragHandler tile, CellController cellController)
{
    if (cellController.narrativeCondition == null) return true;
    if (!cellController.narrativeCondition.requiresSpecificTile) return true;

    if (cellController.narrativeCondition.requiredTileNumber > 0)
    {
        if (tile.tileValue != cellController.narrativeCondition.requiredTileNumber)
        {
            Debug.Log($"❌ Narrative condition failed at [{row},{col}]. Required number: {cellController.narrativeCondition.requiredTileNumber}, Got: {tile.tileValue}");
            return false;
        }
    }

    if (cellController.narrativeCondition.requiredTileEffect != TileEffect.None)
    {
        if (tile.tileData.tileEffect != cellController.narrativeCondition.requiredTileEffect)
        {
            Debug.Log($"❌ Narrative condition failed at [{row},{col}]. Required effect: {cellController.narrativeCondition.requiredTileEffect}, Got: {tile.tileData.tileEffect}");
            return false;
        }
    }

    return true;
}


    private void ProcessCurrencyCell(CellController cellController, ScoreManager scoreManager)
    {
        Debug.Log($"💰 [CurrencyCell] Triggered at [{row},{col}]");

        int currencyAmount = 1;

        var relics = ProgressManager.Instance?.collectedRelics;
        if (relics != null && relics.Count > 0)
        {
            foreach (var relic in relics)
            {
                Debug.Log($"[🔍 Relic] Evaluating {relic.name}.OnCurrencyGain()");
                relic.OnCurrencyGain(ref currencyAmount, cellController);
            }
        }

        ProgressManager.Instance?.AddCurrency(currencyAmount);
        Debug.Log($"💰 Currency awarded: {currencyAmount}");

        if (HarmonyManager.Instance != null)
        {
            HarmonyManager.Instance.AddHarmony(10, $"Currency cell at [{row},{col}]");
            Debug.Log($"⚖️ Harmony awarded: +10");
        }
        else
        {
            Debug.LogWarning("⚠️ HarmonyManager not found in scene.");
        }

        if (scoreManager != null)
        {
            Vector3 popupPosition = transform.position + new Vector3(0, 50, 0);
            scoreManager.ShowCurrencyPopup(currencyAmount, popupPosition);
        }
    }

    private void UpdatePuzzleState(TileDragHandler tile, CellController cellController, PuzzleManager puzzleManager)
    {
        puzzleManager?.UpdateCell(row, col, tile.tileValue);
        cellController?.SetValue(tile.tileValue, true);
    }

    private void CalculateAndAwardScore(TileDragHandler tile, PuzzleManager puzzleManager, ScoreManager scoreManager, GameManager gm)
    {
        ScoringManager scoringManager = FindFirstObjectByType<ScoringManager>();
        if (scoringManager == null || scoreManager == null) return;

        TileScoreBreakdown breakdown = scoringManager.CalculateTileScore(row, col, tile.tileData, puzzleManager.playerGrid);
        Vector3 cellWorldPos = transform.position;

        float delay = 0f;
        scoreManager.ShowPopupDelayed(breakdown.basePoints, "Tile", cellWorldPos + Vector3.zero, delay += 0f);
        
        if (breakdown.boxSum > 0)
            scoreManager.ShowPopupDelayed(breakdown.boxSum, "Box Sum", cellWorldPos + new Vector3(0, 50, 0), delay += 0.5f);
        
        if (breakdown.rowSum > 0)
            scoreManager.ShowPopupDelayed(breakdown.rowSum, "Row Sum", cellWorldPos + new Vector3(-50, 0, 0), delay += 0.5f);
        
        if (breakdown.colSum > 0)
            scoreManager.ShowPopupDelayed(breakdown.colSum, "Col Sum", cellWorldPos + new Vector3(0, -50, 0), delay += 0.5f);
        
        if (breakdown.tileEffectBonus > 0)
            scoreManager.ShowPopupDelayed(breakdown.tileEffectBonus, "Tile Bonus", cellWorldPos + new Vector3(50, 0, 0), delay += 0.5f);
        
        if (breakdown.relicBonus > 0)
            scoreManager.ShowPopupDelayed(breakdown.relicBonus, "Relic Bonus", cellWorldPos + new Vector3(0, 100, 0), delay += 0.5f);

        scoreManager.AddScore(breakdown.totalPoints);
        gm?.CheckForLevelComplete(scoreManager.currentScore);
    }

    private void DestroyTileAndRefill(TileDragHandler tile)
    {
        Destroy(tile.gameObject);
        FindFirstObjectByType<GridSpawner>()?.RefillTileHand();
    }
}
