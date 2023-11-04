using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TileStatusData
{
    public TileStatusBase statusType;
    public ParticleSystem effect;
    public Unit src;
    public int tileStatusDur;

    public TileStatusData(TileStatusBase _statusType, ParticleSystem _effect, Unit _src) {

        statusType = _statusType;
        effect = _effect;
        src = _src;
        tileStatusDur = statusType.turnDuration;
    }
}
