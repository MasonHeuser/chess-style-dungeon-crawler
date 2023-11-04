using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TileData : IHeapItem<TileData>
{   
    public Vector3Int tilePos;
    private Unit s_occupied = null;
    public Unit occupied
    {
        get { return s_occupied; }
        set {
            s_occupied = value;
            if (status != null) {
                status.statusType.TileStatusEffect(this);
            }
        }
    }

    public enum TileType {
        Walkable = 0,
        Wall = 1,
    }

    public TileType type = TileType.Wall;

    public int movementPenalty = 0;
    
    private TileStatusData s_status = null;
    public TileStatusData status
    {
        get { return s_status; }
        set {
            if (s_status != null) {
                
                if (s_status.effect != null) {
                    s_status.effect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            s_status = value;
            // if (s_status != null) {
            //     s_status.statusType.TileStatusEffect(s_status.statusType, this);
            // }
        }
    }

    public int gCost;
    public int hCost;
    public TileData parent;
    int heapIndex;

    public TileData(Vector3Int _tilePos) {
        tilePos = _tilePos;
    }

    public int fCost {
        get {
            return gCost + hCost;
        }
    }

    public int HeapIndex {
        get {
            return heapIndex;
        }
        set {
            heapIndex = value;
        }
    }

    public int CompareTo(TileData nodeToCompare) {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0) {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}
