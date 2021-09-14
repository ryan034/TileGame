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
    protected InternalVariables internalVariables;

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

    public UnitBaseData UnitData { get; protected set; }

    public string Name => UnitData.name;
    public string Race => UnitData.race;
    public int ArmourType => UnitData.armourType;
    public int MovementType => UnitData.movementType;

    public int DayVision => UnitData.dayVision + buffs.Sum(x => x.dayVision);
    public int NightVision => UnitData.nightVision + buffs.Sum(x => x.nightVision);
    public int HPMax => UnitData.hP + buffs.Sum(x => x.hP);
    public int MPMax => UnitData.mP + buffs.Sum(x => x.mP);
    public int Armour => UnitData.armour + buffs.Sum(x => x.armour);
    public bool Charming => buffs.Select(x => x.notHostile).Contains(true);
    public bool Disarmed => buffs.Select(x => x.disarmed).Contains(true);
    public bool Silenced => buffs.Select(x => x.silenced).Contains(true);

    public int HPCurrent => HPMax - DamageTaken;
    public int MPCurrent => MPMax - ManaUsed;
    public double HPPercentage => Math.Max((double)HPCurrent / HPMax, 0);
    public int Team { get => internalVariables.team; set { internalVariables.team = value; Tile.RefreshSprite(); } }

    public bool Actioned { get => internalVariables.actioned; set { internalVariables.actioned = value; Tile.RefreshSprite(); } } //alreadymoved and attacked
    public bool Invisible { get => internalVariables.invisible; set { internalVariables.invisible = value; Tile.RefreshSprite(); } }

    public IEnumerable<string> Abilities => UnitData.Abilities;

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
        UnitData = data;
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

    public virtual void DestroyThis(UnitBase killer)
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

    public virtual void CalculateDamageTakenAndTakeDamage(bool before, UnitBase unit, int damageType, int damage)
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

    public CodeObject GetLogicCode(string s) => UnitData.GetLogicCode(s);

    public CodeObject GetTargetCode(string s) => UnitData.GetTargetCode(s);

    public CodeObject GetAnimationCode(string s) => UnitData.GetAnimationCode(s);


    public bool HasTag(string tag) => (UnitData.HasTag(tag) || buffs.Select(x => x.HasTag(tag)).Contains(true));

    public string GetConvertedForm(string race) => UnitData.GetConvertedForm(race);

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

    public void ChooseMenuAbility(string s) { abilityKey = s; GlobalParser.ChooseMenuAbility(abilityKey, GetTargetCode(abilityKey), GetLogicCode(abilityKey), GetAnimationCode(abilityKey), this); }

    public void CommitTarget(Vector3Int target)
    {
        if (ValidateTargetAndCommit(abilityKey, GetTargetCode(abilityKey), this, target, buildingList, unitList, unitBaseList, vectorList, intList))
        {
            Manager.EventsManager.AddToStack(GetLogicCode(abilityKey), abilityKey, this, GetAnimationCode(abilityKey), intList, unitBaseList, unitList, buildingList, vectorList, "MainAttack");
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

    public List<Unit> SpawnUnit(bool before, List<Vector3Int> tile, string script, int unitTeam)
    {
        if (before) { EventsManager.InvokeOnBeforeSpawnUnit(this); return null; }
        else
        {
            Actioned = true;
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

    public void DamageTarget(bool before, UnitBase target, int baseDamage, int diceDamage, int diceTimes, int damageType)
    {
        if (before)
        {
            EventsManager.InvokeOnBeforeAttack(this, target);
        }
        else
        {
            EventsManager.InvokeOnAttack(this, target);
        }
        int totaldamage = CalculateAttackDamage(baseDamage, diceDamage, diceTimes, target.CoverBonus);
        target.CalculateDamageTakenAndTakeDamage(before, this, damageType, totaldamage);
    }

    protected int CalculateAttackDamage(int baseDamage, int diceDamage, int diceTimes, int cover)
    {
        return (int)Math.Round(HPPercentage * Rolldamage(baseDamage, diceDamage, diceTimes, cover));
    }

    protected void ChangeForm(string form)
    {
        UnitData = Manager.AssetManager.LoadUnitBaseData(form);
        animator.ChangeModel(form);
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
