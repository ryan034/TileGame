using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Globals;

public class UnitBaseData
{
    //public string gUID;
    public readonly string name;
    public readonly string race;
    public readonly int armourType;
    public readonly int movementType;
    public readonly int dayVision;
    public readonly int nightVision;
    public readonly int hP;
    public readonly int mP;
    public readonly int armour;

    public readonly bool infiltrator;
    public readonly int movementTotal;//total movement pts
    public readonly int captureRate;//normally 10
    public readonly bool neutral;
    public readonly int buildingCover;

    private Dictionary<string, string> buildingConversions = new Dictionary<string, string>();
    private Dictionary<string, ActiveAbility> abilitiesCode = new Dictionary<string, ActiveAbility>();
    private List<string> buffs = new List<string>();
    private List<string> unitTags = new List<string>();

    public IEnumerable<string> Abilities
    {
        get
        {
            foreach (string s in abilitiesCode.Keys)
            {
                yield return s;
            }
        }
    }

    public IEnumerable<string> Buffs
    {
        get
        {
            foreach (string s in buffs)
            {
                yield return s;
            }
        }
    }

    private struct ActiveAbility
    {
        public readonly CodeObject logicCode;
        public readonly CodeObject animationCode;

        public ActiveAbility(CodeObject code, CodeObject animation)
        {
            logicCode = code;
            animationCode = animation;
        }
    }

    public UnitBaseData(UnitBaseScript unitScript)
    {
        name = unitScript.name;
        //buffs = unitXml.buffs;
        race = unitScript.race;
        armourType = unitScript.armourType;
        movementType = unitScript.movementType;
        dayVision = unitScript.dayVision;
        nightVision = unitScript.nightVision;
        hP = unitScript.hP;
        mP = unitScript.mP;
        armour = unitScript.armour;
        infiltrator = unitScript.infiltrator;
        movementTotal = unitScript.movementTotal; //total movement pts
        captureRate = unitScript.captureRate; //normally 10
        infiltrator = unitScript.infiltrator;
        neutral = unitScript.neutral; //total movement pts
        buildingCover = unitScript.buildingCover; //normally 10
        foreach (UnitBaseScript.ActiveAbility item in unitScript.abilitiesCode)
        {
            abilitiesCode[item.name] = new ActiveAbility(CodeObject.LoadCode(item.code), CodeObject.LoadCode(item.animation));
        }
        foreach (UnitBaseScript.BuildingConversion item in unitScript.buildingConversions)
        {
            buildingConversions[item.race] = item.building;
        }
        foreach (string item in unitScript.buffs)
        {
            //string[] array = item.Split(':');
            //buffs.Add(array[0], array[1]);
            //Buff buff = AssetManager.globalInstance.LoadBuff(item);
            buffs.Add(item);
        }
        //materials = unitScript.materials;
        unitTags = unitScript.unitTags;
    }

    public UnitBaseData(UnitBaseXml unitXml)
    {
        name = unitXml.name;
        //buffs = unitXml.buffs;
        race = unitXml.race;
        armourType = unitXml.armourType;
        movementType = unitXml.movementType;
        dayVision = unitXml.dayVision;
        nightVision = unitXml.nightVision;
        hP = unitXml.hP;
        mP = unitXml.mP;
        armour = unitXml.armour;
        infiltrator = unitXml.infiltrator;
        movementTotal = unitXml.movementTotal; //total movement pts
        captureRate = unitXml.captureRate; //normally 10
        infiltrator = unitXml.infiltrator;
        neutral = unitXml.neutral; //total movement pts
        buildingCover = unitXml.buildingCover; //normally 10
        foreach (UnitBaseXml.ActiveAbility item in unitXml.abilitiesCode)
        {
            abilitiesCode[item.name] = new ActiveAbility(CodeObject.LoadCode(item.code), CodeObject.LoadCode(item.animation));
        }
        foreach (UnitBaseXml.BuildingConversion item in unitXml.buildingConversions)
        {
            buildingConversions[item.race] = item.building;
        }
        foreach (string item in unitXml.buffs)
        {
            //Buff buff = AssetManager.globalInstance.LoadBuff(item);
            buffs.Add(item);
        }
        //materials = unitXml.materials;
        unitTags = unitXml.unitTags;
    }

    public bool HasTag(string tag) => unitTags.Contains(tag);

    public CodeObject GetCode(string s)
    {
        return abilitiesCode[s].logicCode;
    }

    public CodeObject GetAnimation(string s)
    {
        return abilitiesCode[s].animationCode;
    }

    public string GetConvertedForm(string race)
    {
        if (buildingConversions.ContainsKey(race))
        {
            return buildingConversions[race];
        }
        else { return ""; }
    }
    /*
public UnitScript(UnitXml unitXml)
{
unitName = unitXml.unitName;
buffs = unitXml.buffs;
abilitiesCode = unitXml.abilitiesCode; 
materials = unitXml.materials; 
unitTags = unitXml.unitTags; 
race = unitXml.race; 
armourType = unitXml.armourType; 
movementType = unitXml.movementType; 
dayVision = unitXml.dayVision; 
nightVision = unitXml.nightVision; 
hP = unitXml.hP; 
mP = unitXml.mP; 
armour = unitXml.armour; 

infiltrator = unitXml.infiltrator; 
movementTotal = unitXml.movementTotal; //total movement pts
captureRate = unitXml.captureRate; //normally 10
}*/
}
