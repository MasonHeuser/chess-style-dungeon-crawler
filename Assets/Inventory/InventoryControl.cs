using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryControl : MonoBehaviour
{
    // singleton static reference
    private static InventoryControl assignAsset;
    public static InventoryControl data {
        get {
            if (assignAsset == null) {
                assignAsset = FindObjectOfType<InventoryControl>();
                if (assignAsset == null)
                    Debug.LogError("There is no InventoryControl in the scene!");
            }
            return assignAsset;
        }
    }

    public GridItem gridItem;
    public int cellSize = 32;

    public Canvas canvas;
    public GameObject ghostPrefab;
    public RectTransform ghostItem;
    public Vector2Int curHoveredCell;
    public Vector2Int ghostSize;
    private bool s_ghostFlipped = false;
    public bool ghostFlipped
    {
        get { return s_ghostFlipped; }
        set {
            s_ghostFlipped = value;
            GhostOrientation();
        }
    }

    private GridInventory s_hoveredInventory = null;
    public GridInventory hoveredInventory
    {
        get { return s_hoveredInventory; }
        set {
            if (s_hoveredInventory != value) {
                ClearHighlights();
            }
            s_hoveredInventory = value;
        }
    }
    public GridItem draggedItem = null;
    public GridItem originStack = null;
    public Vector2 mousePos;

    Color errorColor = new Color32(0xFF, 0x00, 0x00, 0xA5);
    Color hoverColor = new Color32(0xE5, 0xE5, 0x6B, 0xA5);
    Color succsColor = new Color32(0x05, 0xD2, 0x35, 0xA5);
    public List<Vector2Int> clearCells = new List<Vector2Int>();

    void Start() {
        ghostItem = Instantiate(ghostPrefab).GetComponent<RectTransform>();
        ghostItem.name = ghostPrefab.name;
        ghostItem.transform.SetParent(canvas.transform, false);
        ghostItem.transform.SetAsLastSibling();
        ghostItem.gameObject.SetActive(false);
    }

    public void iniGhost(GridItem draggingItem) {

        if (Input.GetKey(KeyCode.LeftControl) && draggingItem.count > 1) {
            originStack = draggingItem;
            draggedItem = Instantiate(draggingItem);
            draggedItem.name = draggingItem.name;
            originStack.count -= 1;
            draggedItem.count = 1;
            draggedItem.transform.SetParent(draggingItem.home.itemsParent, false);
            draggedItem.gameObject.SetActive(false);
        } else {
                    
            Color fade = new Color(1f, 1f, 1f, 0.65f);
            draggingItem.icon.color = fade;
            draggedItem = draggingItem;
        }
        hoveredInventory = draggedItem.home;
        ghostFlipped = draggedItem.flipped;
        GhostOrientation();
        ghostItem.gameObject.SetActive(true);
        Highlights(CellHovered());
    }

    void Update() {

        if (draggedItem != null) {
            ghostItem.transform.position = Input.mousePosition;
            Vector2Int checkHover = CellHovered();
            if (checkHover != curHoveredCell) {
                Highlights(checkHover);
                curHoveredCell = checkHover;
            }

            if (Input.GetMouseButtonUp(0)) {

                PlaceItem(checkHover);
            }
          
            if (Input.GetKeyDown(KeyCode.R)) {
                
                if (ghostFlipped) {
                    ghostFlipped = false;
                } else {
                    ghostFlipped = true;
                }
            }
        }
    }

    void Highlights(Vector2Int startCell) {

        ClearHighlights();
        if (hoveredInventory != null) {

            int color = 0;
            List<Vector2Int> cells = new List<Vector2Int>();
            for (int itemX = 0; itemX < ghostSize.x; itemX++) {

                for (int itemY = 0; itemY < ghostSize.y; itemY++) {
                    
                    cells.Add(new Vector2Int(startCell.x + itemX, startCell.y + itemY));
                }
            }

            foreach(Vector2Int cell in cells) {
           
                if (ItemHoverBool(cell, out GridItem itemHovered)) {
                    color = 2;
                    cells.Clear();

                    int x;
                    int y;
                    if (itemHovered.flipped) {
                        x = itemHovered.itemType.size.y;
                        y = itemHovered.itemType.size.x;
                    } else {
                        x = itemHovered.itemType.size.x;
                        y = itemHovered.itemType.size.y;
                    }

                    for (int itemX = 0; itemX < x; itemX++) {

                        for (int itemY = 0; itemY < y; itemY++) {
                            
                            cells.Add(new Vector2Int(itemHovered.pos.x + itemX, itemHovered.pos.y + itemY));
                        }
                    }
                    break;
                }

                if (!hoveredInventory.cells.ContainsKey(cell) || hoveredInventory.items.TryGetValue(cell, out GridItem value) && value != draggedItem) {
                    color = 0;
                    break;
                }

                color = 1;
            }

            foreach (Vector2Int cell in cells) {

                if (hoveredInventory.cells.TryGetValue(cell, out Image value)) {

                    switch (color) {

                        case 0 :
                            value.color = errorColor;                    
                        break;

                        case 1 :
                            value.color = succsColor;
                        break;

                        case 2 :
                            value.color = hoverColor;
                        break;
                    }
                    hoveredInventory.cells[cell].gameObject.SetActive(true);
                }
            }
            clearCells.AddRange(cells);
        }
    }

    void ClearHighlights() {
        
        foreach (Vector2Int cell in clearCells) {
            if (hoveredInventory.cells.TryGetValue(cell, out Image value)) {
                value.gameObject.SetActive(false);
            }
        }
        clearCells.Clear();
    }

    void PlaceItem(Vector2Int hoveredCell) {
       
        List<Vector2Int> tempCells = new List<Vector2Int>();
        GridItem itemHovered = null;
        switch (0) {

            case 0 :

                for (int itemX = 0; itemX < ghostSize.x; itemX++) {

                    for (int itemY = 0; itemY < ghostSize.y; itemY++) {
                                               
                        Vector2Int cell = new Vector2Int(hoveredCell.x + itemX, hoveredCell.y + itemY);

                        if (ItemHoverBool(cell, out itemHovered)) {
                            goto case 1;
                        }
                    
                        GridItem value = null;
                        if (hoveredInventory.cells.ContainsKey(cell) && !hoveredInventory.items.TryGetValue(cell, out value) || value == draggedItem) {
                            tempCells.Add(cell);
                        } else {
                            tempCells.Clear();
                            break;
                        }
                    }
                    if (tempCells.Count <= 0) break;
                }
                if (tempCells.Count > 0) goto case 2;
                if (originStack != null) {
                    Destroy(draggedItem.gameObject);
                    originStack.count += draggedItem.count;
                }
            break;

            case 1 :

                //Stack it
                List<Vector2Int> tempKeysOld = new List<Vector2Int>();
                foreach (KeyValuePair<Vector2Int, GridItem> entry in draggedItem.home.items) {
                    
                    if (entry.Value == draggedItem) {
                        tempKeysOld.Add(entry.Key);
                    }
                }
                foreach(Vector2Int vector in tempKeysOld) {draggedItem.home.items.Remove(vector);}

                itemHovered.count += draggedItem.count;
                Destroy(draggedItem.gameObject);
            break;

            case 2 :
                
                //Keys to remove since item is being moved to new location.
                List<Vector2Int> tempKeys = new List<Vector2Int>();
                foreach (KeyValuePair<Vector2Int, GridItem> entry in draggedItem.home.items) {
                    
                    if (entry.Value == draggedItem) {
                        tempKeys.Add(entry.Key);
                    }
                }

                foreach(Vector2Int vector in tempKeys) {draggedItem.home.items.Remove(vector);}
                if (hoveredInventory != draggedItem.home) {
                    
                    draggedItem.transform.SetParent(hoveredInventory.itemsParent, false);
                    draggedItem.home = hoveredInventory;
                }
                foreach(Vector2Int cell in tempCells) {draggedItem.home.items.Add(cell, draggedItem);}                       
                if (draggedItem.flipped != ghostFlipped) {
                    
                    if (ghostFlipped) {
                        draggedItem.flipped = ghostFlipped;
                        draggedItem.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize * draggedItem.itemType.size.y, cellSize * draggedItem.itemType.size.x);
                        draggedItem.transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, -90f);
                    } else {
                        draggedItem.flipped = ghostFlipped;
                        draggedItem.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize * draggedItem.itemType.size.x, cellSize * draggedItem.itemType.size.y);
                        draggedItem.transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                }
                draggedItem.transform.localPosition = new Vector2((cellSize * hoveredCell.x), -(cellSize * hoveredCell.y));
                draggedItem.pos = new Vector2Int(hoveredCell.x, hoveredCell.y);
            break;
        }
        ghostItem.gameObject.SetActive(false);
        if (draggedItem != null) {Color clear = new Color(1f, 1f, 1f, 1f); draggedItem.icon.color = clear; draggedItem.gameObject.SetActive(true);}
        ClearHighlights();
        draggedItem = null;
        originStack = null;
        hoveredInventory = null;
    }

    bool ItemHoverBool(Vector2Int cell, out GridItem itemHovered) {

        itemHovered = null;
        if (draggedItem.itemType.maxStackSize > 0 && hoveredInventory.items.TryGetValue(cell, out itemHovered) && itemHovered != draggedItem && itemHovered.itemType == draggedItem.itemType && (itemHovered.count + draggedItem.count) <= itemHovered.itemType.maxStackSize) {
            return true;
        }
        return false;
    }

    
    Vector2Int CellHovered() {
               
        Vector3[] corners = new Vector3[4];
        ghostItem.GetWorldCorners(corners);
        Vector2 topLeft = new Vector2(corners[1].x + (cellSize / 2), corners[1].y + -(cellSize / 2));

        //TODO: Optimize this since it triggers everytime they drag.
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = topLeft;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (RaycastResult result in results) {        
            
            if (result.gameObject.TryGetComponent(out GridInventory inv)) {
                hoveredInventory = inv;
                break;
            }
        }

        Vector2Int temp = new Vector2Int(-99,-99);

        if (hoveredInventory != null) {

            RectTransformUtility.ScreenPointToLocalPointInRectangle(hoveredInventory.cellParent, topLeft, null, out Vector2 localPos);
            
            int x = (int)((localPos.x / cellSize));
            int y = (int)((-(localPos.y / cellSize)));
            temp = new Vector2Int(x, y);
        }

        return temp;
    }

    public void GhostOrientation() {
        
        ghostItem.transform.GetChild(0).GetComponent<Image>().sprite = draggedItem.itemType.sprite;
        if (ghostFlipped) {
            ghostItem.transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, -90f);
            ghostItem.sizeDelta = new Vector2(cellSize * draggedItem.itemType.size.y, cellSize * draggedItem.itemType.size.x);
            ghostSize = new Vector2Int(draggedItem.itemType.size.y, draggedItem.itemType.size.x);
        } else {
            ghostItem.transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, 0f);
            ghostItem.sizeDelta = new Vector2(cellSize * draggedItem.itemType.size.x, cellSize * draggedItem.itemType.size.y);
            ghostSize = draggedItem.itemType.size;
        }
        Highlights(CellHovered());
    }

    public ItemData ConvertGridItemToData(GridItem gItem) {

        ItemData iData = new ItemData();
        iData.id = gItem.id;
        iData.idType = gItem.itemType.id;
        iData.xPos = gItem.pos.x;
        iData.yPos = gItem.pos.y;
        iData.flipped = gItem.flipped;
        iData.count = gItem.count;
        return iData;
    }
}
