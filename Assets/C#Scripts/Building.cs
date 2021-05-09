using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Globals;

public class Building : UnitBase
{
    /*buildings can be captured or destroyed and then captured. An occupied building is disabled. 
     * capturing data model based on hold
     * units can be in the same tile as buildings or cannot. capturing resource buildings uses resources. buildings can be repaired by using resources.
     *
     * neutral buildings: observatory, mercenary camp, tavern - need to be adjacent to use services
     * main building - lose this lose the game. Supplies gold and wood and provides supply. Upgrade your tier to get another hero and access to better units
     * 
     * production buildings:
     * abandoned altar(capture for an altar) - heroes are free to recruit, cost to ressurect
     * landspot -type 1 
     * landspot -type 2
     * landspot -type 3 
     * naval landspot - capture for original navy units, must be better value than land/air units
     * air landspot - for air units
     * 
     * landspots turn into production buildings. the units available depend on your age. upgrading your age at your hq unlocks more available units.
     * you unlock a new hero at every upgrade. the heroes xp starts at the average xp of all your other heroes. every age should give your passive upgrades to your units of lower age.
     * some ages will give you specific upgrades to units of the same age that is rare. ages selection works like choosing a god in age of mythology.
     * 
     * resource buildings: 
     * goldmine: gold income
     * farm: provides supply 
     * mill: wood income
     * 
     * if a tile is attacked with a building and unit, units have priority. units get armour bonus when in a building */
    //public int capturehp;
    //public int currentcapturehp; //normally 20
    //damaging building is like permanently partially capturing a building but to the nuetral side

    private List<KeyValuePair<int, int>> hold = new List<KeyValuePair<int, int>>();
    //private string race;

    public bool Neutral => data.neutral;
    public int BuildingCover => data.buildingCover + buffs.Sum(x => x.buildingCover)/*+ other modifiers*/;

    public override int Team
    {
        get
        {
            if (Neutral)
            {
                return TileManager.globalInstance.TeamTurn;
            }
            else if (Race == "Unaligned")
            {
                return -1;
            }
            else
            {
                return base.Team;
            }
        }
        set
        {
            base.Team = value;
//            if (value == -1)
//            {
//                Race = "Unaligned";
//            }
        }
    }

    public override string Race
    {
        get => data.race;
        protected set
        {
            if (value != data.race)
            {
                if (GetConvertedForm(value) != "")
                {
                    if (value == "Unaligned") { Animate("Death"); }
                    ChangeForm(GetConvertedForm(value));
                }
            }
        }
    }

    public override void Load(Vector3Int localPlace, UnitBaseData data, int team)
    {
        base.Load(localPlace, data, team);
        TileManager.globalInstance.AddBuilding(this, localPlace);
        //if (team == -1 && !Neutral) { DamageTaken = HP; }
        //else { Team = team; }
        /*else
        {
            Race_ = TileManager.globalInstance.GetRace(team);
        }*/
        if (Race == "Unaligned" && !Neutral) { DamageTaken = HP; }
        else { Team = team; }
    }

    protected override void DestroyThis()
    {/*death triggers*/
        //Destroyed_v();
        //parse code
        //unit.Destroy_v(this);
        hold.Clear();
        Race = "Unaligned";
        //Race_ = Race.noRace;
    }

    protected override void TakeDamage(UnitBase unit, int damagetype, int damage)
    {
        if (!Neutral)
        {
            base.TakeDamage(unit, damagetype, damage);
        }/*
        damageTaken = damageTaken + (int)Math.Round(GetResistance(damagetype) * damage);
        if (HPCurrent <= 0)
        {
            DestroyedBy(unit);
            //Takedamage_v(damagetype, damage);
            //unit.Dealtdamage_v(damagetype, damage);
            //parse code
        }*/
        if (HPCurrent > 0)
        {
            //rebalance hold of building
            //if team holds all then building is converted
            RebalanceHold(damage, unit);
        }
    }

    public override void ExecuteChosenAbility(string s)
    {
        ////parse code and execute based on string s
        base.ExecuteChosenAbility(s);
        switch (AbilityCode.Task)
        {
            //need to add to stack instead
            case "spawn":
                //execute parsed code for selected abilities
                Actioned = true;
                Unit unit = SpawnUnit(Tile, AbilityCode.GetVariable("unitID"), Team);
                EventsManager.globalInstance.AddToStack(AbilityCode, abilityKey, this, AbilityAnimation, null, new List<UnitBase>() { this, unit });
                EventsManager.InvokeOnBeforeSpawn(this, unit);
                abilityKey = "";
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

    public override void StartOfTurn()
    {
        base.StartOfTurn();
        if (HPCurrent > 0)
        {
            if (Tile.Unit == null || Tile.Unit.Team == Team)
            {
                hold.Clear();
                hold.Add(new KeyValuePair<int, int>(Team, HPCurrent));
            }
        }
    }

    private void ConvertedBy(UnitBase unit)
    {
        Race = unit.Race;
        Team = unit.Team;
        //heal the building some logic here
    }

    private void RebalanceHold(int damage, UnitBase unit)
    {
        //rebalance hold of building
        //if team holds majority then building is converted
        int d = damage;
        for (int i = 0; i < hold.Count; i++)
        {
            if (hold[i].Key != unit.Team)
            {
                hold[i] = new KeyValuePair<int, int>(hold[i].Key, hold[i].Value - d);
                if (hold[i].Value <= 0)
                {
                    d = 0;
                    break;
                }
                else
                {
                    d = -hold[i].Value;
                }
            }
        }
        foreach (KeyValuePair<int, int> item in hold)
        {
            if (item.Value <= 0) { hold.Remove(item); }
        }
        if (d >= 0)
        {
            for (int i = 0; i < hold.Count; i++)
            {
                hold[i] = new KeyValuePair<int, int>(hold[i].Key, hold[i].Value - d);
                if (hold[i].Value <= 0)
                {
                    d = 0;
                    break;
                }
                else
                {
                    d = -hold[i].Value;
                }
            }
            foreach (KeyValuePair<int, int> item in hold)
            {
                if (item.Value <= 0) { hold.Remove(item); }
            }
            ConvertedBy(unit);
            //Race_ = unit.Race_;
            Actioned = true;
            TileManager.globalInstance.WipeTiles();
            Animate("Capture");
        }
    }

    public void TakeCaptureDamage(int damage, Unit unit)
    {
        int d = damage;
        for (int i = 0; i < hold.Count; i++)
        {
            if (hold[i].Key != unit.Team)
            {
                hold[i] = new KeyValuePair<int, int>(hold[i].Key, hold[i].Value - d);
                if (hold[i].Value <= 0)
                {
                    d = 0;
                    break;
                }
                else
                {
                    d = -hold[i].Value;
                }
            }
        }
        //capture the building
        if (d >= 0)
        {
            ConvertedBy(unit);
            //Race_ = unit.Race_;
            Actioned = true;
            TileManager.globalInstance.WipeTiles();
            Animate("Capture");
        }
        foreach (KeyValuePair<int, int> item in hold)
        {
            if (item.Value <= 0) { hold.Remove(item); }
        }
        hold.Add(new KeyValuePair<int, int>(unit.Team, damage));
        DamageTaken -= d;
    }

    public Unit SpawnUnit(TileObject tile, string script, int team_)
    {
        return TileManager.globalInstance.SpawnUnit(Tile.LocalPlace, script, team_);
    }
}
