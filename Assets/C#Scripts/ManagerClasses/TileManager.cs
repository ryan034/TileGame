using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GlobalData;
using static GlobalFunctions;

public class TileManager
{
    private Unit heldUnit;
    private Vector3 heldUnitForward;
    private Vector3Int heldLocation;
    private Vector3Int currentLocation;
    //private int maxX;
    //private int maxY;

    private Dictionary<Vector3Int, Tile> tiles = new Dictionary<Vector3Int, Tile>();
    private Dictionary<UnitBase, Vector3Int> unitBases = new Dictionary<UnitBase, Vector3Int>();
    private Vector3Int CurrentLocation { get; set; }
    private Tile SelectedTile => tiles[CurrentLocation];
    private bool FriendlyBuildingSelected => SelectedTile.building != null && !SelectedTile.building.Actioned && SelectedTile.building.SameTeam(Manager.PlayerManager.TeamTurn);
    private Tile HeldUnitTile => tiles[unitBases[heldUnit]];

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

    public Vector3 UpdateSelectedTile(Vector3Int vector3Int)
    {
        if (tiles.ContainsKey(vector3Int + CurrentLocation))
        {
            CurrentLocation = vector3Int + CurrentLocation;
        }
        return tiles[CurrentLocation].tile.transform.position;
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

    public bool VisibleAndHostileTo(int team, UnitBase targetUnit)
    {
        if (!targetUnit.SameTeam(team) && !targetUnit.Charming)
        {
            TileObject tileOfTarget = targetUnit.Tile;
            int targetTerrain = tileOfTarget.TerrainType;
            if (team == Manager.PlayerManager.TeamTurn) { return !targetUnit.Invisible && tileOfTarget.CanSee; };
            bool seen = false;
            foreach (UnitBase unitLooking in unitBases.Keys)
            {
                if (unitLooking.SameTeam(team))
                {
                    TileObject tileOfLooker = unitLooking.Tile;
                    int terrain = tileOfLooker.TerrainType;
                    int movement = unitLooking.MovementType;
                    int vision = Manager.PlayerManager.IsDay ? TerrainVision(terrain, movement, unitLooking.DayVision) : TerrainVision(terrain, movement, unitLooking.NightVision);
                    if (Distance(tileOfLooker.LocalPlace, tileOfTarget.LocalPlace) > vision) { seen = false; }
                    else if (CanSee(terrain, targetTerrain, movement)) { seen = !targetUnit.Invisible; if (seen) { return seen; } }
                }
            }
        }
        return false;
    }

    public bool AttackableAndHostileTo(UnitBase attacker, UnitBase defender, string attackType)
    {
        return attacker.CanHit(defender, attackType) && VisibleAndHostileTo(attacker.Team, defender) && defender.HPCurrent > 0;
    }

    public Building HostileVisibleBuildingOnTile(int team, Vector3Int v)
    {
        if (tiles[v].building != null && VisibleAndHostileTo(team, tiles[v].building)) { return tiles[v].building; };
        return null;
    }

    public Unit HostileVisibleUnitOnTile(int team, Vector3Int v)
    {
        if (tiles[v].unit != null && VisibleAndHostileTo(team, tiles[v].unit)) { return tiles[v].unit; };
        return null;
    }

    public UnitBase HostileAttackableUnitOrBuildingOnTile(UnitBase attacker, Vector3Int v, string attackType)
    {
        if (HostileAttackableUnitOnTile(attacker, v, attackType) != null)
        {
            return tiles[v].unit;
        }
        else if (HostileAttackableBuildingOnTile(attacker, v, attackType) != null)
        {
            return tiles[v].building;
        }
        return null;
    }

    public TileObject GetTile(UnitBase unit)
    {
        return tiles[unitBases[unit]].tile;
    }

    public Unit GetUnit(Vector3Int v)
    {
        if (tiles.ContainsKey(v))
        { return tiles[v].unit; }
        return null;
    }

    public Building GetBuilding(Vector3Int v)
    {
        if (tiles.ContainsKey(v))
        { return tiles[v].building; }
        return null;
    }

    public void GetMenuOptions(List<string> menu)
    {
        if (SelectedTile.unit != null && SelectedTile.unit.SameTeam(Manager.PlayerManager.TeamTurn) && !SelectedTile.unit.Actioned)
        {
            SelectedTile.unit.SetUp(menu);
            menu.Add(endUnitTurn);
        }
        else if (FriendlyBuildingSelected)
        {
            SelectedTile.building.SetUp(menu);
        }
        else
        {
            menu.Add(endTurn);
        }
    }

    public void SetHeldUnit()
    {
        if (SelectedTile.unit != null && SelectedTile.unit.SameTeam(Manager.PlayerManager.TeamTurn) && !SelectedTile.unit.Actioned && heldUnit == null)
        {
            heldUnit = SelectedTile.unit;
            heldUnitForward = heldUnit.transform.forward;
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
        if (s == endTurn)
        {
            //execute end turn
            Manager.PlayerManager.EndAndStartNextTurn();
        }
        else if (s == endUnitTurn)
        {
            //execute end unit turn
            EndHeldUnitTurn();
        }
        else
        {
            //execute based on if a unit is selected or building
            if (heldUnit != null)
            {
                heldUnit.ChooseMenuAbility(s);
            }
            else if (FriendlyBuildingSelected)
            {
                SelectedTile.building.ChooseMenuAbility(s);
            }
        }
    }

    public Vector3 RetractMove()
    {
        CurrentLocation = heldLocation;
        WipeTiles();
        heldUnit.transform.forward = heldUnitForward;
        heldUnit = null;
        return tiles[CurrentLocation].tile.transform.position;
    }

    public Vector3 RetractMoveFromWindow()
    {
        if (heldUnit != null)
        {
            if (SomewhereToMove)
            {
                HeldUnitTile.unit = null;
                tiles[heldLocation].unit = heldUnit;
                unitBases[heldUnit] = heldLocation;
                CurrentLocation = heldLocation;
                Manager.UnitTransformManager.SnapMove(heldUnit, CurrentLocation);
                SetUpMovementTiles();
                Pointer.globalInstance.GoToMovingMode();
            }
            else
            {
                heldUnit.transform.forward = heldUnitForward;
                heldUnit = null;
                Pointer.globalInstance.GoToOpenMode();
            }
        }
        else
        {
            Pointer.globalInstance.GoToOpenMode();
        }
        return tiles[CurrentLocation].tile.transform.position;
    }

    public Vector3 RetractTarget()
    {
        CurrentLocation = unitBases[heldUnit];
        WipeTiles();
        heldUnit.ClearTargets();
        return tiles[CurrentLocation].tile.transform.position;
    }

    public void CommitMove()
    {
        if (SelectedTile.tile.IsExplored && PercievedUnitOnTile(CurrentLocation, Manager.PlayerManager.TeamTurn) == null)
        {
            List<Vector3Int> path = GetPath(SelectedTile);
            HeldUnitTile.unit = null;
            tiles[path[path.Count - 1]].unit = heldUnit;
            Manager.UnitTransformManager.QueuePath(heldUnit, path);
            unitBases[heldUnit] = path[path.Count - 1];
            WipeTiles();
            if (SelectedTile == HeldUnitTile)
            {
                UIWindow.globalInstance.SpawnMenu();
            }
            else { EndHeldUnitTurn();  /*trigger for ambush attack*/}
        }
        else if (SelectedTile == HeldUnitTile)
        {
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

    public void RefreshFogOfWar(int team)
    {
        foreach (Tile tile in tiles.Values)
        {
            tile.tile.CanSee = false;
        }
        foreach (UnitBase unit in unitBases.Keys)
        {
            if (unit.SameTeam(team)) { ApplySight(unit); }
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

    public void EndAndStartNextTurn(int team)
    {
        foreach (UnitBase unit in unitBases.Keys) { unit.Actioned = false; /*add other start of turn triggers here*/};
        RefreshFogOfWar(team);
        Pointer.globalInstance.GoToOpenMode();
    }

    public void EndHeldUnitTurn()
    {
        if (heldUnit != null)
        {
            heldUnit.Actioned = true;
            RefreshFogOfWar(heldUnit.Team);
            heldUnit = null;
        }
        Pointer.globalInstance.GoToOpenMode();
    }

    public void EndUnitTurn(UnitBase unit)
    {
        unit.Actioned = true;
        RefreshFogOfWar(unit.Team);
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

    public Unit SpawnUnit(Vector3Int v, string unitScript, int unitTeam) => Manager.AssetManager.InstantiateUnit(false, v, unitScript, unitTeam);
    //newunit.teamcolour = teamcolours[unitteam];

    public void DestroyUnitBase(UnitBase unit)//destroyed unit sight still stays for a turn
    {
        tiles[unitBases[unit]].unit = null;
        unitBases.Remove(unit);
        //RefreshTiles();
        //Trigger static event
        if (heldUnit == unit) { heldUnit = null; };
        //Manager.UnitTransformManager.DestroyUnit(unit);
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
                if (heldUnit.MovementTotal >= terrainMoveCostMatrix[heldUnit.MovementType, tile.tile.TerrainType]/*and unit is not rooted*/) { return true; }
            }
            return false;
        }
    }

    /*
    private bool NoBlockingBuildingOnTile(Vector3Int v, Unit unit)
    {
        return tiles[v].building == null || ((tiles[v].building.Team == unit.Team || unit.Infiltrator) && !tiles[v].building.Neutral);
    }*/

    private Building BlockingBuildingOnTile(Vector3Int v, Unit unit)
    {
        if (tiles[v].building != null && ((!tiles[v].building.SameTeam(unit.Team) && !unit.Infiltrator) || tiles[v].building.Race == neutral)) { return tiles[v].building; }
        return null;
    }

    private Unit BlockingUnitOnTile(Vector3Int v, int team)
    {
        if (tiles[v].unit != null && !tiles[v].unit.SameTeam(team)) { return tiles[v].unit; }
        return null;
    }

    /*
    private bool NoPercievedUnitOnTile(Vector3Int n, int team)
    {
        if (tiles[n].unit == null)
        {
            return true;
        }
        else
        {
            return (tiles[n].unit.Invisible && tiles[n].unit.Team != team) || !tiles[n].tile.CanSee;
        }
    }
    */

    private Unit PercievedUnitOnTile(Vector3Int n, int team)
    {
        if (!tiles[n].tile.CanSee || tiles[n].unit == null || (tiles[n].unit.Invisible && !tiles[n].unit.SameTeam(team)))
        {
            return null;
        }
        return tiles[n].unit;
    }

    private void AddNeighbors(Tile key, Unit unit, List<Tile> list_)
    {
        Vector3Int centre = key.tile.LocalPlace;
        foreach (Vector3Int n in Neighbours(centre))
        {
            if (tiles.ContainsKey(n))
            {
                if (!tiles[n].tile.IsExplored && (PercievedUnitOnTile(n, Manager.PlayerManager.TeamTurn) == null || BlockingUnitOnTile(n, Manager.PlayerManager.TeamTurn) == null)
                    && BlockingBuildingOnTile(n, unit) == null)
                {
                    list_.Add(tiles[n]);
                }
            }
        }
    }

    private List<Vector3Int> GetPath(Tile endTile)
    {
        List<Vector3Int> path = new List<Vector3Int>() { endTile.tile.LocalPlace };
        TileObject current = endTile.tile;
        while (current.exploredFrom != null)
        {
            current = current.exploredFrom;
            path.Add(current.LocalPlace);
        };
        path.Reverse();

        for (int i = 0; i < path.Count(); i++)
        {
            if (BlockingUnitOnTile(path[i], Manager.PlayerManager.TeamTurn) != null) { return path.GetRange(0, i); }
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
                int alt = dist[u.Key] + terrainMoveCostMatrix[heldUnit.MovementType, v.tile.TerrainType];
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
        TileObject tileOf = unit.Tile;
        tileOf.CanSee = true;
        int terrain = tileOf.TerrainType;
        int movement = unit.MovementType;
        int vision = Manager.PlayerManager.IsDay ? TerrainVision(terrain, movement, unit.DayVision) : TerrainVision(terrain, movement, unit.NightVision);
        List<Vector3Int> offsets = CircleCoords(1, vision, tileOf.LocalPlace);
        foreach (Vector3Int v in offsets)
        {
            if (tiles.ContainsKey(v) && CanSee(terrain, tiles[v].tile.TerrainType, movement))
            {
                tiles[v].tile.CanSee = true;
            }
        }
    }

    private Building HostileAttackableBuildingOnTile(UnitBase attacker, Vector3Int v, string attackType)
    {
        //if (HostileVisibleBuildingOnTile(attacker.Team, v) != null && attacker.CanHit(tiles[v].building, attackType)) { return tiles[v].building; };
        if (tiles[v].building != null && AttackableAndHostileTo(attacker, tiles[v].building, attackType)) { return tiles[v].building; };
        return null;
    }

    private Unit HostileAttackableUnitOnTile(UnitBase attacker, Vector3Int v, string attackType)
    {
        //if (HostileVisibleUnitOnTile(attacker.Team, v) != null && attacker.CanHit(tiles[v].unit, attackType)) { return tiles[v].unit; };
        if (tiles[v].unit != null && AttackableAndHostileTo(attacker, tiles[v].unit, attackType)) { return tiles[v].unit; };
        return null;
    }

}
