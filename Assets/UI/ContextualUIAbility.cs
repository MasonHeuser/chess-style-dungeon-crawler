using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ContextualUIAbility : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Unit unit;
    public AbilityBase ability;

    public void OnPointerEnter(PointerEventData eventData)
    {   
        if (ability != null) {
            Tooltip t = Control.data.tooltip;
            t.img.sprite = ability.icon;
            t.title.text = ability.title;
            t.title.color = Color.white;
            t.desc.text = ability.Desc(unit);

            t.misc_1.text = ability.cooldown.ToString() + " Cooldown";
            t.misc_2.text = ability.cost.ToString() + " Cost";

            t.TooltipIni(2);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Control.data.tooltip.TooltipQuit();
    }
}
