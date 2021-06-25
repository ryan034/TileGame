using System;
using System.Linq;
using UnityEngine;
using static GlobalData;

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

    public override int CoverBonus => Tile.Building != null && BothLandOrSky(MovementType, Tile.Building.MovementType) ? terrainDefenseMatrix[MovementType, Tile.TerrainType] + Tile.Building.BuildingCover : terrainDefenseMatrix[MovementType, Tile.TerrainType];

    public override void Load(Vector3Int localPlace, UnitBaseData data, int team)
    {
        TileManager.globalInstance.AddUnit(this, localPlace);
        base.Load(localPlace, data, team);
        Team = team;
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
