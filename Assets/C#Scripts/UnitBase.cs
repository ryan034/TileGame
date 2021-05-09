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
    protected CodeObject AbilityCode => GetCode(abilityKey);
    protected CodeObject AbilityAnimation => GetAnimation(abilityKey);
    protected string abilityKey;
    protected List<Vector3Int> targetList = new List<Vector3Int>();
    protected List<Buff> buffs = new List<Buff>();

    protected int DamageTaken
    {
        get => internalVariables.damageTaken;
        set
        {
            if (internalVariables.damageTaken < HP && internalVariables.damageTaken < value)
            {
                if (value >= HP)
                {
                    DestroyThis();
                    internalVariables.damageTaken = HP;
                    //Takedamage_v(damagetype, damage);
                    //unit.Dealtdamage_v(damagetype, damage);
                    //parse code
                }
                else
                {
                    internalVariables.damageTaken = value;
                }
            }
            //need logic for healing, specifically considering building cant heal from 0
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
    public virtual int Team { get => internalVariables.team; set { if (internalVariables.team != value) { internalVariables.team = value; RefreshSprite(); } } }

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
    public int DamageType => int.Parse(AbilityCode.GetVariable("damageType"));

    public bool Actioned { get => internalVariables.actioned; set { if (internalVariables.actioned != value) { internalVariables.actioned = value; RefreshSprite(); } } } //alreadymoved and attacked
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
        this.data = data;
        //Team = team;
        foreach (string s in data.Buffs)
        {
            buffs.Add(Buff.Load(this, s));
        }
        UnitTransformManager.globalInstance.SnapMove(this, localPlace);
    }

    //code=> attack:flying or not:min range: max range:base damage:dicedamage:times:damagetype: animationstring
    public virtual void ExecuteChosenAbility(string s)
    {
        ////parse code and execute based on string s
        //where stack may begin if the spell has no targets
        switch (AbilityCode.Task)
        {
            case "Attack":
                List<Vector3Int> targets = new List<Vector3Int>();
                foreach (Vector3Int v in TileManager.globalInstance.AddTargetTiles(int.Parse(AbilityCode.GetVariable("minRange")), int.Parse(AbilityCode.GetVariable("maxRange"))))
                {
                    if ((TileManager.globalInstance.HostileAttackableBuildingOnTile(this, v, abilityKey) || TileManager.globalInstance.HostileAttackableUnitOnTile(this, v, abilityKey))) { targets.Add(v); }
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
        targetList.Add(target);
        if (targetList.Count == int.Parse(AbilityCode.GetVariable("targets")))
        {
            switch (AbilityCode.Task)
            {
                case "Attack":
                    List<UnitBase> l = new List<UnitBase>() { this };
                    foreach (Vector3Int v in targetList) { l.Add(TileManager.globalInstance.GetUnitOrBuilding(v)); }
                    EventsManager.globalInstance.AddToStack(AbilityCode, abilityKey, this, AbilityAnimation, null, l);
                    for (int i = 1; i < l.Count; i++) { EventsManager.InvokeOnBeforeAttack(this, l[i]); }
                    targetList.Clear();
                    abilityKey = "";
                    TileManager.globalInstance.EndUnitTurn();
                    break;
            }
        }
    }

    public virtual void StartOfTurn()
    { }

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
        switch (GetCode(s).Task)
        {
            case "Attack":
                if (!Disarmed)
                {
                    foreach (Vector3Int v in TileManager.globalInstance.AddTargetTiles(int.Parse(GetCode(s).GetVariable("minRange")), int.Parse(GetCode(s).GetVariable("maxRange"))))
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

    protected bool ValidateTargetForCounterAttack(UnitBase targetunit, string s)
    {
        //string[] ability = GetCode(s).Split(' ');
        bool b = false;
        CodeObject ability = GetCode(s);
        switch (ability.GetVariable("canHit"))
        {
            case "same"://flying hits flying, land hits land
                b = ((MovementType != 6 || MovementType != 7) && (targetunit.MovementType != 6 || targetunit.MovementType != 7)) || ((MovementType == 6 || MovementType == 7) && (targetunit.MovementType == 6 || targetunit.MovementType == 7));
                break;
            case "different"://flying hits land, land hits flying
                b = ((MovementType != 6 || MovementType != 7) && (targetunit.MovementType == 6 || targetunit.MovementType == 7)) || ((MovementType == 6 || MovementType == 7) && (targetunit.MovementType != 6 || targetunit.MovementType != 7));
                break;
            case "all":
                b = true;
                break;
        }
        //return b && TileManager.globalInstance.CanSee(this, targetunit) && Team != targetunit.Team && !targetunit.Invisible && targetunit.HPCurrent > 0 && !targetunit.NotHostile && Distance(targetunit.Tile.LocalPlace, Tile.LocalPlace) >= int.Parse(ability[2]) && Distance(targetunit.Tile.LocalPlace, Tile.LocalPlace) <= int.Parse(ability[3]);
        return b && TileManager.globalInstance.VisibleTo(this, targetunit) && TileManager.globalInstance.WithinRange(int.Parse(ability.GetVariable("minRange")), int.Parse(ability.GetVariable("maxRange")), this, targetunit) && TileManager.globalInstance.HostileTo(this, targetunit);
    }


    public bool HasTag(string tag) => (data.HasTag(tag) || buffs.Select(x => x.HasTag(tag)).Contains(true));

    public string GetConvertedForm(string race) => data.GetConvertedForm(race);

    public void RefreshSprite() => animator.RefreshSprite();

    public bool IsPlaying(string animation) => animator.IsPlaying(animation);

    public void Animate(string animation) => animator.Animate(animation);

    public IEnumerator ParseAnimation(StackItem animation) { yield return animator.ParseAnimation(animation); }

    //protected void ChangeModel(string model) => animator.ChangeModel(model);

    protected void ChangeForm(string form)
    {
        data = AssetManager.globalInstance.LoadUnitBaseData(form);
        animator.ChangeModel(form);
    }

    protected CodeObject GetCode(string s) => data.GetCode(s);

    protected CodeObject GetAnimation(string s) => data.GetAnimation(s);

    public void SetUp(List<string> menu)
    {
        foreach (string s in Abilities)
        {
            AddToMenu(s, menu);
        }
    }

    public void CounterAttack(UnitBase targetunit)
    {
        foreach (string s in Abilities)
        {
            //string[] ability = GetCode(s).Split(' ');
            CodeObject ability = GetCode(s);
            if (ability.Task == "Attack" && ValidateTargetForCounterAttack(targetunit, s)/* todo move target validation to triggerpart of the stackchain*/)
            {
                //code=> attack:flying or not: siege or not:min range: max range:base damage:dicedamage:times:damagetype
                int cover = targetunit.CoverBonus;
                int totaldamage = (int)Math.Round(HPPercentage * Rolldamage(int.Parse(ability.GetVariable("baseDamage")), int.Parse(ability.GetVariable("diceDamage")), int.Parse(ability.GetVariable("diceTimes")), cover));
                targetunit.TakeDamage(this, int.Parse(ability.GetVariable("damageType")), totaldamage);
                //UnitTransformManager.globalInstance.QueueAnimation(this, ability[8], targetunit.Tile.LocalPlace);
                EventsManager.InvokeOnCounterAttack(this, targetunit);
                return;
            }
        }
    }

    public bool CanCounterAttack(UnitBase targetunit)
    {
        foreach (string s in Abilities)
        {
            //string[] ability = GetCode(s).Split(' ');
            if (GetCode(s).Task == "Attack" && ValidateTargetForCounterAttack(targetunit, s)/* todo move target validation to triggerpart of the stackchain*/)
            {
                return true;
            }
        }
        return false;
    }

    public void Attack(UnitBase target)
    {
        /*roll damage from current ability code*/ //(int)Math.Round((double)(currenthp / HP()) * Rolldamage(Damage(index), damagevariance[index]));
        //UnitBase t = TileManager.globalInstance.GetUnitOrBuilding(target);
        int cover = target.CoverBonus;
        int totaldamage = (int)Math.Round(HPPercentage * Rolldamage(int.Parse(AbilityCode.GetVariable("baseDamage")), int.Parse(AbilityCode.GetVariable("diceDamage")), int.Parse(AbilityCode.GetVariable("diceTimes")), cover));
        //animator.Animate(targetAbility[8]);
        //UnitTransformManager.globalInstance.QueueAnimation(this, targetAbility[8], target);
        target.TakeDamage(this, DamageType, totaldamage);
        //t.CounterAttack(this);
        //t.GetAttackedAndCounter(this, DamageType, totaldamage);
        EventsManager.InvokeOnAttack(this, target);
    }

    /*
    public void AddBuff(Buff buff)
    {
        buffs.Add(buff);
        //some buffs may not stack but rather in duration
    }*/

    public float GetResistance(int damagetype)
    {
        //check buffs
        return attackToArmour[damagetype, ArmourType];
    }

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

    public void ClearTargets()
    {
        targetList.Clear();
    }

    public bool CanHit(UnitBase unitbase, string s)
    {
        bool b = false;
        switch (GetCode(s).GetVariable("canHit"))
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

    /*
    public bool ParseBool(string event_, string trigger, List<UnitBase> list = null, List<int> listInt = null)
    {
        if (trigger == "") { return true; }
        // if trigger  == validhostile target return validhostiletarget 
        if (event_ == "OnAttack" && trigger == "this") { return (list[0] == this && CanCounterAttack(list[1])); }
        return false;
    }
    */
    public void Parse(StackItem stack, CodeObject code = null)
    {
        if (code == null) { code = stack.code; }
        if (code.IsConditional)
        {
            bool v = ParseCurrentBool(code, stack.unitData, stack.intData);
            if (v)
            {
                foreach (CodeObject c in code.GetCodeObjects("true"))
                {
                    Parse(stack, c);
                }
            }
            else
            {
                foreach (CodeObject c in code.GetCodeObjects("false"))
                {
                    Parse(stack, c);
                }
            }
        }
        else
        {
            ParseCurrent(code, stack);
            foreach (CodeObject c in code.GetCodeObjects("next"))
            {
                Parse(stack, c);
            }
        }
    }

    public bool ParseBool(CodeObject filter, List<UnitBase> list = null, List<int> listInt = null)
    {
        //throw new NotImplementedException();// general control flow
        bool v = ParseCurrentBool(filter, list, listInt);
        if (v)
        {
            foreach (CodeObject c in filter.GetCodeObjects("true"))
            {
                //parse the code
                bool b = false;
                b = ParseBool(c, list, listInt) || b;
                if (b) { return true; }
            }
        }
        else
        {
            foreach (CodeObject c in filter.GetCodeObjects("false"))
            {
                bool b = false;
                b = ParseBool(c, list, listInt) || b;
                if (b) { return true; }
            }
        }
        return false;
    }

    protected bool ParseCurrentBool(CodeObject filter, List<UnitBase> list, List<int> listInt)
    {

        if (filter.Task == "CanCounterAttack")
        {
            if (filter.GetVariable("from") == "")
            {
                return CanCounterAttack(list[int.Parse(filter.GetVariable("to"))]);
            }
            else
            {
                return list[int.Parse(filter.GetVariable("from"))].CanCounterAttack(list[int.Parse(filter.GetVariable("to"))]);
            }
        }
        if (filter.GetVariable("scope") == "all") { return true; }
        // if trigger  == validhostile target return validhostiletarget 
        if (filter.Task == "OnAttack" && filter.GetVariable("scope") == "self" && filter.GetVariable("side") == "defender") { return (list[1] == this); }
        if (filter.Task == "OnBeforeAttack" && filter.GetVariable("scope") == "self" && filter.GetVariable("side") == "defender") { return (list[1] == this) /*CanCounterAttack(list[0])*/; }
        return false;
    }

    public void ParseCurrent(CodeObject filter, StackItem stack)
    {
        if (stack.code.Task == "Attack")
        {
            for (int i = 1; i < stack.unitData.Count; i++)
            {
                stack.unitData[0].Attack(stack.unitData[i]);
            }
        }
        if (stack.code.Task == "CounterAttack")
        {
            if (stack.code.GetVariable("from") == "")
            {
                CounterAttack(stack.unitData[int.Parse(stack.code.GetVariable("to"))]);
            }
            else
            {
                stack.unitData[int.Parse(stack.code.GetVariable("from"))].CounterAttack(stack.unitData[int.Parse(stack.code.GetVariable("to"))]);
            }
        }
    }
}
