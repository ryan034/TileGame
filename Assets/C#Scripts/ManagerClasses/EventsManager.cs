using static GlobalParser;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class EventsManager : MonoBehaviour
{
    public static event Action<UnitBase, List<UnitBase>> OnMainAttack;
    public static event Action<UnitBase, List<UnitBase>> OnBeforeMainAttack;

    public static event Action<UnitBase, UnitBase> OnAttack; //when unitbase attacks another unitbase
    public static event Action<UnitBase, UnitBase> OnBeforeAttack;

    public static event Action<UnitBase, UnitBase> OnCapture;
    public static event Action<UnitBase, UnitBase> OnBeforeCapture;

    public static event Action<UnitBase, List<Unit>> OnSpawnUnit;
    public static event Action<UnitBase> OnBeforeSpawnUnit;

    public static event Action<UnitBase, UnitBase> OnKill;

    public static event Action<UnitBase> OnDeathUnitBase; //deathrattle effects different to OnObjectDestroyUnitBase as it deals with in game effects
    public static event Action<Building> OnDeathBuilding; //deathrattle effects different to OnObjectDestroyUnitBase as it deals with in game effects
    public static event Action<Unit> OnDeathUnit; //deathrattle effects different to OnObjectDestroyUnitBase as it deals with in game effects

    public static event Action<UnitBase> OnObjectDestroyUnitBase; //called for memory management reasons,
    public static event Action<Unit> OnObjectDestroyUnit; //called for memory management reasons,
    public static event Action<Building> OnObjectDestroyBuilding; //called for memory management reasons,

    private List<StackItem> currentStack = new List<StackItem>();

    public static void InvokeOnAttack(UnitBase attacker, UnitBase defender)
    {
        OnAttack?.Invoke(attacker, defender);
    }

    public static void InvokeOnBeforeAttack(UnitBase attacker, UnitBase defender)
    {
        OnBeforeAttack?.Invoke(attacker, defender);
    }

    public static void InvokeOnMainAttack(UnitBase attacker, List<UnitBase> defender)
    {
        OnMainAttack?.Invoke(attacker, defender);
    }

    public static void InvokeOnBeforeMainAttack(UnitBase attacker, List<UnitBase> defender)
    {
        OnBeforeMainAttack?.Invoke(attacker, defender);
    }

    public static void InvokeOnCapture(Unit unit, Building building)
    {
        OnCapture?.Invoke(unit, building);
    }

    public static void InvokeOnBeforeCapture(Unit unit, Building building)
    {
        OnBeforeCapture?.Invoke(unit, building);
    }

    public static void InvokeOnSpawnUnit(UnitBase spawner, List<Unit> unit)
    {
        OnSpawnUnit?.Invoke(spawner, unit);
    }

    public static void InvokeOnBeforeSpawnUnit(UnitBase spawner)
    {
        OnBeforeSpawnUnit?.Invoke(spawner);
    }

    public static void InvokeOnKill(UnitBase killer, UnitBase killed)
    {
        OnKill?.Invoke(killer, killed);
    }

    public static void InvokeOnDeathUnitBase(UnitBase unitBase)
    {
        OnDeathUnitBase?.Invoke(unitBase);
    }

    public static void InvokeOnDeathBuilding(Building building)
    {
        OnDeathBuilding?.Invoke(building);
    }

    public static void InvokeOnDeathUnit(Unit unit)
    {
        OnDeathUnit?.Invoke(unit);
    }

    public static void InvokeOnObjectDestroyUnitBase(UnitBase unit)
    {
        OnObjectDestroyUnitBase?.Invoke(unit);
    }

    public static void InvokeOnObjectDestroyUnit(Unit unit)
    {
        OnObjectDestroyUnit?.Invoke(unit);
    }

    public static void InvokeOnObjectDestroyBuilding(Building unit)
    {
        OnObjectDestroyBuilding?.Invoke(unit);
    }

    public void AddToStack(CodeObject code, string name, UnitBase owner, CodeObject animation, List<int> intData = null, List<UnitBase> targetData = null, List<Unit> unitTargetData = null, List<Building> buildingTargetData = null, List<Vector3Int> vectorData = null, bool mainPhase = false)
    {
        Debugger.AddToLog("added to stack " + name);
        int i = currentStack.Count;
        StackItem s = new StackItem(code, name, owner, animation, intData, targetData, unitTargetData, buildingTargetData, vectorData, mainPhase);
        currentStack.Add(s);
        Parse(s, null, true);
        if (i == 0) { StartCoroutine(ResolveStack()); }
    }

    private IEnumerator ResolveStack()
    {
        Pointer.globalInstance.haltInput = true;
        StackItem s;
        while (currentStack.Count > 0)
        {
            s = currentStack[currentStack.Count - 1];
            yield return Manager.AnimationManager.ParseAnimation(s);
            Debugger.AddToLog("resolving " + s.name);
            Parse(s);
            s.owner.ClearTargets();
            currentStack.Remove(s);
        }
        Manager.TileManager.EndHeldUnitTurn();
        Manager.TileManager.WipeTiles();
        Pointer.globalInstance.haltInput = false;
    }
}
