using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GlobalData;
using static GlobalFunctions;
using static GlobalParser;
using static GlobalAnimationParser;

public abstract class UnitBase : MonoBehaviour, IUnitBase
{
    protected static TileManager tileManager = TileManager.GlobalInstance;
    protected static PlayerManager playerManager = PlayerManager.GlobalInstance;
    protected static EventsManager eventsManager;
    protected static AssetManager assetManager;

    protected UnitAnimator animator;
    protected InternalVariables internalVariables;

    protected string abilityKey;

    protected List<IUnit> unitList = new List<IUnit>();
    protected List<IBuilding> buildingList = new List<IBuilding>();
    protected List<Vector3Int> vectorList = new List<Vector3Int>();
    protected List<int> intList = new List<int>();
    protected List<IUnitBase> unitBaseList = new List<IUnitBase>();
    public int TargetCount => unitList.Count + buildingList.Count + vectorList.Count + intList.Count + unitBaseList.Count;

    public Vector3 Forward { get => transform.forward; set { transform.forward = value; } }

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

    protected UnitBaseData UnitData;

    public string Name => UnitData.name;
    public string Race => UnitData.race;
    public int ArmourType => UnitData.armourType;
    public int MovementType => UnitData.movementType;

    public int DayVision => UnitData.dayVision + buffs.Sum(x => x.dayVision);
    public int NightVision => UnitData.nightVision + buffs.Sum(x => x.nightVision);
    public int Vision => playerManager.IsDay ? TerrainVision(Tile.TerrainType, MovementType, DayVision) : TerrainVision(Tile.TerrainType, MovementType, NightVision);
    public int HPMax => UnitData.hP + buffs.Sum(x => x.hP);
    public int MPMax => UnitData.mP + buffs.Sum(x => x.mP);
    public int Armour => UnitData.armour + buffs.Sum(x => x.armour);
    public bool Charming => buffs.Select(x => x.notHostile).Contains(true);
    public bool Disarmed => buffs.Select(x => x.disarmed).Contains(true);
    public bool Silenced => buffs.Select(x => x.silenced).Contains(true);

    public bool CurrentTurn => SameTeam(playerManager.TeamTurn);

    public int HPCurrent => HPMax - DamageTaken;
    public int MPCurrent => MPMax - ManaUsed;
    public double HPPercentage => Math.Max((double)HPCurrent / HPMax, 0);
    public double MPPercentage => Math.Max((double)MPCurrent / MPMax, 0);

    public int Team { get => internalVariables.team; set { internalVariables.team = value; Tile.RefreshSprite(); } }

    public bool Actioned { get => internalVariables.actioned; set { internalVariables.actioned = value; Tile.RefreshSprite(); } } //alreadymoved and attacked
    public bool Invisible { get => internalVariables.invisible; set { internalVariables.invisible = value; Tile.RefreshSprite(); } }

    public IEnumerable<string> Abilities => UnitData.Abilities;

    public virtual TileObject Tile { get; protected set; }

    public virtual int CoverBonus => terrainDefenseMatrix[MovementType, Tile.TerrainType];

    protected class InternalVariables
    {
        public int team;
        public bool actioned;
        public bool invisible;
        public int damageTaken;
        public int manaUsed;
    }

    public static void EndAndStartNextTurn() { playerManager.IncrementTurn(); tileManager.EndAndStartNextTurn(playerManager.TeamTurn); }

    public virtual void Load(bool initial, Vector3Int localPlace, UnitBaseData data, int team)
    {
        tileManager.AddUnitBase(this);
        internalVariables = new InternalVariables();
        animator = gameObject.AddComponent<UnitAnimator>();
        animator.Load(this);
        UnitData = data;
        foreach (string s in data.Buffs)
        {
            buffs.Add(Buff.Load(this, s));
        }
        SnapMove(localPlace);
        EventsManager.OnObjectDestroyUnitBase += OnObjectDestroyUnitBase;
        EventsManager.OnObjectDestroyUnit += OnObjectDestroyUnit;
        EventsManager.OnObjectDestroyBuilding += OnObjectDestroyBuilding;
        eventsManager = Manager.EventsManager;
        assetManager = Manager.AssetManager;
    }

    public virtual bool SameTeam(int team_) => team_ == Team;

    public virtual void StartOfTurn() { }

    public virtual void RefreshSprite() => animator.RefreshUnitSprite();

    public virtual void MoveToTile(TileObject destination)
    {
        //Tile.Unit = null;
        //Tile.MoveUnitFromTileTo(destination, this);
        Tile = destination;
        //Tile.Unit = unit;
    }

