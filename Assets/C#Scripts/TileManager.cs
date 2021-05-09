using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Globals;
using System.IO;

public class TileManager : MonoBehaviour
{
    public static TileManager globalInstance;

    private Unit heldUnit;
    private Vector3Int heldLocation;
    private Vector3Int currentLocation;
    //private int maxX;
    //private int maxY;
    private int turnCount;

    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    private Dictionary<Vector3Int, Tile> tiles = new Dictionary<Vector3Int, Tile>();
    private Dictionary<UnitBase, Vector3Int> unitBases = new Dictionary<UnitBase, Vector3Int>();
    private Vector3Int CurrentLocation { get { return currentLocation; } set { currentLocation = value; Pointer.globalInstance.UpdatePosition(tiles[CurrentLocation].tile.transform.position); } }
    private Tile SelectedTile => tiles[CurrentLocation];
    private bool FriendlyBuildingSelected => SelectedTile.building != null && !SelectedTile.building.Actioned && SelectedTile.building.Team == TeamTurn;
    private Tile HeldUnitTile => tiles[unitBases[heldUnit]];

    private int TeamsTotal => players.Count;
    public int TeamTurn => turnCount % TeamsTotal;

    public int GetClockFrame() => (turnCount / TeamsTotal) % clockTotal;
    public bool IsDay { get { if (GetClockFrame() / (clockTotal * 1.0) < 0.5) { return true; } else { return false; } } }

    private class Tile
    {
        public TileObject tile;
        public Unit unit;
        public Building building;

        public Tile(TileObject t)
        {
            tile = t;
        }
    }

    private class Player
    {
        public int wood;
        public int gold;
        public string race;
    }

    private void Awake()
    {
        if (globalInstance == null)
        {
            globalInstance = this;
        }
        else if (globalInstance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //GameObject myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "cursor")).LoadAsset<GameObject>("Cursor");
        //GameObject prefab = Instantiate(myLoadedAssetBundle, new Vector3(0, 0, 0), rotation);
        SpawnMap("bootybay");
        Pointer.globalInstance.Setup();
    }

    private void SpawnMap(string mapName)
    {
        int teams = AssetManager.globalInstance.SpawnMap(mapName);
        //set up player dictionary
        for (int i = 0; i < teams; i++)
        {
            players[i] = new Player();
        }
        RefreshFogOfWar();
    }

    private bool SomewhereToMove
    {
        get
        {
            if (heldUnit.Rooted) { return false; }
            List<Tile> tiles = new List<Tile>();
            AddNeighbors(HeldUnitTile, heldUnit, tiles);
            foreach (Tile tile in tiles)
            {
                if (heldUnit.MovementTotal >= terrainMatrix[heldUnit.MovementType, tile.tile.TerrainType]/*and unit is not rooted*/) { return true; }
            }
            return false;
        }
    }

    /*
    public bool CanCapture
    {
        get
        {
            Vector3Int v = unitbases[heldUnit];
            return tiles[v].building != null && tiles[v].building.Team != heldUnit.Team && !tiles[v].building.Invisible && !tiles[v].building.NotHostile;
        }
    }*/

    private bool NoBlockingBuildingOnTile(Vector3Int v, Unit unit)
    {
        return tiles[v].building == null || ((tiles[v].building.Team == unit.Team || unit.Infiltrator) && !tiles[v].building.Neutral);
    }

    private bool NoPercievedUnitOnTile(Vector3Int n)
    {
        if (tiles[n].unit == null)
        {
            return true;
        }
        else
        {
            return (tiles[n].unit.Invisible && tiles[n].unit.Team == TeamTurn) || !tiles[n].tile.CanSee;
        }
    }

    private void AddNeighbors(Tile key, Unit unit, List<Tile> list_)
    {
        Vector3Int centre = key.tile.LocalPlace;
        foreach (Vector3Int n in Neighbours(centre))
        {
            if (tiles.ContainsKey(n))
            {
                if (!tiles[n].tile.IsExplored && (NoPercievedUnitOnTile(n) || (tiles[n].unit != null && tiles[n].unit.Team == unit.Team))
                    && NoBlockingBuildingOnTile(n, unit))
                {
                    list_.Add(tiles[n]);
                }
            }
        }
    }

    private List<Vector3Int> GetPath(Tile selectedtile)
    {
        List<Vector3Int> path = new List<Vector3Int>() { selectedtile.tile.LocalPlace };
        TileObject current = selectedtile.tile;
        while (current.exploredFrom != null)
        {
            current = current.exploredFrom;
            path.Add(current.LocalPlace);
        };
        path.Reverse();

        for (int i = 0; i < path.Count(); i++)
        {
            if (tiles[path[i]].unit != null && tiles[path[i]].unit.Team != TeamTurn) { return path.GetRange(0, i); }
        }
        return path;
    }

    private void SetUpMovementTiles()
    {
        Dictionary<Tile, int> dist = new Dictionary<Tile, int>();
        List<Tile> tiles = new List<Tile>();
        //use isexplored bool as vertex set q
        Tile t = HeldUnitTile;
        t.tile.moveScore = 0;
        dist[t] = t.tile.moveScore;
        KeyValuePair<Tile, int> u = dist.OrderBy(kvp => kvp.Value).First();
        while (u.Value <= heldUnit.MovementTotal && dist.Count > 0)
        {
            AddNeighbors(u.Key, heldUnit, tiles);
            foreach (Tile v in tiles)
            {
                if (!dist.ContainsKey(v))
                {
                    dist[v] = v.tile.moveScore;
                }
                int alt = dist[u.Key] + terrainMatrix[heldUnit.MovementType, v.tile.TerrainType];
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    v.tile.moveScore = alt;
                    v.tile.exploredFrom = u.Key.tile;
                }
            }
            tiles.Clear();
            u.Key.tile.IsExplored = true;
            dist.Remove(u.Key);
            if (dist.Count > 0) { u = dist.OrderBy(kvp => kvp.Value).First(); }
        }
    }

