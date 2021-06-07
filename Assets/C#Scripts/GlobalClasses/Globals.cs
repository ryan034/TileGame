using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization;

public static class Globals
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
    /*
    public static Vector2Int Direction(Vector3Int here, Vector3Int there)
    {
        Vector3Int herecube = OffsetToCube(here);
        Vector3Int therecube = OffsetToCube(there);
        Vector3Int direction = therecube - herecube;
        if (direction == new Vector3Int(+1, -1, 0)) { return new Vector2Int(1, 0); }
        if (direction == new Vector3Int(+1, 0, -1)) { return new Vector2Int(1, -1); }
        if (direction == new Vector3Int(0, +1, -1)) { return new Vector2Int(-1, -1); }
        if (direction == new Vector3Int(-1, +1, 0)) { return new Vector2Int(-1, 0); }
        if (direction == new Vector3Int(-1, 0, +1)) { return new Vector2Int(-1, 1); }
        if (direction == new Vector3Int(0, -1, +1)) { return new Vector2Int(1, 1); }
        else { return new Vector2Int(0, 0); }
    }*/



    public static Vector3 LocalToWord(Vector3Int localplace)
    {
        float w = (Mathf.Sqrt(3) * tileRadius) / pixelPerUnit;
        float h = 0.75f * (2 * tileRadius / (float)pixelPerUnit);
        float offset = 0;
        if (Math.Abs(localplace.y) % 2 == 1)
        {
            offset = w / 2;
        }
        return new Vector3(localplace.x * w + offset, localplace.y * h, localplace.z);
    }

    public static List<Vector3Int> Neighbours(Vector3Int centre)
    {
        List<Vector3Int> cube_direction = new List<Vector3Int>() { new Vector3Int(+1, -1, 0), new Vector3Int(+1, 0, -1), new Vector3Int(0, +1, -1), new Vector3Int(-1, +1, 0), new Vector3Int(-1, 0, +1), new Vector3Int(0, -1, +1) };
        List<Vector3Int> neighbours = new List<Vector3Int>();
        foreach (Vector3Int n in cube_direction)
        {
            neighbours.Add(CubeToOffset(OffsetToCube(centre) + n));
        }
        return neighbours;
    }

    public static List<Vector3Int> CircleCoords(int radiusinner, int radiusouter, Vector3Int centre)//radiusinner=1,radiusouter=1 for melee 
    {
        List<Vector3Int> circlecoords = new List<Vector3Int>();
        List<Vector3Int> cube_direction = new List<Vector3Int>() { new Vector3Int(+1, -1, 0), new Vector3Int(+1, 0, -1), new Vector3Int(0, +1, -1), new Vector3Int(-1, +1, 0), new Vector3Int(-1, 0, +1), new Vector3Int(0, -1, +1) };
        for (int r = radiusinner; r <= radiusouter; r++)
        {
            Vector3Int cube = OffsetToCube(centre) + cube_direction[4] * r;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < r; j++)
                {
                    circlecoords.Add(CubeToOffset(cube));
                    cube = cube + cube_direction[i];
                }
            }
        }
        return circlecoords;
    }

    public static int Rolldamage(int baseDamage, int dice, int number, int cover)
    {
        int r = 0;
        for (var i = 0; i < number; i++)
        {
            if (cover < 0)
            {
                int roll = 0;
                for (int j = 0; j < cover - 1; j--)
                {
                    roll = Math.Max(UnityEngine.Random.Range(1, dice + 1), roll);
                }
                r = r + roll;
            }
            else
            {
                int roll = dice;
                for (int j = 0; j < cover + 1; j++)
                {
                    roll = Math.Min(UnityEngine.Random.Range(1, dice + 1), roll);
                }
                r = r + roll;
            }
        }
        return r + baseDamage;
    }

    public static int Distance(Vector3Int a, Vector3Int b)
    {
        Vector3Int ac = OffsetToCube(a);
        Vector3Int bc = OffsetToCube(b);
        return CubeDistance(ac, bc);
    }

    private static Vector3Int OffsetToCube(Vector3Int a)
    {
        int x = a.x - (a.y - (a.y & 1)) / 2;
        int z = a.y;
        int y = -x - z;
        return new Vector3Int(x, y, z);
    }

    private static Vector3Int CubeToOffset(Vector3Int a)
    {
        int x = a.x + (a.z - (a.z & 1)) / 2;
        int y = a.z;
        return new Vector3Int(x, y, 0);
    }

    private static int CubeDistance(Vector3Int a, Vector3Int b)
    {
        return (Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) + Math.Abs(a.z - b.z)) / 2;
    }

}
//write add to menu method of unit base
//change control flow like advance wars - done needs testing
//animation triggers, implement stack and animations for stack

//buff manager and events system
//refactor pointer and uiwindow to adhere to controller data ui architecture -done for now
//system for animations states/sprites and loading them from script:
//piety system


//think about when to unload asset ie asset management
//use yield return instead of lists (use yield break to stop based on condition)
//use coroutines
//look into code redundancy
//getting coding standards in place, something like: properties then protected virtual then public virtual then protected then public