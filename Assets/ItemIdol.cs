using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

[CreateAssetMenu(fileName = "Idol", menuName = "ScriptableObjects/Item/Idol")]
public class ItemIdol : ItemBase
{   
    // public int healthDecrease;
    // public TileStatusBase fire;
    // public ParticleSystem vfx_Lightning;
    // public ParticleSystem vfx_Clouds;

    // public override void InitializeItem(Unit unit) {

    //     unit.health.flatModValue += healthDecrease;

    //     ParticleSystem l = Instantiate(vfx_Lightning);
    //     l.transform.SetParent(unit.vfxParent);
    //     l.gameObject.name = "vfx_Lightning";
    //     unit.unitAssets.Add(l.gameObject);
    //     l.gameObject.SetActive(false);

    //     ParticleSystem c = Instantiate(vfx_Clouds);
    //     c.transform.SetParent(unit.vfxParent);
    //     c.gameObject.name = "vfx_Clouds";
    //     unit.unitAssets.Add(c.gameObject);
    //     c.gameObject.SetActive(false);
    // }

    // public override IEnumerator Passive(Unit unit) {

    //     unit.health.flatModValue -= healthDecrease;
    //     unit.unitType.Stats(unit);
    //     unit.health.value = (int)(unit.health.basic / 2);

    //     Time.timeScale = 0;
    //     CacheReference.asset.overheadParent.gameObject.SetActive(false);
    //     CacheReference.asset.damageTextParent.gameObject.SetActive(false);

    //     //Zoom in
    //     yield return Control.data.GetComponent<MonoBehaviour>().StartCoroutine(Control.data.CameraZoom(unit.transform.position, 4f, 0.0035f, 0.005f));

    //     ParticleSystem c = unit.unitAssets.Find( x => x.gameObject.name=="vfx_Clouds").GetComponent<ParticleSystem>();
    //     Vector3 screenTop = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
    //     c.transform.position = new Vector3(unit.transform.position.x, screenTop.y, 0);
    //     c.gameObject.SetActive(true);

    //     ParticleSystem l = unit.unitAssets.Find( x => x.gameObject.name=="vfx_Lightning").GetComponent<ParticleSystem>();

    //     while (true) {

    //         if (c.particleCount >= 600) {
                            
    //             l.transform.position = unit.transform.position;
    //             l.gameObject.SetActive(true);
    //             l.Emit(1);
    //             l.TriggerSubEmitter(0);

    //             unit.overhead.CountSlots();
    //             unit.overhead.UpdateOverhead();
    //             for (int x = -1; x <= 1; x++) {

    //                 for (int y = -1; y <= 1; y++) {

    //                     Vector3Int tilePosInt = new Vector3Int(unit.curTile.tilePos.x + x, unit.curTile.tilePos.y + y, 0);

    //                     if (tilePosInt == unit.curTile.tilePos) {
    //                         continue;
    //                     }

    //                     List<TileData> list = new List<TileData>() { Board.data.Tiles.Find( v => v.tilePos==tilePosInt) };
    //                     Board.data.InstanceTileStatusBoard(unit, fire, list);
    //                 }
    //             }
    //             unit.equippedItem = null;
    //             c.Stop();
    //             Time.timeScale = 1;
    //             yield return new WaitForSeconds(1.5f); 
    //             break;
    //         }
    //         yield return null;
    //     }

    //     //Zoom out
    //     yield return Control.data.GetComponent<MonoBehaviour>().StartCoroutine(Control.data.CameraZoom(Overlook.data.camRestingPoint, Overlook.data.orthoDefaultSize, 0.005f, 0.01f));

    //     if (unit.team != 0) {

    //         Save save = null;

    //         //Open old Save if it exists and add it to oldSave variable.
    //         if (File.Exists(Application.persistentDataPath + RelationalData.data.filePath))
    //         {
    //             BinaryFormatter oldBf = new BinaryFormatter();
    //             FileStream oldFile = File.Open(Application.persistentDataPath + RelationalData.data.filePath, FileMode.Open);
    //             save = (Save)oldBf.Deserialize(oldFile);
    //             oldFile.Close();
    //         }

    //         //TODO: Make this item get destroyed from equipped units inventory
    //         // save.items.Remove(save.items[save.items.FindIndex(z=>z.id==unit.equippedItemId)]);

    //         //Finalized and close save file.
    //         BinaryFormatter bf = new BinaryFormatter();
    //         FileStream file = File.Create(Application.persistentDataPath + RelationalData.data.filePath);
    //         bf.Serialize(file, save);
    //         file.Close();
    //     }
    //     CacheReference.asset.overheadParent.gameObject.SetActive(true);
    //     CacheReference.asset.damageTextParent.gameObject.SetActive(true);

    //     while (true) {

    //         if (c.particleCount <= 0) {
                
    //             Destroy(c.gameObject);
    //             unit.unitAssets.Remove(c.gameObject);

    //             Destroy(l.gameObject);
    //             unit.unitAssets.Remove(l.gameObject);
    //             yield break;
    //         }
    //         yield return null;
    //     }
    // }

    // public override void Active(Unit unit) {
    //     return;
    // }

    // public override string ShortDesc() {
    //     return "On death, gain half of your HP and destroy the idol.";
    // }

    // public override string LongDesc() {
    //     return "Carved and praised by religious Zues fanatics, it is said when holding this item, receiving a fatal blow will not spell defeat allowing the user to comeback with half of their strength.";
    // }

    // public override string StatDesc() {
    //     return "Health -" + healthDecrease;
    // }

    // public override string ActiveDesc() {
    //     return "";
    // }

    // public override string PassiveDesc() {
    //     return "Passive: On death, Zeus strikes down on equipped unit, reviving them with 50% of their max HP.";
    // }
}