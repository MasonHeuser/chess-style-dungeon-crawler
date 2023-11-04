using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityBlindingRadiance", menuName = "ScriptableObjects/Abilities/Cleric/AbilityBlindingRadiance", order = 1)]
public class AbilityBlindingRadiance : AbilityBase
{   
    public float minBase;
    public float maxBase;
    public float perLevelScale;
    public float abilityStrengthPerc;
    public StatusEffectBase statusBlind;

    public override IEnumerator AbilityAction(Unit unit, TileData actionedTile)
    {   
        foreach (TileData tile in Control.data.Pattern(actionedTile.tilePos, targetedPattern)) {

            if (tile.occupied != null && tile.occupied.team != unit.team) {

                statusBlind.InstanceStatusEffect(tile.occupied, unit);
                float min = minBase + (unit.attr.level * perLevelScale) + (unit.stats.abilityStrength.value * abilityStrengthPerc);
                float max = maxBase + (unit.attr.level * perLevelScale) + (unit.stats.abilityStrength.value * abilityStrengthPerc);
                float damage = Random.Range(min, max);
                tile.occupied.DamageUnit(unit, damage, (int)RelationalData.DamageType.Light);
            }
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
        return "";
    }
}
