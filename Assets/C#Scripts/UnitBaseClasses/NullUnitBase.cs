using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullUnitBase : IUnitBase
{
    public Vector3 Forward { get => new Vector3(0, 0, 0); set { } }

    public string Name => "";
    public int TargetCount => 0;
    public string Race => "";

    public int MovementType => 0;
    public int ArmourType => 0;

    public int DayVision => 0;
    public int NightVision => 0;
    public int HPMax => 0;
    public int MPMax => 0;
    public int Armour => 0;

    public bool Disarmed => false;
    public bool Silenced => false;
    public bool Charming => false;

    public int HPCurrent => 0;
    public int MPCurrent => 0;

    public double HPPercentage => 0;
    public double MPPercentage => 0;

    public int Team => 0;

    public bool CurrentTurn => false;

    public bool Invisible { get => false; set { } }
    public bool Actioned { get => false; set { } }

    public IEnumerable<string> Abilities => new List<string>();

    public CodeObject GetLogicCode(string s) => null;
    public CodeObject GetTargetCode(string s) => null;
    public TileObject Tile => null;

    public int CoverBonus => 0;

    public int Vision => 0;

    public IEnumerable<TileObject> AddTargetTiles(int a, int b) { return new List<TileObject>(); }

    public void AddToStack(CodeObject code, string name, List<int> intData = null, List<IUnitBase> targetData = null, List<IUnit> unitTargetData = null, List<IBuilding> buildingTargetData = null, List<Vector3Int> vectorData = null, bool mainPhase = false) { }

    public void Animate(string v) { }

    public IEnumerator CalculateDamageTakenAndTakeDamage(bool before, IUnitBase unitBase, int damageType, int totaldamage) { yield return null; }

    public bool CanAttackAndHostileTo(IUnitBase owner, string v) => false;

    public bool CanHit(IUnitBase defender, string attackType) => false;

    public void ChooseMenuAbility(string s) { }

    public void ClearTargets() { }

    public void CommitTarget(Vector3Int currentLocation) { }

    public IEnumerator AttackTarget(bool before, IUnitBase unitBase, int v1, int v2, int v3, int v4, CodeObject codeObject) { yield return null; }

    public IEnumerator DestroyThis(IUnitBase killer) { yield return null; }

    public string GetConvertedForm(string unaligned) => "";

    public bool IsPlaying(string s) => false;

    public void Load(bool initial, Vector3Int localPlace, UnitBaseData data, int team) { }

    public void MoveToTile(TileObject tileObject) { }

    public IEnumerator PlayAnimationAndFinish(string v) { yield return null; }

    public void RefreshSprite() { }

    public void RotateTo(Vector3Int localPlace) { }

    public bool SameTeam(int team) => false;

    public void SetForward(Vector3 localPlace) { }

    public void SetUp(List<string> menu) { }

    public void SnapMove(Vector3Int currentLocation) { }

    public IEnumerator SpawnUnit(bool before, List<Vector3Int> vectorData, string v, int team, CodeObject codeObject) { yield return null; }

    public bool VisibleAndHostileTo(int team) => false;

    public bool WithinRange(int v1, int v2, IUnitBase owner) => false;
}
