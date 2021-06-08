using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GlobalData;

public class Unit : UnitBase
{
    //public bool infiltrator;// can occupy non allied buildings or not
    //public bool moved;
    //public bool invisible;
    public int MovementTotal => data.movementTotal + buffs.Sum(x => x.movementTotal);
    public int CaptureRateBonus => /*data.captureRate +*/ buffs.Sum(x => x.captureRate);
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
        TileManager.globalInstance.AddUnit(this, localPlace);
        base.Load(localPlace, data, team);
        Team = team;
    }

    protected override void AddToMenu(string s, List<string> menu)
    {
        switch (GetTargetCode(s).Task)
        {
            case "Capture":
                if (TileManager.globalInstance.HostileVisibleBuildingOnTile(this, Tile.LocalPlace))
                {
                    menu.Add(s);
                }
                return;
        }
        //parse code to see if there are valid targets
        base.AddToMenu(s, menu);
    }

    public override void ExecuteChosenAbility(string s)
    {
        abilityKey = s;
        switch (AbilityTargetCode.Task)
        {
            case "Capture":
                EventsManager.globalInstance.AddToStack(AbilityLogicCode, abilityKey, this, AbilityAnimation, null, null, new List<Unit>() { this }, new List<Building>() { Tile.Building });
                abilityKey = "";
                TileManager.globalInstance.EndUnitTurn();
                return;
        }
        ////parse code and execute based on string s
        base.ExecuteChosenAbility(s);
    }

    public void Capture(bool before, Building building, int captureDamage)
    {
        if (before)
        {
            EventsManager.InvokeOnBeforeCapture(this, Tile.Building);
        }
        else
        {
            int damage = (int)Math.Round(HPPercentage * (captureDamage + CaptureRateBonus));
            building.TakeCaptureDamage(damage, this);
            EventsManager.InvokeOnCapture(this, Tile.Building);
        }
    }
}
