using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StackItem
{
    public readonly CodeObject code;
    public readonly string name;
    public readonly CodeObject animationCode;
    public readonly UnitBase owner;

    public List<int> intData = new List<int>();
    public List<UnitBase> unitBaseData = new List<UnitBase>();
    public List<Unit> unitData = new List<Unit>();
    public List<Building> buildingData = new List<Building>();
    public List<Vector3Int> vectorData = new List<Vector3Int>();
    /*
    public IEnumerable<int> intData
    {
        get
        {
            foreach (int s in intData)
            {
                yield return s;
            }
        }
    }

    public IEnumerable<UnitBase> targetData
    {
        get
        {
            foreach (UnitBase s in targetData)
            {
                yield return s;
            }
        }
    }*/

    public StackItem(CodeObject code, string name, UnitBase owner, CodeObject animationCode, List<int> intData, List<UnitBase> unitBaseData, List<Unit> unitData, List<Building> buildingData, List<Vector3Int> vectorData)
    {
        this.code = code;
        this.name = name;
        this.owner = owner;
        this.animationCode = animationCode;
        if (intData == null) { this.intData = new List<int>(); } else { this.intData = intData; }
        if (unitBaseData == null) { this.unitBaseData = new List<UnitBase>(); } else { this.unitBaseData = unitBaseData; }
        if (unitData == null) { this.unitData = new List<Unit>(); } else { this.unitData = unitData; }
        if (buildingData == null) { this.buildingData = new List<Building>(); } else { this.buildingData = buildingData; }
        if (vectorData == null) { this.vectorData = new List<Vector3Int>(); } else { this.vectorData = vectorData; }
    }

    /*
    public static StackItem CreateOnStack(CodeObject code, string name, UnitBase owner, CodeObject animationCode, List<int> intData, List<UnitBase> unitData, List<Vector3Int> vectorData)
    {
        StackItem s = new StackItem(code, name, owner, animationCode, intData, unitData, vectorData);
        s.TriggerOnStack();
        return s;
    }*/
}
