using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GlobalData;
using static GlobalFunctions;
using static GlobalParser;
using System.Collections;

public abstract class UnitBase : MonoBehaviour
{
    protected UnitAnimator animator;
    protected UnitBaseData data;
    protected InternalVariables internalVariables;

    //protected string[] AbilityVariables => GetCode(abilityKey).Split(' ');
    protected CodeObject AbilityTargetCode => GetTargetCode(abilityKey);
    protected CodeObject AbilityLogicCode => GetLogicCode(abilityKey);
    protected CodeObject AbilityAnimation => GetAnimationCode(abilityKey);
    protected string abilityKey;
    protected List<Vector3Int> targetList = new List<Vector3Int>();
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
    public int ArmourType => data.armourType;
    public int MovementType => data.movementType;
    //public bool Biological => data.Biological;
    //public bool Mechanical => data.Mechanical;
    //public bool Magical => data.Magical;

    public virtual string Race { get => data.race; protected set { } }
    public virtual int Team { get => internalVariables.team; set { internalVariables.team = value; RefreshSprite(); } }

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

    public bool Actioned { get => internalVariables.actioned; set { internalVariables.actioned = value; RefreshSprite(); } } //alreadymoved and attacked
    public bool Invisible { get => internalVariables.invisible; set { internalVariables.invisible = value; RefreshSprite(); } }

    public IEnumerable<string> Abilities => data.Abilities;

    public TileObject Tile => TileManager.globalInstance.GetTile(this);

    public virtual int CoverBonus
    {
        get
        {
            if (MovementType == 6 || MovementType == 7)
            {
                return skyDefenseMatrix[MovementType, Tile.SkyTerrainType];
            }
            else { return terrainDefenseMatrix[MovementType, Tile.TerrainType]; }
        }
    }

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
    
    public virtual void ExecuteChosenAbility(string s)
    {
        ////parse code and execute based on string s
        //where stack may begin if the spell has no targets
        abilityKey = s;
        switch (AbilityTargetCode.Task)
        {
            //tasks can be spells, attacks or abilities. attacks are always (multi) target by default (ie cannot target ground)
            case "Attack":
                List<Vector3Int> targets = new List<Vector3Int>();
                foreach (Vector3Int v in TileManager.globalInstance.AddTargetTiles(int.Parse(AbilityTargetCode.GetVariable("minRange")), int.Parse(AbilityTargetCode.GetVariable("maxRange"))))
                {
                    if ((TileManager.globalInstance.HostileAttackableBuildingOnTile(this, v, AbilityTargetCode.GetVariable("canHit")) || TileManager.globalInstance.HostileAttackableUnitOnTile(this, v, AbilityTargetCode.GetVariable("canHit")))) { targets.Add(v); }
                }
                TileManager.globalInstance.SetUpTargetTiles(targets);
                Pointer.globalInstance.GoToAttackingMode();
                break;
        }
    }

    public virtual void StartOfTurn() { }

    protected virtual void DestroyThis() => TileManager.globalInstance.DestroyUnit(this);

    protected virtual void TakeDamage(UnitBase unit, int damagetype, int damage)
    {
        DamageTaken = DamageTaken + (int)Math.Round(GetResistance(damagetype) * damage);
        /*
        else
        {
            //animator.Animate("Damage");
        }*/
    }

    protected virtual void AddToMenu(string s, List<string> menu)
    {
        //parse code to see if there are valid targets
        //todo: incorporate min targets
        List<Vector3Int> potentialTargets = new List<Vector3Int>();
        switch (GetTargetCode(s).Task)
        {
            case "Attack":
                if (!Disarmed)
                {
                    foreach (Vector3Int v in TileManager.globalInstance.AddTargetTiles(int.Parse(GetTargetCode(s).GetVariable("minRange")), int.Parse(GetTargetCode(s).GetVariable("maxRange"))))
                    {
                        if ((TileManager.globalInstance.HostileAttackableBuildingOnTile(this, v, s) || TileManager.globalInstance.HostileAttackableUnitOnTile(this, v, s)))
                        {
                            menu.Add(s);
                            return;
                        }
                    }
                }
                //explore to see if theres enemies
                break;
            case "Spell":
                //explore to see if theres targets
                if (/*TileManager.globalInstance.AnyTargets(targetAbility[1], targetAbility[2],int.Parse(targetAbility[3]), int.Parse(targetAbility[4]))*/ !Silenced)
                {
                    menu.Add(s);
                }
                break;
        }
    }

    public bool HasTag(string tag) => (data.HasTag(tag) || buffs.Select(x => x.HasTag(tag)).Contains(true));

