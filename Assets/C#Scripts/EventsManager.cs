using static Globals;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class EventsManager : MonoBehaviour
{
    public static EventsManager globalInstance;

    public static event Action<UnitBase, UnitBase> OnMainAttack;
    public static event Action<UnitBase, UnitBase> OnAttack;
    public static event Action<UnitBase, UnitBase> OnBeforeMainAttack;
    public static event Action<UnitBase, UnitBase> OnBeforeAttack;
    public static event Action<UnitBase, UnitBase> OnBeforeCapture;
    public static event Action<UnitBase, UnitBase> OnBeforeSpawn;
    public static event Action<UnitBase, UnitBase> OnSpawn;
    public static event Action<UnitBase, UnitBase> OnDestroy;
    public static event Action<UnitBase> OnDeath;

    private List<StackItem> currentStack = new List<StackItem>();
    //public class Buff
    //{
    /*
    //buff bonuses
    public int hp;//flat
    public int mp;
    public int hpregen;
    public int mpregen;

    public int damage;//damageincrease percentage based like armour system
    public int range;//rangeincrease
    public int attacktype;//attack type for damage increase 0=all
    public int damagetype;//damage type for damage increase

    public int armour;//armour bonus
    public int armourtype;//armour type for armour bonus

    public int movementtotal;//movement increase percentage based like armour system
    public int movementtype;//movement type for increase

    public int dayvision;
    public int nightvision;

    public int aurarange;//range of buff >0 means its a aura
    public int auratype;//0 means allies only, 1 means enemies only, 2 means all

    public bool invisible;
    public bool disarmed;
    public bool silenced;
    public bool rooted;

    public int roundduration;
    public int turnduration;

    public bool enemy;//if the buff was placed by ally or enemy
    */

    //}
    public static void InvokeOnAttack(UnitBase attacker, UnitBase defender)
    {
        OnAttack?.Invoke(attacker, defender);
    }

    public static void InvokeOnMainAttack(UnitBase attacker, UnitBase defender)
    {
        OnMainAttack?.Invoke(attacker, defender);
    }

    public static void InvokeOnBeforeMainAttack(UnitBase attacker, UnitBase defender)
    {
        OnBeforeMainAttack?.Invoke(attacker, defender);
    }

    public static void InvokeOnBeforeAttack(UnitBase attacker, UnitBase defender)
    {
        OnBeforeAttack?.Invoke(attacker, defender);
    }

    public static void InvokeOnBeforeCapture(Unit unit, Building building)
    {
        OnBeforeCapture?.Invoke(unit, building);
    }

    public static void InvokeOnBeforeSpawn(Building building, Unit unit)
    {
        OnBeforeSpawn?.Invoke(unit, building);
    }

    public static void InvokeOnSpawn(Building building, Unit unit)
    {
        OnSpawn?.Invoke(unit, building);
    }

    /*
    public void StartStack(CodeObject code, string name, UnitBase owner, CodeObject animation, List<int> intData = null, List<UnitBase> targetData = null)
    {
        //starts the stack
        currentStack = new List<StackItem>() { new StackItem(code, name, owner, animation, intData, targetData) };
        //needs step to parse code that triggers when spell is put into stack like example below:
        //EventsManager.InvokeOnBeforeCapture(this, Tile.Building);
        //then trigger resolving the stack
        //EventsManager.globalInstance.ResolveStack();
    }*/

    public void AddToStack(CodeObject code, string name, UnitBase owner, CodeObject animation, List<int> intData = null, List<UnitBase> targetData = null, List<Unit> unitTargetData = null, List<Building> buildingTargetData = null, List<Vector3Int> vectorData = null)
    {
        int i = currentStack.Count;
        currentStack.Add(new StackItem(code, name, owner, animation, intData, targetData, unitTargetData, buildingTargetData, vectorData));
        //todo: parse code logic and invoke all events here instead of at the source
        if (i == 0) { StartCoroutine(ResolveStack()); }
    }

    /*
    public IEnumerator QuickResolve(CodeObject code, string name, UnitBase owner, CodeObject animation, List<int> intData = null, List<UnitBase> targetData = null, List<Unit> unitTargetData = null, List<Building> buildingTargetData = null, List<Vector3Int> vectorData = null)
    {
        //todo: parse code logic and invoke all events here instead of at the source
        StackItem s = new StackItem(code, name, owner, animation, intData, targetData, unitTargetData, buildingTargetData, vectorData);
        yield return StartCoroutine(s.owner.ParseAnimation(s));
        s.owner.Parse(s);
    }*/

    private IEnumerator ResolveStack()
    {
        StackItem s;
        while (currentStack.Count > 0)
        {
            //yield return StartCoroutine(ResolveStack(currentStack[currentStack.Count - 1]));
            s = currentStack[currentStack.Count - 1];
            yield return StartCoroutine(s.owner.ParseAnimation(s));
            //yield return StartCoroutine(ResolveAnimation(stackItem.owner, stackItem.animationCode));
            s.owner.Parse(s);
            currentStack.Remove(s);
        }
    }
        /*
        private IEnumerator ResolveAnimation(UnitBase unit, CodeObject s)
        {
            //todo need to parse animation code object into animation coroutine
            while (unit.IsPlaying(s))
            {
                yield return null;
            }
            yield break;
        }

        private IEnumerator WaitForAnimation(UnitBase unit, string s)
        {
            while (unit.IsPlaying(s))
            {
                yield return null;
            }
            yield break;
        }*/
        /*
        private IEnumerator ResolveStack(StackItem stackItem)
        {
            yield return StartCoroutine(stackItem.owner.ParseAnimation(stackItem));
            //yield return StartCoroutine(ResolveAnimation(stackItem.owner, stackItem.animationCode));
            stackItem.owner.Parse(stackItem);
            currentStack.Remove(stackItem);
            //Pointer.globalInstance.HaltInput = true;
        }*/

        private void Awake()
    {
        if (globalInstance == null)
        {
            globalInstance = this;
        }
        else if (globalInstance != this)
        {
            Destroy(gameObject);
        }
    }

}
