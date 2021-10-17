using System.Collections.Generic;
using UnityEngine;
using static GlobalParser;
using System;

public class Buff
{
    private UnitBase owner;

    private Dictionary<string, EventCodeBlock> code = new Dictionary<string, EventCodeBlock>();
    private List<string> unitTags = new List<string>();
    private int duration;
    public int Duration { get => duration; set { if (duration != -1) { duration = Math.Max(value, 0); /*if duration==0, then remove buff and update ui*/} } }

    public readonly string name;
    public readonly int armour;
    public readonly int hP;
    public readonly int mP;
    public readonly bool invisible;
    public readonly bool notHostile;
    public readonly bool rooted;
    public readonly bool disarmed;
    public readonly bool silenced;
    public readonly int dayVision;
    public readonly int nightVision;

    public readonly int buildingCover;
    public readonly int movementTotal;
    public readonly int captureDamage;

    public bool HasTag(string tag) => unitTags.Contains(tag);

    private struct EventCodeBlock
    {
        public readonly CodeObject filterCode;
        public readonly CodeObject logicCode;
        //public readonly CodeObject animationCode;

        public EventCodeBlock(string trigger, string code)
        {
            filterCode = CodeObject.LoadCode(trigger);
            logicCode = CodeObject.LoadCode(code);
            //animationCode = CodeObject.LoadCode(animation);
        }
    }

    private void Enable(string s)
    {
        switch (s)
        {
            case "OnMainAttack":
                //execute parsed code for selected abilities
                EventsManager.OnMainAttack += OnMainAttack;
                break;
            case "OnKill":
                EventsManager.OnKill += OnKill;
                break;
            case "OnDeathUnitBase":
                EventsManager.OnDeathUnitBase += OnDeathUnitBase;
                break;
        }
    }

    private void Disable(string s)
    {
        switch (s)
        {
            case "OnMainAttack":
                //execute parsed code for selected abilities
                EventsManager.OnMainAttack -= OnMainAttack;
                break;
            case "OnKill":
                EventsManager.OnKill -= OnKill;
                break;
            case "OnDeathUnitBase":
                EventsManager.OnDeathUnitBase -= OnDeathUnitBase;
                break;
        }
    }

    private void OnMainAttack(UnitBase attacker, List<UnitBase> defender)
    {
        defender.Insert(0, attacker);
        if (code.ContainsKey("OnMainAttack") && ParseReturnBool(code["OnMainAttack"].filterCode, owner, defender))
        {
            Manager.EventsManager.AddToStack(code["OnMainAttack"].logicCode, name, owner, targetData: defender);
            //this is where counterattack would be triggered
        }
    }

    private void OnKill(UnitBase destroyer, UnitBase destroyee)
    {
        if (code.ContainsKey("OnKill") && ParseReturnBool(code["OnKill"].filterCode, owner, new List<UnitBase>() { destroyer, destroyee }))
        {
            Manager.EventsManager.AddToStack(code["OnKill"].logicCode, name, owner, targetData: new List<UnitBase>() { destroyer, destroyee });
        }
    }

    private void OnDeathUnitBase(UnitBase dead)
    {
        if (code.ContainsKey("OnDeathUnitBase") && ParseReturnBool(code["OnDeathUnitBase"].filterCode, owner, new List<UnitBase>() { dead }))
        {
            //extract animation code from s and set it to animation
            Manager.EventsManager.AddToStack(code["OnDeathUnitBase"].logicCode, name, owner, targetData: new List<UnitBase>() { dead });
        }
    }

    private void OnSpawnUnit(Building spawner, Unit spawned)
    {
        if (code.ContainsKey("OnSpawnUnit") && ParseReturnBool(code["OnSpawnUnit"].filterCode, owner, listUnit: new List<Unit>() { spawned }, listBuilding: new List<Building>() { spawner }))
        {
            Manager.EventsManager.AddToStack(code["OnSpawnUnit"].logicCode, name, owner, targetData: new List<UnitBase>() { spawner, spawned });
        }
    }

    public static Buff Load(UnitBase unit, string script)
    {
        Buff loaded = Manager.AssetManager.LoadBuff(script);
        loaded.owner = unit;
        /*
        name = loaded.buffName;

        armour = loaded.armour;
        hP = loaded.hP;
        mP = loaded.mP;
        invisible = loaded.invisible;
        //otherCode = loaded.otherCode;
        */
        foreach (string item in loaded.code.Keys)
        {
            //code.Add(item.event_, new CodeBlock(item.trigger,item.code, item.animation));
            //add itself to events
            loaded.Enable(item);
        }
        return loaded;
    }

    public Buff(BuffScript loaded)
    {
        name = loaded.buffName;
        armour = loaded.armour;
        hP = loaded.hP;
        mP = loaded.mP;
        invisible = loaded.invisible;
        unitTags = loaded.unitTags;
        notHostile = loaded.notHostile;
        rooted = loaded.rooted;
        disarmed = loaded.disarmed;
        silenced = loaded.silenced;
        dayVision = loaded.dayVision;
        nightVision = loaded.nightVision;
        buildingCover = loaded.buildingCover;
        movementTotal = loaded.movementTotal;
        captureDamage = loaded.captureDamage;
        foreach (BuffScript.EventCodeBlock b in loaded.code)
        {
            code[b.event_] = new EventCodeBlock(b.trigger, b.code);
        }
    }

    public Buff(BuffXml loaded)
    {
        name = loaded.name;
        armour = loaded.armour;
        hP = loaded.hP;
        mP = loaded.mP;
        invisible = loaded.invisible;
        duration = loaded.duration;
        //private Dictionary<string, EventCodeBlock> code = new Dictionary<string, EventCodeBlock>();// all the information about events
        unitTags = loaded.unitTags;
        notHostile = loaded.notHostile;
        rooted = loaded.rooted;
        disarmed = loaded.disarmed;
        silenced = loaded.silenced;
        dayVision = loaded.dayVision;
        nightVision = loaded.nightVision;
        buildingCover = loaded.buildingCover;
        movementTotal = loaded.movementTotal;
        captureDamage = loaded.captureDamage;

        foreach (BuffXml.EventCodeBlock b in loaded.code)
        {
            code[b.event_] = new EventCodeBlock(b.trigger, b.code);
        }
    }

    public void Destroy()
    {
        owner = null;
        foreach (string item in code.Keys) { Disable(item); }
    }

}
