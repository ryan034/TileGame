using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Globals;

public class UnitAnimator : MonoBehaviour
{
    //for reactionary animations such as take damage, death etc
    private Animator animator;
    private UnitBase unit;

    private void Start()
    {
        animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
    }

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

    public void RefreshSprite()
    {
        //called when team is  or unit is actioned or if tile goes in fog
        /*if (!moving)
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
        }*/
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
                UnitTransformManager.globalInstance.RotateTo(unit, stackItem.unitData[int.Parse(code.GetVariable("to"))].Tile.LocalPlace);
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
        if (transform.Find(model).gameObject == null)
        {
            AssetManager.globalInstance.InstantiateModel(model).transform.parent = gameObject.transform;
        }
        transform.Find(model).gameObject.SetActive(true);
        animator = transform.Find(model).gameObject.GetComponent<Animator>();
    }
}
