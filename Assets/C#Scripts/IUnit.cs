using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnit : IUnitBase
{
    int MovementTotal { get; }
    bool Infiltrator { get; }
    bool Rooted { get; }
    bool SomewhereToMove { get; }

    IEnumerator Capture(bool before, IBuilding building, int v, CodeObject codeObject);

    void StartCoroutineQueuePath(List<Vector3Int> path);

    void SetUpMovementTiles();
}
