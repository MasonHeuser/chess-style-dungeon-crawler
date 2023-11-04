using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridInventory : MonoBehaviour
{   
    public RectTransform title;
    public RectTransform scrollBar;
    public Dictionary<Vector2Int, GridItem> items = new Dictionary<Vector2Int, GridItem>();

    public RectTransform cellParent;

    public int xSize;
    public int ySize;
    public Dictionary<Vector2Int, Image> cells = new Dictionary<Vector2Int, Image>();

    public RectTransform itemsParent;

    public RectTransform sizingRect;

    public void CreateInventory() {

        //Sizing
        int cellSize = InventoryControl.data.cellSize;
        Vector2 totalSize = new Vector2(cellSize * xSize, cellSize * ySize);

        title.sizeDelta = new Vector2(totalSize.x, title.sizeDelta.y);

        int maxYSize = cellSize * 8;
        if (totalSize.y > maxYSize) {
            sizingRect.sizeDelta = new Vector2(totalSize.x, maxYSize);
            scrollBar.sizeDelta = new Vector2(scrollBar.sizeDelta.x, maxYSize);
        } else {
            sizingRect.sizeDelta = totalSize;
            scrollBar.sizeDelta = new Vector2(scrollBar.sizeDelta.x, totalSize.y);
        }
        cellParent.sizeDelta = totalSize;

        for (int x = 0; x < xSize; x++) {

            for (int y = 0; y < ySize; y++) {
                
                GameObject t = Instantiate(cellParent.GetChild(0).gameObject);
                t.transform.SetParent(cellParent, false);
                t.name = x + ":" + y;
                t.transform.localPosition = new Vector2((cellSize * x), -(cellSize * y));
                t.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize, cellSize);
                cells.Add(new Vector2Int(x, y), t.transform.GetChild(0).GetComponent<Image>());
                t.SetActive(true);

                float r = Random.Range(0.0f, 1.0f);
                t.GetComponent<Image>().color = new Color(r, r, r);
            }
        }
        Destroy(cellParent.GetChild(0).gameObject);
        itemsParent.SetAsLastSibling();
    }

    public void AddItem(ItemBase item) {

        if (item.maxStackSize > 0) {

            foreach(var value in items.Values.Distinct()) {
                
                if (value.itemType == item && value.count < item.maxStackSize) {

                    value.count++;
                    return;
                }
            }
        }

        int cellSize = InventoryControl.data.cellSize;
        List<Vector2Int> tempCells = new List<Vector2Int>();
        int xCount;
        int yCount;
        bool flippedTest = false;

        switch (0) {

            case 0 :

                for (int y = 0; y < ySize; y++) {

                    for (int x = 0; x < xSize; x++) {
                        
                        for (int f = 0; f < 2; f++) {

                            if (flippedTest) {
                                xCount = item.size.y;
                                yCount = item.size.x;
                            } else {
                                xCount = item.size.x;
                                yCount = item.size.y;
                            }

                            for (int itemX = 0; itemX < xCount; itemX++) {

                                for (int itemY = 0; itemY < yCount; itemY++) {

                                    Vector2Int cell = new Vector2Int(x + itemX, y + itemY);
                                    if (cells.ContainsKey(cell) && !items.ContainsKey(cell)) {
                                        tempCells.Add(cell);
                                    } else {
                                        tempCells.Clear();
                                        break;
                                    }
                                }
                                if (tempCells.Count <= 0) break;
                            }
                            if (tempCells.Count > 0) goto case 1;

                            flippedTest = !flippedTest;
                        }
                    }
                }
                Debug.Log("Couldn't Place");
            break;

            case 1 :

                GridItem i = Instantiate(InventoryControl.data.gridItem);
                i.transform.SetParent(itemsParent, false);
                i.id = System.Guid.NewGuid().ToString();
                i.itemType = item;
                i.home = this;
                i.pos = new Vector2Int(tempCells[0].x, tempCells[0].y);
                i.flipped = flippedTest;
                i.count = 1;
                i.icon.sprite = item.sprite;
                i.title.text = item.title;
                i.gameObject.name = item.title;
                if (i.flipped) {
                    i.transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, -90f);
                    i.transform.localPosition = new Vector2((cellSize * tempCells[0].x), -(cellSize * tempCells[0].y));
                    i.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize * item.size.y, cellSize * item.size.x);
                } else {
                    i.transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, 0f);
                    i.transform.localPosition = new Vector2((cellSize * tempCells[0].x), -(cellSize * tempCells[0].y));
                    i.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize * item.size.x, cellSize * item.size.y);
                }
                i.gameObject.SetActive(true);

                foreach(Vector2Int cell in tempCells) {
                    items.Add(cell, i);
                }
            break;
        }
    }

    public bool AddItemSpecify(ItemData iData, Vector2Int pos) {

        ItemBase item = RelationalData.data.itemObjects.Find( x => x.id == iData.idType);

        if (item.maxStackSize > 0) {

            foreach(var value in items.Values.Distinct()) {
                
                if (value.itemType == item && value.count < item.maxStackSize) {

                    value.count++;
                    return true;
                }
            }
        }
        
        int cellSize = InventoryControl.data.cellSize;
        List<Vector2Int> tempCells = new List<Vector2Int>();
        int xCount;
        int yCount;
        bool flippedTest = iData.flipped;

        switch (0) {

            case 0 :

                for (int f = 0; f < 2; f++) {

                    if (flippedTest) {
                        xCount = item.size.y;
                        yCount = item.size.x;
                    } else {
                        xCount = item.size.x;
                        yCount = item.size.y;
                    }

                    for (int itemX = 0; itemX < xCount; itemX++) {

                        for (int itemY = 0; itemY < yCount; itemY++) {

                            Vector2Int cell = new Vector2Int(pos.x + itemX, pos.y + itemY);
                            if (cells.ContainsKey(cell) && !items.ContainsKey(cell)) {
                                tempCells.Add(cell);
                            } else {
                                tempCells.Clear();
                                break;
                            }
                        }
                        if (tempCells.Count <= 0) break;
                    }
                    if (tempCells.Count > 0) goto case 1;

                    flippedTest = !flippedTest;
                }
            Debug.Log("Couldn't Place Specific");
            AddItem(RelationalData.data.itemObjects.Find(x => x.id == iData.idType));
            return false;


            case 1 :

                GridItem i = Instantiate(InventoryControl.data.gridItem);
                i.transform.SetParent(itemsParent, false);
                i.id = iData.id;
                i.itemType = item;
                i.home = this;
                i.pos = new Vector2Int(pos.x, pos.y);
                i.flipped = flippedTest;
                i.count = iData.count;
                i.icon.sprite = item.sprite;
                i.title.text = item.title;
                i.gameObject.name = item.title;
                if (i.flipped) {
                    i.transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, -90f);
                    i.transform.localPosition = new Vector2((cellSize * pos.x), -(cellSize * pos.y));
                    i.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize * item.size.y, cellSize * item.size.x);
                } else {
                    i.transform.GetChild(0).rotation = Quaternion.Euler(0f, 0f, 0f);
                    i.transform.localPosition = new Vector2((cellSize * pos.x), -(cellSize * pos.y));
                    i.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize * item.size.x, cellSize * item.size.y);
                }
                i.gameObject.SetActive(true);

                foreach(Vector2Int cell in tempCells) {
                    items.Add(cell, i);
                }
            return true;
        }
    }
}
