using System.Collections.Generic;
using UnityEngine;

public class StackItem
{
    //public bool mainPhase;

    public readonly CodeObject code;
    public readonly string name;
    public readonly IUnitBase owner;

    public List<int> intData = new List<int>();
    public List<IUnitBase> unitBaseData = new List<IUnitBase>();
    public List<IUnit> unitData = new List<IUnit>();
    public List<IBuilding> buildingData = new List<IBuilding>();
    public List<Vector3Int> vectorData = new List<Vector3Int>();

    /*
    public IEnumerable<int> intData
    {
        get { foreach (int s in intData) { yield return s; } }
    }

    public IEnumerable<UnitBase> targetData
    {
        get { foreach (UnitBase s in targetData) { yield return s; } }
    }
    */

    public StackItem(CodeObject code, IUnitBase owner, string name = "", List<int> intData = null, List<IUnitBase> unitBaseData = null, List<IUnit> unitData = null, List<IBuilding> buildingData = null, List<Vector3Int> vectorData = null)
    {
        this.code = code;
        this.name = name;
        this.owner = owner;
        //this.mainPhase = mainPhase;
        if (intData == null) { this.intData = new List<int>(); } else { this.intData = intData; }
        if (unitBaseData == null) { this.unitBaseData = new List<IUnitBase>(); } else { this.unitBaseData = unitBaseData; }
        if (unitData == null) { this.unitData = new List<IUnit>(); } else { this.unitData = unitData; }
        if (buildingData == null) { this.buildingData = new List<IBuilding>(); } else { this.buildingData = buildingData; }
        if (vectorData == null) { this.vectorData = new List<Vector3Int>(); } else { this.vectorData = vectorData; }
    }
}