    private void ApplySight(UnitBase unit)
    {
        TileObject tileOf = GetTile(unit);
        tileOf.CanSee = true;
        int vision;
        int terrain = tileOf.TerrainType;
        if (IsDay) { vision = unit.DayVision; }
        else { vision = unit.NightVision; }
        if (tileOf.TerrainType == 6 || (unit.MovementType != 6 || unit.MovementType != 7)) { vision += 3; }
        if (terrain == 4 || terrain == 8) { vision = 1; }
        List<Vector3Int> offsets = CircleCoords(1, vision, tileOf.LocalPlace);
        foreach (Vector3Int v in offsets)
        {
            if (tiles.ContainsKey(v))
            {
                if (unit.MovementType != 6 || unit.MovementType != 7)//ground unit
                {
                    if (terrain == 5 || terrain == 9/*highgrass sees highgrass ravines see ravines*/)
                    {
                        if (tiles[v].tile.TerrainType == terrain && tiles[v].tile.TerrainType != 6 /*&& tiles[v].elevated == unit.tile.elevated*/) { tiles[v].tile.CanSee = true; }
                    }
                    else if (terrain == 4 || terrain == 8/*woods/coves see neighbors that are not highgrass or ravines or mountains*/)
                    {
                        if (tiles[v].tile.TerrainType != 5 && tiles[v].tile.TerrainType != 9 && tiles[v].tile.TerrainType != 6 /*&& tiles[v].elevated == unit.tile.elevated*/) { tiles[v].tile.CanSee = true; }
                        //else if (tiles[v].tile.terraintype != 4 && tiles[v].tile.terraintype != 9/* && !tiles[v].elevated && unit.tile.elevated*/) { tiles[v].tile.cansee = true; }
                    }
                    else
                    {
                        if (tiles[v].tile.TerrainType != 5 && tiles[v].tile.TerrainType != 9 && tiles[v].tile.TerrainType != 6 && tiles[v].tile.TerrainType != 4 && tiles[v].tile.TerrainType != 8)
                        {
                            tiles[v].tile.CanSee = true;
                        }
                    }
                }
                else { if (terrain != 4 || terrain != 9 || terrain != 8) { tiles[v].tile.CanSee = true; } }
            }
        }
    }

