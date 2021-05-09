using static Globals;
using UnityEngine;
using System.Collections.Generic;
using System;

public class TileObject : MonoBehaviour
{

    //public Unit unit;
    //public Building building;
    /*
    public lists of buffs from auras
    */
    public Vector3Int LocalPlace { get; private set; }
    //public int id; //index in prefablist
    
    public int TerrainType { get; private set; }//int to determine terrain type
    public int SkyTerrainType { get; private set; } //int to determine terrain type

    public Unit Unit => TileManager.globalInstance.GetUnit(LocalPlace);
    public Building Building => TileManager.globalInstance.GetBuilding(LocalPlace);

    public TileObject exploredFrom;
    public int moveScore = int.MaxValue;

    public bool IsTarget { get => isTarget; set { if (isTarget != value) { isTarget = value; RefreshSprite(); } } } //alreadymoved and attacked
    private bool isTarget;

    public bool IsExplored { get => isExplored; set { if (isExplored != value) { isExplored = value; RefreshSprite(); } } } //alreadymoved and attacked
    private bool isExplored;

    public bool CanSee { get => canSee; set { if (canSee != value) { canSee = value; RefreshSprite(); } } } //alreadymoved and attacked
    private bool canSee;

    //public bool elevated;

    //public Tileasset background;
    //public Tileasset foreground;

    private void RefreshSprite()
    {
        /*
        float r = 1;
        float g = 1;
        float b = 1;
        float w = 1;

        if (IsExplored and movescore is smaller than unit.movementscore)
        {
            //tile is yellow

            b = 0 / 255f;
            g = 255 / 255f;

        }
        else if (IsTarget)
        {

            g = 0;
            b = 0;

            //tile is green if unit on tile and unit.team==teamturn
            //else is red

        }

        else if (CanSee)
        {
            //default w
        }
        else
        {
            //dim tile
            w = 0.5f;
        }

        GetComponent<SpriteRenderer>().color = new Color(r * w, g * w, b * w);
        if (Unit != null) { Unit.RefreshSprite(); }
        if (Building != null) { Building.RefreshSprite(); }*/
    }

    public void Load(Vector3Int localPlace, int terrain, int skyTerrain)
    {
        LocalPlace = localPlace;
        SkyTerrainType = skyTerrain;
        TerrainType = terrain;
        transform.position = LocalToWord(LocalPlace);
        TileManager.globalInstance.AddTile(localPlace,this);
        //gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load("MapSprites/"+ sprite) as Sprite;
    }

    /*
internal void Refreshperspective(Vector3Int key)

{
   Vector3Int offset = Offset_to_cube(localplace) - Offset_to_cube(key);
   if (background != null)
   {
       background.Refreshperspective(offset);
   }

   if (foreground != null)
   {
       foreground.Refreshperspective(offset);
   }
}*/
}
