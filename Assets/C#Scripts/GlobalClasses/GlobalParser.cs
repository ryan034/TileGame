using System;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalParser
{

    public static bool ParseReturnBool(CodeObject filter, UnitBase owner, List<UnitBase> list = null, List<Unit> listUnit = null, List<Building> listBuilding = null, List<int> listInt = null)
    {
        bool v = ParseConditionalControlFlowCode(filter, owner, list, listUnit, listBuilding, listInt);
        bool b;
        if (v)
        {
            foreach (CodeObject c in filter.GetCodeObjects("true"))
            {
                b = false;
                b = ParseReturnBool(c, owner, list, listUnit, listBuilding, listInt) || b;
                if (b) { return true; }
            }
        }
        else
        {
            foreach (CodeObject c in filter.GetCodeObjects("false"))
            {
                b = false;
                b = ParseReturnBool(c, owner, list, listUnit, listBuilding, listInt) || b;
                if (b) { return true; }
            }
        }
        return v;
    }

    public static void Parse(StackItem data, CodeObject currentCode = null, bool before = false)
    {
        if (currentCode == null) { currentCode = data.code; }
        if (data.additionalVariable == "MainAtack" && data.code.Task == "Attack")
        {
            //implement battle
            data.additionalVariable = "";
            for (int i = 0; i < data.unitBaseData.Count; i++)
            {
                foreach (string s in data.unitBaseData[i].Abilities)
                {
                    CodeObject c = data.unitBaseData[i].GetTargetCode(s);
                    if (c.Task == "Attack" && Manager.TileManager.AttackableAndHostileTo(data.unitBaseData[i], data.owner, c.GetVariable("canHit")) && Manager.TileManager.WithinRange(int.Parse(c.GetVariable("minRange")), int.Parse(c.GetVariable("maxRange")), data.unitBaseData[i], data.owner))
                    {
                        int targetSpeed = data.unitBaseData[i].GetLogicCode(s).GetVariable("speed") == "" ? 0 : int.Parse(data.unitBaseData[i].GetLogicCode(s).GetVariable("speed"));
                        int speed = currentCode.GetVariable("speed") == "" ? 0 : int.Parse(currentCode.GetVariable("speed"));
                        if (targetSpeed - speed >= 1)
                        {
                            Parse(new StackItem(data.unitBaseData[i].GetLogicCode(s), s, data.unitBaseData[i], data.unitBaseData[i].GetAnimationCode(s), null, new List<UnitBase>() { data.owner }, null, null, null), null, before);
                            Parse(data, data.code, before);
                        }
                        else if (targetSpeed == speed)
                        {
                            //units attack eachother at the same time
                            //int damage = attackerUnit.CalculateAttackDamage(attackerBaseDamage, attackerDiceDamage, attackerDiceTimes, CoverBonus);
                            //Parse(new StackItem(GetLogicCode(abilityForCounterAttack), abilityForCounterAttack, this, GetAnimationCode(abilityForCounterAttack), null, new List<UnitBase>() { attackerUnit }, null, null, null), null, before);
                            //CalculateDamageTakenAndTakeDamage(before, attackerUnit, attackerDamageType, damage);
                        }
                        else if (targetSpeed - speed == -1)
                        {
                            Parse(data, data.code, before);
                            Parse(new StackItem(data.unitBaseData[i].GetLogicCode(s), s, data.unitBaseData[i], data.unitBaseData[i].GetAnimationCode(s), null, new List<UnitBase>() { data.owner }, null, null, null), null, before);
                        }
                        else if (targetSpeed - speed <= -2)
                        {
                            Parse(data, data.code, before);
                        }
                        break;
                    }
                }
            }
            return;
        }
        if (currentCode.IsConditional)
        {
            bool v = ParseConditionalControlFlowCode(currentCode, data.owner, data.unitBaseData, data.unitData, data.buildingData, data.intData);
            if (v)
            {
                foreach (CodeObject c in currentCode.GetCodeObjects("true"))
                {
                    Parse(data, c, before);
                }
            }
            else
            {
                foreach (CodeObject c in currentCode.GetCodeObjects("false"))
                {
                    Parse(data, c, before);
                }
            }
        }
        else
        {
            ParseCode(currentCode, data, before);
            foreach (CodeObject c in currentCode.GetCodeObjects("next"))
            {
                Parse(data, c, before);
            }
        }
    }

    //public static bool ParseConditionalControlFlowCode(CodeObject code, StackItem data) { return ParseConditionalControlFlowCode(code, data.owner, data.unitBaseData, data.unitData, data.buildingData, data.intData); }

    public static bool ParseConditionalControlFlowCode(CodeObject conditional, UnitBase owner, List<UnitBase> list, List<Unit> listUnit, List<Building> listBuilding, List<int> listInt)
    {
        switch (conditional.Task)
        {
            case "IsAlive":
                //maybe have this default to owner but if variable <to> is present then have it evaluate to.HPCurrent > 0
                return owner.HPCurrent > 0;
            case "NotDisarmed":
                return !owner.Disarmed;
            case "NotSilenced":
                return !owner.Silenced;
            case "CanCounterAttack":
                UnitBase u, v;
                string fromCode = conditional.GetVariable("from") == "" ? "0" : conditional.GetVariable("from");
                string toCode = conditional.GetVariable("to") == "" ? "0" : conditional.GetVariable("to");
                switch (fromCode[0])
                {
                    case 'u':
                        u = listUnit[int.Parse(fromCode.Substring(1))];
                        break;
                    case 'b':
                        u = listBuilding[int.Parse(fromCode.Substring(1))];
                        break;
                    default:
                        u = list[int.Parse(fromCode)];
                        break;
                }
                switch (toCode[0])
                {
                    case 'u':
                        v = listUnit[int.Parse(toCode.Substring(1))];
                        break;
                    case 'b':
                        v = listBuilding[int.Parse(toCode.Substring(1))];
                        break;
                    default:
                        v = list[int.Parse(toCode)];
                        break;
                }
                foreach (string s in u.Abilities)
                {
                    CodeObject c = u.GetTargetCode(s);
                    return c.Task == "Attack" && Manager.TileManager.AttackableAndHostileTo(u, v, c.GetVariable("canHit")) && Manager.TileManager.WithinRange(int.Parse(c.GetVariable("minRange")), int.Parse(c.GetVariable("maxRange")), u, v);
                }
                //if (conditional.GetVariable("scope") == "all") { return true; }
                //if (conditional.Task == "OnAttack" && conditional.GetVariable("scope") == "self" && conditional.GetVariable("side") == "defender") { return (list[1] == owner); }
                return false;
        }
        return false;
    }

    public static void ParseCode(CodeObject code, StackItem data, bool before)
    {
        switch (code.Task)
        {
            case "Attack": // only refers to main attack
                //int toCode = code.GetVariable("to") == "" ? 0 : int.Parse(code.GetVariable("to"));
                data.owner.DamageTarget(before, data.unitBaseData[0], int.Parse(code.GetVariable("baseDamage")), int.Parse(code.GetVariable("diceDamage")), int.Parse(code.GetVariable("diceTimes")), int.Parse(code.GetVariable("damageType")));
                break;
            case "CounterAttack": //refers to when a unit is forced to attack another outside of main attack
                UnitBase u, v;
                string toCode = code.GetVariable("to") == "" ? "0" : code.GetVariable("to");
                string fromCode = code.GetVariable("from") == "" ? "0" : code.GetVariable("from");
                switch (fromCode[0])
                {
                    case 'u':
                        u = data.unitData[int.Parse(fromCode.Substring(1))];
                        break;
                    case 'b':
                        u = data.buildingData[int.Parse(fromCode.Substring(1))];
                        break;
                    default:
                        u = data.unitBaseData[int.Parse(fromCode)];
                        break;
                }
                switch (toCode[0])
                {
                    case 'u':
                        v = data.unitData[int.Parse(toCode.Substring(1))];
                        break;
                    case 'b':
                        v = data.buildingData[int.Parse(toCode.Substring(1))];
                        break;
                    default:
                        v = data.unitBaseData[int.Parse(toCode)];
                        break;
                }
                foreach (string s in u.Abilities)
                {
                    CodeObject c = u.GetTargetCode(s);
                    if (c.Task == "Attack" && Manager.TileManager.AttackableAndHostileTo(u, v, c.GetVariable("canHit")) && Manager.TileManager.WithinRange(int.Parse(c.GetVariable("minRange")), int.Parse(c.GetVariable("maxRange")), u, v))
                    {
                        Parse(new StackItem(u.GetLogicCode(s), s, u, u.GetAnimationCode(s), null, new List<UnitBase>() { v }, null, null, null), null, before); //parse the counter attack and send it
                        break;
                    }
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

    public static void AddToMenu(string s, UnitBase owner, CodeObject abilityTargetCode, List<string> menu)
    {
        switch (abilityTargetCode.Task)
        {
            case "Attack":
                if (!owner.Disarmed)
                {
                    foreach (Vector3Int v in Manager.TileManager.AddTargetTiles(int.Parse(abilityTargetCode.GetVariable("minRange")), int.Parse(abilityTargetCode.GetVariable("maxRange"))))
                    {
                        if (Manager.TileManager.HostileAttackableUnitOrBuildingOnTile(owner, v, abilityTargetCode.GetVariable("canHit")) != null)
                        {
                            menu.Add(s);
                            return;
                        }
                    }
                }
                break;
            case "Spell":
                if (!owner.Silenced)
                {
                    bool b = false;
                    foreach (string target in abilityTargetCode.GetListVariables("targets"))
                    {
                        switch (target)
                        {
                            case "hostileVisibleBuildingOnTile":
                                foreach (Vector3Int v in Manager.TileManager.AddTargetTiles(int.Parse(abilityTargetCode.GetVariable("minRange")), int.Parse(abilityTargetCode.GetVariable("maxRange"))))
                                {
                                    if (Manager.TileManager.HostileVisibleBuildingOnTile(owner.Team, v) != null)
                                    {
                                        b = true;
                                        break;
                                    }
                                }
                                break;
                            case "hostileVisibleUnitOnTile":
                                foreach (Vector3Int v in Manager.TileManager.AddTargetTiles(int.Parse(abilityTargetCode.GetVariable("minRange")), int.Parse(abilityTargetCode.GetVariable("maxRange"))))
                                {
                                    if (Manager.TileManager.HostileVisibleUnitOnTile(owner.Team, v) != null)
                                    {
                                        b = true;
                                        break;
                                    }
                                }
                                break;
                        }
                        if (!b) { return; };
                    }
                    menu.Add(s);
                }
                break;
            case "OnSite":
                foreach (string target in abilityTargetCode.GetListVariables("targets"))
                {
                    switch (target)
                    {
                        case "hostileVisibleBuildingOnThisTile":
                            if (Manager.TileManager.HostileVisibleBuildingOnTile(owner.Team, owner.Tile.LocalPlace) == null) { return; }
                            break;
                    }
                }
                menu.Add(s);
                break;
        }
    }

    public static bool ValidateTargetAndCommit(string abilityKey, CodeObject abilityTargetCode, UnitBase owner, Vector3Int target, List<Building> buildingList, List<Unit> unitList, List<UnitBase> unitBaseList, List<Vector3Int> vectorList, List<int> intList)
    {
        //first validates whether target can be added as a valid target and then returns true when all requirements are fulfilled and the ability can be added to the stack
        if (abilityTargetCode.Task == "Attack")
        {
            unitBaseList.Add(Manager.TileManager.HostileAttackableUnitOrBuildingOnTile(owner, target, abilityTargetCode.GetVariable("canHit")));
            if (owner.TargetCount == int.Parse(abilityTargetCode.GetVariable("targets")))
            {
                return true;
            }
        }
        else
        {
            string item = abilityTargetCode.GetListVariables("targets")[owner.TargetCount];
            switch (item)
            {
                case "hostileVisibleBuildingOnTile":
                    if (Manager.TileManager.HostileVisibleBuildingOnTile(owner.Team, target) != null) { buildingList.Add(Manager.TileManager.HostileVisibleBuildingOnTile(owner.Team, target)); }
                    break;
                case "hostileVisibleUnitOnTile":
                    if (Manager.TileManager.HostileVisibleUnitOnTile(owner.Team, target) != null) { unitList.Add(Manager.TileManager.HostileVisibleUnitOnTile(owner.Team, target)); }
                    break;
                case "v":
                    vectorList.Add(target);
                    break;
            }
            if (owner.TargetCount == abilityTargetCode.GetListVariables("targets").Count)
            {
                return true;
            }
        }
        return false;
    }

    public static void ChooseMenuAbility(string abilityKey, CodeObject abilityTargetCode, CodeObject abilityLogicCode, CodeObject abilityAnimation, UnitBase owner)
    {
        switch (abilityTargetCode.Task)
        {
            //tasks can be spells, attacks or abilities. attacks are always (multi) target by default (ie cannot target ground)
            case "Attack":
                List<Vector3Int> targets = new List<Vector3Int>();
                foreach (Vector3Int v in Manager.TileManager.AddTargetTiles(int.Parse(abilityTargetCode.GetVariable("minRange")), int.Parse(abilityTargetCode.GetVariable("maxRange"))))
                {
                    if (Manager.TileManager.HostileAttackableUnitOrBuildingOnTile(owner, v, abilityTargetCode.GetVariable("canHit")) != null)
                    { targets.Add(v); }
                }
                Manager.TileManager.SetUpTargetTiles(targets);
                Pointer.globalInstance.GoToAttackingMode();
                return;
            case "OnSite":            //onsite for spawn and capture
                List<UnitBase> uBList = new List<UnitBase>();
                List<Unit> uList = new List<Unit>();
                List<Building> bList = new List<Building>();
                List<int> iList = new List<int>();
                List<Vector3Int> vList = new List<Vector3Int>();
                foreach (string s in abilityTargetCode.GetListVariables("targets"))
                {
                    switch (s)
                    {
                        case "hostileVisibleBuildingOnThisTile":
                            bList.Add(owner.Tile.Building);
                            break;
                        case "thisUnit":
                            uList.Add(owner.Tile.Unit);
                            break;
                    }
                }
                Manager.EventsManager.AddToStack(abilityLogicCode, abilityKey, owner, abilityAnimation, iList, uBList, uList, bList, vList);
                return;
        }
    }
}
