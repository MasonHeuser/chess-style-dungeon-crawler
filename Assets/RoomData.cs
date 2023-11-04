using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class RoomData
{   
    public string seed;
    public Vector2Int location;
    public int enemyCount;
    public List<LootData> loot = new List<LootData>();
}
