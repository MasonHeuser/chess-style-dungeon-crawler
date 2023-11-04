using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using TMPro;

public class Overlook : MonoBehaviour
{   
    // singleton static reference
    private static Overlook assignAsset;
    public static Overlook data {
        get {
            if (assignAsset == null) {
                assignAsset = FindObjectOfType<Overlook>();
                if (assignAsset == null)
                    Debug.LogError("There is no Overlook in the scene!");
            }
            return assignAsset;
        }
    }

    //This variable acts as a sort of storage, its safe and secure but we don't reference this specifically to refer to it.
    private int s_turnCount = 0;
    //Instead we reference this [abilityPoints] so that when it is referenced and recieved/changed an account can occur along with it.
    public int turnCount
    {
        get { return s_turnCount; }
        set {
            s_turnCount = value;
            turnCountText.text = s_turnCount.ToString();
        }
    }

    public AudioSource audioSource;
    public float orthoDefaultSize;
    public Transform generalVfxParent;
    public Transform tileStatusEffects;
    public GameObject ghostUnit;

    public Coroutine directorCo = null;

    public List<Vector3Int> enemyHitPoints = new List<Vector3Int>();
    public GridInventory areaInventory;
    public GridInventory playerInventory;

    public int playerTeam = 0;
    public Dictionary<int, List<Unit>> teams = new Dictionary<int, List<Unit>>(); 

    public Vector3Int[] movementAllowed;

    public bool cameraMovePerTurn = true;

    public TextMeshProUGUI turnText;
    public TextMeshProUGUI turnCountText;
    public Image fade;
    public GameObject contextActions;

    public Save saveCache = null;

    void Awake() {
       
        if (File.Exists(Application.persistentDataPath + RelationalData.data.filePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + RelationalData.data.filePath, FileMode.Open);
            saveCache = (Save)bf.Deserialize(file);
            file.Close();

            playerInventory.xSize = saveCache.xInv;
            playerInventory.ySize = saveCache.yInv;
        }
    }

    // void Update() {
    //     //TODO: clean up this debug.
    //     if (Input.GetKeyDown(KeyCode.L)) {
    //         WinCond();
    //     }
    // }

    void Start() {

        CreateMap();
        areaInventory.CreateInventory();
        playerInventory.CreateInventory();
        foreach(ItemData itemData in saveCache.equipmentItems) {

            playerInventory.AddItemSpecify(itemData, new Vector2Int(itemData.xPos, itemData.yPos));
        }
        areaInventory.gameObject.SetActive(false);

        orthoDefaultSize = Camera.main.orthographicSize;
        Camera.main.orthographicSize = orthoDefaultSize;
        
        SetCameraPos();
    }

    public void SetCameraPos() {

        int middleX = Board.data.sizeX / 2;
        int middleY = Board.data.sizeY / 2;
        Vector3Int middletile = new Vector3Int(middleX, middleY, 0);
        Vector3 camPos = new Vector3(Board.data.parentGrid.CellToWorld(middletile).x, Board.data.parentGrid.CellToWorld(middletile).y, -10);
        Camera.main.transform.position = camPos;
    }

    public void UpdateEnemyAttackPos() {

        enemyHitPoints.Clear();
        foreach(KeyValuePair<int, List<Unit>> entry in teams) {

            if (playerTeam != entry.Key) {

                foreach (Unit unit in entry.Value) {

                    foreach (TileData tile in Control.data.Pattern(unit.curTile.tilePos, unit.unitType.pattern)) {
                        enemyHitPoints.Add(tile.tilePos);
                    }
                }
            }
        }
    }

    public void GlobalTileStatus() {

        foreach(TileData tile in Board.data.Tiles.Values)
        {   
            if (tile.status != null) {

                TileStatusBase status = tile.status.statusType;
                status.TileStatusEffect(tile);
                tile.status.tileStatusDur--;
                if (tile.status.tileStatusDur <= 0) {
                    tile.status.statusType.EndTileStatus(tile);
                }
            }
        }
    }

