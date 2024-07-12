using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpecificOutput : MonoBehaviour
{
    public GameObject connectorOBJ;
    private Connector cnt;
    private Logger sceneLogger;

    private TextMeshProUGUI controllerOutTXT;
    public GameObject controllerOutLBL;

    // Start is called before the first frame update
    void Start()
    {
        controllerOutTXT = controllerOutLBL.GetComponent<TextMeshProUGUI>();
        cnt = connectorOBJ.GetComponent<Connector>();
        sceneLogger = cnt.getLogger();
    }

    // Update is called once per frame
    void Update()
    {
        if(sceneLogger == null){
            sceneLogger = cnt.getLogger();
        }
        string [] returnArray = sceneLogger.outputValues();
        controllerOutTXT.text = returnArray[0];
    }
}
