using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TilePowder", menuName = "ScriptableObjects/TileStatus/Powder")]
public class TileStatusPowder : TileStatusBase
{
    public StatusEffectBase blindStatus;

    public override void InstanceTileStatus(Unit src, TileData tile) {

        base.InstanceTileStatus(src, tile);
        if (tile.occupied != null) {

            blindStatus.InstanceStatusEffect(tile.occupied, tile.status.src);
        }
    }

    public override void TileStatusEffect(TileData tile) {
        
        if (tile.occupied != null && tile.type == TileData.TileType.Walkable) {

            blindStatus.InstanceStatusEffect(tile.occupied, tile.status.src);
        }      
    }
}
