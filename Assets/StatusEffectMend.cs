using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "StatusMend", menuName = "ScriptableObjects/StatusEffect/Mend")]
public class StatusEffectMend : StatusEffectBase
{
    public float minHeal;
    public float maxHeal;
    public float abilityStrengthPerc;

    public override void StatusEffectStart(Unit effected, StatData src) {
        Effect(effected, src);
    }

    public override void StatusEffect(Unit effected, StatData src) {
        Effect(effected, src);
    }

    void Effect(Unit effected, StatData src) {
        float heal = Random.Range(minHeal + (src.abilityStrength.value * abilityStrengthPerc), maxHeal + (src.abilityStrength.value * abilityStrengthPerc));
        effected.HealUnit(src.parentUnit, heal);
    }

    public override string Desc(Unit effected, StatData src) {

        return "";
    }
}