    public void StatusEffectTick(List<Unit> units) {
        
        List<Unit> unitList = new List<Unit>();
        unitList.AddRange(units);

        for (int i = 0; i < unitList.Count; i++) {

            if (unitList[i] != null) {

                for (int j = 0; j < unitList[i].activeStatusEffects.Count; j++) {

                    if (unitList[i].activeStatusEffects[j].statusEffectBase.statusType == StatusEffectBase.StatusType.Positive) {

                        unitList[i].activeStatusEffects[j].statusEffectBase.StatusEffect(unitList[i], unitList[i].activeStatusEffects[j].srcSnapShot);

                        unitList[i].activeStatusEffects[j].duration--;
                        if (unitList[i].activeStatusEffects[j].duration <= 0) {                
                            unitList[i].activeStatusEffects[j].statusEffectBase.StatusEffectEnd(unitList[i], unitList[i].activeStatusEffects[j].srcSnapShot);
                            unitList[i].activeStatusEffects.Remove(unitList[i].activeStatusEffects[j]);
                            unitList[i].overhead.UpdateOverhead();
                        }
                    }
                }
            }
            
            if (unitList[i] != null) {

                for (int j = 0; j < unitList[i].activeStatusEffects.Count; j++) {

                    if (unitList[i].activeStatusEffects[j].statusEffectBase.statusType == StatusEffectBase.StatusType.Negative) {
                        
                        unitList[i].activeStatusEffects[j].statusEffectBase.StatusEffect(unitList[i], unitList[i].activeStatusEffects[j].srcSnapShot);

                        unitList[i].activeStatusEffects[j].duration--;
                        if (unitList[i].activeStatusEffects[j].duration <= 0) {                
                            unitList[i].activeStatusEffects[j].statusEffectBase.StatusEffectEnd(unitList[i], unitList[i].activeStatusEffects[j].srcSnapShot);
                            unitList[i].activeStatusEffects.Remove(unitList[i].activeStatusEffects[j]);
                            unitList[i].overhead.UpdateOverhead();
                        }
                    }
                }
            }
        }
    }

    public IEnumerator Director()
    {   
        yield return new WaitForSeconds(0.5f);
        System.GC.Collect();
        yield return new WaitForSeconds(0.5f);
        turnCount++;
        GlobalTileStatus();

        RelationalData.data.UpdateText(turnText, "Enemy Turn", Color.red);
        yield return new WaitForSeconds(0.5f);
        foreach (Unit unit in teams[1]) { unit.movement = 1; }
        StatusEffectTick(teams[1]);

        yield return new WaitForSeconds(1f);
        if (teams[1].Count > 0) {

            foreach (Unit unit in teams[1]) {

                if (cameraMovePerTurn)
                    yield return StartCoroutine(Control.data.CameraZoom(unit.transform.position, Camera.main.orthographicSize, 0, 0.08f));
                yield return new WaitForSeconds(0.5f);
                yield return unit.StartCoroutine(unit.unitType.AI(unit));
            }
            UpdateEnemyAttackPos();
            yield return new WaitForSeconds(0.5f);
        }

        foreach(Unit unit in teams[1]) {
            unit.sigCharge += 5f;
        }
        RelationalData.data.UpdateText(turnText, "Your Turn", Color.white);
        yield return new WaitForSeconds(0.5f);
        foreach (Unit unit in teams[playerTeam]) { unit.movement = 1; }
        StatusEffectTick(teams[playerTeam]);
        Control.data.activeTurn = true;
    }

    public bool TrueIfEven(int number) {
        if (number%2==0)
            return true;
        else
            return false;
    }

    public bool TrueIfOdd(int number) {
        if (number%2==1)
            return true;
        else
            return false;
    }

