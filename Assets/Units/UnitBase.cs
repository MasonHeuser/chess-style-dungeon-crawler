using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitBase", menuName = "ScriptableObjects/UnitBase", order = 1)]
public class UnitBase : ScriptableObject
{
    public string id = System.Guid.NewGuid().ToString();
    public string title;
    public enum Gender {Male = 0, Female = 1}
    public Gender gender = Gender.Male;
    public Sprite sprite;
    public Vector3 spritePos;
    public float overheadOffset;

    public float health;
    public int armor;
    public float minDamage;
    public float maxDamage;
    public float abilityStrength;
    public float accuracy;
    public float criticalChance;
    public float fireResistance;
    public float lightResistance;
    public float darkResistance;

    public Vector3Int[] pattern;
    public AbilityBase signature;
    public float sigCharge;
    public List<AbilityBase> abilityPool = new List<AbilityBase>();
    public List<GameObject> unitAssets = new List<GameObject>();
    public List<AudioClip> audioAssets = new List<AudioClip>();


    public virtual void Initilization(Unit unit) {
        
        unit.gameObject.name = "Unit_" + title;
        unit.sprite.sprite = sprite;
        unit.sprite.gameObject.transform.localPosition = spritePos;
        if (unit.attr.id == "") {
            unit.attr.id = System.Guid.NewGuid().ToString();
            unit.attr.idType = id;
            unit.attr.handle = RelationalData.data.randName.RandomHandle(gender);
        }     
        
        if (unit.equippedItem != null) {
            unit.equippedItem.InitializeItem(unit);
        }
        Stats(unit);

        if (unit.overhead == null) {

            Overhead o = Instantiate(CacheReference.asset.overheadPrefab.GetComponent<Overhead>());
            o.gameObject.name = unit.unitType.title + "_Overhead";
            o.transform.SetParent(CacheReference.asset.overheadParent);
            
            unit.overhead = o;
            o.unit = unit;
        }

        if (unit.unitType.signature != null) {
            unit.abilities.Add(new AbilityData(System.Guid.NewGuid().ToString(), signature));
        }

        if (abilityPool.Count > 0) {

            List<AbilityBase> currentlyEquipped = new List<AbilityBase>();
            foreach(AbilityData data in unit.abilities) {

                if (data.abilityType != null) {
                    currentlyEquipped.Add(data.abilityType);
                }
            }

            //TODO: Clean this up, only needed right now because not every unit has abilities.
            int count = 0;
            if (abilityPool.Count > 2) {
                count = 2;
            } else {
                count = abilityPool.Count;
            }
            int i = 0;
            while (i < count) {

                AbilityBase a = abilityPool[Random.Range(0, abilityPool.Count)];
                if (!currentlyEquipped.Contains(a)) {
                    unit.abilities.Add(new AbilityData(System.Guid.NewGuid().ToString(), a));
                    currentlyEquipped.Add(a);
                    i++;
                }
            }
        }

        if (unit.team == 0) {
            unit.sprite.material.SetColor("outlineColor", CacheReference.asset.friendlyColor);
        } else {
            unit.sprite.material.SetColor("outlineColor", CacheReference.asset.enemyColor);
        }

        // unit.sprite.gameObject.GetComponent<DataAnimator>().curAnimation = animation.Idle;
        foreach (GameObject asset in unitAssets) {
            GameObject g = Instantiate(asset);
            g.transform.SetParent(unit.vfxParent);
            g.transform.position = unit.transform.position;
            g.gameObject.name = asset.name;
            unit.unitAssets.Add(g);
            g.gameObject.SetActive(false);
        }
    }

