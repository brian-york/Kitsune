using UnityEngine;

public class CellDropHandler : MonoBehaviour
{
    public int row;
    public int col;

    public void OnTileDrop(TileDragHandler tile)
    {
        if (tile == null) return;

        CellController cellController = GetComponent<CellController>();
        CellControllerV2 cellControllerV2 = GetComponent<CellControllerV2>();
        
        PuzzleManager puzzleManager = FindFirstObjectByType<PuzzleManager>();
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        GameManager gm = FindFirstObjectByType<GameManager>();

        bool isValidPlacement = ValidateBasicDrop(tile, cellController, cellControllerV2, puzzleManager);

        if (isValidPlacement)
        {
            ProcessNarrativeTriggers(tile, cellController, scoreManager, gm);
            UpdatePuzzleState(tile, cellController, cellControllerV2, puzzleManager);
            
            if (TileEffectHandler.Instance != null)
            {
                TileEffectHandler.Instance.ProcessOnPlacementEffect(tile.tileData, row, col);
            }
            
            CalculateAndAwardScore(tile, puzzleManager, scoreManager, gm);

            RegionTracker.Instance?.CheckRegionCompletion(row, col);

            EnemyManager.Instance?.OnTilePlaced(tile.tileData, true);

            EnemyStatusManager statusMgr = EnemyStatusManager.Instance;
            statusMgr?.OnTilePlaced();
            
            EnemyManager enemyManager = FindFirstObjectByType<EnemyManager>();
            if (enemyManager != null)
            {
                enemyManager.TakeTileDamage(tile.tileData.number, transform.position);
            }
        }
        else
        {
            EnemyManager.Instance?.OnTilePlaced(tile.tileData, false);
        }

        EnemyStatusManager statusMgr2 = EnemyStatusManager.Instance;
        if (statusMgr2 != null && statusMgr2.hasCorruptedTile && statusMgr2.corruptedTile == tile)
        {
            statusMgr2.ClearCorruptedTile();
            Debug.Log(isValidPlacement 
                ? "✅ Corrupted tile played successfully!" 
                : "💀 Corrupted tile destroyed (invalid placement)");
        }

        DestroyTileAndRefill(tile);
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

    private bool ValidateBasicDrop(TileDragHandler tile, CellController cellController, CellControllerV2 cellControllerV2, PuzzleManager puzzleManager)
    {
        bool isLocked = false;
        
        if (cellController != null && cellController.locked)
        {
            isLocked = true;
        }
        
        if (cellControllerV2 != null && cellControllerV2.locked)
        {
            isLocked = true;
        }

        if (isLocked)
        {
            Debug.Log($"Cell [{row},{col}] is locked. Rejecting drop.");
            return false;
        }

        EnemyStatusManager statusMgr = EnemyStatusManager.Instance;
        
        if (statusMgr != null && statusMgr.hasCorruptedTile)
        {
            if (statusMgr.corruptedTile != tile)
            {
                Debug.Log("💀 You must play the corrupted tile first!");
                return false;
            }
        }

        if (statusMgr != null && statusMgr.isRestrictedToBox)
        {
            int cellBoxIndex = (row / 3) * 3 + (col / 3);
            if (cellBoxIndex != statusMgr.restrictedBoxIndex)
            {
                Debug.Log($"🔒 You can only play in box {statusMgr.restrictedBoxIndex}!");
                return false;
            }
        }

        Debug.Log($"Tile dropped on cell [{row},{col}] with value {tile.tileValue}");
        
        if (tile.tileValue == 0)
        {
            Debug.Log("👻 Blank tile played - no effect!");
            return false;
        }

        if (puzzleManager != null && !puzzleManager.IsValid(row, col, tile.tileValue))
        {
            Debug.Log("❌ Invalid placement. Tile destroyed, no score given.");
            return false;
        }

        return true;
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

        if (scoreManager != null)
        {
            Vector3 popupPosition = transform.position + new Vector3(0, 50, 0);
            scoreManager.ShowCurrencyPopup(currencyAmount, popupPosition);
        }
    }

    private void UpdatePuzzleState(TileDragHandler tile, CellController cellController, CellControllerV2 cellControllerV2, PuzzleManager puzzleManager)
    {
        puzzleManager?.UpdateCell(row, col, tile.tileValue);
        cellController?.SetValue(tile.tileValue, true);
        cellControllerV2?.UpdateValue(tile.tileValue);
    }

    private void CalculateAndAwardScore(TileDragHandler tile, PuzzleManager puzzleManager, ScoreManager scoreManager, GameManager gm)
    {
        ScoringManager scoringManager = FindFirstObjectByType<ScoringManager>();
        if (scoringManager == null || scoreManager == null) return;

        TileScoreBreakdown breakdown = scoringManager.CalculateTileScore(row, col, tile.tileData, puzzleManager.playerGrid);
        
        scoreManager.AddScore(breakdown.totalPoints);
    }

    private void DestroyTileAndRefill(TileDragHandler tile)
    {
        Destroy(tile.gameObject);
        FindFirstObjectByType<GridSpawner>()?.RefillTileHand();
    }
}
