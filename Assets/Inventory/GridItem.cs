using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class GridItem : MonoBehaviour, IPointerDownHandler
{   
    public string id;
    public ItemBase itemType;
    public GridInventory home;
    public Vector2Int pos;
    public bool flipped = false;
    private int s_count = 1;
    public int count
    {
        get { return s_count; }
        set {
            s_count = value;
            counter.text = s_count.ToString();
            if (count > 1) {
                counter.gameObject.SetActive(true);
            } else {
                counter.gameObject.SetActive(false);
            }
        }
    }

    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI counter;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryControl.data.iniGhost(this);
        }
    }
}
