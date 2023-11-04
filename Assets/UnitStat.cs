using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class UnitStat
{
    public Unit u;

    public float basicBase;

    public float basic;   
    public float value;

    private float s_multiModValue;
    public float multiModValue
    {
        get { return s_multiModValue; }
        set {
            s_multiModValue = value;
            u.unitType.Stats(u);
        }
    }

    private float s_flatModValue;
    public float flatModValue
    {
        get { return s_flatModValue; }
        set {
            s_flatModValue = value;
            u.unitType.Stats(u);
        }
    }
}
