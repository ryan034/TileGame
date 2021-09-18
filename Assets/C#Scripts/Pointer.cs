﻿using UnityEngine;
using static GlobalData;

public class Pointer : MonoBehaviour
{
    public static Pointer globalInstance;
    /*public bool HaltInput
    {
        get { return haltInput; }
        set
        {
            haltInput = value;
            if (value)
            {
                UIWindow.globalInstance.gameObject.SetActive(false);
            }
            else
            {
                UIWindow.globalInstance.gameObject.SetActive(true);
            }
        }
    }
    */
    public bool haltInput;
    //private bool windowUpBefore;
    private readonly float waitTime = 0.1f;
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
    }
    /*
    private void Start()
    {
        TileManager.globalInstance.UpdateSelectedTile(new Vector3Int(0, 0, 0));
        //transform.rotation = globalRotation;
    }
    */
    private void Update()
    {
        if (!haltInput)
        {
            timer += Time.deltaTime;
            if (timer > waitTime)
            {
                if (Input.GetKey(KeyCode.RightArrow)) { TriggerDirection(new Vector3Int(1, 0, 0)); }
                if (Input.GetKey(KeyCode.LeftArrow)) { TriggerDirection(new Vector3Int(-1, 0, 0)); }
                if (Input.GetKey(KeyCode.DownArrow)) { TriggerDirection(new Vector3Int(0, -1, 0)); }
                if (Input.GetKey(KeyCode.UpArrow)) { TriggerDirection(new Vector3Int(0, 1, 0)); }
                timer = 0f;
            }
            if (Input.GetKeyDown(KeyCode.A)) { TriggerForward(); }
            if (Input.GetKeyDown(KeyCode.B)) { TriggerBack(); }
        }
    }

    private void UpdatePosition(Vector3 v)
    {
        //Debug.Log(v);
        transform.position = v + pointerOffset;
        //CameraController.globalInstance.UpdateCamera(transform.position);
        //UpdatePosition(tiles[CurrentLocation].tile.transform.position);
    }

    private void TriggerDirection(Vector3Int v)
    {
        switch (mode)
        {
            case Mode.window:
                UIWindow.globalInstance.TriggerDirection(v);
                break;
            default:
                UpdatePosition(Manager.TileManager.UpdateSelectedTile(v));
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
                Manager.TileManager.SetHeldUnit();
                //mode = Mode.window;
                break;
            case Mode.moving:
                Manager.TileManager.CommitMove();
                //mode = Mode.window;
                break;
            case Mode.attacking:
                Manager.TileManager.CommitTarget();
                break;
        }
    }


    private void TriggerBack()
    {
        switch (mode)
        {
            case Mode.window:
                UpdatePosition(Manager.TileManager.RetractMoveFromWindow());
                UIWindow.globalInstance.gameObject.SetActive(false);
                break;
            case Mode.moving:
                UpdatePosition(Manager.TileManager.RetractMove());
                mode = Mode.open;
                break;
            case Mode.attacking:
                UpdatePosition(Manager.TileManager.RetractTarget());
                //TileManager.globalInstance.SetHeldUnit();
                UIWindow.globalInstance.SpawnMenu();
                break;
        }
    }

    public void Setup()
    {
        UpdatePosition(Manager.TileManager.UpdateSelectedTile(new Vector3Int(0, 0, 0)));
        //transform.rotation = globalRotation;
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
