using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileFire", menuName = "ScriptableObjects/TileStatus/Fire")]
public class TileStatusFire : TileStatusBase
{
    public TileStatusBase scorchedTile;
    public StatusEffectBase fireStatus;
    public float spreadRate;

    public override void InstanceTileStatus(Unit src, TileData tile) {

        base.InstanceTileStatus(src, tile);
        if (tile.occupied != null) {

            fireStatus.InstanceStatusEffect(tile.occupied, tile.status.src);
        }
    }

    public override void TileStatusEffect(TileData tile) {

        if (tile.occupied != null) {

            fireStatus.InstanceStatusEffect(tile.occupied, tile.status.src);
        }

        bool flag = false;
        for (int x = -1; x <= 1; x++) {

            for (int y = -1; y <= 1; y++) {

                if (x == 0 && y == 0) {
                    continue;
                }

                if (Random.value <= spreadRate) {

                    Vector3Int tilePosInt = new Vector3Int(tile.tilePos.x + x, tile.tilePos.y + y, 0);
                    TileData newEffectedTile = Board.data.Tiles[tilePosInt];

                    List<TileData> list = new List<TileData>() { newEffectedTile };
                    Board.data.InstanceTileStatusBoard(tile.status.src, this, list);
                    flag = true;
                    break;
                }
            }
            if (flag) break;
        }       
    }

    public override void EndTileStatus(TileData tile) {

        Unit unit = tile.status.src;
        base.EndTileStatus(tile);
        List<TileData> list = new List<TileData>() { tile };
        Board.data.InstanceTileStatusBoard(unit, scorchedTile, list);
    }

    public override string Desc() {
        return "Applies the " + fireStatus.title + " status when occupied. Additionally fire will spread across the room periodically each turn.";
    }
}
