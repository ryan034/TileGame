using System.Collections;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{

    public IEnumerator ParseAnimation(StackItem stackItem)
    {
        Vector3 forward = stackItem.owner.transform.forward;
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
                Manager.UnitTransformManager.RotateTo(stackItem.owner, u.Tile.LocalPlace);
            }
            if (code.GetVariable("animation") != "")
            {
                /*
                if (code.GetVariable("duration") == "finish")
                {
                    yield return stackItem.owner.PlayAnimationAndFinish(code.GetVariable("animation"));
                }*/
                yield return stackItem.owner.PlayAnimationAndFinish(code.GetVariable("animation"));
            }
        }
        Manager.UnitTransformManager.RotateTo(stackItem.owner, forward);
    }
}
