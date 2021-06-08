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
        }
    }

    /*
public override void ParseCode(CodeObject code, StackItem data, bool before)
{
    switch (code.Task)
    {
        case "Spawn":
            //code
            //spawn unit
            SpawnUnit(before, Tile, code.GetVariable("unitID"), Team);
            return;
    }
    base.ParseCode(code, data, before);
}*/

    /*
public override void ParseCode(CodeObject code, StackItem data, bool before)
{
    switch (code.Task)
    {
        case "Capture":
            //code
            int index = code.GetVariable("to") == "" ? 0 : int.Parse(code.GetVariable("to"));
            data.unitData[0].Capture(before, data.buildingData[index], int.Parse(code.GetVariable("captureRate")));
            return;
    }
    base.ParseCode(code, data, before);
}
*/
}
