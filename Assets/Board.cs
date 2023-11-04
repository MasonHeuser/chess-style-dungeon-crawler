using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public class Board : MonoBehaviour
{   
    // singleton static reference
    private static Board assignAsset;
    public static Board data {
        get {
            if (assignAsset == null) {
                assignAsset = FindObjectOfType<Board>();
                if (assignAsset == null)
                    Debug.LogError("There is no Overlook in the scene!");
            }
            return assignAsset;
        }
    }

    public Grid parentGrid;

    public Dictionary<Vector3Int, TileData> Tiles = new Dictionary<Vector3Int, TileData>();
    
    [Serializable]
    public struct EleInteractionData {
        public TileStatusBase tStatusCompare1;
        public TileStatusBase tStatusCompare2;
        
        public TileStatusBase tileEffect;
    }
    public List<EleInteractionData> eleInteractionData = new List<EleInteractionData>();
    public Dictionary<TileData, TileStatusBase> interactionProcs = new Dictionary<TileData, TileStatusBase>();

    public int sizeX = 21;
    public int sizeY = 12;

    public int MaxSize {
        get {
            return sizeX * sizeY;
        }
    }

    public string globalSeed;
    public List<RoomData> rooms = new List<RoomData>();
    public RoomData curRoom;

    public Door doorPrefab;
    public Transform doorParent;

    public Tilemap tilemapGround;   
    public Tilemap tilemapStatus;
    public Tilemap tilemapHighlight;

    public Tileset tileSet;

    public Tile tileRed;
    public Tile tileGreen;
    public Tile tileBlue;
    public Tile tileYellow;
    public Tile statusTile;

    // Start is called before the first frame update
    void Awake()
    {   
        tileSet.DefineTiles();
        globalSeed = System.Guid.NewGuid().ToString().Replace("-", "");

        for (int i = 0; i < 4; i++) {
            Door door = Instantiate(doorPrefab);
            door.name = doorPrefab.name;
            door.transform.SetParent(doorParent, false);
            door.gameObject.SetActive(false);
        }

        GenerateRooms();
        curRoom = rooms[0];
        InitializeRoom();
    }

    void Update() {

        //TODO: Clean up debug here
        if (Input.GetKeyDown(KeyCode.L)) {
            GenerateBoard();
        }
    }
    
    void GenerateRooms() {

        System.Random rand = new System.Random(globalSeed.GetHashCode());
        Vector2Int[] posArray = 
        {new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0)};

        Vector2Int curPos = new Vector2Int(0, 0);
        int numberOfRooms = rand.Next(3, 10);
        int i = 0;
        while (i < numberOfRooms) {

            RoomData room = new RoomData();

            int sele = rand.Next(0, posArray.Count());
            Vector2Int temp = (curPos + posArray[sele]);
            RoomData tryRoom = rooms.Find(x => x.location == temp);
            if (tryRoom == null) {
                room.location = temp;
                room.seed = System.Guid.NewGuid().ToString().Replace("-", "");
                room.enemyCount = rand.Next(2, 8);
                rooms.Add(room);
                curPos = temp;
                i++;
            }
        }
    }

    public void InitializeRoom() {

        Overlook.data.turnCount = 0;
        RelationalData.data.UpdateText(Overlook.data.turnText, "Your Turn", Color.white);
        Overlook.data.turnCountText.transform.parent.gameObject.SetActive(true);
        Overlook.data.turnText.transform.parent.gameObject.SetActive(true);

        //If a room existed before, clean it up
        foreach(KeyValuePair<int, List<Unit>> entry in Overlook.data.teams) {

            foreach(Unit unit in entry.Value) {
                Destroy(unit.overhead.gameObject);
                Destroy(unit.gameObject);
            }
        }
        Overlook.data.teams.Clear();
        tilemapGround.ClearAllTiles();
        tilemapStatus.ClearAllTiles();      
        Tiles.Clear();
        Overlook.data.UpdateItemSaveCache();
        Overlook.data.contextActions.SetActive(false);
        Overlook.data.SetCameraPos();

        foreach (Transform child in CacheReference.asset.lootParent) {
            child.gameObject.SetActive(false);
        }

        if (curRoom.loot.Count > 0) {

            Overlook.data.SpawnLoot();
        }
        Overlook.data.teams.Add(0, new List<Unit>());
        Overlook.data.teams.Add(1, new List<Unit>());
        
        GenerateBoard();
        
        int allySpawn = 0;
        while (allySpawn < Overlook.data.saveCache.readyUnits.Count) {

            int randomEntry = UnityEngine.Random.Range(0, Board.data.Tiles.Count);
            TileData tile = Board.data.Tiles[Board.data.Tiles.ElementAt(randomEntry).Key];

            if (tile != null && tile.type == TileData.TileType.Walkable && tile.occupied == null) {

                GameObject g = Instantiate(CacheReference.asset.unitPrefab);
                Unit u = g.GetComponent<Unit>();
                g.transform.position = Board.data.parentGrid.GetCellCenterWorld(tile.tilePos);
                tile.occupied = u;
                u.curTile = tile;
              
                u.team = 0;
                u.unitType = RelationalData.data.unitObjects.Find( x => x.id==Overlook.data.saveCache.readyUnits[allySpawn].idType);
                u.attr = Overlook.data.saveCache.readyUnits[allySpawn];
                u.stats.health.value = Overlook.data.saveCache.readyUnits[allySpawn].health;
                Overlook.data.teams[Overlook.data.playerTeam].Add(u);
                u.unitType.Initilization(u);
                allySpawn++;
            }
        }

        if (curRoom.enemyCount <= 0) {
            Overlook.data.InitializeContextActions();
            return;
        }

        Control.data.activeTurn = true;
        Control.data.actionAble = true;
        Control.data.contextActions = false;

        //Enemy Spawn
        int enemySpawn = 0;
        while (enemySpawn < curRoom.enemyCount) {

            int randomEntry = UnityEngine.Random.Range(0, Board.data.Tiles.Count);
            TileData tile = Board.data.Tiles[Board.data.Tiles.ElementAt(randomEntry).Key];

            if (tile != null && tile.type == TileData.TileType.Walkable && tile.occupied == null) {
                
                GameObject g = Instantiate(CacheReference.asset.unitPrefab);
                Unit u = g.GetComponent<Unit>();
                g.transform.position = Board.data.parentGrid.GetCellCenterWorld(tile.tilePos);
                tile.occupied = u;
                u.curTile = tile;

                u.unitType = Resources.Load("Unit/Skeleton") as UnitBase;
                // u.unitType = RelationalData.data.unitObjects[Random.Range(0, RelationalData.data.unitObjects.Count)];
                u.team = 1;
                u.stats.armor.value = u.unitType.armor;
                u.stats.health.value = u.unitType.health;
                Overlook.data.teams[1].Add(u);
                u.unitType.Initilization(u);
                enemySpawn++;
            }
        }
        Overlook.data.UpdateEnemyAttackPos();
    }

    public IEnumerator MoveRooms(Vector2Int move) {

        RoomData tryRoom = rooms.Find(x => x.location == new Vector2Int(curRoom.location.x + move.x, curRoom.location.y + move.y));
        if (tryRoom != null) {

            yield return StartCoroutine(Overlook.data.FadeIn());
            Overlook.data.UpdateSaveCache();
            curRoom = tryRoom;
            InitializeRoom();
            Overlook.data.UpdateMap();
            yield return StartCoroutine(Overlook.data.FadeOut());          
        }
        yield break;
    }

    void PlaceDoors() {

        foreach(Transform child in doorParent) {
            child.gameObject.SetActive(false);
        }
        int i = 0;
        while (i < doorParent.childCount) {
                                       
            Door d = doorParent.transform.GetChild(i).GetComponent<Door>();
            switch (i) {

                case 0 :
                    
                    Vector2Int dirUp = new Vector2Int(Board.data.curRoom.location.x, Board.data.curRoom.location.y + 1);
                    RoomData tryRoomUp = Board.data.rooms.Find(x => x.location == dirUp);
                    if (tryRoomUp != null) {
                        
                        d.gameObject.SetActive(true);
                        d.dir = new Vector2Int(0, 1);
                        int xUp = (Board.data.sizeX / 2);
                        for (int y = Board.data.sizeY; y >= 0; y--) {
                            
                            Vector3Int check = new Vector3Int(xUp, y, 0);
                            if (tilemapGround.GetTile(check) != null) {
                                d.transform.position = parentGrid.GetCellCenterWorld(check);
                                d.transform.rotation = Quaternion.Euler(0, 0, 0);
                                break;
                            }
                        }                    
                    }
                break;

                case 1 :

                    Vector2Int dirDown = new Vector2Int(Board.data.curRoom.location.x, Board.data.curRoom.location.y + -1);
                    RoomData tryRoomDown = Board.data.rooms.Find(x => x.location == dirDown);
                    if (tryRoomDown != null) {

                        d.gameObject.SetActive(true);
                        d.dir = new Vector2Int(0, -1);
                        int xDown = (Board.data.sizeX / 2);
                        for (int y = 0; y < Board.data.sizeY; y++) {
                            
                            Vector3Int check = new Vector3Int(xDown, y, 0);
                            if (tilemapGround.GetTile(check) != null) {
                                d.transform.position = parentGrid.GetCellCenterWorld(check);
                                d.transform.rotation = Quaternion.Euler(0, 0, 180);
                                break;
                            }
                        }                                                     
                    }
                break;

                case 2 :

                    Vector2Int dirLeft = new Vector2Int(Board.data.curRoom.location.x + -1, Board.data.curRoom.location.y);
                    RoomData tryRoomLeft = Board.data.rooms.Find(x => x.location == dirLeft);
                    if (tryRoomLeft != null) {
                        
                        d.gameObject.SetActive(true);
                        d.dir = new Vector2Int(-1, 0);
                        int yLeft = (Board.data.sizeY / 2);
                        for (int x = 0; x < Board.data.sizeX; x++) {
                            
                            Vector3Int check = new Vector3Int(x, yLeft, 0);
                            if (tilemapGround.GetTile(check) != null) {
                                d.transform.position = parentGrid.GetCellCenterWorld(check);
                                d.transform.rotation = Quaternion.Euler(0, 0, 90);
                                break;
                            }
                        }               
                    }
                break;

                case 3:

                    Vector2Int dirRight = new Vector2Int(Board.data.curRoom.location.x + 1, Board.data.curRoom.location.y);
                    RoomData tryRoomRight = Board.data.rooms.Find(x => x.location == dirRight);
                    if (tryRoomRight != null) {

                        d.gameObject.SetActive(true);
                        d.dir = new Vector2Int(1, 0);
                        int yRight = (Board.data.sizeY / 2);
                        for (int x = Board.data.sizeX; x >= 0; x--) {
                            
                            Vector3Int check = new Vector3Int(x, yRight, 0);
                            if (tilemapGround.GetTile(check) != null) {
                                d.transform.position = parentGrid.GetCellCenterWorld(check);
                                d.transform.rotation = Quaternion.Euler(0, 0, -90);
                                break;
                            }
                        }                 
                    }
                break;
            }
            i++;
        }
    }

    void GenerateBoard() {

        tilemapGround.ClearAllTiles();
        Tiles.Clear();
        RandomFillMap();
        for (int i = 0; i < 5; i++) {
            SmoothMap();
        }
        VisualTiles();
        PlaceDoors();
    }

    void RandomFillMap() {

        System.Random rand = new System.Random(curRoom.seed.GetHashCode());

        for (int x = 0; x < sizeX; x++) {

            for (int y = 0; y < sizeY; y++) {
                
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileData tile = new TileData(pos);

                if (x == 0 || x == sizeX-1 || y == 0 || y == sizeY-1) {
                    tile.type = TileData.TileType.Wall;
                } else {
                    tile.type = (rand.Next(0, 100) > tileSet.randomFillPercent) ? TileData.TileType.Walkable : TileData.TileType.Wall;               
                    // tile.type = TileData.TileType.Walkable;
                }
                Tiles.Add(pos, tile);
            }
        }
    }
    
    void SmoothMap() {
        for (int x = 0; x < sizeX; x++) {

            for (int y = 0; y < sizeY; y++) {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4) {
                    Board.data.Tiles[new Vector3Int(x, y, 0)].type = TileData.TileType.Wall;
                } else if (neighbourWallTiles < 4) {
                    Board.data.Tiles[new Vector3Int(x, y, 0)].type = TileData.TileType.Walkable;
                }
            }
        }
    }

    int GetSurroundingWallCount(int x, int y) {
        int wallCount = 0;
        for (int neighbourX = x - 1; neighbourX <= x + 1; neighbourX++) {

            for (int neighbourY = y - 1; neighbourY <= y + 1; neighbourY++) {

                if (neighbourX != x || neighbourY != y) {

                    if (Tiles.TryGetValue(new Vector3Int(neighbourX, neighbourY, 0), out TileData t)) {

                        if (t.type == TileData.TileType.Wall) {
                            wallCount++;
                        }
                    } else {
                        wallCount++;
                    }
                }
            }
        }
        return wallCount;
    }

    bool Trim(int x, int y) {

        for (int neighbourX = x - 1; neighbourX <= x + 1; neighbourX++) {

            for (int neighbourY = y - 1; neighbourY <= y + 1; neighbourY++) {

                if (neighbourX != x || neighbourY != y) {

                    if (Board.data.Tiles.TryGetValue(new Vector3Int(neighbourX, neighbourY, 0), out TileData t) && t.type == TileData.TileType.Walkable) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void VisualTiles() {
        
        foreach(TileData tile in Tiles.Values) {

            if (tile.type == TileData.TileType.Walkable) {
                tilemapGround.SetTile(tile.tilePos, tileSet.floor);
            } else {
                tilemapGround.SetTile(tile.tilePos, tileSet.pillar);
            }

            if (!Trim(tile.tilePos.x, tile.tilePos.y)) {
                tilemapGround.SetTile(tile.tilePos, null);
            }  
        }

        foreach(TileData tile in Tiles.Values) {
            
            if (tile.type == TileData.TileType.Walkable) {
                tilemapGround.SetTile(tile.tilePos, tileSet.floor);
            }
        }

        foreach(TileData tile in Tiles.Values) {

            if (tilemapGround.GetTile(tile.tilePos) != null && tilemapGround.GetTile(tile.tilePos) != tileSet.floor) {

                string north = "False";
                Tile tempNorth = (Tile)tilemapGround.GetTile(new Vector3Int(tile.tilePos.x, tile.tilePos.y + 1, 0));
                if (tempNorth != null) {
                    north = "True";
                    if (tempNorth == tileSet.floor) {north += "-0";}
                }

                string south = "False";
                Tile tempSouth = (Tile)tilemapGround.GetTile(new Vector3Int(tile.tilePos.x, tile.tilePos.y + -1, 0));
                if (tempSouth != null) {
                    south = "True";
                    if (tempSouth == tileSet.floor) {south += "-0";}
                }

                string east = "False";
                Tile tempEast = (Tile)tilemapGround.GetTile(new Vector3Int(tile.tilePos.x + 1, tile.tilePos.y, 0));
                if (tempEast != null) {
                    east = "True";
                    if (tempEast == tileSet.floor) {east += "-0";}
                }

                string west = "False";
                Tile tempWest = (Tile)tilemapGround.GetTile(new Vector3Int(tile.tilePos.x + -1, tile.tilePos.y, 0));
                if (tempWest != null) {
                    west = "True";
                    if (tempWest == tileSet.floor) {west += "-0";}
                }

                //North_False_South_False_East_False_West_False
                string fileName = "North_" + north + "_South_" + south + "_East_" + east + "_West_" + west;
                Debug.Log(fileName + " | " + tile.tilePos);
                Tile tilePlace = tileSet.tileDefinition[fileName];
                tilemapGround.SetTile(tile.tilePos, tilePlace);
            }
        }
    }

    public void FindPath(Unit unitStart, Vector3 targetPos, out List<TileData> path) {

        TileData startTile = Board.data.Tiles[Board.data.parentGrid.WorldToCell(unitStart.transform.position)];
        TileData targetTile = Board.data.Tiles[Board.data.parentGrid.WorldToCell(targetPos)];

        Heap<TileData> openSet = new Heap<TileData>(Board.data.MaxSize);
        HashSet<TileData> closedSet = new HashSet<TileData>();
        path = null;

        openSet.Add(startTile);

        while (openSet.Count > 0) {

            TileData currentTile = openSet.RemoveFirst();
            // for (int i = 1; i < openSet.Count; i++) {
            
            //     if (openSet[i].fCost < startTile.fCost || openSet[i].fCost == currentTile.fCost && openSet[i].hCost < currentTile.hCost) {
            //         currentTile = openSet[i];
            //     }
            // }

            // openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            if (currentTile == targetTile) {
                path = RetracePath(startTile, targetTile);
                return;
            }

            foreach (TileData neighbour in GetNeighbours(currentTile)) {

                if (neighbour.type != TileData.TileType.Walkable || neighbour.occupied != null && neighbour != targetTile || closedSet.Contains(neighbour)) {
                    continue;
                }

                int newMovementCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour) + neighbour.movementPenalty;
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetTile);
                    neighbour.parent = currentTile;

                    if (!openSet.Contains(neighbour)) {
                        openSet.Add(neighbour);
                    } else {
                        openSet.UpdateItem(neighbour);
                    }
                }
            }
        }
    }

    // public void FindPathFree(Unit unitStart, Vector3 targetPos, out List<TileData> path) {

    //     TileData startTile = Control.data.GetTileData(unitStart.transform.position);
    //     TileData targetTile = Control.data.GetTileData(targetPos);

    //     Heap<TileData> openSet = new Heap<TileData>(Board.data.MaxSize);
    //     HashSet<TileData> closedSet = new HashSet<TileData>();
    //     path = null;

    //     openSet.Add(startTile);

    //     while (openSet.Count > 0) {

    //         TileData currentTile = openSet.RemoveFirst();
    //         closedSet.Add(currentTile);

    //         if (currentTile == targetTile) {
    //             path = RetracePath(startTile, targetTile);
    //             return;
    //         }

    //         foreach (TileData neighbour in GetNeighbours(currentTile)) { //unitStart.unitType.Pattern(currentTile.tilePos, unitStart.unitType.movementPattern)

    //             if (neighbour.type != TileData.TileType.Walkable && neighbour != targetTile || closedSet.Contains(neighbour)) {
    //                 continue;
    //             }

    //             int newMovementCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour) + neighbour.movementPenalty;
    //             if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
    //                 neighbour.gCost = newMovementCostToNeighbour;
    //                 neighbour.hCost = GetDistance(neighbour, targetTile);
    //                 neighbour.parent = currentTile;

    //                 if (!openSet.Contains(neighbour)) {
    //                     openSet.Add(neighbour);
    //                 } else {
    //                     openSet.UpdateItem(neighbour);
    //                 }
    //             }
    //         }
    //     }
    // }

    List<TileData> RetracePath(TileData startTile, TileData endTile) {

        List<TileData> path = new List<TileData>();
        TileData currentTile = endTile;

        while (currentTile != startTile) {
            path.Add(currentTile);
            currentTile = currentTile.parent;
        }
        path.Reverse();
        
        //Debugging
        // for (int i = 0; i < path.Count; i++) {
        //     Board.data.tilemapHighlight.SetTile(path[i].tilePos, Board.data.tileGreen);
        // }
        return path;
    }

    int GetDistance(TileData tileA, TileData tileB) {

        if (tileA != null && tileB != null) {
            int dstX = Mathf.Abs(tileA.tilePos.x - tileB.tilePos.x);
            int dstY = Mathf.Abs(tileA.tilePos.y - tileB.tilePos.y);

            if (dstX > dstY)
                return 14*dstY + 10 * (dstX - dstY);
            return 14*dstX + 10 * (dstY - dstX);
        } else {
            return 0;
        }
    }

    public List<TileData> GetNeighbours(TileData tile) {

        List<TileData> neighbours = new List<TileData>();
        
        int totalCount = 0;
        for (int x = -1; x <= 1; x++) {

            for (int y = -1; y <= 1; y++) {

                if (Overlook.data.TrueIfOdd(totalCount)) { //This allows the path to be only cross style.

                    if (x == 0 && y == 0)
                        continue;

                    int checkX = tile.tilePos.x + x;
                    int checkY = tile.tilePos.y + y;

                    if (checkX >= 0 && checkX < Board.data.sizeX && checkY >= 0 && checkY <  Board.data.sizeY) {

                        Vector3Int check = new Vector3Int(checkX, checkY, 0);
                        neighbours.Add(Board.data.Tiles[check]);
                    }
                }
                totalCount++;
            }
        }

        return neighbours;
    }
    
    public void InstanceTileStatusBoard(Unit src, TileStatusBase tileStatus, List<TileData> tiles) {

        foreach (TileData tile in tiles) {

            tileStatus.InstanceTileStatus(src, tile);
        }
        StartCoroutine(ElementalChainExecute());
    }

    public IEnumerator ElementalChainExecute() {

        if (interactionProcs.Count > 0) {

            // List<TileData> ignoreTiles = new List<TileData>();
            foreach(KeyValuePair<TileData, TileStatusBase> dataSet in interactionProcs) {

                // if (ignoreTiles.Contains(dataSet.Key)) {
                //     continue;
                // }

                Dictionary<TileData, bool> connectedTiles = new Dictionary<TileData, bool>();

                connectedTiles.Add(dataSet.Key, false);
                while (connectedTiles.ContainsValue(false)) {

                    TileData key = null;
                    foreach(KeyValuePair<TileData, bool> entry in connectedTiles)
                    {
                        if (!entry.Value) {
                            key = entry.Key;
                            break;
                        }
                    }

                    if (key != null) {

                        for (int x = -1; x <= 1; x++) {

                            for (int y = -1; y <= 1; y++) {

                                if (x == 0 && y == 0) {
                                    continue;
                                }

                                Vector3Int tilePosInt = new Vector3Int(key.tilePos.x + x, key.tilePos.y + y, 0);
                                TileData newEffectedTile = Board.data.Tiles[tilePosInt];

                                if (newEffectedTile != null && newEffectedTile.status != null && !connectedTiles.ContainsKey(newEffectedTile) && Board.data.CompareEleBackwards(dataSet.Value, newEffectedTile.status.statusType)) {
                                    connectedTiles.Add(newEffectedTile, false);
                                }
                            }
                        }
                    }
                    connectedTiles[key] = true;
                }

                //Check for overlap
                // foreach (var entry in interactionProcs) {

                //     if (connectedTiles.ContainsKey(entry.Key) && entry.Key != connectedTiles.First().Key) {
                //         ignoreTiles.Add(entry.Key);
                //     }
                // }

                foreach(KeyValuePair<TileData, bool> entry in connectedTiles)
                {   
                    // if (!ignoreTiles.Contains(entry.Key)) {
                        dataSet.Value.InstanceTileStatus(connectedTiles.First().Key.status.src, entry.Key);
                        yield return new WaitForSeconds(0.1f);
                    // }
                }
            }
            interactionProcs.Clear();
        }
    }

    public bool ElementalInteraction(TileStatusBase tStatus1, TileStatusBase tStatus2, out TileStatusBase interactionTileStatus) {

        interactionTileStatus = null;
        foreach (EleInteractionData ele in eleInteractionData) {

            if (tStatus1 == ele.tStatusCompare1 && tStatus2 == ele.tStatusCompare2 || tStatus2 == ele.tStatusCompare1 && tStatus1 == ele.tStatusCompare2) {

                interactionTileStatus = ele.tileEffect;
                return true;
            }
        }
        return false;
    }

    public bool CompareEleBackwards(TileStatusBase resultEle, TileStatusBase tileStatus) {

        List<EleInteractionData> listData = eleInteractionData.FindAll( v => v.tileEffect==resultEle);

        foreach (EleInteractionData ele in listData) {

            if (ele.tStatusCompare1 == tileStatus || ele.tStatusCompare2 == tileStatus) {

                return true;
            }
        }
        return false;
    }
}