    public virtual IEnumerator DestroyThis(IUnitBase killer)
    {
        yield return PlayAnimationAndFinish("Death");
        EventsManager.InvokeOnDeathUnitBase(this);
        EventsManager.InvokeOnKill(killer, this);
        tileManager.DestroyUnitBase(this); /*deference everything from here and change state to destroyed*/
        foreach (Buff buff in buffs) { buff.Destroy(); }
        EventsManager.OnObjectDestroyUnitBase -= OnObjectDestroyUnitBase;
        EventsManager.OnObjectDestroyUnit -= OnObjectDestroyUnit;
        //yield return animator.DestroyUnit();
        EventsManager.InvokeOnObjectDestroyUnitBase(this);
    }

    public virtual bool CanHit(IUnitBase unitBase, string attackType)
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

    public virtual IEnumerator CalculateDamageTakenAndTakeDamage(bool before, IUnitBase unit, int damageType, int damage)
    {
        //add take damage event here
        int damageCalculated = (int)Math.Round(GetResistance(damageType) * damage);
        if (before)
        {
            EventsManager.InvokeOnBeforeTakeDamage(this, unit, damageType, damage); //invoke before take damage event
        }
        else
        {
            //Debugger.AddToLog("Damage taken: " + damageCalculated);
            if (damageCalculated > 0)
            {
                yield return PlayAnimationAndFinish("Damage");
                DamageTaken = DamageTaken + damageCalculated;
                if (DamageTaken == HPMax)
                {
                    yield return DestroyThis(unit);
                }
                EventsManager.InvokeOnTakeDamage(this, unit, damageType, damage);                //invoke take damage event
            }
        }
    }

    public virtual IEnumerator AttackTarget(bool before, IUnitBase target, int baseDamage, int diceDamage, int diceTimes, int damageType, CodeObject animationCode)
    {
        if (before)
        {
            EventsManager.InvokeOnBeforeAttack(this, target);
        }
        else
        {
            if (animationCode != null)
            {
                yield return ParseAnimation(new StackItem(animationCode, this));
            }
            else
            {
                //yield return default animation behaviour
                Vector3 forward = transform.forward;
                RotateTo(target.Tile.LocalPlace);
                yield return PlayAnimationAndFinish("Attack");
                SetForward(forward);
            }
        }
        int totaldamage = CalculateAttackDamage(baseDamage, diceDamage, diceTimes, target.CoverBonus);
        yield return target.CalculateDamageTakenAndTakeDamage(before, this, damageType, totaldamage);
        if (!before) { EventsManager.InvokeOnAttack(this, target); }
    }

    public virtual IEnumerator SpawnUnit(bool before, List<Vector3Int> tile, string script, int unitTeam, CodeObject c)
    {
        if (before) { EventsManager.InvokeOnBeforeSpawnUnit(this); }//return null; }
        else
        {
            if (c != null)
            {
                yield return ParseAnimation(new StackItem(c, this));
            }
            else
            {
                //yield return PlayAnimationAndFinish("Spawn");
            }
            Actioned = true;
            List<IUnit> units = new List<IUnit>();
            foreach (Vector3Int t in tile)
            {
                IUnit unit = TileObject.TileAt(t).Unit == null ? assetManager.InstantiateUnit(false, t, script, unitTeam) : null;
                if (unit != null) { units.Add(unit); unit.Actioned = true; }
            }
            EventsManager.InvokeOnSpawnUnit(this, units);
            //return units;
        }
    }

    public CodeObject GetLogicCode(string s) => UnitData.GetLogicCode(s);

    public CodeObject GetTargetCode(string s) => UnitData.GetTargetCode(s);

    //public CodeObject GetAnimationCode(string s) => UnitData.GetAnimationCode(s);


    public bool HasTag(string tag) => (UnitData.HasTag(tag) || buffs.Select(x => x.HasTag(tag)).Contains(true));

    public string GetConvertedForm(string race) => UnitData.GetConvertedForm(race);

    public bool IsPlaying(string animation) => animator.IsPlaying(animation);

    public void Animate(string animation) => animator.Animate(animation);

    //public IEnumerator PlayWaitForDuration(float v) => animator.PlayWaitForDuration(v);

    public IEnumerator PlayAnimationAndFinish(string v) => animator.PlayAnimationAndFinish(v);

    public void AddToStack(CodeObject abilityLogicCode, string abilityKey, List<int> iList, List<IUnitBase> uBList, List<IUnit> uList, List<IBuilding> bList, List<Vector3Int> vList, bool mainPhase = false) => eventsManager.AddToStack(abilityLogicCode, abilityKey, this, iList, uBList, uList, bList, vList, mainPhase);

    public void SnapMove(Vector3Int v)
    {
        transform.position = LocalToWorld(v);
    }

    public void SetForward(Vector3 v)
    {
        Forward = v;
    }