    public string GetConvertedForm(string race) => data.GetConvertedForm(race);

    public void RefreshSprite() => animator.RefreshSprite();

    public void RefreshBuildingSprite() => animator.RefreshBuildingSprite();

    public bool IsPlaying(string animation) => animator.IsPlaying(animation);

    public void Animate(string animation) => animator.Animate(animation);

    public IEnumerator ParseAnimation(StackItem animation) { yield return StartCoroutine(animator.ParseAnimation(animation)); }

    public void SetUp(List<string> menu)
    {
        foreach (string s in Abilities)
        {
            AddToMenu(s, menu);
        }
    }

    public void ClearTargets()
    {
        targetList.Clear();
    }

    public float GetResistance(int damagetype)
    {
        //check buffs
        return attackToArmour[damagetype, ArmourType];
    }

    public void CommitTarget(Vector3Int target)
    {
        //parse code
        //where stack may begin
        //if targets have to be unique from eachother
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
        if (/*AbilityTargetCode.Task == "Attack" &&*/ targetList.Count == int.Parse(AbilityTargetCode.GetVariable("targets")))
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
        abilityKey = "";
    }

    public void Attack(bool before, UnitBase target, int baseDamage, int diceDamage, int diceTimes, int damageType)
    {
        if (before) { EventsManager.InvokeOnBeforeAttack(this, target); }
        else
        {
            /*roll damage from current ability code*/ //(int)Math.Round((double)(currenthp / HP()) * Rolldamage(Damage(index), damagevariance[index]));
                                                      //UnitBase t = TileManager.globalInstance.GetUnitOrBuilding(target);
            int cover = target.CoverBonus;
            int totaldamage = (int)Math.Round(HPPercentage * Rolldamage(baseDamage, diceDamage, diceTimes, cover));
            //animator.Animate(targetAbility[8]);
            //UnitTransformManager.globalInstance.QueueAnimation(this, targetAbility[8], target);
            target.TakeDamage(this, damageType, totaldamage);
            //t.CounterAttack(this);
            //t.GetAttackedAndCounter(this, DamageType, totaldamage);
            EventsManager.InvokeOnAttack(this, target);
        }
    }

    public bool CanHit(UnitBase unitbase, string attackType)
    {
        bool b = false;
        switch (attackType)
        {
            case "same"://flying hits flying, land hits land
                b = ((MovementType != 6 || MovementType != 7) && (unitbase.MovementType != 6 || unitbase.MovementType != 7)) || ((MovementType == 6 || MovementType == 7) && (unitbase.MovementType == 6 || unitbase.MovementType == 7));
                break;
            case "different"://flying hits land, land hits flying
                b = ((MovementType != 6 || MovementType != 7) && (unitbase.MovementType == 6 || unitbase.MovementType == 7)) || ((MovementType == 6 || MovementType == 7) && (unitbase.MovementType != 6 || unitbase.MovementType != 7));
                break;
            case "all":
                b = true;
                break;
        }
        return b;
    }

    public bool CanCounterAttack(UnitBase targetunit)
    {
        foreach (string s in Abilities)
        {
            if (GetTargetCode(s).Task == "Attack" && ValidateTargetForCounterAttack(targetunit, s)/* todo move target validation to triggerpart of the stackchain*/)
            {
                return true;
            }
        }
        return false;
    }

    public void CounterAttack(bool before, UnitBase targetunit)
    {
        foreach (string s in Abilities)
        {
            //string[] ability = GetCode(s).Split(' ');
            CodeObject ability = GetTargetCode(s);
            if (ability.Task == "Attack" && ValidateTargetForCounterAttack(targetunit, s)/* todo move target validation to triggerpart of the stackchain*/)
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
                Parse(new StackItem(GetLogicCode(s), s, this, GetAnimationCode(s), null, new List<UnitBase>() { targetunit }, null, null, null), null, before);
                return;
            }
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

    protected bool ValidateTargetForCounterAttack(UnitBase targetunit, string s)
    {
        CodeObject ability = GetTargetCode(s);
        return CanHit(targetunit, ability.GetVariable("canHit")) && TileManager.globalInstance.VisibleTo(this, targetunit) && TileManager.globalInstance.WithinRange(int.Parse(ability.GetVariable("minRange")), int.Parse(ability.GetVariable("maxRange")), this, targetunit) && TileManager.globalInstance.HostileTo(this, targetunit);
    }

    /*
    public void AddBuff(Buff buff)
    {
        buffs.Add(buff);
        //some buffs may not stack but rather in duration
    }*/
}
