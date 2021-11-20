using System;
using System.Collections.Generic;
using UnityEngine;
using static GlobalData;
using static GlobalFunctions;

public class Pointer : MonoBehaviour
{
    public static Pointer globalInstance;

    public bool haltInput;

    private Vector3Int currentLocation;
    private Vector3Int CurrentLocation { get => currentLocation; set { currentLocation = value; transform.position = LocalToWorld(currentLocation) + pointerOffset; } }

    private TileObject SelectedTile => TileObject.TileAt(CurrentLocation);

    private bool FriendlyBuildingSelected => SelectedTile.Building != null && !SelectedTile.Building.Actioned && SelectedTile.Building.CurrentTurn;

    private IUnit heldUnit;
    private Vector3Int heldLocation;
    private Vector3 heldUnitForward;

    private float timer;
    private Mode mode = Mode.open;

    private enum Mode { open, moving, attacking, window }

    private void Awake()
    {
        if (globalInstance == null)
        {
            globalInstance = this;
        }
        else if (globalInstance != this)
        {
            Destroy(gameObject);
        }

        EventsManager.OnObjectDestroyUnit += OnObjectDestroyUnit;
    }

    private void OnDestroy()
    {
        EventsManager.OnObjectDestroyUnit -= OnObjectDestroyUnit;
    }

    private void Update()
    {
        if (!haltInput)
        {
            timer += Time.deltaTime;
            if (timer > waitTime)
            {
                if (Input.GetKey(right)) { TriggerDirection(new Vector3Int(1, 0, 0)); }
                if (Input.GetKey(left)) { TriggerDirection(new Vector3Int(-1, 0, 0)); }
                if (Input.GetKey(down)) { TriggerDirection(new Vector3Int(0, -1, 0)); }
                if (Input.GetKey(up)) { TriggerDirection(new Vector3Int(0, 1, 0)); }
                timer = 0f;
            }
            if (Input.GetKeyDown(forward)) { TriggerForward(); }
            if (Input.GetKeyDown(back)) { TriggerBack(); }
        }
    }

    private void OnObjectDestroyUnit(IUnit unit)
    {
        if (heldUnit == unit) { heldUnit = null; }
    }

    private void TriggerDirection(Vector3Int v)
    {
        switch (mode)
        {
            case Mode.window:
                UIWindow.globalInstance.TriggerDirection(v);
                break;
            default:
                UpdatePosition(CurrentLocation + v);
                break;
        }
    }

    private void TriggerForward()
    {
        switch (mode)
        {
            case Mode.window:
                UIWindow.globalInstance.Execute();
                break;
            case Mode.open:
                SetHeldUnit();
                //mode = Mode.window;
                break;
            case Mode.moving:
                CommitMove();
                //mode = Mode.window;
                break;
            case Mode.attacking:
                CommitTarget();
                break;
        }
    }

    private void CommitTarget()
    {
        if (SelectedTile.IsTarget)
        {
            heldUnit.CommitTarget(CurrentLocation);
        }
    }

    private void CommitMove()
    {
        if (SelectedTile.IsExplored && SelectedTile.PercievedUnitOnTile == null)
        {
            List<Vector3Int> path = TileObject.GetPath(SelectedTile);

            //HeldUnitTile.Unit = null;
            //TileObject.TileAt(path[path.Count - 1]).Unit = heldUnit;
            //heldUnit.StartCoroutineQueuePath(path);
            //heldUnit.Location = path[path.Count - 1];
            heldUnit.MoveToTile(TileObject.TileAt(path[path.Count - 1]));
            heldUnit.StartCoroutineQueuePath(path);
            TileObject.WipeTiles();
            if (SelectedTile == heldUnit.Tile)
            {
                SpawnMenu();
            }
            else { EndHeldUnitTurn();  /*trigger for ambush attack*/}
        }
        else if (SelectedTile == heldUnit.Tile)
        {
            TileObject.WipeTiles();
            SpawnMenu();
        }
    }


