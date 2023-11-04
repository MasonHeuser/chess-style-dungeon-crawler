using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityHeatedShell", menuName = "ScriptableObjects/Abilities/Buckshot/HeatedShell", order = 1)]
public class AbilityHeatedShell : AbilityBase
{   
    public float percentPhysScaling;
    public float percentFireScaling;
    public TileStatusBase fireTile;

    public override IEnumerator AbilityAction(Unit unit, TileData actionedTile)
    {        
        yield return new WaitForSeconds(0.15f);
        unit.FlipSprite(unit.curTile.tilePos, actionedTile.tilePos);
        yield return new WaitForSeconds(0.5f);
        if (actionedTile.occupied != null && unit.team != actionedTile.occupied.team) {

            float physDamage = Random.Range(unit.stats.damage.value * percentPhysScaling, unit.stats.damage.basic * percentPhysScaling);
            float fireDamage = Random.Range(unit.stats.damage.value * percentFireScaling, unit.stats.damage.basic * percentFireScaling);
            actionedTile.occupied.DamageUnit(unit, physDamage, (int)RelationalData.DamageType.Physical);
            actionedTile.occupied.DamageUnit(unit, fireDamage, (int)RelationalData.DamageType.Fire);
        }
        List<TileData> list = new List<TileData>() { actionedTile };
        Board.data.InstanceTileStatusBoard(unit, fireTile, list);
        
        //Start Sliding
        Vector3Int vectorDiff = unit.curTile.tilePos - actionedTile.tilePos;
        Vector3Int blowbackTilePos = unit.curTile.tilePos + vectorDiff;
        TileData blowbackTile = Board.data.Tiles[blowbackTilePos];
        
        if (blowbackTile != null && blowbackTile.type == TileData.TileType.Walkable) {

            Unit knockedUnit = null;
            Vector3Int vectorDiff2 = Vector3Int.zero;
            Vector3Int blowbackTilePos2 = Vector3Int.zero;
            TileData blowbackTile2 = null;

            if (blowbackTile.occupied != null) {

                knockedUnit = blowbackTile.occupied;

                vectorDiff2 = knockedUnit.curTile.tilePos - unit.curTile.tilePos;
                blowbackTilePos2 = knockedUnit.curTile.tilePos + vectorDiff2;

                blowbackTile2 = Board.data.Tiles[blowbackTilePos2];

                if (blowbackTile2 == null || blowbackTile2.type != TileData.TileType.Walkable || blowbackTile2.occupied != null) {

                    yield return new WaitForSeconds(0.5f);
                    yield break;
                }
            }

            Vector3 dest = Board.data.parentGrid.GetCellCenterWorld(blowbackTilePos);
            float speed = 6.5f;

            while (true)
            {   
                speed = Mathf.Lerp(speed, 2f, 0.8f * Time.deltaTime);

                Vector3 directionToTarget = dest - unit.transform.position;
                float dSqrToTarget = directionToTarget.sqrMagnitude;

                unit.transform.position = Vector2.MoveTowards(unit.transform.position, dest, speed * Time.deltaTime);
                unit.overhead.UpdatePos();

                if (dSqrToTarget <= 0.1f) {

                    unit.transform.position = dest;
                    unit.overhead.UpdatePos();

                    if (blowbackTile.occupied != null) {

                        if (blowbackTile2 != null && blowbackTile2.type == TileData.TileType.Walkable && blowbackTile2.occupied == null) {

                            Vector3 dest2 = Board.data.parentGrid.GetCellCenterWorld(blowbackTilePos2);
                            float speed2 = 6f;
                            while (true) {

                                speed2 = Mathf.Lerp(speed2, 2f, 0.8f * Time.deltaTime);

                                Vector3 directionToTarget2 = dest2 - knockedUnit.transform.position;
                                float dSqrToTarget2 = directionToTarget2.sqrMagnitude;

                                knockedUnit.transform.position = Vector2.MoveTowards(knockedUnit.transform.position, dest2, speed2 * Time.deltaTime);
                                knockedUnit.overhead.UpdatePos();

                                if (dSqrToTarget2 <= 0.01f) {
                                    
                                    knockedUnit.curTile.occupied = null;
                                    blowbackTile2.occupied = knockedUnit;
                                    knockedUnit.curTile = blowbackTile2;

                                    knockedUnit.transform.position = dest2;
                                    knockedUnit.overhead.UpdatePos();
                                    knockedUnit.DamageUnit(unit, 1, (int)RelationalData.DamageType.Physical);
                                    break;
                                }
                                yield return null;
                            }
                        }
                    }

                    unit.curTile.occupied = null;
                    blowbackTile.occupied = unit;
                    unit.curTile = blowbackTile;
                    yield return new WaitForSeconds(0.5f);
                    yield break;              
                }
                yield return null;
            }
        } else {

            yield break; 
        }
    }

    public override string Desc(Unit unit) {

        int phys = (int)RelationalData.DamageType.Physical;
        float percentPhys = percentPhysScaling * 100;
        float minDmgPhys = unit.stats.damage.value * percentPhysScaling;
        float maxDmgPhys = unit.stats.damage.basic * percentPhysScaling;

        int fire = (int)RelationalData.DamageType.Fire;
        float percentFire = percentFireScaling * 100;
        float minDmgFire = unit.stats.damage.value * percentFireScaling;
        float maxDmgFire = unit.stats.damage.basic * percentFireScaling;
        return "Buckshot fires an unstable heated shell that explodes dealing <color=" + RelationalData.data.damageColor[phys] + ">" + minDmgPhys + "-" + maxDmgPhys + " ( <sprite=\"ElementAtlas\" index= " + phys + ">" + percentPhys + "%) physical damage</color> & <color=" + RelationalData.data.damageColor[fire] + ">" + minDmgFire + "-" + maxDmgFire + "</color> <color=" + RelationalData.data.damageColor[phys] + ">( <sprite=\"ElementAtlas\" index= " + phys + ">" + percentFire + "%)</color> <color=" + RelationalData.data.damageColor[fire] + ">fire damage</color> while setting the tile on fire, knocking himself back the opposite direction from the impact.";
    }
}
