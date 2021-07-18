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

    protected CodeObject AbilityTargetCode => GetTargetCode(abilityKey);
    protected CodeObject AbilityLogicCode => GetLogicCode(abilityKey);
    protected CodeObject AbilityAnimationCode => GetAnimationCode(abilityKey);
    protected string abilityKey;

    protected List<Unit> unitList = new List<Unit>();
    protected List<Building> buildingList = new List<Building>();
    protected List<Vector3Int> vectorList = new List<Vector3Int>();
    protected List<int> intList = new List<int>();
    protected List<UnitBase> unitBaseList = new List<UnitBase>();
    public int TargetCount => unitList.Count + buildingList.Count + vectorList.Count + intList.Count + unitBaseList.Count;

    protected List<Buff> buffs = new List<Buff>();

    protected virtual int DamageTaken
    {
        get => internalVariables.damageTaken;
        set
        {
            if (internalVariables.damageTaken < value)//getting damaged
            {
                internalVariables.damageTaken = value > HPMax ? HPMax : value;
            }
            if (internalVariables.damageTaken > value)//getting healed
            {
                internalVariables.damageTaken = value < 0 ? 0 : value;
            }
        }
    }

    protected int ManaUsed
    {
        get => internalVariables.manaUsed;
        set
        {
            if (internalVariables.manaUsed < value)//getting damaged
            {
                if (value >= MPMax)
                {
                    internalVariables.manaUsed = HPMax;
                }
                else
                {
                    internalVariables.manaUsed = value;
                }
            }
            if (internalVariables.manaUsed > value)//getting healed
            {
                if (value < 0)
                {
                    internalVariables.manaUsed = 0;
                }
                else
                {
                    internalVariables.manaUsed = value;
                }
            }
        }
    }

    public string Name => data.name;
    public string Race => data.race;
    public int ArmourType => data.armourType;
    public int MovementType => data.movementType;

    public int DayVision => data.dayVision + buffs.Sum(x => x.dayVision);
    public int NightVision => data.nightVision + buffs.Sum(x => x.nightVision);
    public int HPMax => data.hP + buffs.Sum(x => x.hP);
    public int MPMax => data.mP + buffs.Sum(x => x.mP);
    public int Armour => data.armour + buffs.Sum(x => x.armour);
    public bool Charming => buffs.Select(x => x.notHostile).Contains(true);
    public bool Disarmed => buffs.Select(x => x.disarmed).Contains(true);
    public bool Silenced => buffs.Select(x => x.silenced).Contains(true);

    public int HPCurrent => HPMax - DamageTaken;
    public int MPCurrent => MPMax - ManaUsed;
    public double HPPercentage => Math.Max((double)HPCurrent / HPMax, 0);
    //public int DamageType => int.Parse(AbilityLogicCode.GetVariable("damageType"));
    public int Team { get => internalVariables.team; set { internalVariables.team = value; Tile.RefreshSprite(); } }

    public bool Actioned { get => internalVariables.actioned; set { internalVariables.actioned = value; Tile.RefreshSprite(); } } //alreadymoved and attacked
    public bool Invisible { get => internalVariables.invisible; set { internalVariables.invisible = value; Tile.RefreshSprite(); } }

    public IEnumerable<string> Abilities => data.Abilities;

    public TileObject Tile => Manager.TileManager.GetTile(this);

    public virtual int CoverBonus => terrainDefenseMatrix[MovementType, Tile.TerrainType];

    protected class InternalVariables
    {
        public int team;
        public bool actioned;
        public bool invisible;
        public int damageTaken;
        public int manaUsed;
    }

    public virtual void Load(bool initial, Vector3Int localPlace, UnitBaseData data, int team)
    {
        internalVariables = new InternalVariables();
        animator = gameObject.AddComponent<UnitAnimator>();
        animator.Load(this);
        this.data = data;
        foreach (string s in data.Buffs)
        {
            buffs.Add(Buff.Load(this, s));
        }
        Manager.UnitTransformManager.SnapMove(this, localPlace);
        EventsManager.OnObjectDestroyUnitBase += OnObjectDestroyUnitBase;
        EventsManager.OnObjectDestroyUnit += OnObjectDestroyUnit;
        EventsManager.OnObjectDestroyBuilding += OnObjectDestroyBuilding;
    }

    public virtual bool SameTeam(int team_) => team_ == Team;

    public virtual void StartOfTurn() { }

    public virtual void RefreshSprite() => animator.RefreshUnitSprite();

    protected virtual void DestroyThis(UnitBase killer)
    {
        EventsManager.InvokeOnDeathUnitBase(this);
        EventsManager.InvokeOnKill(killer, this);
        Manager.TileManager.DestroyUnitBase(this); /*deference everything from here and change state to destroyed*/
        foreach (Buff buff in buffs) { buff.Destroy(); }
        EventsManager.OnObjectDestroyUnitBase -= OnObjectDestroyUnitBase;
        EventsManager.OnObjectDestroyUnit -= OnObjectDestroyUnit;
        animator.DestroyUnit();
        EventsManager.InvokeOnObjectDestroyUnitBase(this);
    }

    protected void AttackTarget(bool before, UnitBase target, int baseDamage, int diceDamage, int diceTimes, int damageType)
    {
        int cover = target.CoverBonus;
        int totaldamage = (int)Math.Round(HPPercentage * Rolldamage(baseDamage, diceDamage, diceTimes, cover));
        target.CalculateAndTakeDamage(before, this, damageType, totaldamage);
    }

    protected virtual void CalculateAndTakeDamage(bool before, UnitBase unit, int damageType, int damage)
    {
        //add take damage event here
        int damageCalculated = (int)Math.Round(GetResistance(damageType) * damage);
        if (before)
        {
            //invoke before take damage event
        }
        else
        {
            DamageTaken = DamageTaken + damageCalculated;/*//animator.Animate("Damage");*/
            if (DamageTaken == HPMax)
            {
                DestroyThis(unit);
            }
            //invoke take damage event
        }
    }

    public bool HasTag(string tag) => (data.HasTag(tag) || buffs.Select(x => x.HasTag(tag)).Contains(true));

    public string GetConvertedForm(string race) => data.GetConvertedForm(race);

    public bool IsPlaying(string animation) => animator.IsPlaying(animation);

    public void Animate(string animation) => animator.Animate(animation);

    //public IEnumerator PlayWaitForDuration(float v) => animator.PlayWaitForDuration(v);

    public IEnumerator PlayAnimationAndFinish(string v) => animator.PlayAnimationAndFinish(v);

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
        if (ValidateTargetAndCommit(abilityKey, AbilityTargetCode, this, target, buildingList, unitList, unitBaseList, vectorList, intList))
        {
            Manager.EventsManager.AddToStack(AbilityLogicCode, abilityKey, this, AbilityAnimationCode, intList, unitBaseList, unitList, buildingList, vectorList);
            ClearTargets();
        }
    }

    public void MainAttack(bool before, List<UnitBase> target, int speed, int baseDamage, int diceDamage, int diceTimes, int damageType)
    {
        foreach (UnitBase t in target)
        {
            //AttackTarget(before, t, baseDamage, diceDamage, diceTimes, damageType);
            //t.CounterAttack(before, this);
            t.CounterAttack(before, this, speed + 1, baseDamage, diceDamage, diceTimes, damageType);
        }
        if (before) { EventsManager.InvokeOnBeforeMainAttack(this, target); }
        else
        {
            EventsManager.InvokeOnMainAttack(this, target);
        }
    }

    /*
    protected void GetAttackedAndCounterAttack(bool before, UnitBase attacker, int speed, int baseDamage, int diceDamage, int diceTimes, int damageType)
    {
        attacker.AttackTarget(before, this, baseDamage, diceDamage, diceTimes, damageType);
        CounterAttack(before, attacker);
    }*/

    public void NonMainAttack(bool before, List<UnitBase> target, int baseDamage, int diceDamage, int diceTimes, int damageType)
    {
        foreach (UnitBase t in target)
        {
            AttackTarget(before, t, baseDamage, diceDamage, diceTimes, damageType);
        }
        if (before) { EventsManager.InvokeOnBeforeNonMainAttack(this, target); }
        else
        {
            EventsManager.InvokeOnNonMainAttack(this, target);
        }
    }

    public void CounterAttack(bool before, UnitBase targetUnit, int attackerSpeed = int.MinValue, int baseDamage = 0, int diceDamage = 0, int diceTimes = 0, int damageType = 0)
    {
        foreach (string s in Abilities)
        {
            CodeObject ability = GetTargetCode(s);
            if (ability.Task == "Attack" && ValidateTargetForAttack(targetUnit, s)/* todo move target validation to triggerpart of the stackchain*/)
            {
                int speed = GetLogicCode(s).GetVariable("Speed") == "" ? 0 : int.Parse(GetLogicCode(s).GetVariable("Speed"));
                if (attackerSpeed == int.MinValue)
                {
                    Parse(new StackItem(GetLogicCode(s), s, this, GetAnimationCode(s), null, new List<UnitBase>() { this, targetUnit }, null, null, null), null, before, "NonMainAttack");
                }
                else if (speed - attackerSpeed >= 1)
                {
                    Parse(new StackItem(GetLogicCode(s), s, this, GetAnimationCode(s), null, new List<UnitBase>() { this, targetUnit }, null, null, null), null, before, "NonMainAttack");
                    targetUnit.AttackTarget(before, this, baseDamage, diceDamage, diceTimes, damageType);
                }
                else if (speed == attackerSpeed)
                {
                    //units attack eachother at the same time
                }
                else if (speed - attackerSpeed == -1)
                {
                    targetUnit.AttackTarget(before, this, baseDamage, diceDamage, diceTimes, damageType);
                    Parse(new StackItem(GetLogicCode(s), s, this, GetAnimationCode(s), null, new List<UnitBase>() { this, targetUnit }, null, null, null), null, before, "NonMainAttack");
                }
                else if (speed - attackerSpeed <= -2)
                {
                    targetUnit.AttackTarget(before, this, baseDamage, diceDamage, diceTimes, damageType);
                }
                return;
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
                Unit unit = Manager.TileManager.SpawnUnit(t, script, unitTeam);
                units.Add(unit);
            }
            EventsManager.InvokeOnSpawnUnit(this, units);
            return units;
        }
    }

    protected void ChangeForm(string form)
    {
        data = Manager.AssetManager.LoadUnitBaseData(form);
        animator.ChangeModel(form);
    }

    protected CodeObject GetLogicCode(string s) => data.GetLogicCode(s);

    protected CodeObject GetTargetCode(string s) => data.GetTargetCode(s);

    protected CodeObject GetAnimationCode(string s) => data.GetAnimationCode(s);

    protected bool ValidateTargetForAttack(UnitBase targetUnit, string s)
    {
        CodeObject ability = GetTargetCode(s);
        return Manager.TileManager.AttackableAndHostileTo(this, targetUnit, ability.GetVariable("canHit")) && Manager.TileManager.WithinRange(int.Parse(ability.GetVariable("minRange")), int.Parse(ability.GetVariable("maxRange")), this, targetUnit);
    }

    /*
    public void AddBuff(Buff buff)
    {
        buffs.Add(buff);
        //some buffs may not stack but rather in duration
    }*/
    protected void OnObjectDestroyUnitBase(UnitBase unit) { unitBaseList.Remove(unit); }

    protected void OnObjectDestroyUnit(Unit unit) { unitList.Remove(unit); }

    protected void OnObjectDestroyBuilding(Building unit) { buildingList.Remove(unit); }
}
