using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalParser
{

    public static bool Parse(CodeObject filter, UnitBase owner, List<UnitBase> list = null, List<Unit> listUnit = null, List<Building> listBuilding = null, List<int> listInt = null)
    {
        bool v = ParseConditionalControlFlowCode(filter, owner, list, listUnit, listBuilding, listInt);
        bool b;
        if (v)
        {
            foreach (CodeObject c in filter.GetCodeObjects("true"))
            {
                b = false;
                b = Parse(c, owner, list, listUnit, listBuilding, listInt) || b;
                if (b) { return true; }
            }
        }
        else
        {
            foreach (CodeObject c in filter.GetCodeObjects("false"))
            {
                b = false;
                b = Parse(c, owner, list, listUnit, listBuilding, listInt) || b;
                if (b) { return true; }
            }
        }
        return v;
    }

    public static void Parse(StackItem stack, CodeObject code = null, bool before = false)
    {
        if (code == null) { code = stack.code; }
        if (code.IsConditional)
        {
            bool v = ParseConditionalControlFlowCode(code, stack);
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

    public static bool ParseConditionalControlFlowCode(CodeObject code, StackItem data) { return ParseConditionalControlFlowCode(code, data.owner, data.unitBaseData, data.unitData, data.buildingData, data.intData); }

    public static bool ParseConditionalControlFlowCode(CodeObject conditional, UnitBase owner, List<UnitBase> list, List<Unit> listUnit, List<Building> listBuilding, List<int> listInt)
    {
        if (conditional.Task == "CanCounterAttack")
        {
            //return CanCounterAttack(list[int.Parse(filter.GetVariable("to"))]);
            UnitBase u;
            string toCode = conditional.GetVariable("to") == "" ? "0" : conditional.GetVariable("to");
            string fromCode = conditional.GetVariable("from") == "" ? "0" : conditional.GetVariable("from");
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
        if (conditional.GetVariable("scope") == "all") { return true; }
        if (conditional.Task == "OnAttack" && conditional.GetVariable("scope") == "self" && conditional.GetVariable("side") == "defender") { return (list[1] == owner); }
        //if (filter.Task == "OnBeforeAttack" && filter.GetVariable("scope") == "self" && filter.GetVariable("side") == "defender") { return (list[1] == this) /*CanCounterAttack(list[0])*/; }
        return false;
    }

    public static void ParseCode(CodeObject code, StackItem data, bool before)
    {
        switch (code.Task)
        {
            case "Attack":// only refers to main attack
                //UnitBase fromUnit = code.GetVariable("from") == "" ? data.owner : data.unitBaseData[int.Parse(code.GetVariable("from"))];
                //int toStart = code.GetVariable("toStart") == "" ? 0 : int.Parse(code.GetVariable("toStart"));
                //int toNumber = code.GetVariable("toNumber") == "" ? data.unitBaseData.Count : int.Parse(code.GetVariable("toNumber"));
                data.owner.Attack(before, data.unitBaseData, int.Parse(code.GetVariable("baseDamage")), int.Parse(code.GetVariable("diceDamage")), int.Parse(code.GetVariable("diceTimes")), int.Parse(code.GetVariable("damageType")));
                break;
            case "CounterAttack": //refers to any attack thats not main attack
                /*
                if (stack.code.GetVariable("from") == "") { CounterAttack(stack.unitBaseData[int.Parse(stack.code.GetVariable("to"))]); }
                else { stack.unitBaseData[int.Parse(stack.code.GetVariable("from"))].CounterAttack(stack.unitBaseData[int.Parse(stack.code.GetVariable("to"))]); }
                break;*/
                UnitBase u;
                string toCode = code.GetVariable("to") == "" ? "0" : code.GetVariable("to");
                string fromCode = code.GetVariable("from") == "" ? "0" : code.GetVariable("from");
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
            case "Capture":
                int to = code.GetVariable("to") == "" ? 0 : int.Parse(code.GetVariable("to"));
                int from = code.GetVariable("from") == "" ? 0 : int.Parse(code.GetVariable("from"));
                data.unitData[from].Capture(before, data.buildingData[to], int.Parse(code.GetVariable("captureDamage")));
                return;
            case "SpawnUnit":
                from = code.GetVariable("from") == "" ? 0 : int.Parse(code.GetVariable("from"));
                data.unitBaseData[from].SpawnUnit(before, data.vectorData, code.GetVariable("unitID"), data.unitBaseData[0].Team);
                return;
        }
    }

    public static void CommitTarget(string abilityKey, CodeObject abilityTargetCode, CodeObject abilityLogicCode, CodeObject abilityAnimation, UnitBase owner)
    {
        /*
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
        if (targetList.Count == int.Parse(AbilityTargetCode.GetVariable("targets")))
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
        }*/
    }

    public static void ExecuteChosenAbility(string abilityKey, CodeObject abilityTargetCode, CodeObject abilityLogicCode, CodeObject abilityAnimation, UnitBase owner)
    {
        switch (abilityTargetCode.Task)
        {
            //tasks can be spells, attacks or abilities. attacks are always (multi) target by default (ie cannot target ground)
            case "Attack":
                List<Vector3Int> targets = new List<Vector3Int>();
                foreach (Vector3Int v in TileManager.globalInstance.AddTargetTiles(int.Parse(abilityTargetCode.GetVariable("minRange")), int.Parse(abilityTargetCode.GetVariable("maxRange"))))
                {
                    if ((TileManager.globalInstance.HostileAttackableBuildingOnTile(owner, v, abilityTargetCode.GetVariable("canHit")) || TileManager.globalInstance.HostileAttackableUnitOnTile(owner, v, abilityTargetCode.GetVariable("canHit")))) { targets.Add(v); }
                }
                TileManager.globalInstance.SetUpTargetTiles(targets);
                Pointer.globalInstance.GoToAttackingMode();
                return;
            //need to rework onsite for spawn and capture
            case "OnSite":
                List<UnitBase> uBList = new List<UnitBase>();
                if (abilityTargetCode.GetVariable("unitBaseData") == "this")
                {
                    uBList.Add(owner);
                }
                List<Unit> uList = new List<Unit>();
                if (abilityTargetCode.GetVariable("unitData") == "this")
                {
                    uList.Add(owner.Tile.Unit);
                }
                List<Building> bList = new List<Building>();
                if (abilityTargetCode.GetVariable("buildingData") == "this")
                {
                    bList.Add(owner.Tile.Building);
                }
                List<int> iList = new List<int>();
                List<Vector3Int> vList = new List<Vector3Int>();
                if (abilityTargetCode.GetVariable("vectorData") == "this")
                {
                    vList.Add(owner.Tile.LocalPlace);
                }
                EventsManager.globalInstance.AddToStack(abilityLogicCode, abilityKey, owner, abilityAnimation, iList, uBList, uList, bList, vList);
                return;
        }
    }
}
