using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ContextualUIStatus : MonoBehaviour,  IPointerEnterHandler, IPointerExitHandler
{
    public StatusEffectData status;
    public Unit effected;

    public void OnPointerEnter(PointerEventData eventData)
    {   
        if (status != null) {
            
            Tooltip t = Control.data.tooltip;
            t.img.sprite = status.statusEffectBase.sprite;
            t.title.text = status.statusEffectBase.title;
            t.title.color = Color.white;
            t.desc.text = status.statusEffectBase.Desc(effected, status.srcSnapShot);

            t.misc_1.text = "Source: " + status.srcSnapShot.parentUnit.attr.handle;
            t.misc_2.text = status.duration.ToString() + " Remaining Turn(s)";

            t.TooltipIni(2);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Control.data.tooltip.TooltipQuit();
    }
}