    public void RotateTo(Vector3Int v)
    {
        if (Vector3.Distance(LocalToWorld(v), transform.position) > 0.01f)
        {
            transform.forward = LocalToWorld(v) - transform.position;
        }
    }

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

    public void ChooseMenuAbility(string s) { abilityKey = s; GlobalParser.ChooseMenuAbility(abilityKey, GetTargetCode(abilityKey), GetLogicCode(abilityKey), this); }

    public void CommitTarget(Vector3Int target)
    {
        if (ValidateTargetAndCommit(abilityKey, GetTargetCode(abilityKey), this, target, buildingList, unitList, unitBaseList, vectorList, intList))
        {
            AddToStack(GetLogicCode(abilityKey), abilityKey, intList, unitBaseList, unitList, buildingList, vectorList, true);
            TileObject.WipeTiles();
            if (GetTargetCode(abilityKey).Task == "Spell")
            {
                //trigger spell target most likely here since the trigger only exists before resolution

            }
        }
    }

    public bool VisibleAndHostileTo(int team_)
    {
        if (!SameTeam(team_) && !Charming)
        {
            //TileObject tileOfTarget = Tile;
            //int targetTerrain = tileOfTarget.TerrainType;
            if (team_ == playerManager.TeamTurn) { return !Invisible && Tile.CanSee; };
            return tileManager.VisibleAndHostileTo(team_, this);
            /*
            foreach (IUnitBase unitLooking in unitBases)
            {
                if (unitLooking.SameTeam(team_))
                {
                    TileObject tileOfLooker = unitLooking.Tile;
                    int terrain = tileOfLooker.TerrainType;
                    int movement = unitLooking.MovementType;
                    //int vision = Manager.PlayerManager.IsDay ? TerrainVision(terrain, movement, unitLooking.DayVision) : TerrainVision(terrain, movement, unitLooking.NightVision);
                    if (Distance(tileOfLooker.LocalPlace, tileOfTarget.LocalPlace) > unitLooking.Vision)
                    {
                        seen = false;
                    }
                    else if (CanSee(terrain, targetTerrain, movement))
                    {
                        seen = !Invisible;
                        if (seen)
                        { return seen; }
                    }
                }
            }*/
        }
        return false;
    }


    public bool WithinRange(int min, int max, IUnitBase targetUnit)
    {
        return Distance(targetUnit.Tile.LocalPlace, Tile.LocalPlace) >= min && Distance(targetUnit.Tile.LocalPlace, Tile.LocalPlace) <= max;
    }

    public bool CanAttackAndHostileTo(IUnitBase defender, string attackType)
    {
        return CanHit(defender, attackType) && defender.VisibleAndHostileTo(Team) && defender.HPCurrent > 0;
    }

    public IEnumerable<TileObject> AddTargetTiles(int min, int max)
    {
        foreach (Vector3Int v in CircleCoords(min, max, Tile.LocalPlace))
        {
            if (tileManager.TileAt(v) != null)
            {
                yield return tileManager.TileAt(v);
            }
        }
    }

    protected int CalculateAttackDamage(int baseDamage, int diceDamage, int diceTimes, int cover)
    {
        return (int)Math.Round(HPPercentage * Rolldamage(baseDamage, diceDamage, diceTimes, cover));
    }

    protected void ChangeForm(string form)
    {
        UnitData = assetManager.LoadUnitBaseData(form);
        animator.ChangeModel(form);
    }

    /*
    public void AddBuff(Buff buff)
    {
        buffs.Add(buff);
        //some buffs may not stack but rather in duration
    }
    */
    protected void OnObjectDestroyUnitBase(IUnitBase unit) { unitBaseList.Remove(unit); }

    protected void OnObjectDestroyUnit(IUnit unit) { unitList.Remove(unit); }

    protected void OnObjectDestroyBuilding(IBuilding unit) { buildingList.Remove(unit); }

    /*
    protected void OnObjectDestroyUnitBase(IUnitBase unitBase)
    {
        //unitBaseData.Remove(unitBase);
        if (unitBaseList.Contains(unitBase))
        {
            unitBaseList[unitBaseList.FindIndex(ind => ind.Equals(unitBase))] = new NullUnitBase();
        }
    }

    protected void OnObjectDestroyUnit(IUnit unit)
    {
        //unitData.Remove(unit);
        if (unitList.Contains(unit))
        {
            unitList[unitList.FindIndex(ind => ind.Equals(unit))] = new NullUnit();
        }
    }

    protected void OnObjectDestroyBuilding(IBuilding building)
    {
        //buildingData.Remove(building);
        if (buildingList.Contains(building))
        {
            buildingList[buildingList.FindIndex(ind => ind.Equals(building))] = new NullBuilding();
        }
    }
    */
}
