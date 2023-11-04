using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CacheReference : MonoBehaviour
{   
    // singleton static reference
    private static CacheReference assignAsset;
    public static CacheReference asset {
        get {
            if (assignAsset == null) {
                assignAsset = FindObjectOfType<CacheReference>();
                if (assignAsset == null)
                    Debug.LogError("There is no CacheReference in the scene!");
            }
            return assignAsset;
        }
    }

        
    public GameObject unitPrefab;

    public Transform scalingCanvas;

    public Transform overheadParent;
    public GameObject overheadPrefab;

    [Header("Damage Text")]
    public Transform damageTextParent;
    public GameObject damageText;
    public List<GameObject> damageTextList = new List<GameObject>();

    [Header("Portals")]
    public Transform portalParent;
    public GameObject portal;
    public List<GameObject> portalList = new List<GameObject>();

    [Header("Loot")]
    public Transform lootParent;
    public GameObject loot;
    public List<GameObject> lootList = new List<GameObject>();

    public Color friendlyColor;
    public Color enemyColor; 

    void Awake() {

        for (int i = 0; i < 5; i++) {

            GameObject t = Instantiate(damageText);
            t.name = "DamageText";
            t.transform.SetParent(damageTextParent, false);
            t.gameObject.SetActive(false);
            damageTextList.Add(t);
        }

        for (int i = 0; i < 3; i++) {

            GameObject t = Instantiate(portal);
            t.name = portal.name;
            t.transform.SetParent(portalParent, false);
            t.gameObject.SetActive(false);
            portalList.Add(t);
        }

        for (int i = 0; i < 6; i++) {

            GameObject t = Instantiate(loot);
            t.name = loot.name;
            t.transform.SetParent(lootParent, false);
            t.gameObject.SetActive(false);
            lootList.Add(t);
        }
    }

    public void DamageText(Vector3 pos, string text, Color color) {

        TextMeshPro c = CacheReference.asset.GetPooledGameObject(CacheReference.asset.damageTextList).GetComponent<TextMeshPro>();
        c.transform.SetAsLastSibling();
        c.transform.position = pos;
        c.gameObject.SetActive(true);
        c.text = text;
        c.color = color;
    }

    public GameObject GetPooledGameObject(List<GameObject> list) {

        int activeCount = 0;
        GameObject objectToPool = null;
        Transform itemParent = null;
        string name = null;
        for (int i = 0; i < list.Count; i++) {
            if (!list[i].activeInHierarchy) {

                return list[i];
            } else {
                objectToPool = list[i];
                name = list[i].name;
                itemParent = list[i].transform.parent;
                activeCount++;
            }
        }

        if (activeCount > 0) {
            GameObject obj = (GameObject)Instantiate(objectToPool);
            obj.name = name;
            obj.transform.SetParent(itemParent, false);
            obj.SetActive(false);
            list.Add(obj);
            return obj;
        } else {
            return null;
        }
    }

    // public UnitBase GetRandomUnit(List<UnitBase> unitList) {

    //     float totalRarity = 0f;

    //     //Sum up item rarities to equal totalRarity
    //     for (int i = 0; i < unitList.Count; i++) {
    //         totalRarity += unitList[i].rarity;
    //     }

    //     float randomRarity = Random.Range(0.0f, totalRarity);
    //     float currentRarity = 0.0f;

    //     foreach (UnitBase unit in unitList) {

    //         currentRarity += unit.rarity;
    //         if (randomRarity <= currentRarity) {

    //             return unit; // selected one
    //         } 
    //         else {

    //             randomRarity -= currentRarity;
    //         }
    //     }
    //     return unitList[Random.Range(0, unitList.Count)];
    // }
}
