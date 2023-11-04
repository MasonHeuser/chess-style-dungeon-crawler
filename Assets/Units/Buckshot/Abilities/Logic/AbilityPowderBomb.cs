using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityPowderBomb", menuName = "ScriptableObjects/Abilities/Buckshot/PowderBomb", order = 1)]
public class AbilityPowderBomb : AbilityBase
{
    public TileStatusBase tilePowderStatus;

    public override IEnumerator AbilityAction(Unit unit, TileData actionedTile)
    {   
        unit.FlipSprite(unit.curTile.tilePos, actionedTile.tilePos);
        GameObject bomb = unit.unitAssets.Find( x => x.gameObject.name=="PowderBomb");
        bomb.transform.GetChild(0).gameObject.SetActive(true);

        ParticleSystem trail = bomb.transform.GetChild(1).GetComponent<ParticleSystem>();

        bomb.transform.position = unit.transform.position;
        Vector3 startPos = bomb.transform.position;
        Vector3 targetPos = Board.data.parentGrid.GetCellCenterWorld(actionedTile.tilePos);
        bomb.SetActive(true);

        float height = 5.0f;
        Vector3[] point = new Vector3[3];
        point[0] = startPos;
        point[2] = targetPos;
        point[1] = point[0] +(point[2] -point[0])/2 +Vector3.up * height;

        float count = 0f;
        while (true)
        {   
            count += 1.0f * Time.deltaTime;

            Vector3 m1 = Vector3.Lerp( point[0], point[1], count );
            Vector3 m2 = Vector3.Lerp( point[1], point[2], count );
            bomb.transform.position = Vector3.Lerp(m1, m2, count);
            bomb.transform.Rotate (Vector3.forward * (360f * Time.deltaTime));
            
            // Do something when we reach the target
            if (bomb.transform.position == targetPos) {

                Board.data.InstanceTileStatusBoard(unit, tilePowderStatus, Control.data.Pattern(actionedTile.tilePos, targetedPattern));
                bomb.transform.GetChild(0).gameObject.SetActive(false);
                break;
            }
            yield return null;
        }

        while (true) {

            if (trail != null && trail.particleCount <= 0) {
                
                bomb.SetActive(false);
                yield break;
            }
            yield return null;
        }            
    }

    public override string Desc(Unit unit) {
        return "Toss a powder bomb at target location applying " + tilePowderStatus.title + " status to the surrounding tiles potentially Blinding. If the bomb interacts with fire it instead explodes dealing damage.";
    }
}
