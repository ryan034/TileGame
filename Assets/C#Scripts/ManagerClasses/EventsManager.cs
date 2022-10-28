using static GlobalParser;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class EventsManager : MonoBehaviour
{
    public static event Action<IUnitBase, List<IUnitBase>> OnMainAttack;
    public static event Action<IUnitBase, List<IUnitBase>> OnBeforeMainAttack;

    public static event Action<IUnitBase, IUnitBase> OnAttack; //when unitbase attacks another unitbase
    public static event Action<IUnitBase, IUnitBase> OnBeforeAttack;

    public static event Action<IUnitBase, IUnitBase, int, int> OnTakeDamage; //when unitbase attacks another unitbase
    public static event Action<IUnitBase, IUnitBase, int, int> OnBeforeTakeDamage;

    public static event Action<IUnitBase, IUnitBase> OnCapture;
    public static event Action<IUnitBase, IUnitBase> OnBeforeCapture;

    public static event Action<IUnitBase, List<IUnit>> OnSpawnUnit;
    public static event Action<IUnitBase> OnBeforeSpawnUnit;

    public static event Action<IUnitBase, IUnitBase> OnKill;

    public static event Action<IUnitBase> OnDeathUnitBase; //deathrattle effects different to OnObjectDestroyUnitBase as it deals with in game effects
    public static event Action<IBuilding> OnDeathBuilding; //deathrattle effects different to OnObjectDestroyUnitBase as it deals with in game effects
    public static event Action<IUnit> OnDeathUnit; //deathrattle effects different to OnObjectDestroyUnitBase as it deals with in game effects

    public static event Action<IUnitBase> OnObjectDestroyUnitBase; //called for memory management reasons,
    public static event Action<IUnit> OnObjectDestroyUnit; //called for memory management reasons,
    public static event Action<IBuilding> OnObjectDestroyBuilding; //called for memory management reasons,

    private List<StackItem> currentStack = new List<StackItem>();

    public static void InvokeOnAttack(IUnitBase attacker, IUnitBase defender)
    {
        OnAttack?.Invoke(attacker, defender);
    }

    public static void InvokeOnBeforeAttack(IUnitBase attacker, IUnitBase defender)
    {
        OnBeforeAttack?.Invoke(attacker, defender);
    }

    public static void InvokeOnMainAttack(IUnitBase attacker, List<IUnitBase> defender)
    {
        OnMainAttack?.Invoke(attacker, defender);
    }

    public static void InvokeOnBeforeMainAttack(IUnitBase attacker, List<IUnitBase> defender)
    {
        OnBeforeMainAttack?.Invoke(attacker, defender);
    }
    public static void InvokeOnBeforeTakeDamage(IUnitBase defender, IUnitBase damager, int damageType, int damage)
    {
        OnBeforeTakeDamage?.Invoke(defender, damager, damageType, damage);
    }

    public static void InvokeOnTakeDamage(IUnitBase defender, IUnitBase damager, int damageType, int damage)
    {
        OnTakeDamage?.Invoke(defender, damager, damageType, damage);
    }
    public static void InvokeOnCapture(IUnit unit, IBuilding building)
    {
        OnCapture?.Invoke(unit, building);
    }

    public static void InvokeOnBeforeCapture(IUnit unit, IBuilding building)
    {
        OnBeforeCapture?.Invoke(unit, building);
    }

    public static void InvokeOnSpawnUnit(IUnitBase spawner, List<IUnit> unit)
    {
        OnSpawnUnit?.Invoke(spawner, unit);
    }

    public static void InvokeOnBeforeSpawnUnit(IUnitBase spawner)
    {
        OnBeforeSpawnUnit?.Invoke(spawner);
    }

    public static void InvokeOnKill(IUnitBase killer, IUnitBase killed)
    {
        OnKill?.Invoke(killer, killed);
    }

    public static void InvokeOnDeathUnitBase(IUnitBase unitBase)
    {
        OnDeathUnitBase?.Invoke(unitBase);
    }

    public static void InvokeOnDeathBuilding(IBuilding building)
    {
        OnDeathBuilding?.Invoke(building);
    }

    public static void InvokeOnDeathUnit(IUnit unit)
    {
        OnDeathUnit?.Invoke(unit);
    }

    public static void InvokeOnObjectDestroyUnitBase(IUnitBase unit)
    {
        OnObjectDestroyUnitBase?.Invoke(unit);
    }

    public static void InvokeOnObjectDestroyUnit(IUnit unit)
    {
        OnObjectDestroyUnit?.Invoke(unit);
    }

    public static void InvokeOnObjectDestroyBuilding(IBuilding unit)
    {
        OnObjectDestroyBuilding?.Invoke(unit);
    }

    public void AddToStack(CodeObject code, string name, IUnitBase owner, List<int> intData = null, List<IUnitBase> targetData = null, List<IUnit> unitTargetData = null, List<IBuilding> buildingTargetData = null, List<Vector3Int> vectorData = null, bool mainPhase = false)
    {
        Debugger.AddToLog("added to stack " + name);
        int i = currentStack.Count;
        StackItem s = new StackItem(code, owner, name, intData, targetData, unitTargetData, buildingTargetData, vectorData);
        currentStack.Add(s);
        Parse(s, before: true, mainPhase: mainPhase);
        if (i == 0) { StartCoroutine(ResolveStack()); }
    }

    private IEnumerator ResolveStack()
    {
        Pointer.globalInstance.haltInput = true;
        StackItem s;
        while (currentStack.Count > 0)
        {
            s = currentStack[currentStack.Count - 1];
            //yield return Manager.AnimationManager.ParseAnimation(s);
            Debugger.AddToLog("resolving " + s.name);
            yield return Parse(s, mainPhase: true);
            s.owner.ClearTargets();
            currentStack.Remove(s);
        }
        Pointer.globalInstance.EndHeldUnitTurn();
        Pointer.globalInstance.haltInput = false;
    }

}