    public Unit GetClosestUnit(Unit unitStartPoint, int teamNumber) {

        Unit bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = unitStartPoint.transform.position;
        foreach(Unit potentialTarget in teams[teamNumber])
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if(dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;
    }

    void CreateMap() {

        int roomSize = 32;
        Transform map = contextActions.transform.Find("Map");

        foreach(RoomData room in Board.data.rooms) {
            GameObject roomCell = Instantiate(map.GetChild(0).GetChild(0).gameObject);
            roomCell.name = "Room";
            roomCell.transform.SetParent(map.GetChild(0), false);
            roomCell.SetActive(true);

            roomCell.transform.localPosition = new Vector2(((roomSize + 2) * room.location.x), ((roomSize + 2) * room.location.y));
        }
        map.GetChild(0).GetChild(1).GetChild(0).gameObject.SetActive(true);
        Destroy(map.GetChild(0).GetChild(0).gameObject);

        Bounds bounds = new Bounds();  
        for (int i = 0; i < map.GetChild(0).childCount; i++) {
            bounds.Encapsulate(map.GetChild(0).GetChild(i).transform.localPosition);
        }
        for (int i = 0; i < map.GetChild(0).childCount; i++) {
            map.GetChild(0).GetChild(i).Translate(-bounds.center);
        }
    }

    public void UpdateMap() {
        
        Transform map = contextActions.transform.Find("Map");
        foreach(Transform child in map.GetChild(0)) {
            child.GetChild(0).gameObject.SetActive(false);
        }

        int i = Board.data.rooms.IndexOf(Board.data.curRoom);
        map.GetChild(0).GetChild(i).GetChild(0).gameObject.SetActive(true);
    }

    public void ViewMap() {
        
        GameObject map = contextActions.transform.Find("Map").gameObject;
        if (map.activeSelf) {
            map.SetActive(false);
        } else {
            map.SetActive(true);
            if (areaInventory.gameObject.activeSelf) {
                UpdateItemSaveCache();
            }
            contextActions.transform.Find("Inventories").gameObject.SetActive(false);
        }
    }

    public void ViewInventory() {
        
        GameObject inv = contextActions.transform.Find("Inventories").gameObject;

        if (inv.activeSelf) {

            if (areaInventory.gameObject.activeSelf) {
                UpdateItemSaveCache();
            }
            inv.SetActive(false);
        } else {
            inv.SetActive(true);
            contextActions.transform.Find("Map").gameObject.SetActive(false);
        }
    }

    public void InitializeContextActions() {
        
        Control.data.activeTurn = false;
        Control.data.actionAble = true;
        Control.data.contextActions = true;
        Control.data.actionInterface.SetActive(false);
        turnCountText.transform.parent.gameObject.SetActive(false);
        turnText.transform.parent.gameObject.SetActive(false);
        Board.data.tilemapHighlight.ClearAllTiles();

        SpawnLoot();
        contextActions.SetActive(true);

        // StartCoroutine(Dialogue());

        // Transform selectionParent = contextActions.transform.Find("Dialogue").Find("SelectionParent").GetChild(0);

        // foreach (Transform child in selectionParent) {
        //     child.gameObject.SetActive(false);
        // }
        // int count = selectionParent.childCount;
        // int i = 0;
        // while (i < count) {
                                       
        //     DialogueChoice c = selectionParent.GetChild(i).GetComponent<DialogueChoice>();
            
        //     i++;
        // }
    }

    public void SpawnLoot() {
        
        foreach(LootData lootData in Board.data.curRoom.loot) {

            Loot lootObject = CacheReference.asset.GetPooledGameObject(CacheReference.asset.lootList).GetComponent<Loot>();
            lootObject.lootData = lootData;
            lootObject.transform.position = Board.data.parentGrid.GetCellCenterWorld(lootData.tileLocation);
            lootObject.gameObject.SetActive(true);
        }
    }

    public IEnumerator Dialogue() {

        TextMeshProUGUI dialogueText = contextActions.transform.Find("ContextMenu").Find("DialogueArea").GetChild(1).GetComponent<TextMeshProUGUI>();
        dialogueText.text = "";
        string testText = "If you don't kill me now, you're going to regret it later down the line... That is of course, if you survive that long.";

        Control.data.actionAble = false;
        float pauseTimer;
        char[] letters = testText.ToCharArray();

        int i = 0;
        while (i < letters.Length) {
            
            switch (letters[i]) {
                
                case '?' :
                case '!' :
                case '.' :
                    pauseTimer = 0.75f;
                break;

                case ',' :
                    pauseTimer = 0.3f;
                break;

                default : 
                    pauseTimer = 0.065f;
                break;
            }
            dialogueText.text += letters[i];
            yield return new WaitForSeconds(pauseTimer);
            i++;
        }
        Control.data.actionAble = true;
        yield break;
    }

    public void EndRoom() {
        
        if (teams[1].Count == 0) {

            InitializeContextActions();
        } 
        else if (teams[playerTeam].Count == 0) {
            //TODO: Handle losing.
            InitializeContextActions();
        }

        if (directorCo != null)
            StopCoroutine(directorCo);
            directorCo = null;
    }

    public void UpdateItemSaveCache() {

        if (areaInventory.gameObject.activeSelf) {

            Transform itemArea = areaInventory.GetComponent<GridInventory>().itemsParent;
            LootData loot = Board.data.curRoom.loot.Find(x => x.id == areaInventory.gameObject.name);

            if (loot != null && loot.items.Count > 0) {
                
                loot.items.Clear();
                foreach(Transform child in itemArea) {

                    GridItem item = child.GetComponent<GridItem>();
                    loot.items.Add(InventoryControl.data.ConvertGridItemToData(item));
                }
                
                foreach(Transform child in itemArea) {
                    Destroy(child.gameObject);
                }
                areaInventory.items.Clear();
            }
            areaInventory.gameObject.SetActive(false);
            areaInventory.gameObject.name = "AreaInventory";
        }
    }

    public void UpdateSaveCache() {
        
        //Save to new save instance.
        saveCache.readyUnits.Clear();
        for (int i = 0; i < teams[playerTeam].Count; i++) {

            teams[playerTeam][i].attr.health = teams[playerTeam][i].stats.health.value;
            saveCache.readyUnits.Add(teams[playerTeam][i].attr);
        }

        Board.data.curRoom.enemyCount = teams[1].Count;
    }

    void SaveGameData() {

        if (!Directory.Exists(Application.persistentDataPath + "/saves")) {
            Directory.CreateDirectory(Application.persistentDataPath + "/saves");
        }

        Transform equipment = playerInventory.GetComponent<GridInventory>().itemsParent;
        saveCache.equipmentItems.Clear();
        foreach(Transform child in equipment) {

            GridItem item = child.GetComponent<GridItem>();
            saveCache.equipmentItems.Add(InventoryControl.data.ConvertGridItemToData(item));
        }

        //Finalized and close save file.
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + RelationalData.data.filePath);
        bf.Serialize(file, saveCache);
        file.Close();

        // //Open old Save if it exists and add it to oldSave variable.
        // if (File.Exists(Application.persistentDataPath + RelationalData.data.filePath))
        // {
        //     BinaryFormatter oldBf = new BinaryFormatter();
        //     FileStream oldFile = File.Open(Application.persistentDataPath + RelationalData.data.filePath, FileMode.Open);
        //     save = (Save)oldBf.Deserialize(oldFile);
        //     oldFile.Close();
        // }

        // //If oldSave variable exists, add the old data to new Save variable instanced data.
        // if (save != null) {

        //     //Save to new save instance.
        //     for (int i = 0; i < allyUnits.Count; i++) {

        //         //Cleanse each stat of any modifer but pass in value stat if its meaningful for saving.
        //         allyUnits[i].attr.health = allyUnits[i].health.value;

        //         UnitAttributes rUnit = save.readyUnits.Find( x => x.id==allyUnits[i].attr.id);
        //         if (rUnit != null) {
        //             save.readyUnits.Remove(rUnit);
        //         }
        //         save.readyUnits.Add(allyUnits[i].attr);
        //         // save.readyUnits[save.readyUnits.FindIndex(x => x.id==allyUnits[i].attr.id)] = allyUnits[i].attr;

        //         // if (allyUnits[i].equippedItem != null) {
        //         //     ItemData unitItem = save.items.Find( x => x.id==allyUnits[i].equippedItemId);
        //         //     if (unitItem == null) {
                        
        //         //         ItemData iData = new ItemData();
        //         //         iData.id = System.Guid.NewGuid().ToString();
        //         //         iData.idType = allyUnits[i].equippedItem.id;
        //         //         iData.equippedTo = allyUnits[i].attr.id;

        //         //         save.items.Add(iData);
        //         //     }
        //         // }
        //     }

        //     // for (int i = 0; i < save.campUnits.Count; i++) {

        //     //     UnitBase unit = RelationalData.data.unitObjects.Find( x => x.id==save.campUnits[i].idType);
        //     //     if (save.campUnits[i].health.value > unit.health) {
        //     //         save.campUnits[i].health.value = unit.health;
        //     //     }
        //     // }

        //     //Finalized and close save file.
        //     BinaryFormatter bf = new BinaryFormatter();
        //     FileStream file = File.Create(Application.persistentDataPath + RelationalData.data.filePath);
        //     bf.Serialize(file, save);
        //     file.Close();
    }

    public IEnumerator FadeIn() {

        float rate = 0.85f;
        fade.gameObject.SetActive(true);
        Color set = fade.color;
        set.a = 0f;
        fade.color = set;

        while (fade.color.a < 1f) {

            Color temp = fade.color;
            temp.a += rate * Time.deltaTime;
            fade.color = temp;
            yield return null;
        }
        fade.gameObject.SetActive(false);
        yield break;
    }

    public IEnumerator FadeOut() {

        float rate = 0.95f;
        fade.gameObject.SetActive(true);
        Color set = fade.color;
        set.a = 1f;
        fade.color = set;
        
        while (fade.color.a > 0f) {

            Color temp = fade.color;
            temp.a -= rate * Time.deltaTime;
            fade.color = temp;
            yield return null;
        }
        fade.gameObject.SetActive(false);
        yield break;
    }
}
