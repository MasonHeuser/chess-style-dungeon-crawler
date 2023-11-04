using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ContextualUISimple : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{   
    public Sprite img;
    public string title;
    public Color titleColor;
    [TextArea(3,10)]
    public string desc;

    public void OnPointerEnter(PointerEventData eventData)
    {   
        Tooltip t = Control.data.tooltip;
        t.img.sprite = img;
        t.title.text = title;
        t.title.color = titleColor;
        t.desc.text = desc;

        t.TooltipIni();
        // eventData.pointerCurrentRaycast.gameObject.name
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Control.data.tooltip.TooltipQuit();
    }
}
