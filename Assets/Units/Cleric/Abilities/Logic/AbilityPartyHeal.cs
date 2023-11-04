using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityPartyHeal", menuName = "ScriptableObjects/Abilities/Cleric/AbilityPartyHeal", order = 1)]
public class AbilityPartyHeal : AbilityBase
{   
    public float baseMin;
    public float baseMax;
    public float abilityPerc;
    public float percentMissing;

    public override IEnumerator AbilityAction(Unit unit, TileData actionedTile)
    {   
        //Cleanse
        Unit lowestHealthUnit = null;
        float prevLowest = float.MaxValue;
        foreach(Unit teamUnit in Overlook.data.teams[unit.team]) {
            
            float percMissingHealth = ((teamUnit.stats.health.value - 0) / (teamUnit.stats.health.basic - 0)) * 100;
            if (percMissingHealth < prevLowest) {
                lowestHealthUnit = teamUnit;
            }
            prevLowest = percMissingHealth;
        }

        if (lowestHealthUnit != null) {

            List<StatusEffectData> toRemove = new List<StatusEffectData>();
            foreach(StatusEffectData data in lowestHealthUnit.activeStatusEffects) {

                if (data.statusEffectBase.statusType == StatusEffectBase.StatusType.Negative) {
                    data.statusEffectBase.StatusEffectEnd(lowestHealthUnit, data.srcSnapShot);
                    toRemove.Add(data);
                }
            }
            foreach(StatusEffectData data in toRemove) {
                lowestHealthUnit.activeStatusEffects.Remove(data);
            }
            lowestHealthUnit.overhead.UpdateOverhead();
        }

        //Heal
        foreach(Unit teamUnit in Overlook.data.teams[unit.team]) {
            
            float missingHealth = (teamUnit.stats.health.basic - teamUnit.stats.health.value);
            float randBase = Random.Range(baseMin, baseMax);
            float heal = (randBase + (unit.stats.abilityStrength.value * abilityPerc)) + (missingHealth * percentMissing);
            teamUnit.HealUnit(unit, heal);
        }
        yield break;
    }

    public override List<TileData> AbilityPattern(Unit unit) {
        List<TileData> highlightTiles = new List<TileData>();
        foreach(Unit teamUnit in Overlook.data.teams[unit.team]) {
            highlightTiles.Add(teamUnit.curTile);
        }
        return highlightTiles;
    }

    public override string Desc(Unit unit) {
        int health = (int)RelationalData.StatType.Health;
        float percentPer = percentMissing * 100;
        return "Heal each ally unit <color="+ RelationalData.data.statColor[health] +">" + (baseMin + (unit.stats.abilityStrength.value * abilityPerc)) + "-" + (baseMax + (unit.stats.abilityStrength.value * abilityPerc)) + "</color> <color="+RelationalData.data.statColor[(int)RelationalData.StatType.AbilityStrength]+">( <sprite=\"StatAtlas\" index= " + (int)RelationalData.StatType.AbilityStrength + ">" + (abilityPerc * 100) + "%)</color> plus <color="+ RelationalData.data.statColor[health] +">" + percentPer + "% of their missing health</color>. Additionally, the ally with the least amount of <color="+ RelationalData.data.statColor[health] +">health</color> gets cleansed all <color=#"+ColorUtility.ToHtmlStringRGB(Color.red)+">Negative</color> Status Effects currently effecting them.";
    }
}
