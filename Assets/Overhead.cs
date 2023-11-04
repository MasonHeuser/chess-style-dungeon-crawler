using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Overhead : MonoBehaviour
{   
    public Unit unit;

    public Transform statusContainer;
    
    List<Image> activeArmor = new List<Image>();
    public Transform armorContainer;
    public Sprite fullArmor;
    public Sprite emptyArmor;
    
    public Slider healthBar;
    public RectTransform catchupHealth;

    void OnEnable() {
        if (unit != null)
            UpdatePos();
    }

    void Start() {
        CountSlots();
        UpdateOverhead();
        UpdatePos();
        catchupHealth.sizeDelta = new Vector2(healthBar.fillRect.rect.size.x, catchupHealth.sizeDelta.y);
    }

    void Update() {
        
        if (Input.GetMouseButton(2)) {
            UpdatePos();
        }
    }

    public void CountSlots() {

        if (activeArmor.Count > 0) {

            for (int i = 0; i < activeArmor.Count; i++) {

                activeArmor[i].gameObject.SetActive(false);     
            }
            activeArmor.Clear();
        }

        if (unit.stats.armor.basic > 0) {

            int numberOfArmor = (int)unit.stats.armor.basic;
            if (armorContainer.childCount < numberOfArmor) {

                int count = numberOfArmor - armorContainer.childCount;
                for (int i = 0; i < count; i++) {
                    Image img = Instantiate(armorContainer.GetChild(0).gameObject.GetComponent<Image>());
                    img.gameObject.name = "ArmorPoint";
                    img.transform.SetParent(armorContainer);
                }
            }

            for (int i = 0; i < numberOfArmor; i++) {
                
                GameObject c = armorContainer.GetChild(i).gameObject;
                c.SetActive(true);
                activeArmor.Add(c.GetComponent<Image>());   
            }
        }

        healthBar.maxValue = unit.stats.health.basic;
        healthBar.value = unit.stats.health.value;
    }

    public void UpdateOverhead() {

        for (int i = 0; i < statusContainer.childCount; i++) {

            statusContainer.GetChild(i).gameObject.SetActive(false);     
        }

        if (unit.activeStatusEffects.Count > 0) {
            
            for (int i = 0; i < unit.activeStatusEffects.Count; i++) {

                //TODO: Better method to make sure the status effects list doesn't exceed the childCount | better icon pos' in ui | ... after max
                if (i <= statusContainer.childCount - 1) {

                    Transform child = statusContainer.GetChild(i);
                    child.GetComponent<Image>().sprite = unit.activeStatusEffects[i].statusEffectBase.sprite;
                    child.gameObject.SetActive(true);
                }
            }

            if (unit.activeStatusEffects.Count > 3) {

                foreach (Transform child in statusContainer) {
                    child.GetComponent<RectTransform>().sizeDelta = new Vector2 (8, 8);
                }

            } else {

                foreach (Transform child in statusContainer) {
                    child.GetComponent<RectTransform>().sizeDelta = new Vector2 (16, 16);
                }
            }
        }

        int armorTest = (int)unit.stats.armor.basic - (int)unit.stats.armor.value;
        for (int i = activeArmor.Count; i --> 0;) {

            if (armorTest >= 1) {
                activeArmor[i].sprite = emptyArmor;
            } else {
                activeArmor[i].sprite = fullArmor;
            }
            armorTest -= 1;
        }

        healthBar.value = unit.stats.health.value;
    }

    public void UpdatePos() {

        if (unit != null) {
            // Offset position above object box (in world space)
            float offsetPosY = unit.transform.position.y + unit.unitType.overheadOffset;       
            // Final position of marker above GO in world space
            Vector3 offsetPos = new Vector3(unit.transform.position.x, offsetPosY, unit.transform.position.z);
                
            transform.position = Camera.main.WorldToScreenPoint(offsetPos);
        }
    }

    public IEnumerator ShakePos(float damageIntensity) { 

        Vector3 origPos = Vector3.zero;
        float multi = 4f;
        float counter = 0f;
        float ShakeTime = 1f;
        while(true) {
            counter += Time.deltaTime;
            damageIntensity = Mathf.Lerp(damageIntensity, 0f, 0.015f);
            if(counter >= ShakeTime) {
                transform.GetChild(0).localPosition = origPos;
                break;
            } else {
                transform.GetChild(0).localPosition = origPos + new Vector3((ShakeTime - counter) * Random.Range(-(damageIntensity * multi), (damageIntensity * multi)), (ShakeTime - counter) * Random.Range(-(damageIntensity * multi), (damageIntensity * multi)), 0);
            }
            yield return null;
        }

        float fraction = 0f;
        RectTransform healthBarFill = healthBar.fillRect;

        while(true) {
            
            fraction += Time.unscaledDeltaTime * 0.1f;
            catchupHealth.sizeDelta = new Vector2(Mathf.Lerp(catchupHealth.sizeDelta.x, healthBarFill.rect.size.x, fraction), catchupHealth.sizeDelta.y);

            if (Mathf.Abs(catchupHealth.sizeDelta.x - healthBarFill.rect.size.x) < 0.01f) {
                catchupHealth.sizeDelta = new Vector2(healthBarFill.rect.size.x, catchupHealth.sizeDelta.y);
                yield break;
            }
            yield return null;
        }
    }
}
