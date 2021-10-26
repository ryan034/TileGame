using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnitBase
{
    //Transform transform { get; }
    Vector3 Forward { get; set; }

    string Name { get; }

    int TargetCount { get; }

    string Race { get; }

    int MovementType { get; }
    int ArmourType { get; }

    int DayVision { get; }
    int NightVision { get; }

    int HPMax { get; }
    int MPMax { get; }

    int Armour { get; }

    bool Disarmed { get; }
    bool Silenced { get; }
    bool Charming { get; }

    int HPCurrent { get; }
    int MPCurrent { get; }

    double HPPercentage { get; }
    double MPPercentage { get; }

    int Team { get; }
    bool CurrentTurn { get; }

    bool Invisible { get; set; }
    bool Actioned { get; set; }

    IEnumerable<string> Abilities { get; }

    TileObject Tile { get; }

    int CoverBonus { get; }
    int Vision { get; }

    bool SameTeam(int team);

    void RefreshSprite();

    IEnumerator DestroyThis(IUnitBase killer);

    IEnumerator CalculateDamageTakenAndTakeDamage(bool before, IUnitBase unitBase, int damageType, int totaldamage);

    CodeObject GetTargetCode(string s);
    CodeObject GetLogicCode(string s);

    string GetConvertedForm(string unaligned);

    bool IsPlaying(string s);

    bool VisibleAndHostileTo(int team);

    IEnumerator PlayAnimationAndFinish(string v);

    void SetUp(List<string> menu);

    void ClearTargets();

    void ChooseMenuAbility(string s);

    void CommitTarget(Vector3Int currentLocation);

    void Load(bool initial, Vector3Int localPlace, UnitBaseData data, int team);

    IEnumerable<TileObject> AddTargetTiles(int a, int b);

    bool CanHit(IUnitBase defender, string attackType);

    IEnumerator SpawnUnit(bool before, List<Vector3Int> vectorData, string v, int team, CodeObject codeObject);

    IEnumerator DamageTarget(bool before, IUnitBase unitBase, int v1, int v2, int v3, int v4, CodeObject codeObject);

    bool CanAttackAndHostileTo(IUnitBase owner, string v);

    bool WithinRange(int v1, int v2, IUnitBase owner);
    void RotateTo(Vector3Int localPlace);
    void SetForward(Vector3 localPlace);
    void Animate(string v);
    void MoveToTile(TileObject tileObject);
    void SnapMove(Vector3Int currentLocation);
    void AddToStack(CodeObject code, string name, List<int> intData = null, List<IUnitBase> targetData = null, List<IUnit> unitTargetData = null, List<IBuilding> buildingTargetData = null, List<Vector3Int> vectorData = null, bool mainPhase = false);
}