    public virtual void Stats(Unit unit) {

        unit.stats.health.basicBase = health;
        unit.stats.health.basic = (health + (health * unit.stats.health.multiModValue)) + unit.stats.health.flatModValue;
        unit.stats.health.value = Mathf.Clamp(unit.stats.health.value, 0, unit.stats.health.basic);

        unit.stats.armor.basicBase = armor;
        unit.stats.armor.basic = armor + unit.stats.armor.flatModValue;
        unit.stats.armor.value = Mathf.Clamp(unit.stats.armor.value, 0, unit.stats.armor.basic);
        
        unit.stats.damage.basicBase = maxDamage;
        unit.stats.damage.basic = (maxDamage + (maxDamage * unit.stats.damage.multiModValue)) + unit.stats.damage.flatModValue;
        unit.stats.damage.value = (minDamage + (minDamage * unit.stats.damage.multiModValue)) + unit.stats.damage.flatModValue;

        unit.stats.abilityStrength.basicBase = abilityStrength;
        unit.stats.abilityStrength.value = (abilityStrength + (abilityStrength * unit.stats.abilityStrength.multiModValue)) + unit.stats.abilityStrength.flatModValue;
        unit.stats.abilityStrength.value = Mathf.Clamp(unit.stats.abilityStrength.value, 0f, float.MaxValue);

        unit.stats.accuracy.basicBase = accuracy;
        unit.stats.accuracy.value = (accuracy + (accuracy * unit.stats.accuracy.multiModValue)) + unit.stats.accuracy.flatModValue;
        unit.stats.accuracy.value = Mathf.Clamp(unit.stats.accuracy.value, 0.05f, 1.0f);

        unit.stats.criticalChance.basicBase = criticalChance;
        unit.stats.criticalChance.value = (criticalChance + (criticalChance * unit.stats.criticalChance.multiModValue)) + unit.stats.criticalChance.flatModValue;
        unit.stats.criticalChance.value = Mathf.Clamp(unit.stats.criticalChance.value, 0f, 1.0f);

        unit.stats.threat.basicBase = 0;
        unit.stats.threat.value = 0 + unit.stats.threat.flatModValue;
        unit.stats.threat.value = Mathf.Clamp(unit.stats.threat.value, 0, float.MaxValue);

        unit.stats.fireResistance.basicBase = fireResistance;
        unit.stats.fireResistance.value = (fireResistance + (fireResistance * unit.stats.fireResistance.multiModValue)) + unit.stats.fireResistance.flatModValue;
        unit.stats.fireResistance.value = Mathf.Clamp(unit.stats.fireResistance.value, -0.5f, 1.0f);

        unit.stats.lightResistance.basicBase = lightResistance;
        unit.stats.lightResistance.value = (lightResistance + (lightResistance * unit.stats.lightResistance.multiModValue)) + unit.stats.lightResistance.flatModValue;
        unit.stats.lightResistance.value = Mathf.Clamp(unit.stats.lightResistance.value, -0.5f, 1.0f);

        unit.stats.darkResistance.basicBase = darkResistance;
        unit.stats.darkResistance.value = (darkResistance + (darkResistance * unit.stats.darkResistance.multiModValue)) + unit.stats.darkResistance.flatModValue;
        unit.stats.darkResistance.value = Mathf.Clamp(unit.stats.darkResistance.value, -0.5f, 1.0f);
    }

    public virtual bool AttackCheck(Unit unit, List<TileData> pTargets, out TileData target) {

        //Targeting Logic
        Dictionary<TileData, int> enemies = new Dictionary<TileData, int>();
        target = null;
        if (pTargets.Count > 0) {

            foreach(TileData tile in pTargets) {
                
                if (tile.occupied != null && tile.occupied.team != unit.team) {

                    enemies.Add(tile, (int)tile.occupied.stats.threat.value);
                }
            }
        }

        if (enemies.Count > 0) {

            if (enemies.Values.Distinct().Count() == 0) {

                List<TileData> keys = new List<TileData>(enemies.Keys);
                target = keys[Random.Range(0, keys.Count)];
            } else {
                //Get highest value int.
                target = enemies.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            }
            return true;
        }
        return false;
    }

