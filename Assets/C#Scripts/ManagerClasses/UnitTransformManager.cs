using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Globals;

public class UnitTransformManager : MonoBehaviour
{
    //actual animation functions
    //private Unit unit;
    //private int teamColour;
    //private bool moving;
    //private List<Vector3Int> path = new List<Vector3Int>();
    public static UnitTransformManager globalInstance;
    //private Animator animator;
    //private List<AnimationCommand> commandsQueue = new List<AnimationCommand>();

    /*
private class AnimationCommand
{
    public UnitBase unit;
    public List<Vector3Int> path = new List<Vector3Int>();
    public string animation;
    //public Vector3Int target;
    //public bool death;
    //public float duration = 0f;
    public AnimationCommand(UnitBase unit, List<Vector3Int> path, string animation = "Move")
    {
        this.unit = unit;
        this.animation = animation;
        this.path = path;
    }

    public AnimationCommand(UnitBase unit, string animation, Vector3Int target, bool death = false)
    {
        this.unit = unit;
        this.animation = animation;
        this.target = target;
        this.death = death;
    }
}*/

    /*
    public void RefreshSprite(UnitBase unit)
    {
        //called when team is  or unit is actioned or if tile goes in fog
        if (!moving)
        {
            float r = 1;
            float g = 1;
            float b = 1;
            float a = 1.0f;
            float w = 1.0f;
            bool active = true;
            if (!tile.cansee)
            {
                //hide sprite
                active = false;
            }
            else if (invisible)
            {
                //sprite is opaque
                a = .5f;
            }
            if (unit.Actioned) { w = .5f; }

            GetComponent<SpriteRenderer>().color = new Color(r * w, g * w, b * w, a);
            GetComponent<SpriteRenderer>().enabled = active;
        }
    }

    public void Animate(UnitBase unit, int animation)
    {
        command.unit.GetComponent<Animator>().SetInteger("animation", animation);
        Pointer.globalInstance.HaltInput = true;
        command = new AnimationCommand(unit, animation);
    }
    
    public void ChangeModel(UnitBase unit, int i)
    {
        //changes model of
    }
    */

    public void QueuePath(UnitBase unit, List<Vector3Int> path)
    {
        //commandsQueue.Add(new AnimationCommand(unit, path, animation));
        //if (commandsQueue.Count == 1)
        //{
        //    commandsQueue[0].unit.Animate(animation);
        //    Pointer.globalInstance.HaltInput = true;
        //   RotateTo(unit, path[0]);
        //}
        StartCoroutine(SmoothLerp(unit, path));
    }

    /*
    public void QueueAnimation(UnitBase unit, string animation, Vector3Int target)
    {
        //Pointer.globalInstance.HaltInput = true;
        //unit.transform.forward = LocalToWord(target) - unit.transform.position;
        commandsQueue.Add(new AnimationCommand(unit, animation, target));
        if (commandsQueue.Count == 1)
        {
            commandsQueue[0].unit.Animate(animation);
            RotateTo(unit, target);
        }
    }*/

    public IEnumerator DestroyUnit(UnitBase unit)
    {
        unit.Animate("Death");
        yield return StartCoroutine(WaitForAnimation(unit, "Death"));
        Destroy(unit.gameObject);
    }

    /*
    commandsQueue.Add(new AnimationCommand(unit, animation, unit.Tile.LocalPlace, true));
    if (commandsQueue.Count == 1)
    {
        commandsQueue[0].unit.Animate(animation);
        RotateTo(unit, unit.Tile.LocalPlace);
    }*/

    public void SnapMove(UnitBase unit, Vector3Int v)
    {
        unit.transform.position = LocalToWord(v);
    }

    public void RotateTo(UnitBase unit, Vector3Int v)
    {
        if (LocalToWord(v) != unit.transform.position)
        {
            unit.transform.forward = LocalToWord(v) - unit.transform.position;
        }
    }

    private IEnumerator WaitForAnimation(UnitBase unit, string s)
    {
        while (unit.IsPlaying(s))
        {
            yield return null;
        }
        yield break;
    }

    private IEnumerator SmoothLerp(UnitBase unit, List<Vector3Int> path)
    {
        WaitForSeconds w = new WaitForSeconds(.01f);
        Pointer.globalInstance.haltInput = true;
        //unit.Animate("Run");
        foreach (Vector3Int v in path)
        {
            //RotateTo(unit, v);

            Vector3 finalPos = LocalToWord(v);
            while (Vector3.Distance(unit.transform.position, finalPos) > 0.01f)
            {
                unit.transform.position = Vector3.MoveTowards(unit.transform.position, new Vector3(finalPos.x,finalPos.y, 0), animationSpeed * Time.deltaTime);
                yield return w;
            }
            unit.transform.position = finalPos;
        }
        //unit.Animate("Idle");
        Pointer.globalInstance.haltInput = false;
    }

    private void Awake()
    {
        if (globalInstance == null)
        {
            globalInstance = this;
        }
        else if (globalInstance != this)
        {
            Destroy(gameObject);
        }
    }
}
