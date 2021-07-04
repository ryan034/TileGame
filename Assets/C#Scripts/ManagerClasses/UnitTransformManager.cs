using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalFunctions;
using static GlobalData;

public class UnitTransformManager : MonoBehaviour
{
    public void QueuePath(UnitBase unit, List<Vector3Int> path)
    {
        StartCoroutine(SmoothLerp(unit, path));
    }

    public IEnumerator DestroyUnit(UnitBase unit)
    {
        unit.Animate("Death");
        yield return StartCoroutine(WaitForAnimation(unit, "Death"));
        Destroy(unit.gameObject);
    }

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
                unit.transform.position = Vector3.MoveTowards(unit.transform.position, new Vector3(finalPos.x, finalPos.y, 0), animationSpeed * Time.deltaTime);
                yield return w;
            }
            unit.transform.position = finalPos;
        }
        //unit.Animate("Idle");
        Pointer.globalInstance.haltInput = false;
    }


}
