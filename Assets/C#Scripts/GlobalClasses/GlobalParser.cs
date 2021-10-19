using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
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

    public static IEnumerator Parse(StackItem data, CodeObject currentCode = null, bool before = false, bool mainPhase = false)
    {
        if (currentCode == null) { currentCode = data.code; }
        if (mainPhase && data.code.Task == "Attack")
        {
            //implement battle
            Debugger.AddToLog("Main Attack " + (before ? "before resolution" : "after resolution"));
            //data.mainPhase = false;
            for (int i = 0; i < data.unitBaseData.Count; i++)
            {
                foreach (string s in data.unitBaseData[i].Abilities)
                {
                    CodeObject c = data.unitBaseData[i].GetTargetCode(s);
                    if (c.Task == "Attack" && Manager.TileManager.AttackableAndHostileTo(data.unitBaseData[i], data.owner, c.GetVariable("canHit")) && Manager.TileManager.WithinRange(int.Parse(c.GetVariable("minRange")), int.Parse(c.GetVariable("maxRange")), data.unitBaseData[i], data.owner))
                    {
                        int targetSpeed = data.unitBaseData[i].GetLogicCode(s).GetVariable("speed") == "" ? 0 : int.Parse(data.unitBaseData[i].GetLogicCode(s).GetVariable("speed"));
                        int speed = currentCode.GetVariable("speed") == "" ? 0 : int.Parse(currentCode.GetVariable("speed"));
                        if (targetSpeed - (speed + 1) >= 1)
                        {
                            Debugger.AddToLog("Attack is slower than target");
                            yield return Parse(new StackItem(data.unitBaseData[i].GetLogicCode(s), data.unitBaseData[i], s, unitBaseData: new List<UnitBase>() { data.owner }), before: before);
                            yield return Parse(data, data.code, before);
                        }
                        else if (targetSpeed == (speed + 1))
                        {
                            Debugger.AddToLog("Attack at same time");
                            //units attack eachother at the same time
                            //int damage = attackerUnit.CalculateAttackDamage(attackerBaseDamage, attackerDiceDamage, attackerDiceTimes, CoverBonus);
                            //Parse(new StackItem(GetLogicCode(abilityForCounterAttack), abilityForCounterAttack, this, GetAnimationCode(abilityForCounterAttack), null, new List<UnitBase>() { attackerUnit }, null, null, null), null, before);
                            //CalculateDamageTakenAndTakeDamage(before, attackerUnit, attackerDamageType, damage);
                        }
                        else if (targetSpeed - (speed + 1) == -1)
                        {
                            Debugger.AddToLog("Attack is faster than target");
                            yield return Parse(data, data.code, before);
                            yield return Parse(new StackItem(data.unitBaseData[i].GetLogicCode(s), data.unitBaseData[i], s, unitBaseData: new List<UnitBase>() { data.owner }), before: before);
                        }
                        else if (targetSpeed - (speed + 1) <= -2)
                        {
                            Debugger.AddToLog("Attack is unretaliated");
                            yield return Parse(data, data.code, before);
                        }
                        break;
                    }
                }
            }
            yield break;
        }
        if (currentCode.IsConditional)
        {
            bool v = ParseConditionalControlFlowCode(currentCode, data.owner, data.unitBaseData, data.unitData, data.buildingData, data.intData);
            //Debugger.AddToLog("Evaluated Conditonal to be:" + (v ? "true" : "false"));
            if (v)
            {
                foreach (CodeObject c in currentCode.GetCodeObjects("true"))
                {
                    yield return Parse(data, c, before);
                }
            }
            else
            {
                foreach (CodeObject c in currentCode.GetCodeObjects("false"))
                {
                    yield return Parse(data, c, before);
                }
            }
        }
        else
        {
            yield return ParseCode(currentCode, data, before);
            foreach (CodeObject c in currentCode.GetCodeObjects("next"))
            {
                yield return Parse(data, c, before);
            }
        }
    }

    //public static bool ParseConditionalControlFlowCode(CodeObject code, StackItem data) { return ParseConditionalControlFlowCode(code, data.owner, data.unitBaseData, data.unitData, data.buildingData, data.intData); }

    public static bool ParseConditionalControlFlowCode(CodeObject conditional, UnitBase owner, List<UnitBase> list, List<Unit> listUnit, List<Building> listBuilding, List<int> listInt)
    {
        switch (conditional.Task)
        {
            case "IsAlive":
                Debugger.AddToLog("Evaluate IsAlive " + (owner.HPCurrent > 0 ? "true" : "false"));
                //maybe have this default to owner but if variable <to> is present then have it evaluate to.HPCurrent > 0
                return owner.HPCurrent > 0;
            case "NotDisarmed":
                Debugger.AddToLog("Evaluate NotDisarmed " + (owner.Disarmed ? "true" : "false"));
                return !owner.Disarmed;
            case "NotSilenced":
                Debugger.AddToLog("Evaluate NotSilenced " + (owner.Silenced ? "true" : "false"));
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
                    Debugger.AddToLog("Evaluate CanCounterAttack " + (c.Task == "Attack" && Manager.TileManager.AttackableAndHostileTo(u, v, c.GetVariable("canHit")) && Manager.TileManager.WithinRange(int.Parse(c.GetVariable("minRange")), int.Parse(c.GetVariable("maxRange")), u, v) ? "true" : "false"));
                    return c.Task == "Attack" && Manager.TileManager.AttackableAndHostileTo(u, v, c.GetVariable("canHit")) && Manager.TileManager.WithinRange(int.Parse(c.GetVariable("minRange")), int.Parse(c.GetVariable("maxRange")), u, v);
                }
                //if (conditional.GetVariable("scope") == "all") { return true; }
                //if (conditional.Task == "OnAttack" && conditional.GetVariable("scope") == "self" && conditional.GetVariable("side") == "defender") { return (list[1] == owner); }
                Debugger.AddToLog("Evaluate CanCounterAttack " + (false ? "true" : "false"));
                return false;
        }
        Debugger.AddToLog("Defautled to false, task not found");
        return false;
    }


    public static IEnumerator ParseCode(CodeObject code, StackItem data, bool before)
    {
        switch (code.Task)
        {
            case "Attack": // only refers to main attack
                Debugger.AddToLog("Parse Attack " + (before ? "before resolution" : "after resolution"));
                //int toCode = code.GetVariable("to") == "" ? 0 : int.Parse(code.GetVariable("to"));
                yield return data.owner.DamageTarget(before, data.unitBaseData[0], int.Parse(code.GetVariable("baseDamage")), int.Parse(code.GetVariable("diceDamage")), int.Parse(code.GetVariable("diceTimes")), int.Parse(code.GetVariable("damageType")), code.GetCodeObject("animation"));
                break;
            case "CounterAttack": //refers to when a unit is forced to attack another outside of main attack
                Debugger.AddToLog("Parse CounterAttack " + (before ? "before resolution" : "after resolution"));
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
                        yield return Parse(new StackItem(u.GetLogicCode(s), u, s, unitBaseData: new List<UnitBase>() { v }), before: before); //parse the attack and send it
                        break;
                    }
                }
                break;
            case "Capture":
                Debugger.AddToLog("Parse Capture " + (before ? "before resolution" : "after resolution"));
                int to = code.GetVariable("to") == "" ? 0 : int.Parse(code.GetVariable("to"));
                int from = code.GetVariable("from") == "" ? 0 : int.Parse(code.GetVariable("from"));
                yield return data.unitData[from].Capture(before, data.buildingData[to], int.Parse(code.GetVariable("captureDamage")), code.GetCodeObject("animation"));
                break;
            case "SpawnUnit":
                Debugger.AddToLog("Parse SpawnUnit " + (before ? "before resolution" : "after resolution"));
                from = code.GetVariable("from") == "" ? 0 : int.Parse(code.GetVariable("from"));
                yield return data.unitBaseData[from].SpawnUnit(before, data.vectorData, code.GetVariable("unitID"), data.unitBaseData[from].Team, code.GetCodeObject("animation"));
                break;
                //default:
                //yield break;
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
            string item = abilityTargetCode.GetListVariables("targets")[owner.TargetCount];// always checks if target acceptable with the last requirement of targetlist
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

    public static void ChooseMenuAbility(string abilityKey, CodeObject abilityTargetCode, CodeObject abilityLogicCode, UnitBase owner)
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
                        case "thisUnitBase":
                            uBList.Add(owner.Tile.Building);
                            break;
                        case "thisLocalPlace":
                            vList.Add(owner.Tile.LocalPlace);
                            break;
                    }
                }
                Manager.EventsManager.AddToStack(abilityLogicCode, abilityKey, owner, iList, uBList, uList, bList, vList);
                return;
        }
    }
}
