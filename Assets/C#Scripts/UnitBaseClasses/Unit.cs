using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static GlobalData;
using static GlobalFunctions;
using static GlobalAnimationParser;
using System.Collections.Generic;

public class Unit : UnitBase, IUnit
{

    public int MovementTotal => UnitData.movementTotal + buffs.Sum(x => x.movementTotal);
    public int CaptureDamage => buffs.Sum(x => x.captureDamage);
    public bool Infiltrator => UnitData.infiltrator;
    public bool Rooted => buffs.Select(x => x.rooted).Contains(true);

    public bool SomewhereToMove
    {
        get
        {
            if (Rooted) { return false; }
            List<TileObject> tiles = new List<TileObject>();
            tileManager.AddNeighbors(Tile, this, tiles);
            foreach (TileObject tile in tiles)
            {
                if (MovementTotal >= terrainMoveCostMatrix[MovementType, tile.TerrainType]) { return true; }
            }
            return false;
        }
    }

    public override int CoverBonus => Tile.Building != null && BothLandOrSky(MovementType, Tile.Building.MovementType) ? terrainDefenseMatrix[MovementType, Tile.TerrainType] + Tile.Building.BuildingCover : terrainDefenseMatrix[MovementType, Tile.TerrainType];

    public override void Load(bool initial, Vector3Int localPlace, UnitBaseData data, int team)
    {
        MoveToTile(TileObject.TileAt(localPlace));
        base.Load(initial, localPlace, data, team);
        if (initial) { internalVariables.team = MapTeam(team); }
        else { Team = team; }
        playerManager.LoadPlayer(Team);
    }

    public override void MoveToTile(TileObject destination)
    {
        //Tile.Unit = null;
        destination.MoveUnitToTileFrom(Tile, this);
        base.MoveToTile(destination);
        //Tile.Unit = unit;
    }
    /*
    public IEnumerator DestroyUnit(UnitBase unit)
    {
        unit.Animate("Death");
        yield return StartCoroutine(WaitForAnimation(unit, "Death"));
        Destroy(unit.gameObject);
    }*/

    public override IEnumerator DestroyThis(IUnitBase killer)
    {
        //dead = true;
        EventsManager.InvokeOnDeathUnit(this);
        yield return base.DestroyThis(killer);
        EventsManager.InvokeOnObjectDestroyUnit(this);
        Destroy(gameObject);
    }

    public IEnumerator Capture(bool before, IBuilding building, int cDamage, CodeObject animationCode)
    {
        if (before)
        {
            EventsManager.InvokeOnBeforeCapture(this, Tile.Building);
        }
        else
        {
            if (animationCode != null)
            {
                yield return ParseAnimation(new StackItem(animationCode, this));
            }
            else
            {
                yield return PlayAnimationAndFinish("Capture");
            }
            int damage = (int)Math.Round(HPPercentage * (cDamage + CaptureDamage));
            building.TakeCaptureDamage(damage, this);
            EventsManager.InvokeOnCapture(this, Tile.Building);
        }
    }

    public void StartCoroutineQueuePath(List<Vector3Int> path)
    {
        StartCoroutine(SmoothLerp(path));
    }

    public void SetUpMovementTiles() => tileManager.SetUpMovementTiles(this);


    private IEnumerator WaitForAnimation(string s)
    {
        while (IsPlaying(s))
        {
            yield return null;
        }
        yield break;
    }

    private IEnumerator SmoothLerp(List<Vector3Int> path)
    {
        WaitForSeconds w = new WaitForSeconds(.01f);
        Pointer.globalInstance.haltInput = true;
        Animate("Run");
        foreach (Vector3Int v in path)
        {
            RotateTo(v);

            Vector3 finalPos = LocalToWorld(v);
            while (Vector3.Distance(transform.position, finalPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, finalPos, animationSpeed * Time.deltaTime);
                yield return w;
            }
            transform.position = finalPos;
        }
        Animate("Idle");
        Pointer.globalInstance.haltInput = false;
    }

}
