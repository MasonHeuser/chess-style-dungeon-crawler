using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cleric", menuName = "ScriptableObjects/Units/Cleric", order = 1)]
public class UnitCleric : UnitBase
{   

    public override IEnumerator AttackAction(Unit unit, TileData attackedTile)
    {   
        unit.FlipSprite(unit.curTile.tilePos, attackedTile.tilePos);

        float damage = 0;
        if (Random.value <= unit.stats.accuracy.value) {

            damage = Random.Range(unit.stats.damage.value, unit.stats.damage.basic);
        }
        attackedTile.occupied.DamageUnit(unit, damage, (int)RelationalData.DamageType.Light);
        yield break;
    }
}
