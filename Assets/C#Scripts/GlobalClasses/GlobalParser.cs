using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
public static class GlobalParser
{

    public static bool ParseReturnBool(CodeObject filter, IUnitBase owner, List<IUnitBase> list = null, List<IUnit> listUnit = null, List<IBuilding> listBuilding = null, List<int> listInt = null)
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

    public static IEnumerator Parse(StackItem originalStackItem, CodeObject currentCode = null, bool before = false, bool mainPhase = false)
    {
        if (currentCode == null) { currentCode = originalStackItem.code; /*originalStackItem.SetUp();*/ }
        if (mainPhase && originalStackItem.code.Task == "Attack")
        {
            //implement battle
            Debugger.AddToLog("Main Attack " + (before ? "before resolution" : "after resolution"));
            //data.mainPhase = false;
            for (int i = 0; i < originalStackItem.unitBaseData.Count; i++)
            {
                foreach (string s in originalStackItem.unitBaseData[i].Abilities)
                {
                    CodeObject c = originalStackItem.unitBaseData[i].GetTargetCode(s);
                    if (c.Task == "Attack" && originalStackItem.unitBaseData[i].CanAttackAndHostileTo(originalStackItem.owner, c.GetVariable("canHit")) && originalStackItem.unitBaseData[i].WithinRange(int.Parse(c.GetVariable("minRange")), int.Parse(c.GetVariable("maxRange")), originalStackItem.owner))
                    {
                        int targetSpeed = originalStackItem.unitBaseData[i].GetLogicCode(s).GetVariable("speed") == "" ? 0 : int.Parse(originalStackItem.unitBaseData[i].GetLogicCode(s).GetVariable("speed"));
                        int speed = currentCode.GetVariable("speed") == "" ? 0 : int.Parse(currentCode.GetVariable("speed"));
                        if (targetSpeed - (speed + 1) >= 1)
                        {
                            Debugger.AddToLog("Attack is slower than target");
                            yield return Parse(new StackItem(originalStackItem.unitBaseData[i].GetLogicCode(s), originalStackItem.unitBaseData[i], s, unitBaseData: new List<IUnitBase>() { originalStackItem.owner }), before: before);
                            yield return Parse(originalStackItem, originalStackItem.code, before);
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
                            yield return Parse(originalStackItem, originalStackItem.code, before);
                            yield return Parse(new StackItem(originalStackItem.unitBaseData[i].GetLogicCode(s), originalStackItem.unitBaseData[i], s, unitBaseData: new List<IUnitBase>() { originalStackItem.owner }), before: before);
                        }
                        else if (targetSpeed - (speed + 1) <= -2)
                        {
                            Debugger.AddToLog("Attack is unretaliated");
                            yield return Parse(originalStackItem, originalStackItem.code, before);
                        }
                        break;
                    }
                }
            }
            yield break;
        }
        if (currentCode.IsConditional)
        {
            bool v = ParseConditionalControlFlowCode(currentCode, originalStackItem.owner, originalStackItem.unitBaseData, originalStackItem.unitData, originalStackItem.buildingData, originalStackItem.intData);
            //Debugger.AddToLog("Evaluated Conditonal to be:" + (v ? "true" : "false"));
            if (v)
            {
                foreach (CodeObject c in currentCode.GetCodeObjects("true"))
                {
                    yield return Parse(originalStackItem, c, before);
                }
            }
            else
            {
                foreach (CodeObject c in currentCode.GetCodeObjects("false"))
                {
                    yield return Parse(originalStackItem, c, before);
                }
            }
        }
        else
        {
            yield return ParseCode(currentCode, originalStackItem, before);
            foreach (CodeObject c in currentCode.GetCodeObjects("next"))
            {
                yield return Parse(originalStackItem, c, before);
            }
        }
        if (currentCode == originalStackItem.code) { originalStackItem.Destroy(); };
    }

    //public static bool ParseConditionalControlFlowCode(CodeObject code, StackItem data) { return ParseConditionalControlFlowCode(code, data.owner, data.unitBaseData, data.unitData, data.buildingData, data.intData); }

