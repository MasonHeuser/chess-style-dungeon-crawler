using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemBase", menuName = "ScriptableObjects/ItemBase", order = 1)]
public class ItemBase : ScriptableObject
{   
    public string id = System.Guid.NewGuid().ToString();
    public string title;
    public Vector2Int size;
    public int maxStackSize;
    public Sprite sprite;

    public enum ItemType {
        Attack = 0,
        Death = 1,
        Ability = 2  
    }

    public ItemType type = ItemType.Attack;

    public virtual void InitializeItem(Unit unit) {
        return;
    }

    public virtual IEnumerator Passive(Unit unit) {
        yield break;
    }

    public virtual void Active(Unit unit) {
        return;
    }

    public virtual string ShortDesc() {
        return "";
    }

    public virtual string LongDesc() {
        return "";
    }

    public virtual string StatDesc() {
        return "";
    }

    public virtual string ActiveDesc() {
        return "";
    }

    public virtual string PassiveDesc() {
        return "";
    }
}
