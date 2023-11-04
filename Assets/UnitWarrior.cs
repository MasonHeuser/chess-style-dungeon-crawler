using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Warrior", menuName = "ScriptableObjects/Units/Warrior", order = 1)]
public class UnitWarrior : UnitBase
{
    public override IEnumerator AttackAction(Unit unit, TileData attackedTile)
    {   
        GameObject axe = unit.unitAssets.Find( x => x.gameObject.name=="vfx_Axe");
        AudioSource axeAudio = axe.GetComponent<AudioSource>();
        Transform axeVisual = axe.transform.GetChild(0);
        Transform hitPoint = axe.transform.GetChild(1);
        ParticleSystem particles = axe.transform.GetChild(2).GetComponent<ParticleSystem>();
        axe.transform.position = unit.transform.position;
        axe.transform.rotation = Quaternion.Euler(0f, 0f, -45f);
        yield return new WaitForSeconds(0.25f);

        List<TileData> pattern = new List<TileData>();
        for (int x = -1; x <= 1; x++) {

            for (int y = -1; y <= 1; y++) {

                    TileData cordsAroundTile = Board.data.Tiles[new Vector3Int(unit.curTile.tilePos.x + x, unit.curTile.tilePos.y + y, 0)];

                    if (cordsAroundTile != null)                          
                        pattern.Add(cordsAroundTile);
            }
        }
        List<TileData> damaged = new List<TileData>();

        axe.SetActive(true);
        axeVisual.gameObject.SetActive(true);
        hitPoint.gameObject.SetActive(true);
        float speed = 0f;

        while (true) {
            
            axeAudio.pitch = Random.Range(0.75f, 1.5f);
            axeVisual.Rotate(-Vector3.forward * 720f * Time.deltaTime);
            speed += 270f * Time.deltaTime;
            axe.transform.Rotate(-Vector3.forward * speed * Time.deltaTime);

            foreach (TileData tile in pattern) {

                if (new Vector3((int)hitPoint.position.x, (int)hitPoint.position.y, 0) == tile.tilePos && !damaged.Contains(tile)) {

                    if (tile.occupied != null && tile.occupied.team != unit.team) {
                        float damage = 0;
                        if (Random.value <= unit.stats.accuracy.value) {

                            damage = Random.Range(unit.stats.damage.value, unit.stats.damage.basic);
                            speed = 35f;
                            unit.stats.armor.value += 1;
                            unit.overhead.UpdateOverhead();
                            particles.Emit(Random.Range(20, 31));
                            Overlook.data.audioSource.PlayOneShot(audioAssets.Find( x => x.name=="axe_hit"));
                        }
                        tile.occupied.DamageUnit(unit, damage, (int)RelationalData.DamageType.Physical);
                        damaged.Add(tile);
                    }
                }
            }

            if (Quaternion.Angle(axe.transform.rotation, Quaternion.Euler(0, 0, -44f)) <= 1f) {
                axeVisual.gameObject.SetActive(false);
                hitPoint.gameObject.SetActive(false);
                break;
            }
            yield return null;
        }

        while (true) {

            if (particles.particleCount <= 0) {

                axe.SetActive(false);
                yield break;
            }
            yield return null;
        }
        // List<TileData> pTargets = unit.unitType.MovePattern(unit.curTile);

        // foreach (TileData tile in pTargets) {

        //     if (tile.occupied != null && tile.occupied.attr.team != unit.attr.team) {
        //         int damage = 0;
        //         if (Random.value <= unit.attr.accuracy.value) {

        //             damage = Random.Range((int)unit.attr.damage.value, (int)unit.attr.damage.basic + 1); //Add one to include the max limit.
        //         }
        //         tile.occupied.TakeDamage(damage);
        //     }
        // }
        // yield return new WaitForSeconds(0.25f);       
    }
}
