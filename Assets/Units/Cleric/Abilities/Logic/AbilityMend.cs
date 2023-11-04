using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityMend", menuName = "ScriptableObjects/Abilities/Cleric/AbilityMend", order = 1)]
public class AbilityMend : AbilityBase
{   
    public StatusEffectBase mendStatus;

    public override IEnumerator AbilityAction(Unit unit, TileData actionedTile)
    {   
        mendStatus.InstanceStatusEffect(actionedTile.occupied, unit);
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