    public virtual IEnumerator MoveAction(Unit unit, TileData newTile)
    {   
        Vector3 newTileCenter = Board.data.parentGrid.GetCellCenterWorld(newTile.tilePos);
        unit.FlipSprite(unit.transform.position, newTileCenter);
        yield return new WaitForSeconds(0.1f);
        while (true)
        {   
            Vector3 directionToTarget = newTileCenter - unit.transform.position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            unit.transform.position = Vector2.MoveTowards(unit.transform.position, newTileCenter, 2.5f * Time.deltaTime);
            unit.overhead.UpdatePos();

            if (dSqrToTarget <= 0.001f) {

                unit.curTile.occupied = null;
                unit.transform.position = newTileCenter;
                unit.overhead.UpdatePos();
                newTile.occupied = unit;
                unit.curTile = newTile;
                Overlook.data.ghostUnit.SetActive(false);

                for (int i = 0; i < unit.moveEvents.Count; i++) {
                    unit.moveEvents[i]();
                }
                
                List<TileData> possibleAttacks = Control.data.Pattern(unit.curTile.tilePos, unit.unitType.pattern);
                if (unit.unitType.AttackCheck(unit, possibleAttacks, out TileData hitTarget)) {
                    if (Overlook.data.cameraMovePerTurn)
                        yield return Overlook.data.StartCoroutine(Control.data.CameraZoom(unit.transform.position, Camera.main.orthographicSize, 0, 0.08f));
                    yield return new WaitForSeconds(0.5f);
                    yield return unit.StartCoroutine(unit.unitType.AttackAction(unit, hitTarget));
                    yield return new WaitForSeconds(0.5f);
                    unit.sigCharge += 2.5f;
                }
                unit.sigCharge += 2.5f;
                unit.movement -= 1;
                yield break;
            }       
            yield return null;
        }
    }

    // public virtual IEnumerator MoveAction(Unit unit, Vector3 newPos)
    // {   
    //     Board.data.FindPathFree(unit, Control.data.GetTileData(newPos).tilePos, out List<TileData> path);
    //     int curPosInPath = 0;
    //     Vector3 posPath = Board.data.parentGrid.GetCellCenterWorld(path[curPosInPath].tilePos);
    //     unit.FlipSprite(unit.transform.position, posPath);

    //     yield return new WaitForSeconds(0.1f);
    //     while (true)
    //     {   
    //         Vector3 directionToTarget = posPath - unit.transform.position;
    //         float dSqrToTarget = directionToTarget.sqrMagnitude;
    //         unit.transform.position = Vector2.MoveTowards(unit.transform.position, posPath, 2.5f * Time.deltaTime);
    //         unit.overhead.UpdatePos();

    //         if (dSqrToTarget <= 0.001f) {
    //             unit.transform.position = posPath;

    //             // unit.attr.actionPoints.value += 1;
    //             // unit.overhead.UpdateOverhead();
                
    //             if (curPosInPath == path.Count - 1) {

    //                 unit.overhead.UpdatePos();
    //                 yield break;
    //             }
    //             curPosInPath++;
    //             posPath = Board.data.parentGrid.GetCellCenterWorld(path[curPosInPath].tilePos);
    //             unit.FlipSprite(unit.transform.position, posPath);
    //         }
            
    //         yield return null;
    //     }
    // }

    public virtual IEnumerator AttackAction(Unit unit, TileData attackedTile)
    {   
        Unit attackedUnit = attackedTile.occupied;
        Vector3 attkUnitPos = Board.data.parentGrid.GetCellCenterWorld(attackedTile.tilePos);
        Vector3 returnPos = Board.data.parentGrid.GetCellCenterWorld(unit.curTile.tilePos);
        float transitionSpeed = 10f;
        unit.FlipSprite(returnPos, attkUnitPos);
        bool recovering = false;

        yield return new WaitForSeconds(0.1f);
        while (true)
        {   
            if (recovering == false) {

                Vector3 directionToTarget = attkUnitPos - unit.transform.position;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                unit.transform.position = Vector2.MoveTowards(unit.transform.position, attkUnitPos, transitionSpeed * Time.deltaTime);
                unit.overhead.UpdatePos();

                if (dSqrToTarget <= 0.15f) {

                    float damage = 0;
                    if (Random.value <= unit.stats.accuracy.value) {

                        damage = Random.Range(unit.stats.damage.value, unit.stats.damage.basic);
                    }
                    attackedUnit.DamageUnit(unit, damage, (int)RelationalData.DamageType.Physical);
                    recovering = true;              
                }

            } else {

                Vector3 directionToTarget = returnPos - unit.transform.position;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                unit.transform.position = Vector2.MoveTowards(unit.transform.position, returnPos, transitionSpeed / 2f * Time.deltaTime);
                unit.overhead.UpdatePos();

                if (dSqrToTarget <= 0.01f) {

                    unit.transform.position = returnPos;
                    unit.overhead.UpdatePos();
                    yield break;              
                }
            }
            yield return null;
        }
    }

