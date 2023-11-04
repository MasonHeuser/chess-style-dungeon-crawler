using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour
{   
    public Image img;
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;
    public LayoutElement layoutElement;
    int characterWrapLimit = 60;

    public RectTransform rectTransform;
    
    public TextMeshProUGUI misc_1;
    public TextMeshProUGUI misc_2;
    public TextMeshProUGUI misc_3;

    public void TooltipIni(int misc = 0) {
        TooltipContext(misc);
    }

    void TooltipContext(int misc = 0) {
        
        int headerLength = title.text.Length;
        int contentLength = desc.text.Length;

        layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit) ? true : false;

        if (img.sprite == null) {
            img.gameObject.SetActive(false);
        } else {
            img.gameObject.SetActive(true);
        }

        this.gameObject.SetActive(true);
        Transform t = this.gameObject.transform.Find("Suffix");

        for (int i = 0; i < t.childCount; i++) {
            if (i < misc) {
                t.GetChild(i).gameObject.SetActive(true);
            } else {
                t.GetChild(i).gameObject.SetActive(false);
            }
        }

        Vector2 mousePos = Input.mousePosition;

        float pivotX = mousePos.x / Screen.width;
        float pivotY = mousePos.y / Screen.height;

        var nearestX = Mathf.RoundToInt(pivotX);
        var nearestY = Mathf.RoundToInt(pivotY);

        rectTransform.pivot = new Vector2(nearestX, nearestY);

        transform.position = mousePos;
    }

    public void TooltipQuit() {
        this.gameObject.SetActive(false);
    }

    void Update() {

        if (this.gameObject.activeSelf) {

            transform.position = Vector3.Lerp(transform.position, new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0), 3f * Time.deltaTime);
        }
    }
}
