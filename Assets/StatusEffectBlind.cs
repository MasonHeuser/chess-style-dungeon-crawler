using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "StatusBlind", menuName = "ScriptableObjects/StatusEffect/Blind")]
public class StatusEffectBlind : StatusEffectBase
{
    public float accuracyPenality;
    
    public override void StatusEffectStart(Unit effected, StatData src) {
        effected.moveEvents.Add(Test);
        effected.stats.accuracy.flatModValue += accuracyPenality;
    }

    public override void StatusEffectEnd(Unit effected, StatData src) {
        effected.moveEvents.Remove(Test);
        effected.stats.accuracy.flatModValue -= accuracyPenality;
    }

    public void Test() {
        Debug.Log("YOU MOVED");
    }
}
