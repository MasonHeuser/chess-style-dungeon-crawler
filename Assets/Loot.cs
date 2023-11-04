using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public SpriteRenderer sprite;
    bool outline = false;

    public LootData lootData;

    void Start() {
        sprite.material.SetInt("outlineUnit", 0);
    }

    void OnDisable() {
        sprite.material.SetInt("outlineUnit", 0);
    }
    
    void OnMouseOver() {
        
        if (Control.data.contextActions && Control.data.actionAble) {

            if (Input.GetMouseButtonDown(0)) {

                Overlook.data.UpdateItemSaveCache();
                OpenLoot();
            }

            if (!outline) {
                sprite.material.SetInt("outlineUnit", 1);
                outline = true;
            }
        }
    }

    void OnMouseExit() {

        if (Control.data.contextActions && Control.data.actionAble) {
            sprite.material.SetInt("outlineUnit", 0);
            outline = false;
        }
    }

    void OpenLoot() {

        Overlook.data.contextActions.transform.Find("Inventories").gameObject.SetActive(true);
        GridInventory inv = Overlook.data.areaInventory;
        inv.gameObject.SetActive(true);
        inv.gameObject.name = lootData.id;
        inv.title.GetComponent<TextMeshProUGUI>().text = lootData.title;
        foreach(ItemData item in lootData.items) {
            inv.AddItemSpecify(item, new Vector2Int(item.xPos, item.yPos));
        }
    }
}
