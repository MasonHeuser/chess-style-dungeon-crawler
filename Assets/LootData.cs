using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LootData
{   
    public string id;
    public string title;
    public Vector3Int tileLocation;
    public List<ItemData> items = new List<ItemData>();
}
