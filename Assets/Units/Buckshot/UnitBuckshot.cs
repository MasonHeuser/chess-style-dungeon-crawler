using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Buckshot", menuName = "ScriptableObjects/Units/Buckshot", order = 1)]
public class UnitBuckshot : UnitBase
{   

    public override IEnumerator AttackAction(Unit unit, TileData attackedTile)
    {   
        GameObject shootEffect = unit.unitAssets.Find( x => x.gameObject.name=="BuckshotShoot");
        ParticleSystem shootParticles = shootEffect.GetComponent<ParticleSystem>();
        List<Vector3Int> damageTiles = new List<Vector3Int>();
        Vector3Int midTile = Vector3Int.zero;

        unit.FlipSprite(unit.curTile.tilePos, attackedTile.tilePos);
        yield return new WaitForSeconds(0.75f);

        //Find Middle Point
        int totalCount = 0;
        for (int x = -1; x <= 1; x++) {

            for (int y = -1; y <= 1; y++) {

                if (Overlook.data.TrueIfOdd(totalCount) || x == 0 && y == 0) { //This allows the path to be only cross style.

                    Vector3Int check = new Vector3Int(attackedTile.tilePos.x + x, attackedTile.tilePos.y + y, 0);

                    if (check.x == unit.curTile.tilePos.x && Mathf.Abs(check.y - unit.curTile.tilePos.y) == 2 || check.y == unit.curTile.tilePos.y && Mathf.Abs(check.x - unit.curTile.tilePos.x) == 2) {
                        midTile = check;
                        damageTiles.Add(midTile);
                        break;
                    }                  
                }
                totalCount++;
            }
        }

        Vector3 direction = (Board.data.parentGrid.GetCellCenterWorld(midTile) - shootEffect.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        shootEffect.transform.rotation = lookRotation;
        shootEffect.SetActive(true);

        int caseSwitch = 0;
        int xDiff = 0;
        int yDiff = 0;

        if (unit.curTile.tilePos.y == midTile.y) {
            yDiff++;
        } else {
            xDiff++;
        }

        while (caseSwitch < 2) {

            switch (caseSwitch)
            {
                case 0:
                    damageTiles.Add(new Vector3Int(midTile.x + xDiff, midTile.y + yDiff, 0));
                    caseSwitch++;
                    break;
                case 1:
                    damageTiles.Add(new Vector3Int(midTile.x - xDiff, midTile.y - yDiff, 0));
                    caseSwitch++;
                    break;
            }
            yield return null;
        }

        while (true) {

            if (shootParticles.particleCount <= 0) {
                break;
            }
            yield return null;
        }                          
        
        for (int i = 0; i < damageTiles.Count; i++) {

            TileData nullCheck = Board.data.Tiles[damageTiles[i]];

            if (nullCheck != null) {

                if (nullCheck.occupied != null && unit.team != nullCheck.occupied.team) {
                    
                    float damage = 0;
                    if (Random.value <= unit.stats.accuracy.value) {

                        damage = Random.Range(unit.stats.damage.value, unit.stats.damage.basic);
                    }
                    nullCheck.occupied.DamageUnit(unit, damage, (int)RelationalData.DamageType.Physical);
                } else {
                    CacheReference.asset.DamageText(Board.data.parentGrid.GetCellCenterWorld(nullCheck.tilePos), "Miss!", Color.white);
                }
            }
        }
        yield return new WaitForSeconds(0.5f);
        yield break;
    }
}
