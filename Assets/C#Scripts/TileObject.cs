using static GlobalFunctions;
using UnityEngine;
using System.Collections.Generic;
using System;

public class TileObject : MonoBehaviour
{
    /*
    public lists of buffs from auras
    */
    public Vector3Int LocalPlace { get; private set; }
    //public int id; //index in prefablist

    public int TerrainType { get; private set; }//int to determine terrain type
    //public int SkyTerrainType { get; private set; } //int to determine terrain type

    public Unit Unit => TileManager.globalInstance.GetUnit(LocalPlace);
    public Building Building => TileManager.globalInstance.GetBuilding(LocalPlace);

    public TileObject exploredFrom;
    public int moveScore = int.MaxValue;

    public bool IsTarget { get => isTarget; set { isTarget = value; RefreshSprite(); } } //alreadymoved and attacked
    private bool isTarget;

    public bool IsExplored { get => isExplored; set { isExplored = value; RefreshSprite(); } } //alreadymoved and attacked
    private bool isExplored;

    public bool CanSee { get => canSee; set { canSee = value; RefreshSprite(); } } //alreadymoved and attacked
    private bool canSee;

    //public bool elevated;
    private void RefreshSprite()
    {
        //gameObject.GetComponent<Renderer>().material.color = new Color(255, 255, 255);

        float r = 1;
        float g = 1;
        float b = 1;
        float w = 1;

        if (IsExplored /*&& moveScore < Unit.MovementTotal*/)
        {
            //tile is yellow
            b = 0f;

        }
        else if (IsTarget)
        {
            g = 0f;
            b = 0f;
            //tile is red
        }
        if (!CanSee)
        {
            //dim tile
            w = 0.5f;
        }

        GetComponent<Renderer>().material.color = new Color(r * w, g * w, b * w);
        foreach (Transform child in transform)
        {
            child.GetComponent<Renderer>().material.color = new Color(r * w, g * w, b * w);
        }
        if (Unit != null) { Unit.RefreshUnitSprite(); }
        if (Building != null) { Building.RefreshBuildingSprite(); }
    }

    public void Load(Vector3Int localPlace, int terrain)
    {
        LocalPlace = localPlace;
        //SkyTerrainType = skyTerrain;
        TerrainType = terrain;
        transform.position = LocalToWord(LocalPlace);
        TileManager.globalInstance.AddTile(localPlace, this);
        //gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load("MapSprites/"+ sprite) as Sprite;
    }

}
