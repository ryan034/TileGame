using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Globals;
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

    public virtual void ParseCode(CodeObject code, StackItem data, bool before)
    {
        switch (code.Task)
        {
            case "Attack":
                for (int i = 1; i < data.unitBaseData.Count; i++)
                {
                    data.unitBaseData[0].Attack(before, data.unitBaseData[i], int.Parse(code.GetVariable("baseDamage")), int.Parse(code.GetVariable("diceDamage")), int.Parse(code.GetVariable("diceTimes")), int.Parse(code.GetVariable("damageType")));
                }
                break;
            case "CounterAttack":
                /*
                if (stack.code.GetVariable("from") == "") { CounterAttack(stack.unitBaseData[int.Parse(stack.code.GetVariable("to"))]); }
                else { stack.unitBaseData[int.Parse(stack.code.GetVariable("from"))].CounterAttack(stack.unitBaseData[int.Parse(stack.code.GetVariable("to"))]); }
                break;*/
                UnitBase u;
                string toCode = code.GetVariable("to");
                string fromCode = code.GetVariable("from");
                if (fromCode != null)
                {
                    switch (fromCode[0])
                    {
                        case 'u':
                            //return u.CanCounterAttack(listUnit[int.Parse(toCode.Substring(1))]);
                            u = data.unitData[int.Parse(fromCode.Substring(1))];
                            break;
                        case 'b':
                            u = data.buildingData[int.Parse(fromCode.Substring(1))];
                            //return u.CanCounterAttack(listBuilding[int.Parse(toCode.Substring(1))]);
                            break;
                        default:
                            u = data.unitBaseData[int.Parse(fromCode)];
                            //return u.CanCounterAttack(list[int.Parse(toCode)]);
                            break;
                    }
                }
                else { u = this; }
                switch (toCode[0])
                {
                    case 'u':
                        u.CounterAttack(before, data.unitData[int.Parse(toCode.Substring(1))]);
                        break;
                    case 'b':
                        u.CounterAttack(before, data.buildingData[int.Parse(toCode.Substring(1))]);
                        break;
                    default:
                        u.CounterAttack(before, data.unitBaseData[int.Parse(toCode)]);
                        break;
                }
                break;
        }
    }

    //code=> attack:flying or not:min range: max range:base damage:dicedamage:times:damagetype: animationstring
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

    public virtual void CommitTarget(Vector3Int target)
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

    public void Parse(StackItem stack, CodeObject code = null, bool before = false)
    {
        if (code == null) { code = stack.code; }
        if (code.IsConditional)
        {
            bool v = ParseControlFlowBool(code, stack.unitBaseData, stack.unitData, stack.buildingData, stack.intData);
            if (v)
            {
                foreach (CodeObject c in code.GetCodeObjects("true"))
                {
                    Parse(stack, c, before);
                }
            }
            else
            {
                foreach (CodeObject c in code.GetCodeObjects("false"))
                {
                    Parse(stack, c, before);
                }
            }
        }
        else
        {
            ParseCode(code, stack, before);
            foreach (CodeObject c in code.GetCodeObjects("next"))
            {
                Parse(stack, c, before);
            }
        }
    }

    public bool ParseBool(CodeObject filter, List<UnitBase> list = null, List<Unit> listUnit = null, List<Building> listBuilding = null, List<int> listInt = null)
    {
        //throw new NotImplementedException();// general control flow
        bool v = ParseControlFlowBool(filter, list, listUnit, listBuilding, listInt);
        bool b;
        if (v)
        {
            foreach (CodeObject c in filter.GetCodeObjects("true"))
            {
                //parse the code
                b = false;
                b = ParseBool(c, list, listUnit, listBuilding, listInt) || b;
                if (b) { return true; }
            }
        }
        else
        {
            foreach (CodeObject c in filter.GetCodeObjects("false"))
            {
                b = false;
                b = ParseBool(c, list, listUnit, listBuilding, listInt) || b;
                if (b) { return true; }
            }
        }
        return v;
    }

    protected bool ParseControlFlowBool(CodeObject filter, List<UnitBase> list, List<Unit> listUnit = null, List<Building> listBuilding = null, List<int> listInt = null)
    {
        if (filter.Task == "CanCounterAttack")
        {
            //return CanCounterAttack(list[int.Parse(filter.GetVariable("to"))]);
            UnitBase u;
            string toCode = filter.GetVariable("to");
            string fromCode = filter.GetVariable("from");
            if (fromCode != null)
            {
                switch (fromCode[0])
                {
                    case 'u':
                        //return u.CanCounterAttack(listUnit[int.Parse(toCode.Substring(1))]);
                        u = listUnit[int.Parse(fromCode.Substring(1))];
                        break;
                    case 'b':
                        u = listBuilding[int.Parse(fromCode.Substring(1))];
                        //return u.CanCounterAttack(listBuilding[int.Parse(toCode.Substring(1))]);
                        break;
                    default:
                        u = list[int.Parse(fromCode)];
                        //return u.CanCounterAttack(list[int.Parse(toCode)]);
                        break;
                }
            }
            else { u = this; }
            switch (toCode[0])
            {
                case 'u':
                    return u.CanCounterAttack(listUnit[int.Parse(toCode.Substring(1))]);
                case 'b':
                    return u.CanCounterAttack(listBuilding[int.Parse(toCode.Substring(1))]);
                default:
                    return u.CanCounterAttack(list[int.Parse(toCode)]);
            }
        }
        if (filter.GetVariable("scope") == "all") { return true; }
        if (filter.Task == "OnAttack" && filter.GetVariable("scope") == "self" && filter.GetVariable("side") == "defender") { return (list[1] == this); }
        //if (filter.Task == "OnBeforeAttack" && filter.GetVariable("scope") == "self" && filter.GetVariable("side") == "defender") { return (list[1] == this) /*CanCounterAttack(list[0])*/; }
        return false;
    }

    //protected void ChangeModel(string model) => animator.ChangeModel(model);
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

    protected void Attack(bool before, UnitBase target, int baseDamage, int diceDamage, int diceTimes, int damageType)
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

    protected bool CanCounterAttack(UnitBase targetunit)
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

    protected void CounterAttack(bool before, UnitBase targetunit)
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

    /*
    public void AddBuff(Buff buff)
    {
        buffs.Add(buff);
        //some buffs may not stack but rather in duration
    }*/

    /*
    public virtual void Setup(List<string> menu) { }
    public virtual void Attack_v(Unitbase target) { }
    public virtual void Getattacked_v(Unitbase unit, int damagetype, int damage) { }
    public virtual void Dealtdamage_v(int damagetype, int damage) { }
    public virtual void Takedamage_v(int damagetype, int damage) { }
    public virtual void Destroy_v(Unitbase unit) {}
    public virtual void Destroyed_v() {}
    public virtual bool Validunittarget_v(Tileobject tile, int index) { return true; }
    public virtual bool Validbuildingtarget_v(Tileobject tile, int index) { return true; }
    */
    /// 
}
