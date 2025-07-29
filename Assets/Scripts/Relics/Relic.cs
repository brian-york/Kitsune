using UnityEngine;

/// <summary>
/// Base class for all relics.
/// </summary>
public abstract class Relic : ScriptableObject
{
    public string relicName;
    [TextArea]
    public string description;
    public Sprite icon;

    /// <summary>
    /// Optional override. Modifies score when a tile is placed.
    /// </summary>
    public virtual int ModifyScore(int basePoints, TileData tile)
    {
        return basePoints;
    }

    /// <summary>
    /// Optional override. Called at the start of each puzzle.
    /// </summary>
    public virtual void OnPuzzleStart(GridSpawner gridSpawner)
    {
        // Default does nothing
    }

    /// <summary>
    /// Optional override. Called whenever a tile is drawn.
    /// </summary>
    public virtual void OnTileDraw(TileData tile)
    {
        // Default does nothing
    }

    public virtual void OnCurrencyGain(ref int amount, CellController cell) { }
}
