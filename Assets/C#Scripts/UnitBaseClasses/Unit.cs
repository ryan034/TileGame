using System;
using System.Linq;
using UnityEngine;
using static GlobalData;
using static GlobalFunctions;

public class Unit : UnitBase
{
    //public bool infiltrator;// can occupy non allied buildings or not
    //public bool moved;
    //public bool invisible;
    public int MovementTotal => data.movementTotal + buffs.Sum(x => x.movementTotal);
    public int CaptureDamage => buffs.Sum(x => x.captureDamage);
    public bool Infiltrator => data.infiltrator;
    public bool Rooted => buffs.Select(x => x.rooted).Contains(true);
    //protected override TileObject Tile => TileManager.globalInstance.GetTile(this);
    //public bool dead;
    public override int CoverBonus => Tile.Building != null && BothLandOrSky(MovementType, Tile.Building.MovementType) ? terrainDefenseMatrix[MovementType, Tile.TerrainType] + Tile.Building.BuildingCover : terrainDefenseMatrix[MovementType, Tile.TerrainType];

    public override void Load(bool initial, Vector3Int localPlace, UnitBaseData data, int team)
    {
        Manager.TileManager.AddUnit(this, localPlace);
        base.Load(initial, localPlace, data, team);
        if (initial) { internalVariables.team = MapTeam(team); }
        else { Team = team; }
        Manager.PlayerManager.LoadPlayer(Team);
    }
    
    protected override void DestroyThis(UnitBase killer)
    {
        //dead = true;
        EventsManager.InvokeOnDeathUnit(this);
        base.DestroyThis(killer);
        EventsManager.InvokeOnObjectDestroyUnit(this);
    }

    public void Capture(bool before, Building building, int cDamage)
    {
        if (before)
        {
            EventsManager.InvokeOnBeforeCapture(this, Tile.Building);
        }
        else
        {
            int damage = (int)Math.Round(HPPercentage * (cDamage + CaptureDamage));
            building.TakeCaptureDamage(damage, this);
            EventsManager.InvokeOnCapture(this, Tile.Building);
        }
    }
}
