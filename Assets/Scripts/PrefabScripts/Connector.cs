using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Text;
using System.IO;
using TMPro;
using Varjo.XR;
using System.Configuration;
using System.Linq;
using UnityEngine.XR;
using System;
using System.IO.Pipes;

public class Connector : MonoBehaviour
{
    public GameObject sceneLoggerObj;
    private Logger sceneLogger;
    public Grapher sceneGrapher;

    public int graph1, graph2, graph3, graph4 = 0;
    public GameObject RhorizontalAngleLBL;
    public GameObject RverticalAngleLBL;
    public GameObject LhorizontalAngleLBL;
    public GameObject LverticalAngleLBL;
    public GameObject ValueOnPressGO;
    public GameObject vergenceGO;
    public GameObject RIrisLBL;
    public GameObject RPupilLBL;
    public GameObject LIrisLBL;
    public GameObject LPupilLBL;
    public GameObject IPDLBL;
    public GameObject canvasScale;


    private TextMeshProUGUI RhorizontalAngleTXT;
    private TextMeshProUGUI RverticalAngleTXT;
    private TextMeshProUGUI LhorizontalAngleTXT;
    private TextMeshProUGUI LverticalAngleTXT;
    private TextMeshProUGUI valueOnPressTXT;
    private TextMeshProUGUI vergenceTXT;
    private TextMeshProUGUI RIrisTXT;
    private TextMeshProUGUI LIrisTXT;
    private TextMeshProUGUI RPupilTXT;
    private TextMeshProUGUI LPupilTXT;
    private TextMeshProUGUI IPDTXT;

    // Start is called before the first frame update
    void Start()
    {
        sceneLogger = sceneLoggerObj.GetComponent<Logger>();

        RhorizontalAngleTXT = RhorizontalAngleLBL.GetComponent<TextMeshProUGUI>();
        LhorizontalAngleTXT = LhorizontalAngleLBL.GetComponent<TextMeshProUGUI>();
        RverticalAngleTXT = RverticalAngleLBL.GetComponent<TextMeshProUGUI>();
        LverticalAngleTXT = LverticalAngleLBL.GetComponent<TextMeshProUGUI>();
        valueOnPressTXT = ValueOnPressGO.GetComponent<TextMeshProUGUI>();
        vergenceTXT = vergenceGO.GetComponent<TextMeshProUGUI>();
        RIrisTXT = RIrisLBL.GetComponent<TextMeshProUGUI>();
        LIrisTXT = LIrisLBL.GetComponent<TextMeshProUGUI>();
        RPupilTXT = RPupilLBL.GetComponent<TextMeshProUGUI>();
        LPupilTXT = LPupilLBL.GetComponent<TextMeshProUGUI>();
        IPDTXT = IPDLBL.GetComponent<TextMeshProUGUI>();
    }
    // Update is called once per frame
    void Update()
    {
        //obtain array containing data and names
        string [] returnArray = sceneLogger.connectorAccess();
        string[] returnArrayNames = sceneLogger.connectorFieldNames();

        //Data on txt fields
        RhorizontalAngleTXT.text = returnArray[0];
        LhorizontalAngleTXT.text = returnArray[1];
        RverticalAngleTXT.text = returnArray[2];
        LverticalAngleTXT.text = returnArray[3];
        vergenceTXT.text = returnArray[4];
        RIrisTXT.text = returnArray[5];
        LIrisTXT.text = returnArray[6];
        RPupilTXT.text = returnArray[7];
        LPupilTXT.text = returnArray[8];
        IPDTXT.text = returnArray[9];

        //data graphed
        sceneGrapher.setGraph1(float.Parse(returnArray[graph1]));
        sceneGrapher.setGraph2(float.Parse(returnArray[graph2]));
        sceneGrapher.setGraph3(float.Parse(returnArray[graph3]));
        sceneGrapher.setGraph4(float.Parse(returnArray[graph4]));

        //names of Data Graphed
        sceneGrapher.setGraph1Name(returnArrayNames[graph1]);
        sceneGrapher.setGraph2Name(returnArrayNames[graph2]);
        sceneGrapher.setGraph3Name(returnArrayNames[graph3]);
        sceneGrapher.setGraph4Name(returnArrayNames[graph4]);

    }

    public Logger getLogger() {
        return sceneLogger;
    }

    public GameObject getScaleCanvas() {
        if (canvasScale== null) {
            return null;
        } 
        return canvasScale;
    }
}
