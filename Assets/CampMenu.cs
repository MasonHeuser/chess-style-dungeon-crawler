using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using TMPro;

public class CampMenu : MonoBehaviour
{   
    // singleton static reference
    private static CampMenu assignAsset;
    public static CampMenu data {
        get {
            if (assignAsset == null) {
                assignAsset = FindObjectOfType<CampMenu>();
                if (assignAsset == null)
                    Debug.LogError("There is no CampMenu in the scene!");
            }
            return assignAsset;
        }
    }

    public Save tempSave = null;
    
    public List<GameObject> mainMenuGroup = new List<GameObject>();
    public List<GameObject> campMenuGroup = new List<GameObject>();

    private UnitUI s_selUnit = null;
    public UnitUI selUnit
    {
        get { return s_selUnit; }
        set {
            if (s_selUnit != null) {
                selUnit.highlight.SetActive(false);
            }
            s_selUnit = value;
            if (s_selUnit != null) {
                s_selUnit.highlight.SetActive(true);
            }
        }
    }

    public Grid grid;
    public Tilemap groundTilemap;
    public Tile groundTile;

    public GameObject unitPrefab;
    float cameraSetPoint = 5.0f;

    void Awake() {

        if (File.Exists(Application.persistentDataPath + RelationalData.data.filePath)) {

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + RelationalData.data.filePath, FileMode.Open);
            tempSave = (Save)bf.Deserialize(file);
            file.Close();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (RelationalData.data.gameState) {
            foreach(GameObject g in mainMenuGroup) {
                g.SetActive(false);
            }
            InitializeCamp();
        } else {
            foreach(GameObject g in mainMenuGroup) {
                g.SetActive(true);
            }
        }

        if (File.Exists(Application.persistentDataPath + RelationalData.data.filePath))
        {

            List<Vector3Int> unitPositions = new List<Vector3Int>();
            Vector3Int middletile = new Vector3Int(0, 0, 0);

            for (int x = -1; x <= 1; x++) {

                for (int y = -1; y <= 1; y++) {

                    unitPositions.Add(new Vector3Int(middletile.x + x, middletile.y + y, 0));
                }
            }

            List<Vector3Int> usedPositions = new List<Vector3Int>();

            for (int i = 0; i < tempSave.readyUnits.Count; i++) {

                Vector3Int tileTest = unitPositions[Random.Range(0, unitPositions.Count)];

                Vector3Int pos = usedPositions.Find( z => z==tileTest);

                if (tileTest != pos) {                 
                                                   
                    GameObject g = Instantiate(unitPrefab);
                    UnitBase unitType = RelationalData.data.unitObjects.Find( x => x.id==tempSave.readyUnits[i].idType);
                    g.name = "Unit_" + unitType.title;

                    usedPositions.Add(tileTest);

                    Vector3 unitPos = grid.GetCellCenterWorld(tileTest);

                    g.transform.position = unitPos;

                    Unit unit = g.GetComponent<Unit>();
                    if (grid.GetCellCenterWorld(middletile).x < unitPos.x) {
                        unit.sprite.flipX = true;
                    } else {
                        unit.sprite.flipX = false;
                    }
                    unit.sprite.sprite = unitType.sprite;
                    unit.sprite.material.SetFloat("outlineUnit", 0f);
                    Destroy(g.GetComponent<Unit>());
                } else {
                    i--;
                }
            }
        } else {
            mainMenuGroup.Find( x => x.gameObject.name=="Continue").SetActive(false);
        }
    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.Z)) {
            GetCampMenu("Stash").GetComponent<GridInventory>().AddItem(RelationalData.data.itemObjects[Random.Range(0, RelationalData.data.itemObjects.Count)]);
        }

        if (Input.GetKeyDown(KeyCode.S)) {
            SaveSync();
        }
    }
    
    public void NewGameStart() {
       
        if (!Directory.Exists(Application.persistentDataPath + "/saves")) {
            Directory.CreateDirectory(Application.persistentDataPath + "/saves");
        }

        if (File.Exists(Application.persistentDataPath + RelationalData.data.filePath)) {

            File.Delete(Application.persistentDataPath + RelationalData.data.filePath);
        }

        Save save = new Save();
        save.xInv = 4;
        save.yInv = 4;

        UnitBase uBase = null;

        for (int i = 0; i < 4; i++) {

            switch (i) {
                case 0 :
                uBase = Resources.Load("Unit/Cleric") as UnitBase;
                break;

                default :
                uBase = Resources.Load("Unit/Buckshot") as UnitBase;
                break;
            }
            UnitAttributes unit = new UnitAttributes();
            unit.id = System.Guid.NewGuid().ToString();
            unit.idType = uBase.id;
            unit.handle = RelationalData.data.randName.RandomHandle(uBase.gender);
            unit.health = uBase.health;
            save.readyUnits.Add(unit);
        }

        //Finalized and close save file.
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + RelationalData.data.filePath);
        bf.Serialize(file, save);
        file.Close();
        
        SceneManager.LoadScene("Board");
    }

    public void ContinueGameStart() {

        SceneManager.LoadScene("Board");
    }

    public void TransitionToCamp()
    {   
        StartCoroutine(FadeMainMenu());
    }
 
    IEnumerator FadeMainMenu()
    {      
        float disappearSpeed = 0.45f;
        float zoomFraction = 0f;
        bool endPointReached = false;
        while (true) {

            if (!endPointReached) {
                foreach(GameObject child in mainMenuGroup)
                {   
                    if (child.TryGetComponent<Button>(out Button button)) {
                        button.interactable = false;
                    }
                                
                    if (child.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI text)) {
                        var tempColorTxt = text.color;
                        tempColorTxt.a -= disappearSpeed * Time.deltaTime;
                        text.color = tempColorTxt;
                    } else if (child.transform.GetChild(0).TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI textC)) {

                        var tempColorTxt = textC.color;
                        tempColorTxt.a -= disappearSpeed * Time.deltaTime;
                        textC.color = tempColorTxt;
                    }

                    if (child.TryGetComponent<Image>(out Image img)) {
                        var tempColorImg = img.color;
                        tempColorImg.a -= disappearSpeed * Time.deltaTime;
                        img.color = tempColorImg;

                        if (img.color.a <= 0f) {
                            endPointReached = true;
                        }
                    }
                }
            } else {

                foreach(GameObject child in mainMenuGroup)
                { 
                    child.SetActive(false);
                }
            }

            zoomFraction += Time.unscaledDeltaTime * 0.0025f;
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, cameraSetPoint, zoomFraction);
            if (Mathf.Abs(Camera.main.orthographicSize - cameraSetPoint) < 0.1f) {
                InitializeCamp();
                yield break;
            }
            yield return null;
        }
    }

    public void InitializeCamp() {

        campMenuGroup.Find( x => x.gameObject.name=="ReadyUnitsPanel").SetActive(true);
        campMenuGroup.Find( x => x.gameObject.name=="EditReadyButton").SetActive(true);
        campMenuGroup.Find( x => x.gameObject.name=="InventoryButton").SetActive(true);
        campMenuGroup.Find( x => x.gameObject.name=="VentureFourth").SetActive(true);
        
        if (File.Exists(Application.persistentDataPath + RelationalData.data.filePath))
        {    
            GameObject ready = GetCampMenu("ReadyUnitsPanel");
            for (int i = 0; i < tempSave.readyUnits.Count; i++) {

                UnitUI u = ready.transform.GetChild(i).GetComponent<UnitUI>();
                UnitBase uBase = RelationalData.data.unitObjects.Find( x => x.id==tempSave.readyUnits[i].idType);

                u.id = tempSave.readyUnits[i].id;
                u.title.text = tempSave.readyUnits[i].handle;
                u.icon.sprite = uBase.sprite;
                u.details.SetActive(true);
            }

            GameObject camp = GetCampMenu("CampUnitsPanel");
            for (int i = 1; i <= tempSave.campUnits.Count; i++) {
                        
                UnitUI u = camp.transform.GetChild(i).GetComponent<UnitUI>();
                UnitBase uBase = RelationalData.data.unitObjects.Find( x => x.id==tempSave.campUnits[i - 1].idType);

                u.id = tempSave.campUnits[i - 1].id;
                u.title.text = tempSave.campUnits[i - 1].handle;
                u.icon.sprite = uBase.sprite;
                u.gameObject.SetActive(true);
                u.details.SetActive(true);
            }

            //SPAWN IN ITEMS HERE;
            GetCampMenu("Inventories").SetActive(true);
            GridInventory stash = GetCampMenu("Stash").GetComponent<GridInventory>();
            foreach(ItemData itemData in tempSave.stashItems) {

                stash.AddItemSpecify(itemData, new Vector2Int(itemData.xPos, itemData.yPos));
            }
            GridInventory equipment = GetCampMenu("Equipment").GetComponent<GridInventory>();
            foreach(ItemData itemData in tempSave.equipmentItems) {

                equipment.AddItemSpecify(itemData, new Vector2Int(itemData.xPos, itemData.yPos));
            }
            GetCampMenu("Inventories").SetActive(false);
        }

        foreach(GameObject child in mainMenuGroup)
        { 
            child.SetActive(false);
        }
        Camera.main.orthographicSize = cameraSetPoint;
    }

    public void InteractInventory() {
        
        GameObject inventories = GetCampMenu("Inventories");
        if (inventories.activeSelf) {
            inventories.SetActive(false); 
        } else {
            inventories.SetActive(true);
        }
    }

    public void InteractUnits() {

        GameObject units = GetCampMenu("CampUnitsPanel");
        if (units.activeSelf) {
            selUnit = null;
            units.SetActive(false); 
        } else {
            units.SetActive(true);
        }
    }

    public void SaveSync() {

        Transform stash = GetCampMenu("Stash").GetComponent<GridInventory>().itemsParent;
        tempSave.stashItems.Clear();
        foreach(Transform child in stash) {

            GridItem item = child.GetComponent<GridItem>();
            // ItemData data = tempSave.stashItems.Find( x => x.id==item.id);
            // if (data != null) {

            //     data.xPos = item.pos.x;
            //     data.yPos = item.pos.y;
            //     data.flipped = item.flipped;
            //     tempSave.stashItems[tempSave.stashItems.FindIndex( x => x.id==data.id)] = data;
            // } else {
                ItemData iData = new ItemData();
                iData.id = item.id;
                iData.idType = item.itemType.id;
                iData.xPos = item.pos.x;
                iData.yPos = item.pos.y;
                iData.flipped = item.flipped;
                iData.count = item.count;

                tempSave.stashItems.Add(iData);
            // }
        }

        Transform equipment = GetCampMenu("Equipment").GetComponent<GridInventory>().itemsParent;
        tempSave.equipmentItems.Clear();
        foreach(Transform child in equipment) {

            GridItem item = child.GetComponent<GridItem>();
            ItemData iData = new ItemData();
            iData.id = item.id;
            iData.idType = item.itemType.id;
            iData.xPos = item.pos.x;
            iData.yPos = item.pos.y;
            iData.flipped = item.flipped;
            iData.count = item.count;

            tempSave.equipmentItems.Add(iData);
        }

        //Finalized and close save file.
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + RelationalData.data.filePath);
        bf.Serialize(file, tempSave);
        file.Close();
        Debug.Log("Saved.");
    }

    public GameObject GetCampMenu(string str) {
        return campMenuGroup.Find( x => x.gameObject.name==str);
    }

    public void CloseDarken() {

        campMenuGroup.Find( x => x.gameObject.name=="EditReadyButton").SetActive(true);
        campMenuGroup.Find( x => x.gameObject.name=="CampUnitsPanel").SetActive(false);
        Image img = campMenuGroup.Find( x => x.gameObject.name=="ReadyUnitsPanel").transform.parent.GetComponent<Image>();
        img.raycastTarget = false;
        Color tempColor = img.color;
        tempColor.a = 0f;
        img.color = tempColor;
    }
}
