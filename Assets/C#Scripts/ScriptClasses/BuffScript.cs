using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBuff", menuName = "Buff")]

public class BuffScript : ScriptableObject
{
    public string buffName;
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
}
