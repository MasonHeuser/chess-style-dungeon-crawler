using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileStatusBase", menuName = "ScriptableObjects/TileStatusBase")]
public class TileStatusBase : ScriptableObject
{   
    public string id = System.Guid.NewGuid().ToString();
    public string title;
    // public Tile tileSprite;
    // public AnimatedTile tileA;
    public Color color;
    public ParticleSystem effect;
    public AudioClip sound;
    [Tooltip("Should always be more than 10 so if agency would be 2 make it 12")]
    public int movementPenalty;
    public int turnDuration;
    public TileStatusBase[] denyElements;

    public virtual void InstanceTileStatus(Unit src, TileData tile) {

        if (tile != null && tile.type == TileData.TileType.Walkable) {

            if (tile.status != null) {
                
                if (denyElements.Contains(tile.status.statusType)) { return; }

                if (this == tile.status.statusType) {
                    tile.status.src = src;
                    tile.status.tileStatusDur = turnDuration;
                    return;
                }

                if (Board.data.ElementalInteraction(this, tile.status.statusType, out TileStatusBase interactionTileStatus)) {

                    Board.data.interactionProcs.Add(tile, interactionTileStatus);
                    tile.status.src = src;
                    return;
                }
            }

            Tile tileBase = Board.data.statusTile;
            tileBase.color = color;
            Board.data.tilemapStatus.SetTile(tile.tilePos, tileBase);
            Board.data.tilemapStatus.RefreshTile(tile.tilePos);
            ParticleSystem effectGO = null;
            if (effect != null) {
                effectGO = Instantiate(effect);
                effectGO.transform.SetParent(Overlook.data.tileStatusEffects, false);
                effectGO.name = effect.name;
                effectGO.transform.position = Board.data.parentGrid.GetCellCenterWorld(tile.tilePos);
            }
            if (sound != null) {
                Overlook.data.audioSource.PlayOneShot(sound);
            }
            tile.status = new TileStatusData(this, effectGO, src);
            tile.movementPenalty = movementPenalty;
            
        }
    }

    public virtual void TileStatusEffect(TileData tile) {
        return;
    }

    public virtual void EndTileStatus(TileData tile) {

        if (tile.status.effect != null) { tile.status.effect.Stop(true, ParticleSystemStopBehavior.StopEmitting); }
        tile.status = null;
        tile.movementPenalty = 0;
        Board.data.tilemapStatus.SetTile(tile.tilePos, null);
    }

    public virtual string Desc() {
        return "";
    }
}
