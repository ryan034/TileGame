/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using static Globals;

public class Brush : MonoBehaviour
{
    public static Brush globalinstance;
    public Text settings_text;
    float waittime = 0.1f;
    float timer;
    Vector3Int localplace = new Vector3Int(0,0,0);
    int brush_colour;//tile, building, unit
    int brush_colour_team;
    int brush_mode;//erase or spawn
    int brush_size; //single tile, small round brush, big round brush
    int symmetry_mode; //0 no symmetry, 1 vertical, 2 horizontal, 3 rotational

    void Awake()
    {
        if (globalinstance == null)
        {
            globalinstance = this;
        }
        else if (globalinstance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        transform.position = LocalToWord(localplace);
        transform.rotation = globalRotation;
        Refreshtext();
    }

    void Update()
    {
        timer += Time.deltaTime;
        {
            if (timer > waittime)
            {
                if (Input.GetKey(KeyCode.RightArrow)) { Move(new Vector3Int(1, 0, 0)); }
                if (Input.GetKey(KeyCode.LeftArrow)) { Move(new Vector3Int(-1, 0, 0)); }
                if (Input.GetKey(KeyCode.DownArrow)) { Move(new Vector3Int(0, -1, 0)); }
                if (Input.GetKey(KeyCode.UpArrow)) { Move(new Vector3Int(0, 1, 0)); }
                timer = 0f;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.A)) { Brush_apply(false, localplace); }
                if (Input.GetKeyDown(KeyCode.Q)) { Switch_colour(); Refreshtext(); }
                if (Input.GetKeyDown(KeyCode.W)) { Switch_team(); Refreshtext(); }
                if (Input.GetKeyDown(KeyCode.E)) { Switch_mode(); Refreshtext(); }
                if (Input.GetKeyDown(KeyCode.R)) { Switch_size(); Refreshtext(); }
                if (Input.GetKeyDown(KeyCode.T)) { Switch_symmetry(); Refreshtext(); }
                if (Input.GetKeyDown(KeyCode.S)) {MapCanvas.globalInstance.Savemap(); Refreshtext(); }
                if (Input.GetKeyDown(KeyCode.D)) { MapCanvas.globalInstance.Spawnmap(); Refreshtext(); }
            }
        }
    }

    private void Refreshtext()
    {
        settings_text.text = "";
        settings_text.text = "brush colour: " + brush_colour.ToString() + "\n"
            + "team: " + brush_colour_team.ToString() + "\n"
            + "brush mode: " + brush_mode.ToString() + "\n"
            + "brush size: " + brush_size.ToString() + "\n"
            + "symmetry mode: " + symmetry_mode.ToString();
    }

    private void Switch_symmetry()
    {
        symmetry_mode = (symmetry_mode + 1) % 4;//0 no symmetry, 1 vertical, 2 horizontal, 3 rotational
    }

    private void Switch_size()
    {
        brush_size = (brush_size + 1) % 5; //0=single tile
    }

    private void Switch_mode()
    {
        brush_mode = (brush_mode + 1) % 2; //0=paint, 1 = erase
    }

    private void Switch_team()
    {
        brush_colour_team = (brush_colour_team + 1) % MapCanvas.globalInstance.totalteams;
    }

    private void Switch_colour()
    {
        int brush_total = MapCanvas.globalInstance.mytileprefabs.Count + MapCanvas.globalInstance.myunitprefabs.Count + MapCanvas.globalInstance.mybuildingprefabs.Count;
    brush_colour = (brush_colour+1)%brush_total;
    }

    private void Brush_apply(bool symmetryapplied, Vector3Int localplace)
    {
        if(brush_mode == 0) {
            if (brush_size != 0)
            {
                foreach(Vector3Int v in CircleCoords(1,brush_size, localplace)) {
                     MapCanvas.globalInstance.Spawn(v, brush_colour, brush_colour_team);  
                }
            }
             MapCanvas.globalInstance.Spawn(localplace, brush_colour, brush_colour_team); 
        }
        else {
            if (brush_size != 0)
            {
                foreach (Vector3Int v in CircleCoords(1, brush_size, localplace))
                {
                     MapCanvas.globalInstance.Erase(v, brush_colour, brush_colour_team);
                }
            }
            MapCanvas.globalInstance.Erase(localplace, brush_colour, brush_colour_team); 
        }
        if (!symmetryapplied)
        {
            Spawn_symmertry();
        }
    }

    private void Spawn_symmertry()
    {
        //0 no symmetry, 1 vertical, 2 horizontal, 3 rotational
        Vector3Int mirror = new Vector3Int();
        int offset = 0;
        if (symmetry_mode == 1 || symmetry_mode == 3)
        {
            if (Math.Abs(localplace.y) % 2 == 1) { offset = -1; }
            if (symmetry_mode == 3) { mirror = new Vector3Int(-localplace.x + offset, -localplace.y, localplace.z); Brush_apply(true, mirror); }
            else { mirror = new Vector3Int(-localplace.x + offset, localplace.y, localplace.z); Brush_apply(true, mirror); }
        }
        if (symmetry_mode == 2) { mirror = new Vector3Int(localplace.x, -localplace.y, localplace.z); Brush_apply(true, mirror); }
    }

    private void Move(Vector3Int vector3Int)
    {
        if (MapCanvas.globalInstance.Withinbounds(localplace + vector3Int)) {
            localplace = localplace + vector3Int;
            transform.position = LocalToWord(localplace);
            CameraController.globalInstance.Updatecamera(transform.position, localplace);
        }
    }
}
*/
