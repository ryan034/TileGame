using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuilding : IUnitBase
{
    int BuildingCover { get; }

    void TakeCaptureDamage(int damage, IUnit unit);
}
