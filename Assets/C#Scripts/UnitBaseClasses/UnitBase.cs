using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GlobalData;
using static GlobalFunctions;
using static GlobalParser;

public abstract class UnitBase : MonoBehaviour
{
    protected UnitAnimator animator;
    protected UnitBaseData data;
    protected InternalVariables internalVariables;

    //protected string[] AbilityVariables => GetCode(abilityKey).Split(' ');
    protected CodeObject AbilityTargetCode => GetTargetCode(abilityKey);
    protected CodeObject AbilityLogicCode => GetLogicCode(abilityKey);
    protected CodeObject AbilityAnimationCode => GetAnimationCode(abilityKey);
    protected string abilityKey;
    //protected List<Vector3Int> targetList = new List<Vector3Int>();
    protected List<Unit> unitList = new List<Unit>();
    protected List<Building> buildingList = new List<Building>();
    protected List<Vector3Int> vectorList = new List<Vector3Int>();
    protected List<int> intList = new List<int>();
    protected List<UnitBase> unitBaseList = new List<UnitBase>();
    public int TargetCount => unitList.Count + buildingList.Count + vectorList.Count + intList.Count + unitBaseList.Count;
    //protected int targetIndex;
    protected List<Buff> buffs = new List<Buff>();

    protected virtual int DamageTaken
    {
        get => internalVariables.damageTaken;
        set
        {
            if (/*internalVariables.damageTaken < HP && */internalVariables.damageTaken < value)//getting damaged
            {
                if (value >= HP)
                {
                    DestroyThis();
                    internalVariables.damageTaken = HP;
                    //parse code triggers
                }
                else
                {
                    internalVariables.damageTaken = value;
                    //Takedamage_v(damagetype, damage);
                    //unit.Dealtdamage_v(damagetype, damage);
                    //parse code
                }
            }
            if (internalVariables.damageTaken > value)//getting healed
            {
                if (value < 0)
                {
                    //healed, if race is not unaligned
                    internalVariables.damageTaken = 0;
                    //parse code triggers
                }
                else
                {
                    internalVariables.damageTaken = value;
                    //parse code triggers
                }
            }
        }
    }

    protected int manaUsed;//may need to mvoe this into internal variables

    public string Name => data.name;
    public string Race => data.race;
    public int ArmourType => data.armourType;
    public int MovementType => data.movementType;

    public int DayVision => data.dayVision + buffs.Sum(x => x.dayVision);
    public int NightVision => data.nightVision + buffs.Sum(x => x.nightVision);
    public int HP => data.hP + buffs.Sum(x => x.hP);
    public int MP => data.mP + buffs.Sum(x => x.mP);
    public int Armour => data.armour + buffs.Sum(x => x.armour);
    public bool Charming => buffs.Select(x => x.notHostile).Contains(true);
    public bool Disarmed => buffs.Select(x => x.disarmed).Contains(true);
    public bool Silenced => buffs.Select(x => x.silenced).Contains(true);

    public int HPCurrent => HP - DamageTaken;
    public int MPCurrent => MP - manaUsed;
    public double HPPercentage => Math.Max((double)HPCurrent / HP, 0);
    //public int DamageType => int.Parse(AbilityLogicCode.GetVariable("damageType"));
    public int Team { get => internalVariables.team; set { internalVariables.team = value; RefreshUnitSprite(); } }

    public bool Actioned { get => internalVariables.actioned; set { internalVariables.actioned = value; RefreshUnitSprite(); } } //alreadymoved and attacked
    public bool Invisible { get => internalVariables.invisible; set { internalVariables.invisible = value; RefreshUnitSprite(); } }

    public IEnumerable<string> Abilities => data.Abilities;

    public TileObject Tile => TileManager.globalInstance.GetTile(this);

    public virtual int CoverBonus => terrainDefenseMatrix[MovementType, Tile.TerrainType];

    protected class InternalVariables
    {
        public int team;
        public bool actioned;
        public bool invisible;
        public int damageTaken;
        public int manaUsed;
    }

    public virtual void Load(Vector3Int localPlace, UnitBaseData data, int team)
    {
        internalVariables = new InternalVariables();
        animator = transform.GetComponent<UnitAnimator>();
        animator.Load(this);
        this.data = data;
        //Team = team;
        foreach (string s in data.Buffs)
        {
            buffs.Add(Buff.Load(this, s));
        }
        UnitTransformManager.globalInstance.SnapMove(this, localPlace);
    }

