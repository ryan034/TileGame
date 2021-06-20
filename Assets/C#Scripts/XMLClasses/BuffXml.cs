using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using static GlobalData;

[XmlRoot("BuffXml")]
public class BuffXml
{
    public string name;
    public List<EventCodeBlock> code;
    public List<string> unitTags;
    public int duration;
    public int armour;
    public int hP;
    public int mP;
    public bool invisible;
    public bool notHostile;
    public bool rooted;
    public bool disarmed;
    public bool silenced;
    public int dayVision;
    public int nightVision;

    public int buildingCover;
    public int movementTotal;
    public int captureDamage;

    public struct EventCodeBlock
    {
        public string event_;
        public string trigger;
        public string code;
        public string animation;
    }

    public static BuffXml Load(string file)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(BuffXml));
        try
        {
            using (var stream = new FileStream(buffModXmlPath + file + ".xml", FileMode.Open))
            {
                BuffXml buffXml = serializer.Deserialize(stream) as BuffXml;
                //return new MapScript(mapXml);
                return buffXml;
            }
        }

        catch (System.Exception)
        {
            return null;
        }
    }
}