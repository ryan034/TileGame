using static GlobalFunctions;
using UnityEngine;
using System.Collections.Generic;
using static GlobalData;
using System;

public class TileObject : MonoBehaviour
{
    protected static TileManager tileManager = TileManager.GlobalInstance;
    protected static PlayerManager playerManager = PlayerManager.GlobalInstance;

    public Vector3Int LocalPlace { get; private set; }
    //public int id; //index in prefablist

    public int TerrainType { get; private set; }//int to determine terrain type
    //public int SkyTerrainType { get; private set; } //int to determine terrain type

    public IUnit Unit { get; private set; }
    public IBuilding Building { get; private set; }

    public TileObject exploredFrom;
    public int moveScore = int.MaxValue;

    public bool IsTarget { get => isTarget; set { isTarget = value; RefreshSprite(); } } //alreadymoved and attacked
    private bool isTarget;

    public bool IsExplored { get => isExplored; set { isExplored = value; RefreshSprite(); } } //alreadymoved and attacked
    private bool isExplored;

    public bool CanSee { get => canSee; set { canSee = value; RefreshSprite(); } } //alreadymoved and attacked
    private bool canSee;

    public IUnit PercievedUnitOnTile
    {
        get
        {
            if (!CanSee || Unit == null || (Unit.Invisible && !Unit.SameTeam(playerManager.TeamTurn)))
            {
                return null;
            }
            return Unit;
        }
    }

    public IUnit BlockingUnitOnTile
    {
        get
        {
            if (Unit != null && !Unit.SameTeam(playerManager.TeamTurn)) { return Unit; }
            return null;
        }
    }

    public static void WipeTiles() => tileManager.WipeTiles();

    //public static void SetUpTargetTiles(List<TileObject> targetList) { foreach (TileObject t in targetList) { t.IsTarget = true; } }

    public static void RefreshFogOfWar(int team) => tileManager.RefreshFogOfWar(team);

    public static TileObject TileAt(Vector3Int currentLocation) => tileManager.TileAt(currentLocation);

    public static List<Vector3Int> GetPath(TileObject selectedTile) => tileManager.GetPath(selectedTile);

    public IBuilding HostileVisibleBuildingOnTile(int team)
    {
        if (Building != null && Building.VisibleAndHostileTo(team)) { return Building; };
        return null;
    }

    public IUnit HostileVisibleUnitOnTile(int team)
    {
        if (Unit != null && Unit.VisibleAndHostileTo(team)) { return Unit; };
        return null;
    }

    public IUnitBase HostileAttackableUnitOrBuildingOnTile(IUnitBase attacker, string attackType)
    {
        if (HostileAttackableUnitOnTile(attacker, attackType) != null)
        {
            return Unit;
        }
        if (HostileAttackableBuildingOnTile(attacker, attackType) != null)
        {
            return Building;
        }
        return null;
    }

    //public bool elevated;
    public void RefreshSprite()
    {
        //gameObject.GetComponent<Renderer>().material.color = new Color(255, 255, 255);

        float r = 1;
        float g = 1;
        float b = 1;
        float w = 1;

        if (IsExplored /*&& moveScore < Unit.MovementTotal*/)
        {
            //tile is yellow
            b = 0f;

        }
        else if (IsTarget)
        {
            g = 0f;
            b = 0f;
            //tile is red
        }
        if (!CanSee)
        {
            //dim tile
            w = 0.5f;
        }

        GetComponent<Renderer>().material.color = new Color(r * w, g * w, b * w);
        foreach (Transform child in transform)
        {
            child.GetComponent<Renderer>().material.color = new Color(r * w, g * w, b * w);
        }
        if (Unit != null) { Unit.RefreshSprite(); }
        if (Building != null) { Building.RefreshSprite(); }
    }

    public void Load(Vector3Int localPlace, int terrain)
    {
        LocalPlace = localPlace;
        //SkyTerrainType = skyTerrain;
        TerrainType = terrain;
        transform.position = LocalToWorld(LocalPlace);
        //if (tileManager == null) { tileManager = new TileManager(); }
        //if (playerManager == null) { playerManager = new PlayerManager(); }
        tileManager.AddTile(this);
        //gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load("MapSprites/"+ sprite) as Sprite;
    }

    public IBuilding BlockingBuildingOnTile(IUnit unit)
    {
        if (Building != null && ((!Building.SameTeam(unit.Team) && !unit.Infiltrator) || Building.Race == neutral)) { return Building; }
        return null;
    }

    public void MoveBuildingFromTileTo(TileObject tile, Building building)
    {
        if (tile != null)
        {
            tile.Building = null;
        }
        Building = building;
    }

    public void MoveUnitToTileFrom(TileObject tile, Unit unit)
    {
        if (tile != null)
        {
            tile.Unit = null;
        }
        Unit = unit;
    }

    public void RemoveUnit()
    {
        Unit = null;
    }

    private IBuilding HostileAttackableBuildingOnTile(IUnitBase attacker, string attackType)
    {
        //if (HostileVisibleBuildingOnTile(attacker.Team, v) != null && attacker.CanHit(tiles[v].building, attackType)) { return tiles[v].building; };
        if (Building != null && attacker.CanAttackAndHostileTo(Building, attackType)) { return Building; };
        return null;
    }

    private IUnit HostileAttackableUnitOnTile(IUnitBase attacker, string attackType)
    {
        //if (HostileVisibleUnitOnTile(attacker.Team, v) != null && attacker.CanHit(tiles[v].unit, attackType)) { return tiles[v].unit; };
        if (Unit != null && attacker.CanAttackAndHostileTo(Unit, attackType)) { return Unit; };
        return null;
    }
}
