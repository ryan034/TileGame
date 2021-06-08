using System.IO;
using UnityEngine;

public static class GlobalData
{
    /*
    public enum UnitMaterial {
        [EnumMember(Value = "mechanical")]
        mechanical,
        [EnumMember(Value = "biological")]
        biological,
        [EnumMember(Value = "magical")]
        magical };
        */
    /*public enum Race {
        [EnumMember(Value = "noRace")]
        noRace,
        [EnumMember(Value = "race1")]
        race1,
        [EnumMember(Value = "race2")]
        race2,
        [EnumMember(Value = "race3")]
        race3,
        [EnumMember(Value = "race4")]
        race4 };
        */
    public static readonly Quaternion globalRotation = Quaternion.Euler(-40, 0, 0);
    public static readonly Quaternion rotation = Quaternion.Euler(90, 0, 180);
    //public static MapXml map;
    //public static readonly int tileHeight = 72;
    //public static readonly int tileWidth = 63;
    public static readonly int pixelPerUnit = 100;
    public static readonly int tileRadius = 60;

    //called from resources.load
    public static readonly string unitBaseScriptPath = "Scripts/UnitScripts/";
    //public static readonly string buildingScriptPath = "Scripts/BuildingScripts/";
    public static readonly string mapScriptPath = "Scripts/MapScripts/";
    public static readonly string buffScriptPath = "Scripts/BuffScripts/";

    //called from xml load
    //public static readonly string unitModXmlPath = Path.Combine(Application.streamingAssetsPath, "Xml/UnitXml/");
    public static readonly string unitBaseModXmlPath = Path.Combine(Application.streamingAssetsPath, "Xml/UnitBaseXml/");
    public static readonly string customMapXmlPath = Path.Combine(Application.streamingAssetsPath, "Xml/MapXml/");
    public static readonly string buffModXmlPath = Path.Combine(Application.streamingAssetsPath, "Xml/BuffXml/");

    //called from resources.load
    public static readonly string unitAssetPath = "Prefabs/UnitPrefabs/";
    public static readonly string buildingAssetPath = "Prefabs/BuildingPrefabs/";
    public static readonly string tileAssetPath = "Prefabs/TilePrefabs/";

    //called from assetbundle loadfromfile
    public static readonly string unitModAssetPath = /*"StreamingAssets/Prefabs/UnitPrefabs/";*/ Path.Combine(Application.streamingAssetsPath, "Prefabs/UnitPrefabs/");
    public static readonly string buildingModAssetPath = Path.Combine(Application.streamingAssetsPath, "Prefabs/BuildingPrefabs/");
    public static readonly string tileModAssetPath = Path.Combine(Application.streamingAssetsPath, "Prefabs/TilePrefabs/");

    //Application.dataPath + "/Resources/Maps/" + mapName + ".xml"

    public static float animationSpeed = 15f;
    public static int animationTime = 50;
    public static int clockTotal = 6;
    //public static float elevated_units = 0.25f;
    /*terrain matrix movement type 
     * 0:ship: all ships
     * 1:fish: naval units that arent ships, cant go on beaches
     * 2:amphibious: naga hero, amphibious units
     * 3:spider: crypt fiends, crypt lord, carrion beetle
     * 4:foot: footmen, riflemen, archers, mortars, grunts, head hunters, all other spellcasters, all other heroes
     * 5:hoof: knight, archmage, tauren, spiritwalker, tauren hero, dryad, keeper, dk
     * 6:wing: all other air units 
     * 7:motor: flying machines, zepplin
     * 8:tread: all other siege units
     * 9:float: lich, statues, sorceress, fire lord, banshee, shade, elementals
     * 10:paw: pit lord, kodo, ghouls, huntresses, raiders, bears, wolves, far seer, potm
    terraintype:
    terrain can be elevated or ground level, elevated terrain gives sight bonus for ground units but incurs extra movement cost to reach there
    0:grass
    1:roads
    2:river/shallows -both land and sea can traverse on foot/hoof
    3:beach -both land and sea can traverse on foot/hoof
    4:woods - obstructs vision from flying
    5:highgrass
    6:mountain
    7:deepwater
    8:coves - gives ships -3 defense, good defense for amphibious, like woods for sea
    9:canyons/ravines/storms - obstructs movement of flying units. Obstructs motor the most. similiar to mountains for ground units
    10:fissure - similiar to mountains but walls of ground units, 
    sky terrain types:
    0:clear sky
    1:clouds - gives flying units high defense generate in symmetrical pattern
    terrain movecost due to movement type - movecost = terrainmatrix[movementtype, terraintype]*/
    public static readonly int[,] terrainMatrix = new int[,] { { 2, 1, 4, 2, 3, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
    /*terrain defense star = 1 armour per star varying from -3 to +3 terrain defense bonus due to movement type - armourstars = terraindefensematrix[movementtype, terraintype]  */
    public static readonly int[,] terrainDefenseMatrix = new int[,] { { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
    public static readonly int[,] skyDefenseMatrix = new int[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 3 }, { 0, 3 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
    /* attack types = 0 normal, 1 piercing, 2 siege, 3 magic, 4 chaos, 5 hero*/
    /* armour types = 0 no armour, 1 light, 2 medium, 3 heavy, 4 fortified, 5 hero*/
    /*attack modifier = attacktoarmour[attacktype, targetarmourtype]*/
    public static readonly float[,] attackToArmour = new float[,] { { 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1 } };

}