    public virtual bool SameTeam(int team_) => team_== Team;

    public virtual void StartOfTurn() { }

    protected virtual void DestroyThis() => TileManager.globalInstance.DestroyUnit(this);

    protected virtual void CalculateAndTakeDamage(UnitBase unit, int damageType, int damage)
    {
        DamageTaken = DamageTaken + (int)Math.Round(GetResistance(damageType) * damage);
        /*
        else
        {
            //animator.Animate("Damage");
        }*/
    }

    public bool HasTag(string tag) => (data.HasTag(tag) || buffs.Select(x => x.HasTag(tag)).Contains(true));

    public string GetConvertedForm(string race) => data.GetConvertedForm(race);

    public void RefreshUnitSprite() => animator.RefreshUnitSprite();

    public void RefreshBuildingSprite() => animator.RefreshBuildingSprite();

    public bool IsPlaying(string animation) => animator.IsPlaying(animation);

    public void Animate(string animation) => animator.Animate(animation);

    public IEnumerator ParseAnimation(StackItem animation) { yield return StartCoroutine(animator.ParseAnimation(animation)); }

    public void SetUp(List<string> menu)
    {
        foreach (string s in Abilities)
        {
            AddToMenu(s, this, GetTargetCode(s), menu);
        }
    }

    public void ClearTargets()
    {
        unitList.Clear();
        buildingList.Clear();
        vectorList.Clear();
        intList.Clear();
        unitBaseList.Clear();
    }

    public float GetResistance(int damageType)
    {
        //check buffs
        return attackToArmour[damageType, ArmourType];
    }

    public void ChooseMenuAbility(string s) { abilityKey = s; GlobalParser.ChooseMenuAbility(s, AbilityTargetCode, AbilityLogicCode, AbilityAnimationCode, this); }

    public void CommitTarget(Vector3Int target)
    {
        if (GlobalParser.CommitTarget(abilityKey, AbilityTargetCode, this, target, buildingList, unitList, unitBaseList, vectorList, intList))
        {
            EventsManager.globalInstance.AddToStack(AbilityLogicCode, abilityKey, this, AbilityAnimationCode, intList, unitBaseList, unitList, buildingList, vectorList);
            ClearTargets();
        }
        /*
        if (AbilityTargetCode.GetVariable("unique") == "true")
        {
            if (!targetList.Contains(target))
            {
                targetList.Add(target);
            }
        }
        else
        {
            targetList.Add(target);
        }
        //need to implement min/max targets
        if ( targetList.Count == int.Parse(AbilityTargetCode.GetVariable("targets")))
        {
            //todo: should really just have a simple parse target code, attack is an exception
            //target code should have two things, what are the target requirements, and how are the targets used by logic code, there should be no logic code parsing here
            switch (AbilityTargetCode.Task)
            {
                case "Attack":
                    List<UnitBase> l = new List<UnitBase>() { this };
                    foreach (Vector3Int v in targetList) { l.Add(TileManager.globalInstance.GetHostileAttackableUnitOrBuilding(this, v, AbilityTargetCode.GetVariable("canHit"))); }
                    EventsManager.globalInstance.AddToStack(AbilityLogicCode, abilityKey, this, AbilityAnimation, null, l);
                    EventsManager.InvokeOnBeforeMainAttack(this, l);
                    TileManager.globalInstance.EndUnitTurn();
                    break;
            }
        }
        targetList.Clear();
        abilityKey = "";*/
    }

    public void Attack(bool before, List<UnitBase> target, int baseDamage, int diceDamage, int diceTimes, int damageType)
    {
        if (before) { EventsManager.InvokeOnBeforeAttack(this, target); }
        else
        {
            foreach (UnitBase t in target)
            {
                int cover = t.CoverBonus;
                int totaldamage = (int)Math.Round(HPPercentage * Rolldamage(baseDamage, diceDamage, diceTimes, cover));
                //animator.Animate(targetAbility[8]);
                //UnitTransformManager.globalInstance.QueueAnimation(this, targetAbility[8], target);
                t.CalculateAndTakeDamage(this, damageType, totaldamage);
                //t.CounterAttack(this);
                //t.GetAttackedAndCounter(this, DamageType, totaldamage);
                EventsManager.InvokeOnAttack(this, target);
            }
        }
    }

