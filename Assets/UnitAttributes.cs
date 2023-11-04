using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class UnitAttributes
{   
    public string id;
    public string idType;
    public string handle;
    public int level;
    public float health;

    public List<string> abilities = new List<string>();

    // public UnitAttributes(string _id, string _idType, string _handle, int _team, UnitStat _health, UnitStat _armor, UnitStat _damage, UnitStat _accuracy) {

    //     id = _id;
    //     idType = _idType;
    //     handle = _handle;
    //     team = _team;
    //     health = _health;
    //     armor = _armor;
    //     damage = _damage;
    //     accuracy = _accuracy;
    // }
}
