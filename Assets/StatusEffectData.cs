using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class StatusEffectData
{   
    public StatusEffectBase statusEffectBase;
    public StatData srcSnapShot;
    public int duration;

    public StatusEffectData(StatusEffectBase _statusEffect, StatData _src) {
        statusEffectBase = _statusEffect;
        srcSnapShot = _src;
        duration = statusEffectBase.turnDuration + 1;
    }
}
