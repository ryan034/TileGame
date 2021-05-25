using UnityEngine;
using System;
using System.Collections.Generic;
using static Globals;

[CreateAssetMenu(fileName = "UnitBaseScript", menuName = "UnitBaseScript")]

public class UnitBaseScript : ScriptableObject
{
    //public string gUID;
    public string buildingName;
    public List<string> buffs;
    public List<ActiveAbility> abilitiesCode;
    public List<BuildingConversion> buildingConversions;
    //public List<UnitMaterial> materials;
    public List<string> unitTags;
    public string race;
    public int armourType;
    public int movementType;
    public int dayVision;
    public int nightVision;
    public int hP;
    public int mP;
    public int armour;

    public bool neutral;
    public int buildingCover;//total movement pts // for armour bonuses

    public bool infiltrator;
    public int movementTotal;//total movement pts
    //public int captureRate;//normally 10

    public struct ActiveAbility
    {
        public string name;
        public string target;
        public string code;
        public string animation;
    }

    public struct BuildingConversion
    {
        public string race;
        public string building;
    }
    /*
    public BuildingScript(BuildingXml buildingXml)
    {
        buildingName = buildingXml.buildingName;
        buffs = buildingXml.buffs;
        abilitiesCode = buildingXml.abilitiesCode; 
        materials = buildingXml.materials; 
        race = buildingXml.race; 
        armourType = buildingXml.armourType; 
        movementType = buildingXml.movementType; 
        dayVision = buildingXml.dayVision; 
        nightVision = buildingXml.nightVision; 
        hP = buildingXml.hP; 
        mP = buildingXml.mP; 
        armour = buildingXml.armour; 
    }
*/
}