    public void UpdateSelectedTile(Vector3Int vector3Int)
    {
        if (tiles.ContainsKey(vector3Int + CurrentLocation))
        {
            CurrentLocation = vector3Int + CurrentLocation;
            //Pointer.globalInstance.UpdatePosition(SelectedTileGlobalPosition);
            Debug.Log(CurrentLocation);
        }
    }

    public IEnumerable<Vector3Int> AddTargetTiles(int min, int max)
    {
        foreach (Vector3Int v in CircleCoords(min, max, unitBases[heldUnit]))
        {
            if (tiles.ContainsKey(v))
            {
                yield return v;
            }
        }
    }

    public bool WithinRange(int min, int max, UnitBase u, UnitBase targetUnit)
    {
        return Distance(unitBases[targetUnit], unitBases[u]) >= min && Distance(unitBases[targetUnit], unitBases[u]) <= max;
    }

    public bool VisibleTo(UnitBase unitLooking, UnitBase targetUnit)
    {
        if (unitLooking.Team == targetUnit.Team) { return true; }
        TileObject tileOfLooker = GetTile(unitLooking);
        TileObject tileOfTarget = GetTile(targetUnit);
        int vision;
        int terrain = tileOfLooker.TerrainType;
        int targetTerrain = tileOfTarget.TerrainType;
        if (IsDay) { vision = unitLooking.DayVision; }
        else { vision = unitLooking.NightVision; }
        if (terrain == 6 || (unitLooking.MovementType != 6 || unitLooking.MovementType != 7)) { vision += 3; }
        if (terrain == 4 || terrain == 8) { vision = 1; }
        if (Distance(tileOfLooker.LocalPlace, tileOfTarget.LocalPlace) > vision) { return false; }
        if (unitLooking.MovementType != 6 || unitLooking.MovementType != 7)//ground unit
        {
            if (terrain == 5 || terrain == 9/*highgrass sees highgrass ravines see ravines*/)
            {
                if (targetTerrain == terrain && targetTerrain != 6 /*&& tiles[v].elevated == unit.tile.elevated*/) { return !targetUnit.Invisible; }
            }
            else if (terrain == 4 || terrain == 8/*woods/coves see neighbors that are not highgrass or ravines or mountains*/)
            {
                if (targetTerrain != 5 && targetTerrain != 9 && targetTerrain != 6 /*&& tiles[v].elevated == unit.tile.elevated*/) { return !targetUnit.Invisible; }
            }
            else
            {
                if (targetTerrain != 5 && targetTerrain != 9 && targetTerrain != 6 && targetTerrain != 4 && targetTerrain != 8)
                {
                    return !targetUnit.Invisible;
                }
            }
        }
        else { if (terrain != 4 || terrain != 9 || terrain != 8) { return !targetUnit.Invisible; } }
        return false;
    }

    public bool HostileTo(UnitBase unit, UnitBase target)
    {
        return unit.Team != target.Team && !target.Charming;
    }

    public bool HostileVisibleBuildingOnTile(UnitBase u, Vector3Int v)
    {
        return tiles[v].building != null && HostileTo(u, tiles[v].building) && VisibleTo(u, tiles[v].building) /*&& u.CanHit(tiles[v].building)*/;
    }

    public bool HostileVisibleUnitOnTile(UnitBase u, Vector3Int v)
    {
        return tiles[v].unit != null && HostileTo(u, tiles[v].unit) && VisibleTo(u, tiles[v].unit) /*&& u.CanHit(tiles[v].building)*/;
    }

    public bool HostileAttackableBuildingOnTile(UnitBase u, Vector3Int v, string s)
    {
        return HostileVisibleBuildingOnTile(u, v) && u.CanHit(tiles[v].building, s);
    }

    public bool HostileAttackableUnitOnTile(UnitBase u, Vector3Int v, string s)
    {
        return HostileVisibleUnitOnTile(u, v) && u.CanHit(tiles[v].unit, s);
    }

    /*
    public Race GetRace(int team)
    {
        return players[team].race;
    }*/

    public TileObject GetTile(UnitBase unit)
    {
        return tiles[unitBases[unit]].tile;
    }

