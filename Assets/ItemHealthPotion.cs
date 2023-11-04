using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealthPotion", menuName = "ScriptableObjects/Item/HealthPotion")]
public class ItemHealthPotion : ItemBase
{
    public int healthGainMax;
    public int healthGainMin;

    public override void InitializeItem(Unit unit) {
        return; 
    }

    public override IEnumerator Passive(Unit unit) {
        yield break;
    }

    public override void Active(Unit unit) {
        return;
    }

    public override string ShortDesc() {
        return "";
    }

    public override string LongDesc() {
        return "";
    }

    public override string StatDesc() {
        return "";
    }

    public override string ActiveDesc() {
        return "";
    }

    public override string PassiveDesc() {
        return "";
    }
}
