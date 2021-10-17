using static GlobalFunctions;
using static GlobalAnimationParser;
using static GlobalData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

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

    //private Dictionary<int, int> hold = new Dictionary<int, int>();
    //private string race;

    private Hold hold = new Hold();

    private class Hold
    {
        private Dictionary<int, int> hold = new Dictionary<int, int>();

        public int this[int index]
        {
            get
            {
                if (!hold.ContainsKey(index))
                {
                    hold[index] = 0;
                }
                return hold[index];
            }
            set
            {
                hold[index] = value;
            }
        }

        public List<int> Keys => hold.Keys.ToList();
        public List<int> Values => hold.Values.ToList();
    }

    protected override int DamageTaken
    {
        get => base.DamageTaken;
        set
        {
            if (Race != unaligned && Race != neutral)
            {
                if (base.DamageTaken > value)
                {
                    base.DamageTaken = value;
                    hold[Team] += base.DamageTaken - value;
                }
                else
                {
                    base.DamageTaken = value;
                }
            }
        }
    }

    //public bool Neutral => data.neutral;
    public int BuildingCover => UnitData.buildingCover + buffs.Sum(x => x.buildingCover)/*+ other modifiers*/;

    /*
    public override string Race
    {
        get => base.Race;
        protected set
        {
            if (value != base.Race)
            {
                if (GetConvertedForm(value) != "")
                {
                    if (value == "Unaligned") { Animate("Death"); }
                    ChangeForm(GetConvertedForm(value));
                }
            }
        }
    }*/

    public override bool SameTeam(int team_)
    {
        if (Race == neutral)
        {
            return true;
        }
        else if (Race == unaligned)
        {
            return false;
        }
        else
        {
            return base.SameTeam(team_);
        }
    }

    public override void Load(bool initial, Vector3Int localPlace, UnitBaseData data, int team)
    {
        Manager.TileManager.AddBuilding(this, localPlace);
        base.Load(initial, localPlace, data, team);
        //if (team == -1 && !Neutral) { DamageTaken = HP; }
        //else { Team = team; }
        /*else
        {
            Race_ = TileManager.globalInstance.GetRace(team);
        }*/
        if (Race == unaligned /*&& !Neutral*/)
        {
            //    DamageTaken = HP;
            SetHPToZero();
        }
        else if (Race != neutral)
        {
            if (initial) { internalVariables.team = MapTeam(team); }
            else { Team = team; }
            Manager.PlayerManager.LoadPlayer(Team);
        }
    }

    public override void RefreshSprite() => animator.RefreshBuildingSprite();

    public override IEnumerator DestroyThis(UnitBase killer)
    {
        yield return PlayAnimationAndFinish("Death");
        EventsManager.InvokeOnDeathBuilding(this);
        EventsManager.InvokeOnDeathUnitBase(this);
        EventsManager.InvokeOnKill(killer, this);
        ClearHold();
        ChangeBuildingUsingRace(unaligned);
        foreach (Buff buff in buffs) { buff.Destroy(); }
        animator.Animate("Death");
        EventsManager.InvokeOnObjectDestroyBuilding(this);
        EventsManager.InvokeOnObjectDestroyUnitBase(this);
        //Race_ = Race.noRace;
    }

    public override IEnumerator CalculateDamageTakenAndTakeDamage(bool before, UnitBase unit, int damageType, int damage)
    {
        yield return base.CalculateDamageTakenAndTakeDamage(before, unit, damageType, damage);
        if (!before) { RebalanceHoldAfterTakingHPDamage(damage, unit); }
    }

    /*
    public override void ExecuteChosenAbility(string s)
    {
        abilityKey = s;
        switch (AbilityLogicCode.Task)
        {
            //need to add to stack instead
            case "Spawn":
                //execute parsed code for selected abilities
                //EventsManager.InvokeOnSpawn(this, unit);
                EventsManager.globalInstance.AddToStack(AbilityLogicCode, abilityKey, this, AbilityAnimation, null, new List<UnitBase>() { this });
                //EventsManager.InvokeOnBeforeSpawn(this, unit);
                abilityKey = "";
                return;
        }
        ////parse code and execute based on string s
        base.ExecuteChosenAbility(s);
    }*/

    public override void StartOfTurn()
    {
        base.StartOfTurn();
        if (HPCurrent > 0)
        {
            if (Tile.Unit == null || SameTeam(Tile.Unit.Team))
            {
                ClearHold();
                hold[Team] = HPCurrent;
            }
        }
    }

    private void ChangeBuildingUsingRace(string race)
    {
        if (race != Race)
        {
            if (GetConvertedForm(race) != "")
            {
                if (race == unaligned)
                {
                    Animate("Death");
                    ChangeForm(GetConvertedForm(race));
                    SetHPToZero();
                }
                else
                {
                    ChangeForm(GetConvertedForm(race));
                }
            }
        }
    }

    private void SetHPToZero()
    {
        internalVariables.damageTaken = HPMax;
    }

    private void ConvertedBy(UnitBase unit)
    {
        ChangeBuildingUsingRace(unit.Race);
        Team = unit.Team;
        Actioned = true;
        //Animate("Captured");
    }

    private void ClearHold()
    {
        foreach (int i in hold.Keys)
        {
            hold[i] = 0;
        }
    }

    private int RemoveFromHold(int damage, UnitBase unit)
    {
        int residual = damage;
        List<int> sortedList = new List<int>(hold.Keys);
        sortedList.OrderByDescending(o => hold[o]).ToList();
        foreach (int i in sortedList)
        {
            if (i != unit.Team)
            {
                hold[i] -= residual;
                if (hold[i] > 0)
                {
                    return -1;
                }
                else
                {
                    residual = -hold[i];
                    hold[i] = 0;
                }
            }
        }
        ConvertedBy(unit);
        return residual;
    }

    private void RebalanceHoldAfterTakingHPDamage(int damage, UnitBase unit)
    {
        if (HPCurrent > 0)
        {
            int residual = RemoveFromHold(damage, unit);
            if (residual >= 0)
            {
                hold[unit.Team] -= residual;
            }
        }
    }

    public void TakeCaptureDamage(int damage, Unit unit)
    {
        int residual;
        if (HPCurrent > 0)
        {
            residual = RemoveFromHold(damage, unit);
        }
        else
        {
            residual = hold.Values.Sum() + damage - HPMax;
            if (residual >= 0)
            {
                residual = RemoveFromHold(residual, unit);
                if (residual >= 0)
                {
                    DamageTaken = 0;
                }
            }
        }
        hold[unit.Team] = hold[unit.Team] + damage > HPMax ? HPMax : hold[unit.Team] + damage;
    }

}
