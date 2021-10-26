using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalAnimationParser
{
    //private List<StackItem> animationQueue = new List<StackItem>();

    /*
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
         Manager.UnitTransformManager.RotateTo(unitBase, u.Tile.LocalPlace);
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
}*/

    public static IEnumerator ParseAnimation(StackItem stackItem)
    {
        Vector3 forward = stackItem.owner.Forward;
        CodeObject code = stackItem.code;
        if (code.Task == "Animate")
        {
            if (code.GetVariable("to") != "")
            {
                IUnitBase u;
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
                stackItem.owner.RotateTo(u.Tile.LocalPlace);
            }
            if (code.GetVariable("animation") != "")
            {
                //if (code.GetVariable("duration") == "finish"){yield return stackItem.owner.PlayAnimationAndFinish(code.GetVariable("animation"));}
                yield return stackItem.owner.PlayAnimationAndFinish(code.GetVariable("animation"));
            }
        }
        stackItem.owner.SetForward(forward);
    }

    /*
    public void AddToAnimationQueue(CodeObject code, UnitBase owner)
    {
        int i = animationQueue.Count;
        StackItem s = new StackItem(code, "animation", owner);
        animationQueue.Add(s);
        if (i == 0) { StartCoroutine(ResolveAnimations()); }
    }

    private IEnumerator ResolveAnimations()
    {
        Pointer.globalInstance.haltInput = true;
        while (animationQueue.Count > 0)
        {
            yield return ParseAnimation(animationQueue[0]);
            animationQueue.RemoveAt(0);
        }
        Pointer.globalInstance.haltInput = false;
    }*/
}
