using UnityEngine;

[System.Serializable]
public class TileData
{
    public int number;                  // The tile’s numeric value (e.g. 5)
    public Color tileColor;             // Optional color highlight for visuals
    public string specialEffect;        // Name of any special effect (e.g. "Burning")
    public int scoreBonus;              // Extra score for playing this tile
    public bool isWild;                 // True if the tile can be placed anywhere
    public string narrativeTag;         // Used for story events or dialogue triggers
    public int copies;                  // Used to classify copies of a tile


    public TileData(
        int number,
        Color tileColor,
        string specialEffect,
        int scoreBonus,
        bool isWild,
        string narrativeTag
    )
    {
        this.number = number;
        this.tileColor = tileColor;
        this.specialEffect = specialEffect;
        this.scoreBonus = scoreBonus;
        this.isWild = isWild;
        this.narrativeTag = narrativeTag;
    }

    // Convenience constructor for plain numeric tiles
    public TileData(int number)
    {
        this.number = number;
        this.tileColor = Color.white;
        this.specialEffect = "";
        this.scoreBonus = 0;
        this.isWild = false;
        this.narrativeTag = "";
    }
}