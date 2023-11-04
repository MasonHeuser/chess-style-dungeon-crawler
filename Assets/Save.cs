using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Save
{   
    public List<UnitAttributes> readyUnits = new List<UnitAttributes>();
    
    public List<UnitAttributes> campUnits = new List<UnitAttributes>();
    
    public int xInv;
    public int yInv;
    public List<ItemData> equipmentItems = new List<ItemData>();
    public List<ItemData> stashItems = new List<ItemData>();
}
