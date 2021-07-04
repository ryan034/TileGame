using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWindow : MonoBehaviour
{
    public static UIWindow globalInstance;
    private int index;
    private List<string> menu = new List<string>();
    //List<generaldelegate> myDelegatelist;
    private Text text;

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

    private void Start()
    {
        gameObject.SetActive(false);
        text = transform.GetChild(0).gameObject.GetComponent<Text>();
    }

    private void RefreshText()
    {
        text.text = "";
        for (int i = 0; i < menu.Count; i++)
        {
            if (index == i)
            {
                text.text += ">" + menu[i] + "\n";
            }
            else
            {
                text.text += menu[i] + "\n";
            }
        }
    }

    public void Execute()
    {
        GlobalManager.TileManager.Execute(menu[index]);
        gameObject.SetActive(false);
    }

    public void SpawnMenu()
    {
        index = 0;
        menu.Clear();
        GlobalManager.TileManager.GetMenuOptions(menu);
        if (menu.Count == 0)
        {
            return;
        }
        RefreshText();
        gameObject.SetActive(true);
        Pointer.globalInstance.GoToWindowMode();
    }

    public void TriggerDirection(Vector3Int v)
    {
        if (v == new Vector3Int(0, 1, 0))
        {
            if (index > 0)
            {
                index = index - 1;
                RefreshText();
            }
        }
        else if (v == new Vector3Int(0, -1, 0))
        {
            if (index < menu.Count - 1)
            {
                index = index + 1;
                RefreshText();
            }
        }
    }

    /*
    public static void SetupMove()
    {
        //globalinstance.gameObject.SetActive(false);
        Pointer.globalinstance.GoToMovingMode();
        Tilemanager.globalinstance.SetUpMovementTiles();
    }
    */
}