    public bool CanHit(UnitBase unitBase, string attackType)
    {
        bool b = false;
        switch (attackType)
        {
            case "same"://flying hits flying, land hits land
                b = BothLandOrSky(MovementType, unitBase.MovementType);
                break;
            case "different"://flying hits land, land hits flying
                b = !BothLandOrSky(MovementType, unitBase.MovementType);
                break;
            case "all":
                b = true;
                break;
        }
        return b;
    }

    public bool CanCounterAttack(UnitBase targetUnit)
    {
        foreach (string s in Abilities)
        {
            if (GetTargetCode(s).Task == "Attack" && ValidateTargetForAttack(targetUnit, s)/* todo move target validation to triggerpart of the stackchain*/)
            {
                return true;
            }
        }
        return false;
    }

    public void CounterAttack(bool before, UnitBase targetUnit)
    {
        foreach (string s in Abilities)
        {
            //string[] ability = GetCode(s).Split(' ');
            CodeObject ability = GetTargetCode(s);
            if (ability.Task == "Attack" && ValidateTargetForAttack(targetUnit, s)/* todo move target validation to triggerpart of the stackchain*/)
            {
                //code=> attack:flying or not: siege or not:min range: max range:base damage:dicedamage:times:damagetype
                /*
                int cover = targetunit.CoverBonus;
                int totaldamage = (int)Math.Round(HPPercentage * Rolldamage(int.Parse(ability.GetVariable("baseDamage")), int.Parse(ability.GetVariable("diceDamage")), int.Parse(ability.GetVariable("diceTimes")), cover));
                targetunit.TakeDamage(this, int.Parse(ability.GetVariable("damageType")), totaldamage);
                //UnitTransformManager.globalInstance.QueueAnimation(this, ability[8], targetunit.Tile.LocalPlace);
                EventsManager.InvokeOnCounterAttack(this, targetunit);
                return;
                */
                //change this to just parse whatever the logic code may be with the targetunit as the single target
                Parse(new StackItem(GetLogicCode(s), s, this, GetAnimationCode(s), null, new List<UnitBase>() { this, targetUnit }, null, null, null), null, before);
                return;
            }
        }
    }

    public List<Unit> SpawnUnit(bool before, List<Vector3Int> tile, string script, int unitTeam)
    {
        if (before) { EventsManager.InvokeOnBeforeSpawnUnit(this); return null; }
        else
        {
            Actioned = true;
            //Unit unit = SpawnUnit(Tile, AbilityLogicCode.GetVariable("unitID"), Team);
            List<Unit> units = new List<Unit>();
            foreach (Vector3Int t in tile)
            {
                Unit unit = TileManager.globalInstance.SpawnUnit(t, script, unitTeam);
                units.Add(unit);
            }
            EventsManager.InvokeOnSpawnUnit(this, units);
            return units;
        }
    }

    protected void ChangeForm(string form)
    {
        data = AssetManager.globalInstance.LoadUnitBaseData(form);
        animator.ChangeModel(form);
    }

    protected CodeObject GetLogicCode(string s) => data.GetLogicCode(s);

    protected CodeObject GetTargetCode(string s) => data.GetTargetCode(s);

    protected CodeObject GetAnimationCode(string s) => data.GetAnimationCode(s);

    protected bool ValidateTargetForAttack(UnitBase targetUnit, string s)
    {
        CodeObject ability = GetTargetCode(s);
        //return CanHit(targetunit, ability.GetVariable("canHit")) && TileManager.globalInstance.VisibleAndHostileTo(Team, targetunit) && TileManager.globalInstance.WithinRange(int.Parse(ability.GetVariable("minRange")), int.Parse(ability.GetVariable("maxRange")), this, targetunit);
        return TileManager.globalInstance.AttackableAndHostileTo(this, targetUnit, ability.GetVariable("canHit")) && TileManager.globalInstance.WithinRange(int.Parse(ability.GetVariable("minRange")), int.Parse(ability.GetVariable("maxRange")), this, targetUnit);

    }

    /*
    public void AddBuff(Buff buff)
    {
        buffs.Add(buff);
        //some buffs may not stack but rather in duration
    }*/
}