    private void SpawnMenu()
    {
        //get muenu options from held unit
        List<string> menu = new List<string>();
        if (SelectedTile.Unit != null && SelectedTile.Unit.CurrentTurn && !SelectedTile.Unit.Actioned)
        {
            SelectedTile.Unit.SetUp(menu);
            menu.Add(endUnitTurn);
        }
        else if (FriendlyBuildingSelected)
        {
            SelectedTile.Building.SetUp(menu);
        }
        else
        {
            menu.Add(endTurn);
        }

        UIWindow.globalInstance.SpawnMenu(menu);
        //GoToWindowMode();
    }


    private void SetHeldUnit()
    {
        if (SelectedTile.Unit != null && SelectedTile.Unit.CurrentTurn && !SelectedTile.Unit.Actioned && heldUnit == null)
        {
            heldUnit = SelectedTile.Unit;
            heldUnitForward = heldUnit.Forward;
            heldLocation = SelectedTile.LocalPlace;
            if (heldUnit.SomewhereToMove)
            {
                heldUnit.SetUpMovementTiles();
                GoToMovingMode();
            }
            else
            {
                SpawnMenu();
            }
        }
        else
        {
            SpawnMenu();
        }
    }

    private void TriggerBack()
    {
        switch (mode)
        {
            case Mode.window:
                RetractMoveFromWindow();
                UIWindow.globalInstance.gameObject.SetActive(false);
                break;
            case Mode.moving:
                RetractMove();
                mode = Mode.open;
                break;
            case Mode.attacking:
                RetractTarget();
                SpawnMenu();
                break;
        }
    }

    private void UpdatePosition(Vector3Int v)
    {
        if (TileObject.TileAt(v) != null)
        {
            CurrentLocation = v;
        }
    }

    private void RetractMoveFromWindow()
    {
        if (heldUnit != null)
        {
            if (heldUnit.SomewhereToMove)
            {
                //heldUnit.Tile.Unit = null;
                //TileObject.TileAt(heldLocation).Unit = heldUnit;
                //heldUnit.Tile = TileObject.TileAt(heldLocation);
                heldUnit.MoveToTile(TileObject.TileAt(heldLocation));
                CurrentLocation = heldLocation;
                heldUnit.SnapMove(CurrentLocation);
                heldUnit.SetUpMovementTiles();
                GoToMovingMode();
            }
            else
            {
                heldUnit.SetForward(heldUnitForward);
                heldUnit = null;
                GoToOpenMode();
            }
        }
        else
        {
            GoToOpenMode();
        }
    }

    private void RetractMove()
    {
        CurrentLocation = heldLocation;
        TileObject.WipeTiles();
        heldUnit.SetForward(heldUnitForward);
        heldUnit = null;
    }

    private void RetractTarget()
    {
        CurrentLocation = heldUnit.Tile.LocalPlace;
        TileObject.WipeTiles();
        heldUnit.ClearTargets();
    }

    public void EndHeldUnitTurn()
    {
        if (heldUnit != null)
        {
            Debugger.AddToLog("end unit turn");
            heldUnit.Actioned = true;
            TileObject.RefreshFogOfWar(heldUnit.Team);
            heldUnit = null;
        }
        GoToOpenMode();
    }

    public void Execute(string s)
    {
        if (s == endTurn)
        {
            //execute end turn
            UnitBase.EndAndStartNextTurn();
            GoToOpenMode();
        }
        else if (s == endUnitTurn)
        {
            //execute end unit turn
            EndHeldUnitTurn();
        }
        else
        {
            //execute based on if a unit is selected or building
            if (heldUnit != null)
            {
                heldUnit.ChooseMenuAbility(s);
            }
            else if (FriendlyBuildingSelected)
            {
                SelectedTile.Building.ChooseMenuAbility(s);
            }
        }
    }

    public void ClearHeldUnit(IUnitBase unit)
    {
        if (heldUnit == unit) { heldUnit = null; };
    }

    public void Setup()
    {
        UpdatePosition(new Vector3Int(0, 0, 0));
        UnitBase.EndAndStartNextTurn();
    }

    public void GoToWindowMode()
    {
        mode = Mode.window;
    }

    public void GoToMovingMode()
    {
        mode = Mode.moving;
    }

    public void GoToOpenMode()
    {
        mode = Mode.open;
    }

    public void GoToAttackingMode()
    {
        mode = Mode.attacking;
    }
}
