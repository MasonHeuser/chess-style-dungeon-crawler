using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "StatusEffectBase", menuName = "ScriptableObjects/StatusEffectBase")]
public class StatusEffectBase : ScriptableObject
{   
    public string id = System.Guid.NewGuid().ToString();
    public string title;
    public Sprite sprite;
    public enum StatusType {
        Negative = 0,
        Positive = 1,
    }
    public StatusType statusType = StatusType.Negative;
    public int turnDuration;

    public virtual void InstanceStatusEffect(Unit effected, Unit src) {
       
        for (int i = 0; i < effected.activeStatusEffects.Count; i++)
        {
            if (effected.activeStatusEffects[i].statusEffectBase.id == id)
            {   
                effected.activeStatusEffects[i].duration = turnDuration;
                return;
            }
        }

        StatData snapshot = System.ObjectExtensions.Copy(src.stats);
        StatusEffectData statusData = new StatusEffectData(this, snapshot);
        effected.activeStatusEffects.Add(statusData);
        effected.overhead.UpdateOverhead();
        StatusEffectStart(effected, snapshot);
        return;
    }

    public virtual void StatusEffectStart(Unit effected, StatData src) {
        return;
    }

    public virtual void StatusEffect(Unit effected, StatData src) {
        return;
    }

    public virtual void StatusEffectEnd(Unit effected, StatData src) {
        return;
    }

    public virtual string Desc(Unit effected, StatData src) {
        return "";
    }
}
