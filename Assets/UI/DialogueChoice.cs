using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class DialogueChoice : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI textComp;
    Color defaultColor;
    public Color highlightColor;

    public delegate void ClickAction();
    public ClickAction ActionMethod = null;

    void Start() {
        defaultColor = textComp.color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ActionMethod != null) {
            ActionMethod();
        }
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        textComp.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        textComp.color = defaultColor;
    }
}
