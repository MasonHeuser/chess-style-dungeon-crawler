using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AbilityData
{
    public string tempIdRef;
    public AbilityBase abilityType;
    public int cooldownStart;

    public AbilityData(string _id, AbilityBase _ability) {

        tempIdRef = _id;
        abilityType = _ability;
        if(abilityType != null)
            cooldownStart = -abilityType.cooldown;
    }
}
