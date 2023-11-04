using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Control : MonoBehaviour
{   
    // singleton static reference
    private static Control assignAsset;
    public static Control data {
        get {
            if (assignAsset == null) {
                assignAsset = FindObjectOfType<Control>();
                if (assignAsset == null)
                    Debug.LogError("There is no Control in the scene!");
            }
            return assignAsset;
        }
    }

    public Unit selectedUnit;
    public AbilityData selectedAbility;
    float cameraSpeed = 100f;

    //This variable acts as a sort of storage, its safe and secure but we don't reference this specifically to refer to it.
    private int s_abilityPoints = 1;
    //Instead we reference this [abilityPoints] so that when it is referenced and recieved/changed an account can occur along with it.
    public int abilityPoints
    {
        get { return s_abilityPoints; }
        set {
            s_abilityPoints = Mathf.Clamp(value, 0, 12);
            actionInterface.transform.Find("AbilityPoints").GetChild(1).GetComponent<Text>().text = s_abilityPoints.ToString();
        }
    }

    public Vector3Int curTileHover;
    public GameObject actionInterface;
    public Tooltip tooltip;

    public bool activeTurn = true;
    public bool actionAble = true;
    public bool contextActions = false;

    public enum Action {
        Ability = 0,
        Move = 1,
    }

    Action action = Action.Move;

    public void SwitchAction(int value) {

        Transform abilityParent = actionInterface.transform.Find("Abilities");
        for (int i = 0; i < selectedUnit.abilities.Count; i++) {

            if (i == 0) {

                if (selectedUnit.sigCharge >= selectedUnit.unitType.sigCharge) {
                    abilityParent.GetChild(i).GetChild(0).gameObject.GetComponent<Button>().interactable = true;
                } else {
                    abilityParent.GetChild(i).GetChild(0).gameObject.GetComponent<Button>().interactable = false;
                }
            } else {

                int diff = Overlook.data.turnCount - selectedUnit.abilities[i].cooldownStart;
                if (diff >= selectedUnit.abilities[i].abilityType.cooldown && abilityPoints >= selectedUnit.abilities[i].abilityType.cost) {
                    abilityParent.GetChild(i).GetChild(0).gameObject.GetComponent<Button>().interactable = true;
                } else {
                    abilityParent.GetChild(i).GetChild(0).gameObject.GetComponent<Button>().interactable = false;
                }
            }
        }
        actionInterface.transform.Find("Movement").GetChild(0).gameObject.GetComponent<Button>().interactable = true;

        if (value == 0) {
            
            if (EventSystem.current.currentSelectedGameObject != null) {

                AbilityData ability = selectedUnit.abilities.Find(x => x.tempIdRef==EventSystem.current.currentSelectedGameObject.name);
                if (ability != null) {
                    
                    int diff = Overlook.data.turnCount - ability.cooldownStart;
                    if (diff >= ability.abilityType.cooldown) {
                        selectedAbility = ability;
                    }
                }
            }
        }

        action = (Action)value;
        UpdateHighlights();
    }

    public List<TileData> UpdateHighlights() {

        Board.data.tilemapHighlight.ClearAllTiles();
        Overlook.data.ghostUnit.SetActive(false);
        if (selectedUnit != null) {

            switch (action) {

                case Action.Move : {
                    
                    List<TileData> possibleMoves = Pattern(selectedUnit.curTile.tilePos, Overlook.data.movementAllowed);
                    if (selectedUnit.movement > 0) {
                        
                        foreach (TileData tile in possibleMoves) {

                            if (tile.occupied == null) {
                                if (tile.type == TileData.TileType.Walkable)
                                    Board.data.tilemapHighlight.SetTile(tile.tilePos, Board.data.tileGreen);

                            }
                        }
                    }
                    foreach (TileData tile in Pattern(selectedUnit.curTile.tilePos, selectedUnit.unitType.pattern)) {
                        if (tile.type == TileData.TileType.Walkable)
                            Board.data.tilemapHighlight.SetTile(tile.tilePos, Board.data.tileRed);
                    }
                    return possibleMoves;
                }

                case Action.Ability : {
                    
                    if (selectedAbility != null && selectedAbility.abilityType != null) {
                        List<TileData> possibleTiles = selectedAbility.abilityType.AbilityPattern(selectedUnit);
                        foreach (TileData tile in possibleTiles) {

                            if (tile.type == TileData.TileType.Walkable) {
                                Board.data.tilemapHighlight.SetTile(tile.tilePos, Board.data.tileBlue);
                            }
                        }
                        return possibleTiles;
                    }
                    break;
                }
            }
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {   
        if (activeTurn && actionAble) {

            //Mouse Highlights
            if (!IsPointerOverUIObject()) {
                
                Vector3Int tilePos = Board.data.parentGrid.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                if (curTileHover != tilePos) {

                    MouseHighlights(tilePos);
                }
            }

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject()) {

                // RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                TileData tileRef = Board.data.Tiles[Board.data.parentGrid.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition))];
                if (tileRef != null && tileRef.occupied != null) {

                    selectedAbility = null;
                    selectedUnit = tileRef.occupied;
                    actionInterface.SetActive(true);
                    SwitchAction(1);
                    UpdateActionInterface();
                    UpdateHighlights();
                    Control.data.tooltip.TooltipQuit();
                } 
                else {

                    Board.data.tilemapHighlight.ClearAllTiles();
                    Board.data.tilemapHighlight.SetTile(curTileHover, null);
                    if (tileRef != null && tileRef.type == TileData.TileType.Walkable) 
                        Board.data.tilemapHighlight.SetTile(tileRef.tilePos, Board.data.tileYellow);
                    Overlook.data.ghostUnit.SetActive(false);
                    actionInterface.SetActive(false);
                    selectedUnit = null;      
                }
            }

            if (Input.GetMouseButtonDown(1)) {

                if (selectedUnit != null) {

                    StartCoroutine(PlayerAction());
                }             
            }
        }

        if (Input.GetMouseButton(2)) {

            Camera.main.transform.position += new Vector3(-Input.GetAxis("Mouse X") * Time.deltaTime * cameraSpeed, -Input.GetAxis("Mouse Y") * Time.deltaTime * cameraSpeed, -10f);
            Camera.main.transform.position = new Vector3(Mathf.Clamp(Camera.main.transform.position.x, -1f, Board.data.sizeX), Mathf.Clamp(Camera.main.transform.position.y, -1f, Board.data.sizeY), -10f);
        }

        //TODO: Clean up this debug stuff.
        if (Input.GetKey(KeyCode.Mouse3)) {

            TileData tileRef = Board.data.Tiles[Board.data.parentGrid.WorldToCell((Camera.main.ScreenToWorldPoint(Input.mousePosition)))];

            if (Input.GetKeyDown(KeyCode.O)) {

                // if ((Tile)Board.data.tilemapGround.GetTile(tileRef.tilePos) == Board.data.tileSet.wall) {
                //     Board.data.tilemapGround.SetTile(tileRef.tilePos, Board.data.tileSet.pillar);
                // } else {
                //     Board.data.tilemapGround.SetTile(tileRef.tilePos, Board.data.tileSet.wall);
                // }
                if (tileRef.status != null) {
                    Debug.Log(tileRef.status.statusType.title + " | src: " + tileRef.status.src.attr.handle);
                } else {
                    Debug.Log("Null");
                }
                
                // Vector3Int tilePos = Board.data.parentGrid.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                // if (tileRef.status != null) {
                //     Debug.Log(tileRef.status.statusType.title);
                // }
                // string north = "False";
                // Tile tempNorth = (Tile)Board.data.tilemapGround.GetTile(new Vector3Int(tilePos.x, tilePos.y + 1, 0));
                // if (tempNorth != null) {
                //     north = "True";
                //     if (tempNorth == Board.data.tileSet.floor) {north += "-0";}
                // }

                // string south = "False";
                // Tile tempSouth = (Tile)Board.data.tilemapGround.GetTile(new Vector3Int(tilePos.x, tilePos.y + -1, 0));
                // if (tempSouth != null) {
                //     south = "True";
                //     if (tempSouth == Board.data.tileSet.floor) {south += "-0";}
                // }

                // string east = "False";
                // Tile tempEast = (Tile)Board.data.tilemapGround.GetTile(new Vector3Int(tilePos.x + 1, tilePos.y, 0));
                // if (tempEast != null) {
                //     east = "True";
                //     if (tempEast == Board.data.tileSet.floor) {east += "-0";}
                // }

                // string west = "False";
                // Tile tempWest = (Tile)Board.data.tilemapGround.GetTile(new Vector3Int(tilePos.x + -1, tilePos.y, 0));
                // if (tempWest != null) {
                //     west = "True";
                //     if (tempWest == Board.data.tileSet.floor) {west += "-0";}
                // }

                // //North_False_South_False_East_False_West_False
                // string fileName = "North_" + north + "_South_" + south + "_East_" + east + "_West_" + west;
                // Debug.Log(fileName + " | " + tilePos);

                // Vector3 cellCenter = Overlook.data.parentGrid.GetCellCenterWorld(tilePos);
                // Vector3 directionToTarget = cellCenter - selectedUnit.transform.position;
                // float dSqrToTarget = directionToTarget.sqrMagnitude;
                // Debug.Log(dSqrToTarget);

            }

            if (Input.GetKeyDown(KeyCode.K)) {

                if (tileRef.occupied != null && selectedUnit != null) {
                    tileRef.occupied.DamageUnit(selectedUnit, 999f, (int)RelationalData.DamageType.Physical);
                }
            }
        }
    }

    public void MouseHighlights(Vector3Int tilePos) {

        // TileData hoveredTile = Board.data.Tiles[tilePos];

        if (selectedUnit != null && Board.data.Tiles.TryGetValue(tilePos, out TileData hoveredTile)) {

            switch (action) {

                case Action.Move : {
                    
                    TileData tileCheck = UpdateHighlights().Find( x => x.tilePos==tilePos);
                        
                    //Draw pattern around mouse location if it exists in possible moves
                    if (selectedUnit.team == Overlook.data.playerTeam && tileCheck != null && tileCheck.type == TileData.TileType.Walkable && tileCheck.occupied == null && selectedUnit.movement > 0) {
                        
                        SpriteRenderer childSprite = Overlook.data.ghostUnit.transform.GetChild(0).GetComponent<SpriteRenderer>();

                        childSprite.transform.localPosition = selectedUnit.unitType.spritePos;
                        childSprite.sprite = selectedUnit.unitType.sprite;                   
                        if (tileCheck.tilePos.x < selectedUnit.curTile.tilePos.x) {
                            childSprite.flipX = true;
                        }
                        else if (tileCheck.tilePos.x > selectedUnit.curTile.tilePos.x) {
                            childSprite.flipX = false;
                        } else {
                            childSprite.flipX = selectedUnit.sprite.flipX;
                        }
                        Overlook.data.ghostUnit.transform.position = Board.data.parentGrid.GetCellCenterWorld(tilePos);
                        // if (Overlook.data.enemyHitPoints.Contains(tilePos)) {
                        //     Overlook.data.ghostUnit.transform.GetChild(1).gameObject.SetActive(true);
                        // } else {
                        //     Overlook.data.ghostUnit.transform.GetChild(1).gameObject.SetActive(false);
                        // }
                        Overlook.data.ghostUnit.SetActive(true);

                        Board.data.tilemapHighlight.ClearAllTiles();
                        foreach (TileData tile in Pattern(tilePos, selectedUnit.unitType.pattern)) {
                            if (tile.type == TileData.TileType.Walkable)
                                Board.data.tilemapHighlight.SetTile(tile.tilePos, Board.data.tileRed);
                        }
                    } else {

                        foreach (TileData tile in Pattern(selectedUnit.curTile.tilePos, selectedUnit.unitType.pattern)) {
                            if (tile.type == TileData.TileType.Walkable)
                                Board.data.tilemapHighlight.SetTile(tile.tilePos, Board.data.tileRed);
                        }
                        if (hoveredTile != null && hoveredTile.type == TileData.TileType.Walkable) {
                            Board.data.tilemapHighlight.SetTile(tilePos, Board.data.tileYellow);
                        }
                        Overlook.data.ghostUnit.SetActive(false);
                    }
                    break;
                }

                case Action.Ability : {
                    
                    if (selectedAbility != null && selectedAbility.abilityType != null) {

                        TileData tileCheck = UpdateHighlights().Find( x => x.tilePos==tilePos);

                        if (selectedAbility.abilityType.targetedPattern.Length > 0 && tileCheck != null) {

                            foreach (TileData tile in Pattern(tilePos, selectedAbility.abilityType.targetedPattern)) {
                                if (tile.type == TileData.TileType.Walkable)
                                    Board.data.tilemapHighlight.SetTile(tile.tilePos, Board.data.tileYellow);
                            }
                        } else {

                            if (hoveredTile != null && hoveredTile.type == TileData.TileType.Walkable) {
                                Board.data.tilemapHighlight.SetTile(tilePos, Board.data.tileYellow);
                            }
                        }
                    }
                    break;
                }
            }

        } else {

            Board.data.tilemapHighlight.SetTile(curTileHover, null);
            if (Board.data.Tiles.TryGetValue(tilePos, out TileData tile) && tile.type == TileData.TileType.Walkable) {
                
                Board.data.tilemapHighlight.SetTile(tilePos, Board.data.tileYellow);
            }
            Control.data.tooltip.TooltipQuit();

            if (tile != null && tile.status != null) {
                Tooltip t = Control.data.tooltip;
                t.img.sprite = null;
                t.title.text = tile.status.statusType.title;
                t.title.color = tile.status.statusType.color;
                t.desc.text = tile.status.statusType.Desc();
                t.misc_1.text = "Source: " + tile.status.src.attr.handle;
                t.misc_2.text = tile.status.tileStatusDur + " Remaining Turn(s)"; 
                t.TooltipIni(2);
            }
        }
        curTileHover = tilePos;
    }

    public IEnumerator PlayerAction() {

        TileData tileRef = Board.data.Tiles[Board.data.parentGrid.WorldToCell((Camera.main.ScreenToWorldPoint(Input.mousePosition)))];

        if (activeTurn && actionAble && selectedUnit.team == Overlook.data.playerTeam) {

            if (tileRef != null) {
                
                switch (action) {

                    case Action.Move : {

                        List<TileData> possibleMoves = Control.data.Pattern(selectedUnit.curTile.tilePos, Overlook.data.movementAllowed);

                        if (possibleMoves.Contains(tileRef) && tileRef.occupied == null && tileRef.type == TileData.TileType.Walkable && selectedUnit.movement > 0) {
                            
                            Board.data.tilemapHighlight.ClearAllTiles();
                            actionAble = false;
                            yield return StartCoroutine(selectedUnit.unitType.MoveAction(selectedUnit, tileRef));
                            if (activeTurn) {
                                actionAble = true;
                                if (!EndTurn()) {
                                    MouseHighlights(tileRef.tilePos);
                                }
                            }
                        }
                        break;
                    }

                    case Action.Ability : {
                
                        List<TileData> possibleTiles = selectedAbility.abilityType.AbilityPattern(selectedUnit);

                        if (possibleTiles.Contains(tileRef) && tileRef.type == TileData.TileType.Walkable && selectedAbility.abilityType.cost <= abilityPoints) {

                            Board.data.tilemapHighlight.ClearAllTiles();
                            if (selectedAbility.abilityType == selectedUnit.unitType.signature) {
                                selectedUnit.sigCharge = 0;
                            } else {
                                abilityPoints = abilityPoints - selectedAbility.abilityType.cost;
                            }
                            actionAble = false;
                            selectedUnit.abilities.Find( x => x.tempIdRef==selectedAbility.tempIdRef).cooldownStart = Overlook.data.turnCount;
                            yield return StartCoroutine(selectedAbility.abilityType.AbilityAction(selectedUnit, tileRef));

                            selectedAbility = null;
                            if (activeTurn) {
                                SwitchAction(1);
                                UpdateActionInterface();
                                actionAble = true;
                                MouseHighlights(tileRef.tilePos);
                            }
                        }
                        break;
                    }
                }
            }
        }
    }

    public void UpdateActionInterface() {

        //UIText Setting
        Transform uiTextParent = actionInterface.transform.Find("UIText");

        uiTextParent.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = selectedUnit.attr.handle;
        uiTextParent.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = selectedUnit.unitType.title;

        //Stat Setting
        Transform statsParent = actionInterface.transform.Find("Stats").GetChild(0);
        
        TextMeshProUGUI healthText = statsParent.transform.Find("HealthStat").GetChild(1).GetComponent<TextMeshProUGUI>();
        healthText.color = Color.white;
        if (selectedUnit.stats.health.basic < selectedUnit.stats.health.basicBase) {healthText.color = Color.red;} 
        else if (selectedUnit.stats.health.basic > selectedUnit.stats.health.basicBase) {healthText.color = Color.green;}

        TextMeshProUGUI armorText = statsParent.transform.Find("ArmorStat").GetChild(1).GetComponent<TextMeshProUGUI>();
        armorText.color = Color.white;
        if (selectedUnit.stats.armor.basic < selectedUnit.stats.armor.basicBase) {armorText.color = Color.red;} 
        else if (selectedUnit.stats.armor.basic > selectedUnit.stats.armor.basicBase) {armorText.color = Color.green;}

        TextMeshProUGUI damageText = statsParent.transform.Find("DamageStat").GetChild(1).GetComponent<TextMeshProUGUI>();
        damageText.color = Color.white;
        if (selectedUnit.stats.damage.basic < selectedUnit.stats.damage.basicBase) {damageText.color = Color.red;} 
        else if (selectedUnit.stats.damage.basic > selectedUnit.stats.damage.basicBase) {damageText.color = Color.green;}

        TextMeshProUGUI abilityText = statsParent.transform.Find("AbilityStat").GetChild(1).GetComponent<TextMeshProUGUI>();
        abilityText.color = Color.white;
        if (selectedUnit.stats.abilityStrength.value < selectedUnit.stats.abilityStrength.basicBase) {abilityText.color = Color.red;} 
        else if (selectedUnit.stats.abilityStrength.value > selectedUnit.stats.abilityStrength.basicBase) {abilityText.color = Color.green;}

        TextMeshProUGUI accuracyText = statsParent.transform.Find("AccuracyStat").GetChild(1).GetComponent<TextMeshProUGUI>();
        accuracyText.color = Color.white;
        if (selectedUnit.stats.accuracy.value < selectedUnit.stats.accuracy.basicBase) {accuracyText.color = Color.red;} 
        else if (selectedUnit.stats.accuracy.value > selectedUnit.stats.accuracy.basicBase) {accuracyText.color = Color.green;}

        TextMeshProUGUI critText = statsParent.transform.Find("CritChanceStat").GetChild(1).GetComponent<TextMeshProUGUI>();
        critText.color = Color.white;
        if (selectedUnit.stats.criticalChance.value < selectedUnit.stats.criticalChance.basicBase) {critText.color = Color.red;} 
        else if (selectedUnit.stats.criticalChance.value > selectedUnit.stats.criticalChance.basicBase) {critText.color = Color.green;}

        TextMeshProUGUI threatText = statsParent.transform.Find("ThreatStat").GetChild(1).GetComponent<TextMeshProUGUI>();
        threatText.color = Color.white;
        if (selectedUnit.stats.threat.value < selectedUnit.stats.threat.basicBase) {threatText.color = Color.red;} 
        else if (selectedUnit.stats.threat.value > selectedUnit.stats.threat.basicBase) {threatText.color = Color.green;}

        TextMeshProUGUI fireText = statsParent.transform.Find("FireResistanceStat").GetChild(1).GetComponent<TextMeshProUGUI>();
        fireText.color = Color.white;
        if (selectedUnit.stats.fireResistance.value < selectedUnit.stats.fireResistance.basicBase) {fireText.color = Color.red;} 
        else if (selectedUnit.stats.fireResistance.value > selectedUnit.stats.fireResistance.basicBase) {fireText.color = Color.green;}

        TextMeshProUGUI lightText = statsParent.transform.Find("LightResistanceStat").GetChild(1).GetComponent<TextMeshProUGUI>();
        lightText.color = Color.white;
        if (selectedUnit.stats.lightResistance.value < selectedUnit.stats.lightResistance.basicBase) {lightText.color = Color.red;} 
        else if (selectedUnit.stats.lightResistance.value > selectedUnit.stats.lightResistance.basicBase) {lightText.color = Color.green;}

        TextMeshProUGUI darkText = statsParent.transform.Find("DarkResistanceStat").GetChild(1).GetComponent<TextMeshProUGUI>();
        darkText.color = Color.white;
        if (selectedUnit.stats.darkResistance.value < selectedUnit.stats.darkResistance.basicBase) {darkText.color = Color.red;} 
        else if (selectedUnit.stats.darkResistance.value > selectedUnit.stats.darkResistance.basicBase) {darkText.color = Color.green;}

        healthText.text = System.Math.Round(selectedUnit.stats.health.value,1)+"/"+System.Math.Round(selectedUnit.stats.health.basic,1);
        armorText.text = selectedUnit.stats.armor.value+"/"+selectedUnit.stats.armor.basic;

        damageText.text = System.Math.Round(selectedUnit.stats.damage.value,1)+"-"+System.Math.Round(selectedUnit.stats.damage.basic,1);
        abilityText.text = System.Math.Round(selectedUnit.stats.abilityStrength.value,0).ToString();

        accuracyText.text = (selectedUnit.stats.accuracy.value * 100)+"%";
        critText.text = (selectedUnit.stats.criticalChance.value * 100)+"%";

        threatText.text = System.Math.Round(selectedUnit.stats.threat.value,0).ToString();
        fireText.text = (selectedUnit.stats.fireResistance.value * 100)+"%";

        lightText.text = (selectedUnit.stats.lightResistance.value * 100)+"%";
        darkText.text = (selectedUnit.stats.darkResistance.value * 100)+"%";

        //Ability Setting
        Transform abilitiesParent = actionInterface.transform.Find("Abilities");

        Slider sigCharge = abilitiesParent.Find("Ability_SIG").GetChild(1).GetComponent<Slider>();
        sigCharge.maxValue = selectedUnit.unitType.sigCharge;
        sigCharge.value = selectedUnit.sigCharge;

        foreach(Transform child in abilitiesParent) {

            Transform input = child.GetChild(0);
            input.GetComponent<Button>().interactable = false;
            input.GetComponent<Image>().sprite = null;
            input.GetChild(0).gameObject.SetActive(false);
            ContextualUIAbility temp = input.GetComponent<ContextualUIAbility>();
            temp.ability = null;
            temp.unit = null;
        }

        for (int i = 0; i < selectedUnit.abilities.Count; i++) {

            Transform child = abilitiesParent.GetChild(i).GetChild(0);
            child.GetComponent<Image>().sprite = null;

            AbilityData data = selectedUnit.abilities[i];
            Button button = child.GetComponent<Button>();
            ContextualUIAbility contAbility = child.GetComponent<ContextualUIAbility>();
            contAbility.ability = data.abilityType;
            contAbility.unit = selectedUnit;
            child.GetComponent<Image>().sprite = data.abilityType.icon;
            child.name = data.tempIdRef;

            Transform textChild = child.GetChild(0);

            if (i == 0) {

                if (selectedUnit.sigCharge >= selectedUnit.unitType.sigCharge) {
                    button.interactable = true;
                } else {
                    button.interactable = false;
                }
            } else {

                int diff = Overlook.data.turnCount - data.cooldownStart;
                if (diff >= data.abilityType.cooldown) {
                    button.interactable = true;
                    textChild.gameObject.SetActive(false);
                } else {
                    button.interactable = false;
                    textChild.GetComponent<Text>().text = (data.abilityType.cooldown - diff).ToString();
                    textChild.gameObject.SetActive(true);
                }

                if (abilityPoints < data.abilityType.cost) {
                    button.interactable = false;
                }
            }
        }

        //Status Setting
        Transform statusParent = actionInterface.transform.Find("StatusBar");
           
        foreach (Transform child in statusParent) {
            child.gameObject.SetActive(false);
            child.GetComponent<ContextualUIStatus>().status = null;
        }

        //TODO: make sure there is a check if the child count maximum is reached so that it doesn't break
        if (selectedUnit.activeStatusEffects.Count > 0) {
            
            for (int i = 0; i < selectedUnit.activeStatusEffects.Count; i++) {
                
                Transform child = statusParent.GetChild(i);
                child.GetComponent<Image>().sprite = selectedUnit.activeStatusEffects[i].statusEffectBase.sprite;
                ContextualUIStatus statusUI = child.GetComponent<ContextualUIStatus>();
                statusUI.status = selectedUnit.activeStatusEffects[i];
                statusUI.effected = selectedUnit;
                child.gameObject.SetActive(true);
            }
        }

        actionInterface.transform.Find("AbilityPoints").GetChild(1).GetComponent<Text>().text = abilityPoints.ToString();
    }

    public bool EndTurn() {

        foreach (Unit unit in Overlook.data.teams[Overlook.data.playerTeam]) {

            if (unit.movement > 0) {
                return false;
            }
        }

        Board.data.tilemapHighlight.ClearAllTiles();
        actionInterface.SetActive(false);
        foreach(Unit unit in Overlook.data.teams[Overlook.data.playerTeam]) {
            unit.sigCharge += 5f;
        }
        abilityPoints++;
        selectedUnit = null;
        selectedAbility = null;
        activeTurn = false;
        Overlook.data.ghostUnit.SetActive(false);
        tooltip.gameObject.SetActive(false);
        Overlook.data.directorCo = Overlook.data.StartCoroutine(Overlook.data.Director());
        return true;
    }

    public Vector3Int[] CoordinatesWithinRadius (Vector3Int origin, int dist) {
        List <Vector3Int> r = new List<Vector3Int> (); 
        // create a circle in a 2d grid
        int size = dist * 2;

        int xx = size;while(xx>0){xx--;
        int yy = size;while(yy>0){yy--;
                //check distance formua from center for coordinates in square distance
                if(Mathf.Sqrt(((dist-xx)*(dist-xx))+((dist-yy)*(dist-yy)))<dist){
                    r.Add(new Vector3Int(xx-dist+origin.x,yy-dist+origin.y, 0));
        }}}
        return r.ToArray();
    }

    public List<TileData> Pattern(Vector3Int tile, Vector3Int[] pattern) {

        List<TileData> possibleMoves = new List<TileData>();

        for (int i = 0; i < pattern.Length; i++) {

            if (Board.data.Tiles.TryGetValue(new Vector3Int(tile.x + pattern[i].x, tile.y + pattern[i].y, 0), out TileData cordsAroundTile)) //cordsAroundTile.occupied == null && cordsAroundTile.walkable                           
                possibleMoves.Add(cordsAroundTile);
        }
        return possibleMoves;
    }

    public bool IsPointerOverUIObject() {

        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public IEnumerator CameraZoom(Vector3 pos, float setPoint, float zoomSpeed, float panSpeed)
    { 
        Vector3 destPos = new Vector3(pos.x, pos.y, -10);
        float zoomFraction = 0f;
        float panFraction = 0f;

        List<Unit> allUnits = new List<Unit>();

        foreach(KeyValuePair<int, List<Unit>> entry in Overlook.data.teams) {
            allUnits.AddRange(entry.Value);
        }
        
        while (true) {
            
            zoomFraction += Time.unscaledDeltaTime * zoomSpeed;
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, setPoint, zoomFraction);

            panFraction += Time.unscaledDeltaTime * panSpeed;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, destPos, panFraction);

            foreach(Unit unit in allUnits) {
                unit.overhead.UpdatePos();
            }

            if (Vector3.Distance(Camera.main.transform.position, destPos) < 0.01f && Mathf.Abs(Camera.main.orthographicSize - setPoint) < 0.01f) {
                Camera.main.transform.position = destPos;
                Camera.main.orthographicSize = setPoint;
                break;
            }
            yield return null;
        }
    }
}
