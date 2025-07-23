using UnityEngine;

public enum TileEffect
{
    None,
    Booned,
    Leaf,
    Flame,
    Baned,
    Lunar,
    Solar,
    Portent,
    Wild
}



[System.Serializable]
public class TileData
{

    [System.NonSerialized] public int row = -1;
[System.NonSerialized] public int col = -1;
    public int number;
    public Color tileColor;
    public TileEffect tileEffect;
    public int scoreBonus;
    public bool isWild;
    public string narrativeTag;
    public int copies;

    public TileData(
        int number,
        Color tileColor,
        TileEffect tileEffect,
        int scoreBonus,
        bool isWild,
        string narrativeTag
    )
    {
        this.number = number;
        this.tileColor = tileColor;
        this.tileEffect = tileEffect;
        this.scoreBonus = scoreBonus;
        this.isWild = isWild;
        this.narrativeTag = narrativeTag;
    }

    public TileData(int number)
    {
        this.number = number;
        this.tileColor = KitsuneColors.WashedRicePaper;
        this.tileEffect = TileEffect.None;
        this.scoreBonus = 0;
        this.isWild = false;
        this.narrativeTag = "";
    }
}