    // public virtual int PriorityPlead(Unit unit) {

    //     int priority = 0;

    //     if (unit.curTile.movementPenalty > 0) {
    //         priority += 6;
    //     }

    //     if (unit.unitType.AttackCheck(unit, Control.data.Pattern(unit.curTile.tilePos, pattern), out TileData hitTarget)) {
    //         priority -= 4;
    //     }
    //     // for(int i = 0; i < Overlook.data.allyUnits.Count; i++) {

    //     //     int x = Mathf.Abs(unit.curTile.tilePos.x - Overlook.data.allyUnits[i].curTile.tilePos.x);
    //     //     int y = Mathf.Abs(unit.curTile.tilePos.y - Overlook.data.allyUnits[i].curTile.tilePos.y);
            
    //     //     if (x <= 1 && y <= 1) {

    //     //         priority -= 4;
    //     //     }
    //     // }
    //     return priority;
    // }

    public virtual IEnumerator AI(Unit unit) {

        if (unit.movement > 0) {

            Unit nearestEnemy = Overlook.data.GetClosestUnit(unit, Overlook.data.teams.FirstOrDefault(x => x.Key != unit.team).Key);

            Vector3Int[] cordsAround = Control.data.CoordinatesWithinRadius(nearestEnemy.curTile.tilePos, 12);
            TileData bestTile = null;

            foreach (Vector3Int cord in cordsAround) {

                if (cord == nearestEnemy.curTile.tilePos)
                    continue;

                if (Board.data.Tiles.TryGetValue(cord, out TileData cordTile) && cordTile.occupied == null && cordTile.type == TileData.TileType.Walkable) {

                    List<TileData> possibleMoves = Control.data.Pattern(cordTile.tilePos, pattern);

                    if (possibleMoves.Count > 0) {

                        foreach(TileData potentialTile in possibleMoves) {

                            if (potentialTile.occupied != null && potentialTile.occupied == nearestEnemy) {
                                
                                if (bestTile == null)
                                    bestTile = cordTile;
                                
                                float bestDist = Vector3.Distance(bestTile.tilePos, unit.transform.position);

                                if (cordTile.movementPenalty <= bestTile.movementPenalty && Vector3.Distance(cord, unit.transform.position) < bestDist) {
                                    bestTile = cordTile;
                                }
                            }
                        }
                    }
                }
            }

            if (bestTile != null) {

                Board.data.FindPath(unit, bestTile.tilePos, out List<TileData> path);

                if (path != null) {

                    List<TileData> possibleMovesMove = Control.data.Pattern(unit.curTile.tilePos, Overlook.data.movementAllowed);

                    for (int i = 0; i < path.Count; i++) {

                        if (possibleMovesMove.Contains(path[i]) && path[i].occupied == null && path[i].type == TileData.TileType.Walkable) {
                            yield return unit.StartCoroutine(unit.unitType.MoveAction(unit, path[i]));
                            yield return new WaitForSeconds(0.75f);
                            yield break;
                        }
                    }
                }
            }
        }
    }

    
    public virtual void Death(Unit unit) {

        if (unit.stats.health.value <= 0 && unit != null) {

            unit.stats.health.value = 0;
            Overlook.data.teams[unit.team].Remove(unit);
            Destroy(unit.overhead.gameObject);
          
            // PortalVfx p = CacheReference.asset.GetPooledGameObject(CacheReference.asset.portalList).GetComponent<PortalVfx>();
            // p.IniPortal(unit);
            Destroy(unit.gameObject);

            LootData loot = new LootData();
            loot.id = System.Guid.NewGuid().ToString();
            loot.title = unit.attr.handle;
            loot.tileLocation = unit.curTile.tilePos;
            int numberOfItems = Random.Range(0, 9);
            for (int i = 0; i < numberOfItems; i++) {
                ItemBase item = RelationalData.data.itemObjects[Random.Range(0, RelationalData.data.itemObjects.Count)];
                ItemData iData = new ItemData();
                iData.idType = item.id;
                loot.items.Add(iData);
            }
            Board.data.curRoom.loot.Add(loot);

            Overlook.data.EndRoom();
        }
    }
}
