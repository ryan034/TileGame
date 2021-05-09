using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Globals;

public class Unit : UnitBase
{

    //public bool infiltrator;// can occupy non allied buildings or not
    //public bool moved;
    //public bool invisible;
    public int MovementTotal => data.movementTotal + buffs.Sum(x => x.movementTotal);
    public int CaptureRate => data.captureRate + buffs.Sum(x => x.captureRate);
    public bool Infiltrator => data.infiltrator;
    public bool Rooted => buffs.Select(x => x.rooted).Contains(true);
    //protected override TileObject Tile => TileManager.globalInstance.GetTile(this);

    public override int CoverBonus
    {
        get
        {
            //returns total resistance of damage increase factoring all stats (eg. armour, armourtype, terrain, building), as percentage
            //returns 1 for now
            int t;
            if (MovementType == 6 || MovementType == 7)
            {
                t = skyDefenseMatrix[MovementType, Tile.SkyTerrainType]/*flying building armour*/;
                if (Tile.Building != null && (Tile.Building.MovementType == 6 || Tile.Building.MovementType == 7)) { t = t + Tile.Building.BuildingCover; }
            }
            else
            {
                t = terrainDefenseMatrix[MovementType, Tile.TerrainType]/*building armour*/;
                if (Tile.Building != null && (Tile.Building.MovementType != 6 || Tile.Building.MovementType != 7)) { t = t + Tile.Building.BuildingCover; }
            }
            return t;
        }
    }


    public override void Load(Vector3Int localPlace, UnitBaseData data, int team)
    {
        base.Load(localPlace, data, team);
        Team = team;
        TileManager.globalInstance.AddUnit(this, localPlace);
    }

    protected override void AddToMenu(string s, List<string> menu)
    {
        //parse code to see if there are valid targets
        base.AddToMenu(s, menu);
        switch (GetCode(s).Task)
        {
            case "Capture":
                if (TileManager.globalInstance.HostileVisibleBuildingOnTile(this, Tile.LocalPlace))
                {
                    menu.Add(s);
                }
                break;
        }
    }

    public override void ExecuteChosenAbility(string s)
    {
        ////parse code and execute based on string s
        base.ExecuteChosenAbility(s);
        switch (GetCode(s).Task)
        {
            case "Capture":
                EventsManager.globalInstance.AddToStack(AbilityCode, abilityKey, this, AbilityAnimation, null, new List<UnitBase>() { this, Tile.Building });
                EventsManager.InvokeOnBeforeCapture(this, Tile.Building);
                abilityKey = "";
                TileManager.globalInstance.EndUnitTurn();
                break;
        }
    }

    public override void CommitTarget(Vector3Int target)
    {
        //parse code
        base.CommitTarget(target);
        /*
        targetList.Add(target);
        switch (targetAbility[1])
        {
            case "attack":
                //execute parsed code for selected abilities
                Attack(target);
                targetList.Clear();
                targetAbility = null;
                Tilemanager.globalinstance.EndUnitTurn();
                break;
        }*/
    }

    public void Capture()
    {
        int damage = (int)Math.Round(HPPercentage * CaptureRate);
        Tile.Building.TakeCaptureDamage(damage, this);
    }

}
