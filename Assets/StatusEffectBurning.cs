using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "StatusBurning", menuName = "ScriptableObjects/StatusEffect/Burning")]
public class StatusEffectBurning : StatusEffectBase
{
    public float minDamage;
    public float maxDamage;
    public float abilityStrengthScaling;

    public override void StatusEffect(Unit effected, StatData src) {

        float damage = Random.Range(minDamage + (src.abilityStrength.value * abilityStrengthScaling), maxDamage + (src.abilityStrength.value * abilityStrengthScaling));
        effected.DamageUnit(src.parentUnit, damage, (int)RelationalData.DamageType.Fire);
    }

    public override string Desc(Unit effected, StatData src) {
        int fire = (int)RelationalData.DamageType.Fire;
        float percentStrengthScaling = abilityStrengthScaling * 100;
        float minDmg = minDamage + (src.abilityStrength.value * abilityStrengthScaling);
        float maxDmg = maxDamage + (src.abilityStrength.value * abilityStrengthScaling);

        return "Burn effected unit every turn for <color=" + RelationalData.data.damageColor[fire] + ">" + minDmg + "-" + maxDmg + "</color> <color=" + RelationalData.data.statColor[(int)RelationalData.StatType.AbilityStrength] + ">(  <sprite=\"StatAtlas\" index= " + (int)RelationalData.StatType.AbilityStrength + ">" + percentStrengthScaling + "%)</color> <color=" + RelationalData.data.damageColor[fire] + ">fire damage.</color>";
    }
}
