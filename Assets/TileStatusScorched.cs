using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Scorched", menuName = "ScriptableObjects/TileStatus/Scorched")]
public class TileStatusScorched : TileStatusBase
{
    public override string Desc() {
        return "Recently burned tile. Denys fire spread to this tile.";
    }
}
