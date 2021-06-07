using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

[XmlRoot("Map")]

public class MapXml
{
    //public int maxX;
    //public int maxY;
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

    public static MapXml Load(string file)
    {
        //Application.dataPath + "/Resources/Maps/" + mapName + ".xml"
        XmlSerializer serializer = new XmlSerializer(typeof(MapXml));
        try
        {
            using (var stream = new FileStream(customMapXmlPath + file + ".xml", FileMode.Open))
            {
                MapXml mapXml = serializer.Deserialize(stream) as MapXml;
                //return new MapScript(mapXml);
                return mapXml;
            }
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    public void Save(string file)
    {
        //Application.dataPath + "/Resources/Maps/" + mapName + ".xml"
        var serializer = new XmlSerializer(typeof(MapXml));
        using (var stream = new FileStream(customMapXmlPath + file, FileMode.Create))
        {
            serializer.Serialize(stream, this);
        }
    }
    /*
    //Loads the xml directly from the given string. Useful in combination with www.text.
    public static Mapxml LoadFromText(string text)
    {
        var serializer = new XmlSerializer(typeof(Mapxml));
        return serializer.Deserialize(new StringReader(text)) as Mapxml;
    }*/
}

