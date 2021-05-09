/*using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class MapCanvas : MonoBehaviour
{
    public static MapCanvas globalInstance;
    //public List<Tileobject> mytileprefabs;
    //public List<Unit> myunitprefabs;
    //public List<Building> mybuildingprefabs;
    public int totalteams;
    int max_x;
    int max_y;
    List<int> teamcolours;
    Dictionary<Vector3Int, TileObject> tiles;

    void Awake()
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

    void Start()
    {
        tiles = new Dictionary<Vector3Int, TileObject>();
        teamcolours=Enumerable.Range(0, totalteams).ToList();
    }

    public bool Withinbounds(Vector3Int vector3Int)
    {
        return (Math.Abs(vector3Int.x) <= max_x && Math.Abs(vector3Int.y) <= max_y);
    }

    public void Erase(Vector3Int localplace, int brush_colour, int brush_colour_team)
    {
        if (tiles.ContainsKey(localplace))
        {
            TileObject tile = tiles[localplace];
            if (brush_colour < mytileprefabs.Count)
            {
                if (tile.id == mytileprefabs[brush_colour].id)
                {
                    if (tile.unit != null) { Destroy(tile.unit.gameObject); }
                    if (tile.building != null) { Destroy(tile.building.gameObject); }
                    tiles.Remove(tile.LocalPlace);
                    Destroy(tile.gameObject);
                }
            }
            else if (brush_colour < mytileprefabs.Count + mybuildingprefabs.Count)
            {
                if (tile.building != null)
                {
                    if (tile.building.id == mybuildingprefabs[brush_colour - mytileprefabs.Count].Id)
                    {
                        Destroy(tile.building.gameObject);
                        tile.building = null;
                    }
                }
            }
            else
            {
                if (tile.unit != null)
                {
                    if (tile.unit.unittags[0] == myunitprefabs[brush_colour - mytileprefabs.Count - mybuildingprefabs.Count].unitTags[0])
                    {
                        Destroy(tile.unit.gameObject);
                        tile.unit = null;
                    }
                }
            }
        }
    }

    public void Spawn(Vector3Int localplace, int brush_colour, int brush_colour_team)
    {
        if (Withinbounds(localplace))
        {
            if (brush_colour < mytileprefabs.Count)
            {
                TileObject spawnedtile = Instantiate(mytileprefabs[brush_colour], new Vector3(0, 0, 0), Quaternion.identity);
                spawnedtile.localplace = localplace;
                spawnedtile.CanSee = true;
                spawnedtile.RefreshSprite();
                if (tiles.ContainsKey(localplace)) { Destroy(tiles[localplace].gameObject); }
                tiles[localplace] = spawnedtile;
            }
            else if (brush_colour < mytileprefabs.Count + mybuildingprefabs.Count)
            {
                if (tiles.ContainsKey(localplace))
                {
                    Building spawnedbuilding = Instantiate(mybuildingprefabs[brush_colour - mytileprefabs.Count]);
                    spawnedbuilding.Team = brush_colour_team;
                    spawnedbuilding.tile = tiles[localplace];
                    if (tiles[localplace].building != null) { Destroy(tiles[localplace].building.gameObject); }
                    tiles[localplace].building = spawnedbuilding;
                    spawnedbuilding.Refreshsprite();
                }
            }
            else
            {
                if (tiles.ContainsKey(localplace))
                {
                    Unit spawnedunit = Instantiate(myunitprefabs[brush_colour - mytileprefabs.Count - mybuildingprefabs.Count]);
                    spawnedunit.Team = brush_colour_team;
                    spawnedunit.tile = tiles[localplace];
                    if (tiles[localplace].unit != null) { Destroy(tiles[localplace].unit.gameObject); }
                    tiles[localplace].unit = spawnedunit;
                }
            }
        }
    }

    public void Spawnmap()
    {
        Debug.Log("map loaded");
        foreach(KeyValuePair<Vector3Int, TileObject> t in tiles)
        {
            if (t.Value.unit != null) { Destroy(t.Value.unit.gameObject); }
            if (t.Value.building != null) { Destroy(t.Value.building.gameObject); }
            Destroy(t.Value.gameObject);
        }
        tiles = new Dictionary<Vector3Int, TileObject>();
        MapXml map = MapXml.Load(Application.dataPath + "/Resources/Maps/" + "map1" + ".xml");
        teamcolours = map.teamsColours;
        totalteams = map.teamsTotal;
        foreach (MapXml.TileInfo t in map.tilesInfo)
        {
            if (Withinbounds(t.localPlace))
            {
                TileObject spawnedtile = Instantiate(mytileprefabs[t.tile], new Vector3(0, 0, 0), Quaternion.identity);
                spawnedtile.localplace = t.localPlace;
                spawnedtile.CanSee = true;
                spawnedtile.SkyTerrainType = t.skyTerrain;
                tiles.Add(t.localPlace, spawnedtile);
                Spawnbuilding(spawnedtile, t.buildingScript, t.buildingTeam);
                Spawnunit(spawnedtile, t.unitScript, t.unitTeam);
            }
        }
    }

    public void Savemap()
    {
        Debug.Log("map saved");
        MapXml map = new MapXml
        {
            teamsTotal = totalteams,
            teamsColours = teamcolours,
            tilesInfo = new List<MapXml.TileInfo>(),
            maxX = max_x,
            maxY=max_y,
        };
        foreach (KeyValuePair<Vector3Int, TileObject> t in tiles)
        {
            int x1 = 0, x2 = 0, x3 = 0, x4 = 0;
            if (t.Value.unit != null) { x3 = Int32.Parse(t.Value.unit.unittags[1]); x4 = t.Value.unit.team; }
            if (t.Value.building != null) { x1 = t.Value.building.id; x2 = t.Value.building.team; }
            //Vector3Int localplace, int tile,int skyterrain,int building,int buildingteam,int unit,int unitteam
            MapXml.TileInfo tileinfo = new MapXml.TileInfo
            {
                localPlace = t.Key,
                tile = t.Value.id,
                skyTerrain = 0,
                buildingScript = x1,
                buildingTeam = x2,
                unitScript = x3,
                unitTeam = x4
            };
            map.tilesInfo.Add(tileinfo);
        }
        map.Save(Application.dataPath + "/Resources/Maps/" + "map1" + ".xml");
    }

    public void Refreshperspective(Vector3Int key)
    {
        foreach (KeyValuePair<Vector3Int, TileObject> t in tiles)
        {
            t.Value.Refreshperspective(key);
        }
    }

    public int Getteamcolour(int team)
    {
        return teamcolours[team];
    }

    private void Spawnbuilding(TileObject tile, int building, int buildingteam)
    {
        if (building > 0)
        {
            Building newbuilding = Instantiate(mybuildingprefabs[building - 1]);
            newbuilding.Team = buildingteam;//-1 means neutral
            newbuilding.tile = tile;
            tile.building = newbuilding;
        }
    }

    public void Spawnunit(TileObject tile, int unit, int unitteam)
    {
        if (unit > 0)
        {
            Unit newunit = Instantiate(myunitprefabs[unit - 1]);
            newunit.Team = unitteam;//-1 means neutral
            newunit.teamcolour = teamcolours[unitteam];
            newunit.tile = tile;
            tile.unit = newunit;
        }
    }
}
*/