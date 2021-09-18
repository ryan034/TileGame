using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Debugger : MonoBehaviour
{
    public static Debugger globalInstance;
    private Text text;
    private List<string> log = new List<string>();
    private int maxLength = 10;
    // Start is called before the first frame update
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
        text = gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    private void Update()
    {
        //text.text = Pointer.globalInstance.haltInput? "yes" : "no";
    }

    public static void AddToLog(string message)
    {
        globalInstance.log.Add(message);
        while (globalInstance.log.Count > globalInstance.maxLength)
        {
            globalInstance.log.RemoveAt(0);
        }
        globalInstance.text.text = "";
        foreach (string item in globalInstance.log)
        {
            globalInstance.text.text += item + "\n";
        }
    }
}