    public Unit GetUnit(Vector3Int v)
    {
        if (tiles.ContainsKey(v))
        { return tiles[v].unit; }
        else { return null; }
    }

    public Building GetBuilding(Vector3Int v)
    {
        if (tiles.ContainsKey(v))
        { return tiles[v].building; }
        else { return null; }
    }

    public void GetMenuOptions(List<string> menu)
    {
        if (SelectedTile.unit != null && SelectedTile.unit.Team == TeamTurn && !SelectedTile.unit.Actioned)
        {
            /*
            if (!selectedtile.unit.moved) { menu.Add("move"); myDelegatelist.Add(UIwindow.Setup_move); startingtile = selectedtile; heldunit = selectedtile.unit; }
            if (!selectedtile.unit.actioned) { selectedtile.unit.Setup(menu, myDelegatelist); }
            menu.Add("end unit turn"); myDelegatelist.Add(UIwindow.Endunitturn);
            */
            SelectedTile.unit.SetUp(menu);
            menu.Add("end unit turn");
        }
        else if (FriendlyBuildingSelected)
        {
            SelectedTile.building.SetUp(menu);
        }
        else
        {
            menu.Add("end turn");
        }
    }

    public void SetHeldUnit()
    {
        if (SelectedTile.unit != null && SelectedTile.unit.Team == TeamTurn && !SelectedTile.unit.Actioned && heldUnit == null)
        {
            heldUnit = SelectedTile.unit;
            heldLocation = unitBases[heldUnit];
            if (SomewhereToMove)
            {
                SetUpMovementTiles();
                Pointer.globalInstance.GoToMovingMode();
            }
            else { UIWindow.globalInstance.SpawnMenu(); }
        }
        else
        {
            UIWindow.globalInstance.SpawnMenu();
        }
    }

    public void Execute(string s)
    {
        if (s == "end turn")
        {
            //execute end turn
            EndTurn();
        }
        else if (s == "end unit turn")
        {
            //execute end unit turn
            EndUnitTurn();
        }
        else
        {
            //execute based on if a unit is selected or building
            if (heldUnit != null)
            {
                heldUnit.ExecuteChosenAbility(s);
            }
            else if (FriendlyBuildingSelected)
            {
                SelectedTile.building.ExecuteChosenAbility(s);
            }
        }
    }

    public void RetractMove()
    {
        CurrentLocation = heldLocation;
        //Pointer.globalInstance.UpdatePosition(SelectedTileGlobalPosition);
        WipeTiles();
        heldUnit = null;
    }

    public void RetractMoveFromWindow()
    {
        if (heldUnit != null)
        {
            if (SomewhereToMove)
            {
                HeldUnitTile.unit = null;
                tiles[heldLocation].unit = heldUnit;
                unitBases[heldUnit] = heldLocation;
                CurrentLocation = heldLocation;
                UnitTransformManager.globalInstance.SnapMove(heldUnit, CurrentLocation);

                //Execute("move");
                SetUpMovementTiles();
                Pointer.globalInstance.GoToMovingMode();

                //Pointer.globalInstance.UpdatePosition(SelectedTileGlobalPosition);
            }
            else
            {
                heldUnit = null;
                Pointer.globalInstance.GoToOpenMode();
            }
        }
        else
        {
            Pointer.globalInstance.GoToOpenMode();
            //heldUnit = null;
        }
    }

    public void RetractTarget()
    {
        //currentLocation = heldLocation;
        CurrentLocation = unitBases[heldUnit];
        //Pointer.globalInstance.UpdatePosition(SelectedTileGlobalPosition);
        WipeTiles();
        heldUnit.ClearTargets();
    }

