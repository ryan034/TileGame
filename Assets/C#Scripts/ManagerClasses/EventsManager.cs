using static GlobalParser;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class EventsManager : MonoBehaviour
{
    public static event Action<UnitBase, List<UnitBase>> OnMainAttack;
    public static event Action<UnitBase, List<UnitBase>> OnBeforeMainAttack;

    public static event Action<UnitBase, List<UnitBase>> OnAttack;
    public static event Action<UnitBase, List<UnitBase>> OnBeforeAttack;

    public static event Action<UnitBase, UnitBase> OnCapture;
    public static event Action<UnitBase, UnitBase> OnBeforeCapture;

    public static event Action<UnitBase, List<Unit>> OnSpawnUnit;
    public static event Action<UnitBase> OnBeforeSpawnUnit;

    public static event Action<UnitBase, UnitBase> OnDestroy;

    public static event Action<UnitBase> OnDeath;

    private List<StackItem> currentStack = new List<StackItem>();

    public static void InvokeOnAttack(UnitBase attacker, List<UnitBase> defender)
    {
        OnAttack?.Invoke(attacker, defender);
    }

    public static void InvokeOnBeforeAttack(UnitBase attacker, List<UnitBase> defender)
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

    public void AddToStack(CodeObject code, string name, UnitBase owner, CodeObject animation, List<int> intData = null, List<UnitBase> targetData = null, List<Unit> unitTargetData = null, List<Building> buildingTargetData = null, List<Vector3Int> vectorData = null)
    {
        int i = currentStack.Count;
        StackItem s = new StackItem(code, name, owner, animation, intData, targetData, unitTargetData, buildingTargetData, vectorData);
        currentStack.Add(s);
        //todo: parse code logic and invoke all events here instead of at the source
        Parse(s, null, true);
        if (i == 0) { StartCoroutine(ResolveStack()); }
    }

    private IEnumerator ResolveStack()
    {
        StackItem s;
        while (currentStack.Count > 0)
        {
            //yield return StartCoroutine(ResolveStack(currentStack[currentStack.Count - 1]));
            s = currentStack[currentStack.Count - 1];
            yield return StartCoroutine(s.owner.ParseAnimation(s));
            //yield return StartCoroutine(ResolveAnimation(stackItem.owner, stackItem.animationCode));
            Parse(s);
            currentStack.Remove(s);
        }
    }


}
