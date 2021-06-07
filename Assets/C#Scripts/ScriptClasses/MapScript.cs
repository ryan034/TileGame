using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMap", menuName = "Map")]

public class MapScript : ScriptableObject
{
    public int teamsTotal;

    //public List<int> teamsColours;

    public List<TileInfo> tilesInfo;

    public struct TileInfo
    {
        public Vector3Int localPlace;
        public int terrain;
        public int skyTerrain;
        public string building;
        public int buildingTeam;
        public string unit;
        public int unitTeam;
        public string prefab;
        // main, gold, wood, production, dock, tier3, altar, tavern, camp, observatory
    }

    /*
    public MapScript(MapXml mapXml)
    {
        teamsTotal = mapXml.teamsTotal;
        foreach (TileInfo t in tilesInfo)
        {
            TileInfo tileInfo = new TileInfo
            {
                localPlace = t.localPlace,
                terrain = t.terrain,
                skyTerrain = t.skyTerrain,
                building = t.building,
                buildingTeam = t.buildingTeam,
                unit = t.unit,
                unitTeam = t.unitTeam,
                prefab = t.prefab
            };
            tilesInfo.Add(tileInfo);
        }
    }

    */
}
