using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ItemData
{
    public string id = System.Guid.NewGuid().ToString();
    public string idType;
    public int xPos;
    public int yPos;
    public bool flipped;
    public int count = 1;

    public string equippedTo;
}
