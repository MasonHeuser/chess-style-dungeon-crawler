using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileExplosion", menuName = "ScriptableObjects/TileStatus/Explosion")]
public class TileStatusExplosion : TileStatusBase
{   
    public TileStatusBase scorchedTile;

    public override void InstanceTileStatus(Unit src, TileData tile) {

        if (tile.status.effect != null) {
            tile.status.effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        ParticleSystem effectGO = null;
        if (effect != null) {
            effectGO = Instantiate(effect);
            effectGO.transform.position = Board.data.parentGrid.GetCellCenterWorld(tile.tilePos);
        }
        if (sound != null) {
            Overlook.data.audioSource.PlayOneShot(sound);
        }
        if (tile.occupied != null) {
            float dmg = Random.Range(1f, 6f);
            tile.occupied.DamageUnit(src, dmg, (int)RelationalData.DamageType.Fire);
        }
        scorchedTile.InstanceTileStatus(src, tile);
    }
}
