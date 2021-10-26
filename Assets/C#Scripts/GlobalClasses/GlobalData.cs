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
    //public static readonly Quaternion globalRotation = Quaternion.Euler(-40, 0, 0);
    //public static readonly Quaternion rotation = Quaternion.Euler(-90, 0, 0);

    public static readonly KeyCode up = KeyCode.UpArrow;
    public static readonly KeyCode down = KeyCode.DownArrow;
    public static readonly KeyCode left = KeyCode.LeftArrow;
    public static readonly KeyCode right = KeyCode.RightArrow;

    public static readonly KeyCode forward = KeyCode.A;
    public static readonly KeyCode back = KeyCode.B;

    public static readonly float waitTime = 0.1f;

    public static readonly Vector3 pointerOffset = new Vector3(0, 0.5f, 0);

    public static readonly int pixelPerUnit = 100;
    public static readonly int tileRadius = 60;

    public static readonly string unaligned = "Unaligned";
    public static readonly string neutral = "Neutral";
    public static readonly string endTurn = "end turn";
    public static readonly string endUnitTurn = "end unit turn";
    //called from resources.load
    public static readonly string unitBaseScriptPath = "Scripts/UnitScripts/";
    public static readonly string mapScriptPath = "Scripts/MapScripts/";
    public static readonly string buffScriptPath = "Scripts/BuffScripts/";

    //called from xml load
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
    
    public static readonly float animationSpeed = 15f;
    //public static readonly int animationTime = 50; //will uncomment when it is needed

    //configurable statics below
    public static int clockTotal = 6;

    //public static float elevated_units = 0.25f;
    /*terrain matrix movement type 
     * 0:ship: all ships
     * 1:fin: naval units that arent ships, cant go on beaches/ shallows
     * 2:slither: naga hero, amphibious units
     * 3:spider: crypt fiends, crypt lord, carrion beetle
     * 4:foot: footmen, riflemen, archers, mortars, grunts, head hunters, all other spellcasters, all other heroes
     * 5:hoof: knight, archmage, tauren, spiritwalker, tauren hero, dryad, keeper, dk
     * 6:wing: all other air units 
     * 7:propellor: flying machines, zepplin
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
    scrapping sky terrain types, clouds will be tile buffs that obstruct sky unit vision but up defense, will be part of weather patterns feature
    */

    private static readonly bool[,] landVision = new bool[,] {
        { true, true, true, true, false, false, false, true, false, false, true },
        { true, true, true, true, false, false, false, true, false, false, true },
        { true, true, true, true, false, false, false, true, false, false, true },
        { true, true, true, true, false, false, false, true, false, false, true },
        { true, true, true, true, true, false, false, true, true, false, true },//
        { false, false, false, false, false, true, false, false, false, false, false },//
        { false, false, false, false, false, false, false, false, false, false, false },//
        { true, true, true, true, false, false, false, true, false, false, true },
        { true, true, true, true, true, false, false, true, true, false, true },//
        { false, false, false, false, false, false, false, false, false, true, false },//
        { true, true, true, true, false, false, false, true, false, false, true },
    };

    private static readonly bool[,] skyVision = new bool[,] {
        { true, true, true, true, false, true, true, true, false, false, true },
        { true, true, true, true, false, true, true, true, false, false, true },
        { true, true, true, true, false, true, true, true, false, false, true },
        { true, true, true, true, false, true, true, true, false, false, true },
        { true, true, true, true, false, true, true, true, false, false, true },
        { true, true, true, true, false, true, true, true, false, false, true },
        { true, true, true, true, false, true, true, true, false, false, true },
        { true, true, true, true, false, true, true, true, false, false, true },
        { true, true, true, true, false, true, true, true, false, false, true },
        { true, true, true, true, false, true, true, true, false, false, true },
        { true, true, true, true, false, true, true, true, false, false, true },
    };

    //terrain movecost due to movement type - movecost = terrainmatrix[movementtype, terraintype]
    public static readonly int[,] terrainMoveCostMatrix = new int[,] {
        { 2, 1, 4, 2, 3, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
    };

    /*terrain defense star = 1 armour per star varying from -3 to +3 terrain defense bonus due to movement type - armourstars = terraindefensematrix[movementtype, terraintype]  */
    public static readonly int[,] terrainDefenseMatrix = new int[,] {
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
    };

    /* attack types = 0 normal, 1 piercing, 2 siege, 3 magic, 4 chaos, 5 hero*/
    /* armour types = 0 no armour, 1 light, 2 medium, 3 heavy, 4 fortified, 5 hero*/
    /*attack modifier = attacktoarmour[attacktype, targetarmourtype]*/
    public static readonly float[,] attackToArmour = new float[,] {
        { 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1 }
    };

    public static bool BothLandOrSky(int movementTypeA, int movementTypeB) => ((movementTypeA == 6 || movementTypeA == 7) && (movementTypeB == 6 || movementTypeB == 7)) || ((movementTypeA != 6 || movementTypeA != 7) && (movementTypeB != 6 || movementTypeB != 7));

    public static int TerrainVision(int terrain, int movementType, int vision)
    {
        if (movementType != 6 || movementType != 7)
        {
            if (terrain == 6) { return vision + 3; }
            else if (terrain == 4 || terrain == 8) { return 1; }
        }
        return vision;
    }

    public static bool CanSee(int terrain, int terrainThere, int movementType)
    {
        if (movementType != 6 || movementType != 7)//ground unit
        {
            /*
            if ((terrain == 5 || terrain == 9)/highgrass sees highgrass ravines see ravines/&& terrainThere == terrain)
            {
                return true;
            }
            else if ((terrain == 4 || terrain == 8)/woods/coves see neighbors that are not highgrass or ravines or mountains/&& terrainThere != 5 && terrainThere != 9 && terrainThere != 6)
            {
                return true;
            }
            else if (terrainThere != 5 && terrainThere != 9 && terrainThere != 6 && terrainThere != 4 && terrainThere != 8)
            {
                return true;
            }*/
            return landVision[terrain, terrainThere];
        }
        /*
        else if (terrainThere != 4 && terrainThere != 9 && terrainThere != 8)
        {
            return true;
        }
        return false;*/
        else { return skyVision[terrain, terrainThere]; }
    }

}