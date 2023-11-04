using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class UnitUI : MonoBehaviour, IPointerDownHandler
{   
    public string id;
    public GameObject details;
    public TextMeshProUGUI title;
    public Image icon;
    public GameObject highlight;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && CampMenu.data.GetCampMenu("CampUnitsPanel").activeSelf)
        {
            if (CampMenu.data.selUnit == null && id != "") {

                CampMenu.data.selUnit = this;
            } else {
                GameObject ready = CampMenu.data.GetCampMenu("ReadyUnitsPanel");
                GameObject camp = CampMenu.data.GetCampMenu("CampUnitsPanel");

                if (CampMenu.data.selUnit != null && this.gameObject.transform.parent.gameObject.name == ready.name && CampMenu.data.selUnit.transform.parent.gameObject.name == camp.name) {
                    
                    UnitAttributes attr = CampMenu.data.tempSave.campUnits.Find( x => x.id==CampMenu.data.selUnit.id);
                    CampMenu.data.tempSave.campUnits.Remove(attr);
                    CampMenu.data.tempSave.readyUnits.Add(attr);                     
                    id = CampMenu.data.selUnit.id;

                    CampMenu.data.selUnit.id = "";
                    CampMenu.data.selUnit.gameObject.SetActive(false);
                    details.SetActive(true);
                    UpdateUnit(this);
                }
                else if (CampMenu.data.selUnit != null && this.gameObject.transform.parent.gameObject.name == camp.name && CampMenu.data.selUnit.transform.parent.gameObject.name == ready.name) {

                    for (int i = 1; i < camp.transform.childCount; i++) {
                        
                        UnitUI child = camp.transform.GetChild(i).GetComponent<UnitUI>();
                        if (!child.gameObject.activeSelf) {

                            UnitAttributes attr = CampMenu.data.tempSave.readyUnits.Find( x => x.id==CampMenu.data.selUnit.id);
                            CampMenu.data.tempSave.readyUnits.Remove(attr);
                            CampMenu.data.tempSave.campUnits.Add(attr);

                            child.id = CampMenu.data.selUnit.id;

                            CampMenu.data.selUnit.id = "";
                            CampMenu.data.selUnit.details.SetActive(false);

                            child.gameObject.SetActive(true);
                            child.details.SetActive(true);
                            UpdateUnit(child);
                            break;
                        }
                    }
                }
                CampMenu.data.selUnit = null;
            }
        }
    }

    public void UpdateUnit(UnitUI u) {

        UnitAttributes attr = null;
        if (this.gameObject.transform.parent.gameObject.name == CampMenu.data.GetCampMenu("ReadyUnitsPanel").name) {
            attr = CampMenu.data.tempSave.readyUnits.Find( x => x.id==u.id);
        } else if (this.gameObject.transform.parent.gameObject.name == CampMenu.data.GetCampMenu("CampUnitsPanel").name) {
            attr = CampMenu.data.tempSave.campUnits.Find( x => x.id==u.id);
        }

        UnitBase uBase = RelationalData.data.unitObjects.Find( x => x.id==attr.idType);
        u.title.text = attr.handle;
        u.icon.sprite = uBase.sprite;
    }
}
