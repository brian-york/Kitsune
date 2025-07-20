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


    if (tile != null)
        {
    CellController cellController = GetComponent<CellController>();
    
    if (cellController != null && cellController.locked)
    {
        Debug.Log($"Cell [{row},{col}] is locked (pre-filled). Rejecting drop.");

        Destroy(tile.gameObject);

        GridSpawner gridSpawner = FindFirstObjectByType<GridSpawner>();
        if (gridSpawner != null)
        {
            gridSpawner.RefillTileHand();
        }

        return;
    }


            Debug.Log($"Tile dropped on cell [{row},{col}] with value {tile.tileValue}");

            if (puzzleManager != null && !puzzleManager.IsValid(row, col, tile.tileValue))
            {
                // INVALID PLACEMENT
                Debug.Log("❌ Invalid placement. Tile destroyed, no score given.");

                Destroy(tile.gameObject);

                GridSpawner gridSpawner = FindFirstObjectByType<GridSpawner>();
                if (gridSpawner != null)
                {
                    gridSpawner.RefillTileHand();
                }

                return; // abort the rest of OnDrop
            }

            // ✅ Narrative cell check
            if (cellController != null && cellController.narrativeCellType != CellController.NarrativeCellType.None)
            {
                if (cellController.narrativeCondition != null && cellController.narrativeCondition.requiresSpecificTile)
                {
                    if (tile.tileValue != cellController.narrativeCondition.requiredTileNumber)
                    {
                        Debug.Log($"Cell [{row},{col}] requires tile {cellController.narrativeCondition.requiredTileNumber}, but player tried {tile.tileValue}. Rejecting drop.");

                        Destroy(tile.gameObject);

                        GridSpawner gridSpawner = FindFirstObjectByType<GridSpawner>();
                        if (gridSpawner != null)
                        {
                            gridSpawner.RefillTileHand();
                        }

                        return;
                    }
                    else
                    {
                        Debug.Log($"✅ Narrative condition met at cell [{row},{col}] for tile {tile.tileValue}!");

                        cellController.narrativeTriggered = true;
                        
                        GameManager gm = FindFirstObjectByType<GameManager>();
                        if (gm != null)
                        {
                            gm.lastTriggeredNarrative = cellController.narrativeDescription;
                            gm.lastTriggeredCellType = cellController.narrativeCellType;
                        }

                        if (cellController.narrativeCellType == CellController.NarrativeCellType.Currency)
                        {
                            ProgressManager.Instance?.AddCurrency(1);
                            Debug.Log("💰 Currency awarded for playing on Currency cell.");
                        }

                        if (cellController.narrativeCellType == CellController.NarrativeCellType.Currency && scoreManager != null)
                        {
                            Vector3 popupPosition = transform.position + new Vector3(0, 50, 0);
                            scoreManager.ShowCurrencyPopup(1, popupPosition);
                        }



                    }
                }
                else
{
    // No specific tile required, automatically trigger
    cellController.narrativeTriggered = true;

    GameManager gm = FindFirstObjectByType<GameManager>();
    if (gm != null)
    {
        gm.lastTriggeredNarrative = cellController.narrativeDescription;
        gm.lastTriggeredCellType = cellController.narrativeCellType;
    }

    // ✅ Handle Currency if applicable
    if (cellController.narrativeCellType == CellController.NarrativeCellType.Currency)
    {
        ProgressManager.Instance?.AddCurrency(1);
        Debug.Log("💰 Currency awarded for playing on Currency cell.");

        if (scoreManager != null)
        {
            Vector3 popupPosition = transform.position + new Vector3(0, 50, 0);
            scoreManager.ShowCurrencyPopup(1, popupPosition);
        }
    }
}

            }

            // ✅ Update cell in puzzle grid
            puzzleManager.UpdateCell(row, col, tile.tileValue);

            if (cellController != null)
            {
                cellController.SetValue(tile.tileValue, true);

                // ✅ Check for narrative cell trigger
                if (cellController.narrativeCellType != CellController.NarrativeCellType.None &&
                    !cellController.narrativeTriggered)
                {
                    bool trigger = false;

                    if (cellController.narrativeCondition == null ||
                        !cellController.narrativeCondition.requiresSpecificTile)
                    {
                        trigger = true;
                    }
                    else
                    {
                        if (tile.tileValue == cellController.narrativeCondition.requiredTileNumber ||
                            tile.tileEffect == cellController.narrativeCondition.requiredTileEffect)
                        {
                            trigger = true;
                        }
                    }

                    if (trigger)
                    {
                        cellController.narrativeTriggered = true;

                        Debug.Log($"🎴 Narrative cell triggered at [{row},{col}]: {cellController.narrativeDescription}");

                        GameManager gm = FindFirstObjectByType<GameManager>();
                        if (gm != null)
                        {
                            gm.lastTriggeredNarrative = cellController.narrativeDescription;
                            gm.lastTriggeredCellType = cellController.narrativeCellType;
                        }
                    }
                }
            }

            ScoringManager scoringManager = FindFirstObjectByType<ScoringManager>();


            if (scoringManager != null && scoreManager != null)
            {
                TileScoreBreakdown breakdown = scoringManager.CalculateTileScore(row, col, tile.tileData, puzzleManager.playerGrid);
                Vector3 cellWorldPos = transform.position;

                // Define offsets for each popup type
                Vector3 tileOffset = Vector3.zero;
                Vector3 boxOffset = new Vector3(0, +50, 0);
                Vector3 rowOffset = new Vector3(-50, 0, 0);
                Vector3 colOffset = new Vector3(0, -50, 0);
                Vector3 tileEffectOffset = new Vector3(+50, 0, 0);
                Vector3 relicOffset = new Vector3(0, +100, 0);

                // Use delays so popups appear in sequence
                float delay = 0f;

                if (breakdown.basePoints > 0)
                    scoreManager.ShowPopupDelayed(breakdown.basePoints, "Tile", cellWorldPos + tileOffset, delay += 0.0f);
                if (breakdown.boxSum > 0)
                    scoreManager.ShowPopupDelayed(breakdown.boxSum, "Box Sum", cellWorldPos + boxOffset, delay += 0.5f);
                if (breakdown.rowSum > 0)
                    scoreManager.ShowPopupDelayed(breakdown.rowSum, "Row Sum", cellWorldPos + rowOffset, delay += 0.5f);
                if (breakdown.colSum > 0)
                    scoreManager.ShowPopupDelayed(breakdown.colSum, "Col Sum", cellWorldPos + colOffset, delay += 0.5f);
                if (breakdown.tileEffectBonus > 0)
                    scoreManager.ShowPopupDelayed(breakdown.tileEffectBonus, "Tile Bonus", cellWorldPos + tileEffectOffset, delay += 0.5f);
                if (breakdown.relicBonus > 0)
                    scoreManager.ShowPopupDelayed(breakdown.relicBonus, "Relic Bonus", cellWorldPos + relicOffset, delay += 0.5f);


                // Finally add the total
                scoreManager.AddScore(breakdown.totalPoints);



                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.CheckForLevelComplete(scoreManager.currentScore);
                }
            }

            Destroy(tile.gameObject);

            GridSpawner gs = FindFirstObjectByType<GridSpawner>();
            if (gs != null)
            {
                gs.RefillTileHand();
            }
        }
}




}