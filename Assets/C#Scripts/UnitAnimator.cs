using System.Collections;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    //for reactionary animations such as take damage, death etc
    private Animator animator;
    private UnitBase unitBase;

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
        this.unitBase = unitBase;
        animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
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
        if (!unitBase.SameTeam(GlobalManager.PlayerManager.TeamTurn))
        {
            if (!unitBase.Tile.CanSee || unitBase.Invisible)//hide sprite
            { active = false; }
        }
        else if (unitBase.Invisible && unitBase.SameTeam(GlobalManager.PlayerManager.TeamTurn))
        {
            //sprite is opaque
            a = .5f;
        }
        if (unitBase.Actioned) { w = .5f; }
        if (unitBase.Tile.Building != null && !unitBase.Tile.Building.SameTeam(unitBase.Team) && unitBase.Invisible) { unitBase.Invisible = false; }
        animator.gameObject.SetActive(active);
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
            ChangeModel(unitBase.GetConvertedForm("Unaligned"));
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
                GlobalManager.UnitTransformManager.RotateTo(unitBase, u.Tile.LocalPlace);
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
        if (model != "")
        {
            if (transform.Find(model) == null)
            {
                GameObject g = GlobalManager.AssetManager.InstantiateModel(model);
                if (g != null)
                {
                    g.transform.parent = gameObject.transform;
                }
                else { return; }
            }
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
            //transform.Find(model).gameObject.SetActive(true);
            animator = transform.Find(model).gameObject.GetComponent<Animator>();
            animator.gameObject.SetActive(true);
        }
    }

}
