using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullBuilding : NullUnitBase, IBuilding
{
    public int BuildingCover => 0;

    public void TakeCaptureDamage(int damage, IUnit unit) { }
}
