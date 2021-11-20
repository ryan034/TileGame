using System.Collections;
using UnityEngine;
using static GlobalData;

public class UnitAnimator : MonoBehaviour
{
    //for reactionary animations such as take damage, death etc
    private Animator animator;
    private IUnitBase unitBase;

    protected static AssetManager assetManager;

    private IEnumerator WaitForAnimation(string s)
    {
        yield return new WaitForSeconds(2f);
        while (IsPlaying(s))
        {
            yield return null;
        }
    }

    /*
    private IEnumerator WaitForDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
    }*/

    public void Load(IUnitBase unitBase)
    {
        this.unitBase = unitBase;
        animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        assetManager = Manager.AssetManager;
    }

    private void HideModel(bool b, Animator a = null)
    {
        a = a ?? animator;
        if (a.gameObject.GetComponent<MeshRenderer>() != null)
        {
            a.gameObject.GetComponent<MeshRenderer>().enabled = b;
        }
        else
        {
            foreach (Transform child in a.transform)
            {
                child.gameObject.SetActive(b);
            }
        }
    }

    public void RefreshUnitSprite()
    {
        //called when team is  or unit is actioned or if tile goes in fog
        /*float r = 1;
        float g = 1;
        float b = 1;*/
        float a = 1f;
        float w = 1f;
        bool active = true;
        if (!unitBase.CurrentTurn)
        {
            if (!unitBase.Tile.CanSee || unitBase.Invisible)//hide sprite
            { active = false; }
        }
        else if (unitBase.Invisible && unitBase.CurrentTurn)
        {
            //sprite is opaque
            a = .5f;
        }
        if (unitBase.Actioned) { w = .5f; }
        if (unitBase.Tile.Building != null && !unitBase.Tile.Building.SameTeam(unitBase.Team) && unitBase.Invisible) { unitBase.Invisible = false; }
        //animator.gameObject.SetActive(active);
        HideModel(active);
    }

    public void RefreshBuildingSprite()
    {
        //called when team is  or unit is actioned or if tile goes in fog
        /*float r = 1;
        float g = 1;
        float b = 1;*/
        float a = 1f;
        float w = 1f;
        if (!unitBase.Tile.CanSee)
        {
            //go to default form of building
            ChangeModel(unitBase.GetConvertedForm(unaligned));
            w = .5f;
        }
        else if (unitBase.Tile.CanSee)
        {
            ChangeModel(unitBase.GetConvertedForm(unitBase.Race));
        }
        else if (unitBase.Invisible)
        {
            //sprite is opaque
            a = .5f;
        }
        if (unitBase.Actioned) { w = .5f; }
    }

    public void Animate(string code)
    {
        animator.Play(code, 0, 0f);
    }
    /*
    public IEnumerator DestroyUnit()
    {
        yield return PlayAnimationAndFinish("Death");
        Destroy(gameObject);
    }

    public IEnumerator PlayWaitForDuration(float v)
    {
        yield return StartCoroutine(WaitForDuration(v));
    }*/

    public IEnumerator PlayAnimationAndFinish(string s)
    {
        Animate(s);
        return WaitForAnimation(s);
    }

    public bool IsPlaying(string animationCode)
    {
        //parse animation code
        return animator.GetCurrentAnimatorStateInfo(0).length > animator.GetCurrentAnimatorStateInfo(0).normalizedTime && animator.GetCurrentAnimatorStateInfo(0).IsName(animationCode);
        //return animator.GetCurrentAnimatorStateInfo(0).IsName(animationCode); // for now this is the function
    }

    public void ChangeModel(string model)
    {
        if (model != "")
        {
            if (transform.Find(model) == null)
            {
                GameObject g = assetManager.InstantiateModel(model);
                if (g != null)
                {
                    g.transform.parent = transform;
                    g.transform.localPosition = new Vector3(0, 0, 0);
                }
                else { return; }
            }
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Animator>() != null)
                {
                    HideModel(false, child.GetComponent<Animator>());
                    //child.gameObject.SetActive(false);}
                }
                //transform.Find(model).gameObject.SetActive(true);
                animator = transform.Find(model).gameObject.GetComponent<Animator>();
                HideModel(true);
            }
        }
    }
}
