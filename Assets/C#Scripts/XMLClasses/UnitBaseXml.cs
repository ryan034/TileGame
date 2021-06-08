using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using static GlobalData;


[XmlRoot("UnitBaseXml")]
public class UnitBaseXml
{
    public string name;
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

    public bool infiltrator;
    public int movementTotal;//total movement pts
    //public int captureRate;//normally 10

    public bool neutral;
    public int buildingCover;//total movement pts // for armour bonuses

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
    public static UnitBaseXml Load( string form)
    {

        //Application.dataPath + "/Resources/Maps/" + mapName + ".xml"
        XmlSerializer serializer = new XmlSerializer(typeof(UnitBaseXml));
        try
        {
            using (FileStream stream = new FileStream(unitBaseModXmlPath + form + ".xml", FileMode.Open))
            {
                UnitBaseXml buildingXml = serializer.Deserialize(stream) as UnitBaseXml;
                return buildingXml;
            }
        }
        catch (System.Exception)
        {
            return null;
        }
    }
    /*
    public static UnitBaseXml Load(UnitBaseScript buildingScript)
    {
        UnitBaseXml b = new UnitBaseXml
        {
            name = buildingScript.buildingName,
            buffs = buildingScript.buffs,
            abilitiesCode = buildingScript.abilitiesCode,
            materials = buildingScript.materials,
            //race = buildingScript.race,
            armourType = buildingScript.armourType,
            movementType = buildingScript.movementType,
            dayVision = buildingScript.dayVision,
            nightVision = buildingScript.nightVision,
            hP = buildingScript.hP,
            mP = buildingScript.mP,
            armour = buildingScript.armour,
            unitTags = buildingScript.unitTags,
            neutral = buildingScript.neutral,
            buildingCover = buildingScript.buildingCover,//total movement pts // for armour bonuses
            infiltrator = buildingScript.infiltrator,
            movementTotal = buildingScript.movementTotal,//total movement pts
            captureRate = buildingScript.captureRate//normally 10
        };
        return b;
    }*/
    /*
public void Save(string file)
{
   //Application.dataPath + "/Resources/Maps/" + mapName + ".xml"
   var serializer = new XmlSerializer(typeof(MapXml));
   using (var stream = new FileStream(unitModScriptPath + file, FileMode.Create))
   {
       serializer.Serialize(stream, this);
   }
}*/

}