    public void CommitMove()
    {
        if (SelectedTile.tile.IsExplored && NoPercievedUnitOnTile(CurrentLocation))//and unit not invisible)
                                                                                   // (heldunit.infiltrator || selectedtile.building == null || (selectedtile.building.team == heldunit.team && selectedtile.building.currenthp > 0 && !selectedtile.building.neutral))
        {
            Debug.Log("move");
            List<Vector3Int> path = GetPath(SelectedTile);
            //heldunit.tile.unit = null;
            //heldunit.tile = path[path.Count - 1];
            HeldUnitTile.unit = null;
            tiles[path[path.Count - 1]].unit = heldUnit;
            UnitTransformManager.globalInstance.QueuePath(heldUnit, path);
            unitBases[heldUnit] = path[path.Count - 1];
            //path[path.Count - 1].unit = heldunit;
            //heldunit.moved = true;
            WipeTiles();
            if (SelectedTile == HeldUnitTile)
            {
                UIWindow.globalInstance.SpawnMenu();
            }
            else { EndUnitTurn();  /*trigger for ambush attack*/}
        }
        else if (SelectedTile == HeldUnitTile)
        {
            //heldunit.moved = true;
            WipeTiles();
            UIWindow.globalInstance.SpawnMenu();
        }
    }

    public void CommitTarget()
    {
        if (SelectedTile.tile.IsTarget)
        {
            heldUnit.CommitTarget(CurrentLocation);
        }
    }

    /*
    public void Setup_back(Tile startingtile, Unit heldunit)
    {
        heldunit.tile.unit = null;
        heldunit.tile = startingtile;
        heldunit.Snapmove();
        startingtile.unit = heldunit;
        heldunit.moved = false;
        heldunit.actioned = false;
        WipeTiles();
    }
    */

    public void RefreshFogOfWar()
    {
        foreach (Tile tile in tiles.Values)
        {
            tile.tile.CanSee = false;
        }
        foreach (UnitBase unit in unitBases.Keys)
        {
            if (unit.Team == TeamTurn) { ApplySight(unit); }
        }
    }

    public void WipeTiles()
    {
        foreach (KeyValuePair<Vector3Int, Tile> t in tiles) { t.Value.tile.moveScore = int.MaxValue; t.Value.tile.IsExplored = false; t.Value.tile.exploredFrom = null; t.Value.tile.IsTarget = false; }
    }

    public void SetUpTargetTiles(List<Vector3Int> targetList)
    {
        foreach (Vector3Int t in targetList) { tiles[t].tile.IsTarget = true; }
    }

    public UnitBase GetUnitOrBuilding(Vector3Int v)
    {
        if (tiles[v].unit != null)
        {
            return tiles[v].unit;
        }
        else if (tiles[v].building != null)
        {
            return tiles[v].building;
        }
        return null;
    }

    public void EndTurn()
    {
        turnCount = turnCount + 1;
        foreach (UnitBase unit in unitBases.Keys) { unit.Actioned = false; /*add other start of turn triggers here*/};
        //foreach (Building building in buildings.Keys) { building.Actioned = false; /*add other start of turn triggers here*/};
        RefreshFogOfWar();
        Pointer.globalInstance.GoToOpenMode();
    }

    public void EndUnitTurn()
    {
        if (heldUnit != null)
        {
            heldUnit.Actioned = true;
            heldUnit = null;
        }
        RefreshFogOfWar();
        Pointer.globalInstance.GoToOpenMode();
    }

    public void AddTile(Vector3Int localPlace, TileObject tileObject)
    {
        tiles[localPlace] = new Tile(tileObject);
    }

    public void AddBuilding(Building newBuilding, Vector3Int v)
    {
        tiles[v].building = newBuilding;
        unitBases[newBuilding] = v;
    }

    public void AddUnit(Unit newUnit, Vector3Int v)
    {
        if (newUnit != null)
        {
            tiles[v].unit = newUnit;
            unitBases[newUnit] = v;
        }
    }

    public Unit SpawnUnit(Vector3Int v, string unitScript, int unitTeam) => AssetManager.globalInstance.InstantiateUnit(v, unitScript, unitTeam);
    //newunit.teamcolour = teamcolours[unitteam];
    //load in the details from the script
    //newUnit.Load(v, unitScript, unitTeam);

    public void DestroyUnit(UnitBase unit)//todo
    {
        tiles[unitBases[unit]].unit = null;
        unitBases.Remove(unit);
        //RefreshTiles();
        //Trigger static event
        if (heldUnit == unit) { heldUnit = null; };
        UnitTransformManager.globalInstance.DestroyUnit(unit);
    }
}
