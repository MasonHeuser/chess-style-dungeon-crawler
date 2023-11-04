using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

[Serializable]
public class StatData
{   
    public Unit parentUnit; 

    public UnitStat health;
    public UnitStat armor;
    public UnitStat damage;
    public UnitStat abilityStrength;
    public UnitStat accuracy;
    public UnitStat criticalChance;
    public UnitStat threat;
    public UnitStat fireResistance;
    public UnitStat lightResistance;
    public UnitStat darkResistance;
}