    public static bool ParseConditionalControlFlowCode(CodeObject conditional, IUnitBase owner, List<IUnitBase> list, List<IUnit> listUnit, List<IBuilding> listBuilding, List<int> listInt)
    {
        switch (conditional.Task)
        {
            //case "IsAlive":
            //Debugger.AddToLog("Evaluate IsAlive " + (owner.HPCurrent > 0 ? "true" : "false"));
            //maybe have this default to owner but if variable <to> is present then have it evaluate to.HPCurrent > 0
            //return owner.HPCurrent > 0;
            case "NotDisarmed":
                Debugger.AddToLog("Evaluate NotDisarmed " + (owner.Disarmed ? "true" : "false"));
                return !owner.Disarmed;
            case "NotSilenced":
                Debugger.AddToLog("Evaluate NotSilenced " + (owner.Silenced ? "true" : "false"));
                return !owner.Silenced;
            case "CanCounterAttack":
                IUnitBase u, v;
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
                    Debugger.AddToLog("Evaluate CanCounterAttack " + (c.Task == "Attack" && u.CanAttackAndHostileTo(v, c.GetVariable("canHit")) && u.WithinRange(int.Parse(c.GetVariable("minRange")), int.Parse(c.GetVariable("maxRange")), v) ? "true" : "false"));
                    return c.Task == "Attack" && u.CanAttackAndHostileTo(v, c.GetVariable("canHit")) && u.WithinRange(int.Parse(c.GetVariable("minRange")), int.Parse(c.GetVariable("maxRange")), v);
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
                yield return data.owner.AttackTarget(before, data.unitBaseData[0], int.Parse(code.GetVariable("baseDamage")), int.Parse(code.GetVariable("diceDamage")), int.Parse(code.GetVariable("diceTimes")), int.Parse(code.GetVariable("damageType")), code.GetCodeObject("animation"));
                break;
            case "CounterAttack": //refers to when a unit is forced to attack another outside of main attack
                Debugger.AddToLog("Parse CounterAttack " + (before ? "before resolution" : "after resolution"));
                IUnitBase u, v;
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
                    if (c.Task == "Attack" && u.CanAttackAndHostileTo(v, c.GetVariable("canHit")) && u.WithinRange(int.Parse(c.GetVariable("minRange")), int.Parse(c.GetVariable("maxRange")), v))
                    {
                        yield return Parse(new StackItem(u.GetLogicCode(s), u, s, unitBaseData: new List<IUnitBase>() { v }), before: before); //parse the attack and send it
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

    public static void AddToMenu(string s, IUnitBase owner, CodeObject abilityTargetCode, List<string> menu)
    {
        switch (abilityTargetCode.Task)
        {
            case "Attack":
                if (!owner.Disarmed)
                {
                    foreach (TileObject v in owner.AddTargetTiles(int.Parse(abilityTargetCode.GetVariable("minRange")), int.Parse(abilityTargetCode.GetVariable("maxRange"))))
                    {
                        if (v.HostileAttackableUnitOrBuildingOnTile(owner, abilityTargetCode.GetVariable("canHit")) != null)
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
                                foreach (TileObject v in owner.AddTargetTiles(int.Parse(abilityTargetCode.GetVariable("minRange")), int.Parse(abilityTargetCode.GetVariable("maxRange"))))
                                {
                                    if (v.HostileVisibleBuildingOnTile(owner.Team) != null)
                                    {
                                        b = true;
                                        break;
                                    }
                                }
                                break;
                            case "hostileVisibleUnitOnTile":
                                foreach (TileObject v in owner.AddTargetTiles(int.Parse(abilityTargetCode.GetVariable("minRange")), int.Parse(abilityTargetCode.GetVariable("maxRange"))))
                                {
                                    if (v.HostileVisibleUnitOnTile(owner.Team) != null)
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
                            if (owner.Tile.HostileVisibleBuildingOnTile(owner.Team) == null) { return; }
                            break;
                    }
                }
                menu.Add(s);
                break;
        }
    }

    public static bool ValidateTargetAndCommit(string abilityKey, CodeObject abilityTargetCode, IUnitBase owner, Vector3Int target, List<IBuilding> buildingList, List<IUnit> unitList, List<IUnitBase> unitBaseList, List<Vector3Int> vectorList, List<int> intList)
    {
        //first validates whether target can be added as a valid target and then returns true when all requirements are fulfilled and the ability can be added to the stack
        TileObject targetTile = TileObject.TileAt(target);
        if (abilityTargetCode.Task == "Attack")
        {
            //unitBaseList.Add(targetTile.HostileAttackableUnitOrBuildingOnTile(owner, abilityTargetCode.GetVariable("canHit")));
            if (targetTile.HostileAttackableUnitOrBuildingOnTile(owner, abilityTargetCode.GetVariable("canHit")) != null) { unitBaseList.Add(targetTile.HostileAttackableUnitOrBuildingOnTile(owner, abilityTargetCode.GetVariable("canHit"))); }
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
                    if (targetTile.HostileVisibleBuildingOnTile(owner.Team) != null) { buildingList.Add(targetTile.HostileVisibleBuildingOnTile(owner.Team)); }
                    break;
                case "hostileVisibleUnitOnTile":
                    if (targetTile.HostileVisibleUnitOnTile(owner.Team) != null) { unitList.Add(targetTile.HostileVisibleUnitOnTile(owner.Team)); }
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

    public static void ChooseMenuAbility(string abilityKey, CodeObject abilityTargetCode, CodeObject abilityLogicCode, IUnitBase owner)
    {
        switch (abilityTargetCode.Task)
        {
            //tasks can be spells, attacks or abilities. attacks are always (multi) target by default (ie cannot target ground)
            case "Attack":
                List<TileObject> targets = new List<TileObject>();
                foreach (TileObject v in owner.AddTargetTiles(int.Parse(abilityTargetCode.GetVariable("minRange")), int.Parse(abilityTargetCode.GetVariable("maxRange"))))
                {
                    if (v.HostileAttackableUnitOrBuildingOnTile(owner, abilityTargetCode.GetVariable("canHit")) != null)
                    { targets.Add(v); }
                }
                foreach (TileObject t in targets) { t.IsTarget = true; }
                Pointer.globalInstance.GoToAttackingMode();
                return;
            case "OnSite":            //onsite for spawn and capture
                List<IUnitBase> uBList = new List<IUnitBase>();
                List<IUnit> uList = new List<IUnit>();
                List<IBuilding> bList = new List<IBuilding>();
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
                owner.AddToStack(abilityLogicCode, abilityKey, iList, uBList, uList, bList, vList);
                return;
        }
    }
}
