using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitBase unitType;
    public UnitAttributes attr;
    public int team;

    public StatData stats;

    public ItemBase equippedItem;
    public string equippedItemId;
    
    public List<StatusEffectData> activeStatusEffects = new List<StatusEffectData>();

    public List<AbilityData> abilities = new List<AbilityData>();

    private float s_sigCharge;
    public float sigCharge
    {
        get { return s_sigCharge; }
        set {
            s_sigCharge = Mathf.Clamp(value, 0, unitType.sigCharge);
        }
    }

    private int s_movement = 1;
    public int movement
    {
        get { return s_movement; }
        set {
            s_movement = Mathf.Clamp(value, 0, 1);
        }
    }

    public delegate void EventMethod();
    public List<EventMethod> attackEvents = new List<EventMethod>();
    public List<EventMethod> moveEvents = new List<EventMethod>();
    public List<EventMethod> deathEvents = new List<EventMethod>();

    public List<GameObject> unitAssets = new List<GameObject>();
    public Overhead overhead;
    public SpriteRenderer sprite;
    public Transform vfxParent;
    public TileData curTile;

    void Start() {
        stats.parentUnit = this;
        
        stats.health.u = this;
        stats.armor.u = this;
        stats.damage.u = this;
        stats.abilityStrength.u = this;
        stats.accuracy.u = this;
        stats.criticalChance.u = this;
        stats.threat.u = this;
        stats.fireResistance.u = this;
        stats.lightResistance.u = this;
        stats.darkResistance.u = this;
    }

    void Update() {
        
        if (stats.health.value <= 0f) {
            unitType.Death(this);
        }
    }

    public void DamageUnit(Unit src, float damage, int damageType) {

        switch (damageType) {

            case (int)RelationalData.DamageType.Physical :

                if (damage == 0) {
                    CacheReference.asset.DamageText(transform.position, "Miss!", Color.white);
                } else {

                    string critText = "";
                    if (Random.value <= src.stats.criticalChance.value) {
                        damage = damage * 2;
                        critText = "!";
                    }

                    if (stats.armor.value > 0) {
                        stats.armor.value -= 1;
                    } else {
                        stats.health.value -= damage;
                    }
                    overhead.UpdateOverhead();

                    Color eleColor;
                    ColorUtility.TryParseHtmlString(RelationalData.data.damageColor[(int)RelationalData.DamageType.Physical], out eleColor);
                    CacheReference.asset.DamageText(transform.position, "<sprite index= " + (int)RelationalData.DamageType.Physical +"> " + System.Math.Round(damage,1).ToString() + critText, eleColor);
                }
            break;

            case (int)RelationalData.DamageType.Fire :

                if (damage == 0) {
                    CacheReference.asset.DamageText(transform.position, "Miss!", Color.white);
                } else {

                    string critText = "";
                    if (Random.value <= src.stats.criticalChance.value) {
                        damage = damage * 2;
                        critText = "!";
                    }
                    damage -= damage * stats.fireResistance.value;
                    
                    stats.health.value -= damage;
                    overhead.UpdateOverhead();

                    Color eleColor;
                    ColorUtility.TryParseHtmlString(RelationalData.data.damageColor[(int)RelationalData.DamageType.Fire], out eleColor);
                    CacheReference.asset.DamageText(transform.position, "<sprite index= " + (int)RelationalData.DamageType.Fire +"> " + System.Math.Round(damage,1).ToString() + critText, eleColor);
                }
            break;

            case (int)RelationalData.DamageType.Light :

                if (damage == 0) {
                    CacheReference.asset.DamageText(transform.position, "Miss!", Color.white);
                } else {

                    string critText = "";
                    if (Random.value <= src.stats.criticalChance.value) {
                        damage = damage * 2;
                        critText = "!";
                    }
                    damage -= damage * stats.lightResistance.value;
                    
                    stats.health.value -= damage;
                    overhead.UpdateOverhead();

                    Color eleColor;
                    ColorUtility.TryParseHtmlString(RelationalData.data.damageColor[(int)RelationalData.DamageType.Light], out eleColor);
                    CacheReference.asset.DamageText(transform.position, "<sprite index= " + (int)RelationalData.DamageType.Light +"> " + System.Math.Round(damage,1).ToString() + critText, eleColor);
                }
            break;

            case (int)RelationalData.DamageType.Dark :

                if (damage == 0) {
                    CacheReference.asset.DamageText(transform.position, "Miss!", Color.white);
                } else {

                    string critText = "";
                    if (Random.value <= src.stats.criticalChance.value) {
                        damage = damage * 2;
                        critText = "!";
                    }
                    damage -= damage * stats.darkResistance.value;
                    
                    stats.health.value -= damage;
                    overhead.UpdateOverhead();

                    Color eleColor;
                    ColorUtility.TryParseHtmlString(RelationalData.data.damageColor[(int)RelationalData.DamageType.Dark], out eleColor);
                    CacheReference.asset.DamageText(transform.position, "<sprite index= " + (int)RelationalData.DamageType.Dark +"> " + System.Math.Round(damage,1).ToString() + critText, eleColor);
                }
            break;
        }

        overhead.StartCoroutine(overhead.ShakePos(damage));
    }

    public void HealUnit(Unit src, float amount) {

        string critText = "";
        if (Random.value <= src.stats.criticalChance.value) {
            amount = amount * 2;
            critText = "!";
        }

        stats.health.value = Mathf.Clamp((stats.health.value + amount), 0, stats.health.basic);
        overhead.UpdateOverhead();

        Color eleColor;
        ColorUtility.TryParseHtmlString(RelationalData.data.statColor[0], out eleColor);
        CacheReference.asset.DamageText(transform.position, "<sprite=\"StatAtlas\" index= " + "0" +"> " + System.Math.Round(amount,1).ToString() + critText, eleColor);
    }

    public void FlipSprite(Vector3 oldPos, Vector3 newPos) {
                            
        if (newPos.x < oldPos.x) {
            sprite.flipX = true;
        }
        else if (newPos.x > oldPos.x) {
            sprite.flipX = false;
        }
    }

    // public IEnumerator Move(TileData oldTile, TileData newTile) {

    //     oldTile.occupied = null;
    //     FlipSprite(oldTile.tilePos, newTile.tilePos);
    //     yield return StartCoroutine(unitType.MoveAction(this, Board.data.parentGrid.GetCellCenterWorld(newTile.tilePos)));
    //     newTile.occupied = this;
    //     curTile = newTile;
    //     yield break;
    // }
}
