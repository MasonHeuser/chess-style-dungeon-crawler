using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityBase", menuName = "ScriptableObjects/AbilityBase", order = 1)]
public class AbilityBase : ScriptableObject
{
    public string id = System.Guid.NewGuid().ToString();
    public string title;
    public Sprite icon;
    public int cooldown;
    public int cost;
    public Vector3Int[] pattern;
    public Vector3Int[] targetedPattern;

    public virtual void IniAbility(Unit unit, AbilityData data) {
        return;
    }

    public virtual IEnumerator AbilityAction(Unit unit, TileData actionedTile) {   
        yield break;
    }

    public virtual List<TileData> AbilityPattern(Unit unit) {
        return Control.data.Pattern(unit.curTile.tilePos, pattern);
    }

    public virtual string Desc(Unit unit) {
        return "";
    }
}
