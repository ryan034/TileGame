using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullUnit : NullUnitBase, IUnit
{
    public int MovementTotal => 0;

    public bool Infiltrator => false;

    public bool Rooted => false;

    public bool SomewhereToMove => false;

    public IEnumerator Capture(bool before, IBuilding building, int v, CodeObject codeObject) { yield return null; }

    public void SetUpMovementTiles() { }

    public void StartCoroutineQueuePath(List<Vector3Int> path) { }
}
