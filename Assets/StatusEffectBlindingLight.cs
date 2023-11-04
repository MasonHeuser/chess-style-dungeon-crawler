using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "StatusBlindingLight", menuName = "ScriptableObjects/StatusEffect/BlindingLight")]
public class StatusEffectBlindingLight : StatusEffectBase
{
    public float multiAccuracyPenality;
    
    public override void StatusEffectStart(Unit effected, StatData src) {

        effected.stats.accuracy.multiModValue += multiAccuracyPenality;
    }

    public override void StatusEffectEnd(Unit effected, StatData src) {

        effected.stats.accuracy.multiModValue -= multiAccuracyPenality;
    }
}
