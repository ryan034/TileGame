using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GlobalData;
using static GlobalFunctions;

public class TileManager
{
    //private int maxX;
    //private int maxY;
    private static TileManager globalInstance;
    public static TileManager GlobalInstance
    {
        get { if (globalInstance == null) { globalInstance = new TileManager(); } return globalInstance; }
    }

    private Dictionary<Vector3Int, TileObject> tiles = new Dictionary<Vector3Int, TileObject>();
    private List<IUnitBase> unitBases = new List<IUnitBase>();

    public TileObject TileAt(Vector3Int currentLocation)
    {
        if (tiles.ContainsKey(currentLocation)) { return tiles[currentLocation]; }
        return null;
    }

    public bool VisibleAndHostileTo(int team, IUnitBase targetUnit)
    {
        TileObject tileOfTarget = targetUnit.Tile;
        int targetTerrain = tileOfTarget.TerrainType;
        bool seen = false;
        foreach (IUnitBase unitLooking in unitBases)
        {
            if (unitLooking.SameTeam(team))
            {
                TileObject tileOfLooker = unitLooking.Tile;
                int terrain = tileOfLooker.TerrainType;
                int movement = unitLooking.MovementType;
                //int vision = Manager.PlayerManager.IsDay ? TerrainVision(terrain, movement, unitLooking.DayVision) : TerrainVision(terrain, movement, unitLooking.NightVision);
                if (Distance(tileOfLooker.LocalPlace, tileOfTarget.LocalPlace) > unitLooking.Vision) { seen = false; }
                else if (CanSee(terrain, targetTerrain, movement)) { seen = !targetUnit.Invisible; if (seen) { return seen; } }
            }
        }
        return false;
    }

    public void RefreshFogOfWar(int team)
    {
        foreach (TileObject tile in tiles.Values)
        {
            tile.CanSee = false;
        }
        foreach (IUnitBase unit in unitBases)
        {
            if (unit.SameTeam(team)) { ApplySight(unit); }
        }
    }

    public void WipeTiles()
    {
        foreach (KeyValuePair<Vector3Int, TileObject> t in tiles) { t.Value.moveScore = int.MaxValue; t.Value.IsExplored = false; t.Value.exploredFrom = null; t.Value.IsTarget = false; }
    }

    public void EndAndStartNextTurn(int team)
    {
        foreach (IUnitBase unit in unitBases) { unit.Actioned = false; /*add other start of turn triggers here*/};
        RefreshFogOfWar(team);
    }

    public void AddTile(TileObject tileObject)
    {
        tiles[tileObject.LocalPlace] = tileObject;
    }

    //public IUnit SpawnUnit(Vector3Int v, string unitScript, int unitTeam) { if (tiles[v].Unit == null) { return Manager.AssetManager.InstantiateUnit(false, v, unitScript, unitTeam); } return null; }
    //newunit.teamcolour = teamcolours[unitteam];

    public void AddUnitBase(IUnitBase newUnit)
    {
        if (newUnit != null)
        {
            unitBases.Add(newUnit);
        }
    }

    public void DestroyUnitBase(IUnitBase unit)//destroyed unit sight still stays for a turn
    {
        unit.Tile.RemoveUnit();
        unitBases.Remove(unit);
        //RefreshTiles();
        //Trigger static event
        Pointer.globalInstance.ClearHeldUnit(unit);
        //Manager.UnitTransformManager.DestroyUnit(unit);
    }

    public void AddNeighbors(TileObject key, IUnit unit, List<TileObject> list_)
    {
        Vector3Int centre = key.LocalPlace;
        //List<TileObject> tilesList = new List<TileObject>();
        foreach (Vector3Int n in Neighbours(centre))
        {
            if (tiles.ContainsKey(n))
            {
                if (!tiles[n].IsExplored && (tiles[n].PercievedUnitOnTile == null || tiles[n].BlockingUnitOnTile == null)
                    && tiles[n].BlockingBuildingOnTile(unit) == null)
                {
                    list_.Add(tiles[n]);
                }
            }
        }
    }

    public List<Vector3Int> GetPath(TileObject endTile)
    {
        List<Vector3Int> path = new List<Vector3Int>() { endTile.LocalPlace };
        TileObject current = endTile;
        while (current.exploredFrom != null)
        {
            current = current.exploredFrom;
            path.Add(current.LocalPlace);
        };
        path.Reverse();

        for (int i = 0; i < path.Count(); i++)
        {
            if (tiles[path[i]].BlockingUnitOnTile != null) { return path.GetRange(0, i); }
        }
        return path;
    }

    public void SetUpMovementTiles(IUnit heldUnit)
    {
        Dictionary<TileObject, int> dist = new Dictionary<TileObject, int>();
        List<TileObject> tiles = new List<TileObject>();
        //use isexplored bool as vertex set q
        TileObject keyTile = heldUnit.Tile;
        keyTile.moveScore = 0;
        dist[keyTile] = keyTile.moveScore;
        KeyValuePair<TileObject, int> u = dist.OrderBy(kvp => kvp.Value).First();
        while (u.Value <= heldUnit.MovementTotal && dist.Count > 0)
        {
            AddNeighbors(u.Key, heldUnit, tiles);
            foreach (TileObject v in tiles)
            {
                if (!dist.ContainsKey(v))
                {
                    dist[v] = v.moveScore;
                }
                int alt = dist[u.Key] + terrainMoveCostMatrix[heldUnit.MovementType, v.TerrainType];
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    v.moveScore = alt;
                    v.exploredFrom = u.Key;
                }
            }
            tiles.Clear();
            u.Key.IsExplored = true;
            dist.Remove(u.Key);
            if (dist.Count > 0) { u = dist.OrderBy(kvp => kvp.Value).First(); }
        }
    }

    private void ApplySight(IUnitBase unit)
    {
        TileObject tileOf = unit.Tile;
        tileOf.CanSee = true;
        int terrain = tileOf.TerrainType;
        int movement = unit.MovementType;
        List<Vector3Int> offsets = CircleCoords(1, unit.Vision, tileOf.LocalPlace);
        foreach (Vector3Int v in offsets)
        {
            if (tiles.ContainsKey(v) && CanSee(terrain, tiles[v].TerrainType, movement))
            {
                tiles[v].CanSee = true;
            }
        }
    }

}
