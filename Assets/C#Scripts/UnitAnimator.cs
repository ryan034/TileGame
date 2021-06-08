using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalData;

public class UnitAnimator : MonoBehaviour
{
    //for reactionary animations such as take damage, death etc
    private Animator animator;
    private UnitBase unit;

    private IEnumerator WaitForAnimation(string s)
    {
        while (IsPlaying(s))
        {
            yield return null;
        }
        yield break;
    }

    private IEnumerator WaitForDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
    }

    public void Load(UnitBase unitBase)
    {
        unit = unitBase;
        animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
    }

    public void RefreshSprite()
    {
        //called when team is  or unit is actioned or if tile goes in fog
        float r = 1;
        float g = 1;
        float b = 1;
        float a = 1f;
        float w = 1f;
        bool active = true;
        if (unit.Team != TileManager.globalInstance.TeamTurn)
        {
            if (!unit.Tile.CanSee || unit.Invisible)//hide sprite
            { active = false; }
        }
        else if (unit.Invisible && unit.Team == TileManager.globalInstance.TeamTurn)
        {
            //sprite is opaque
            a = .5f;
        }
        if (unit.Actioned) { w = .5f; }
        foreach (Transform child in transform)
        {
            //need to look into this
            //child.GetComponent<Renderer>().material.color = new Color(r * w, g * w, b * w, a);
            child.gameObject.SetActive(active);
        }
    }

    public void RefreshBuildingSprite()
    {
        //called when team is  or unit is actioned or if tile goes in fog
        float r = 1;
        float g = 1;
        float b = 1;
        float a = 1f;
        float w = 1f;
        bool active = true;
        if (!unit.Tile.CanSee)
        {
            //go to default form of building
            ChangeModel(unit.GetConvertedForm("Unaligned"));
            w = .5f;
        }
        else if (unit.Invisible)
        {
            //sprite is opaque
            a = .5f;
        }
        if (unit.Actioned) { w = .5f; }

        foreach (Transform child in transform)
        {
            //need to look into this
            //child.GetComponent<Renderer>().material.color = new Color(r * w, g * w, b * w, a);
            child.gameObject.SetActive(active);
        }
    }

    public void Animate(string code)
    {
        //parse code
        animator.Play(code);
    }

    public IEnumerator ParseAnimation(StackItem stackItem)
    {
        CodeObject code = stackItem.animationCode;
        if (code.Task == "Animate")
        {
            if (code.GetVariable("to") != "")
            {
                UnitBase u;
                string indexCode = code.GetVariable("to");
                switch (indexCode[0])
                {
                    case 'u':
                        u = stackItem.unitData[int.Parse(indexCode.Substring(1))];
                        break;
                    case 'b':
                        u = stackItem.buildingData[int.Parse(indexCode.Substring(1))];
                        break;
                    default:
                        u = stackItem.unitBaseData[int.Parse(indexCode)];
                        break;
                }
                UnitTransformManager.globalInstance.RotateTo(unit, u.Tile.LocalPlace);
            }
            if (code.GetVariable("animation") != "")
            {
                animator.Play(code.GetVariable("animation"));
            }
            if (code.GetVariable("duration") == "finish")
            {
                //int i = int.Parse(code.GetVariable("duration"));
                yield return StartCoroutine(WaitForAnimation(code.GetVariable("animation")));
            }
            else
            {
                yield return StartCoroutine(WaitForDuration(int.Parse(code.GetVariable("duration"))));
            }
        }
    }

    public bool IsPlaying(string animationCode)
    {
        //parse animation code
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animationCode); // for now this is the function
    }

    public void ChangeModel(string model)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        if (transform.Find(model) == null)
        {
            AssetManager.globalInstance.InstantiateModel(model).transform.parent = gameObject.transform;
        }
        transform.Find(model).gameObject.SetActive(true);
        animator = transform.Find(model).gameObject.GetComponent<Animator>();
    }

}
