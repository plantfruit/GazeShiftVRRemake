using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

// Credit: Luis Mesias Flores
public class Logger_FollowObject : MonoBehaviour, Logger
{
    //Varjo Eye variables
    private VarjoEyeTracking.GazeCalibrationMode gazeCalibrationMode = VarjoEyeTracking.GazeCalibrationMode.Fast;
    private VarjoEyeTracking.GazeOutputFilterType gazeOutputFilterType = VarjoEyeTracking.GazeOutputFilterType.Standard;
    private VarjoEyeTracking.GazeOutputFrequency gazeOutputFrequency;
    private VarjoEyeTracking.GazeData gazeData;
    private bool calibrated = false;
    private bool run = true;
    private bool justPressed = false;

    public string [] returnArray;

    // Unity timer
    public Stopwatch timer;

    // Thread
    private Thread thread;

    // logger
    private StringBuilder eyeTrackingLogger;
    private string trialLogFile;
    private int logFileCounter = 0;

    private string LGazeO = "()";
    private string RGazeO = "()";
    private string LGazeD = "()";
    private string RGazeD = "()";
    private string LGazeDX = "()";
    private string RGazeDX = "()";
    private string LGazeDY = "()";
    private string RGazeDY = "()";
    private string RGazeDZ = "()";
    private string LGazeDZ = "()";
    private string RHorA = "()";
    private string RVerA = "()";
    private string LHorA = "()";
    private string LVerA = "()";
    private string valueOnPress = "Empty";
    private string vergence = "()";
    private string LPupilD = "()";
    private string RPupilD = "()";
    private string InterPupilD = "()";
    private string LIrisD = "()";
    private string RIrisD = "()";

    private float RHorAng = 0;
    private float LHorAng = 0;
    private float RVerAng = 0;
    private float LVerAng = 0;
    private float vergenceAng = 0;

    private string Quaternionx = "0";
    private string Quaterniony= "0";
    private string Quaternionz= "0";
    private string Quaternionw= "0";
    private string r = "0";
    private string showing = "NaN";
    private string readerHeaders = "";

    private bool buttonPressed = false;
    private bool errorFound = false;
    private double tstep = 0;
    private long timeViveCur = 0;
    private long oldStamp = 0;
    private string namestr = "";
    private float radToD = 180.0f / Mathf.PI;

    private List<VarjoEyeTracking.GazeData> dataSinceLastUpdate;
    private List<VarjoEyeTracking.EyeMeasurements> eyeMeasurementsSinceLastUpdate;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Debug.Log(gazeOutputFrequency.ToString());
        gazeOutputFrequency = VarjoEyeTracking.GazeOutputFrequency.Frequency200Hz;
        VarjoEyeTracking.SetGazeOutputFrequency(VarjoEyeTracking.GazeOutputFrequency.Frequency200Hz);
        UnityEngine.Debug.Log(gazeOutputFrequency.ToString());

        timer = new Stopwatch();

        // Start Unity timer
        timer.Start();

        //starts logger
        eyeTrackingLogger = new StringBuilder();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            StartCoroutine(logTrial());
            UnityEngine.Debug.Log("Logging");
        }

        if (Input.GetButtonDown("A Button"))
        {
            buttonPressed = true;
            justPressed = true;
        } else if (Input.GetButtonUp("A Button"))
        {
            buttonPressed = false;
        }

        int dataCount = VarjoEyeTracking.GetGazeList(out dataSinceLastUpdate, out eyeMeasurementsSinceLastUpdate);
        for (int i = 0; i < dataCount; i++)
        {
            LogGazeData(dataSinceLastUpdate[i], eyeMeasurementsSinceLastUpdate[i]);
        }
    }

    void LogGazeData(VarjoEyeTracking.GazeData data, VarjoEyeTracking.EyeMeasurements eyeMeasurements)
    {
        errorFound = false;
        if(data.status == VarjoEyeTracking.GazeStatus.Invalid){
            long timeViveCur = data.captureTime;
            double tstep = timer.Elapsed.TotalSeconds;
            //UnityEngine.Debug.Log("Eye status invalid");
            if (buttonPressed)
            {
                valueOnPress = "NaN";
            }
            logTrialData(HighResolutionDateTime().ToString(),tstep.ToString(), "NaN", "NaN,NaN,NaN", "NaN,NaN,NaN", "NaN,NaN,NaN", "NaN,NaN,NaN", "NaN", "NaN", "NaN", "NaN", "NaN", "NaN", "NaN", "NaN", "NaN", Quaternionx, Quaterniony, Quaternionz, Quaternionw, r, buttonPressed, showing);
            // logTrialData(tstep.ToString(), "NaN", "NaN,NaN,NaN", "NaN,NaN,NaN", "NaN,NaN,NaN", "NaN,NaN,NaN", "NaN", "NaN", "NaN", "NaN", Quaternionx, Quaterniony, Quaternionz, Quaternionw, r, buttonPressed, showing);
            errorFound = true;
        } else
        {
            long timeViveCur = data.captureTime;
            calculateFR(timeViveCur);
            float eyeLOpen = (float)data.leftStatus;
            float eyeROpen = (float)data.rightStatus;
            Vector3 eyeLGazeOrigin = data.left.origin;
            Vector3 eyeRGazeOrigin = data.right.origin;
            Vector3 eyeCombinedGazeOrigin = data.gaze.origin;
            Vector3 eyeLGazeDir = data.left.forward;
            Vector3 eyeRGazeDir = data.right.forward;
            Vector3 eyeCombinedGazeDir = data.gaze.forward;

            //format data
            double tstep = timer.Elapsed.TotalSeconds;
            LGazeO = eyeLGazeOrigin.x.ToString("0.####") + "," + eyeLGazeOrigin.y.ToString("0.####") + "," + eyeLGazeOrigin.z.ToString("0.####");
            RGazeO = eyeRGazeOrigin.x.ToString("0.####") + "," + eyeRGazeOrigin.y.ToString("0.####") + "," + eyeRGazeOrigin.z.ToString("0.####");
            LGazeD = eyeLGazeDir.x.ToString() + "," + eyeLGazeDir.y.ToString() + "," + eyeLGazeDir.z.ToString();
            RGazeD = eyeRGazeDir.x.ToString() + "," + eyeRGazeDir.y.ToString() + "," + eyeRGazeDir.z.ToString();
            RGazeDX = eyeRGazeDir.x.ToString("0.####");
            RGazeDY = eyeRGazeDir.y.ToString("0.####");
            LGazeDX = eyeLGazeDir.x.ToString("0.####");
            LGazeDY = eyeLGazeDir.y.ToString("0.####");
            RGazeDZ = eyeRGazeDir.z.ToString("0.####");
            LGazeDZ = eyeLGazeDir.z.ToString("0.####");
            RPupilD = eyeMeasurements.rightPupilDiameterInMM.ToString("0.####");
            LPupilD = eyeMeasurements.leftPupilDiameterInMM.ToString("0.####");
            InterPupilD = eyeMeasurements.interPupillaryDistanceInMM.ToString("0.####");
            RIrisD = eyeMeasurements.rightIrisDiameterInMM.ToString("0.####");
            LIrisD = eyeMeasurements.leftIrisDiameterInMM.ToString("0.####");
            //calculate angle
            if (eyeRGazeDir.z != 0) //&& eyeRGazeDir.y != 0 && eyeRGazeDir.x != 0)
            {
                RVerAng = toDegrees(Mathf.Atan(eyeRGazeDir.y / eyeRGazeDir.z));
                RHorAng = toDegrees(Mathf.Atan(eyeRGazeDir.x / eyeRGazeDir.z));
                //format angle data
                RVerA = RVerAng.ToString("0.####");
                RHorA = RHorAng.ToString("0.####");
            }
            else
            {
                RHorA = "NaN";
                RVerA = "NaN";
            }
            if (eyeLGazeDir.z != 0)// && eyeLGazeDir.y != 0 && eyeLGazeDir.x != 0)
            {
                LVerAng = toDegrees(Mathf.Atan(eyeLGazeDir.y / eyeLGazeDir.z));
                LHorAng = toDegrees(Mathf.Atan(eyeLGazeDir.x / eyeLGazeDir.z));
                //format angle data
                LHorA = LHorAng.ToString("0.####");
                LVerA = LVerAng.ToString("0.####");
            }
            else
            {
                LHorA = "NaN";
                LVerA = "NaN";
            }
            if(eyeRGazeDir.z != 0 && eyeLGazeDir.z != 0)
            {
                vergenceAng = RHorAng - LHorAng;
                vergence = vergenceAng.ToString("0.####");
            }
            else
            {
                vergence = "NaN";
            }

            if (buttonPressed && justPressed)
            {
                float ipd = eyeMeasurements.interPupillaryDistanceInMM;
                float calculatedVergence = Mathf.Atan((ipd / 2000.0f) / float.Parse(r)) * radToD * 2.0f;
                valueOnPress = calculatedVergence.ToString("0.####");
                justPressed = false;
            }

            logTrialData(HighResolutionDateTime().ToString(),tstep.ToString(), timeViveCur.ToString(), LGazeO, RGazeO, LGazeD, RGazeD, RHorA, LHorA, RVerA, LVerA, RPupilD, LPupilD, InterPupilD, RIrisD, LIrisD, Quaternionx, Quaterniony, Quaternionz, Quaternionw, r, buttonPressed, showing);
        }
    }

    private float toDegrees(float rads)
    {
        return (rads * 360.0f) / (2 * Mathf.PI);
    }

    private void OnApplicationQuit()
    {
        run = false;
    }

    private void OnDisable()
    {
        run = false;
    }

    void OnDestroy()
    {
        run = false;
    }


    private void logTrialData(string performaceTimer, string tStep, string tStamp, string LGazeO, string RGazeO, string LGazeD, string RGazeD, string RHorA, string LHorA, string RVerA, string LVerA, string RPupil, string LPupil, string IntPupil, string RIris, string LIris, string qx, string qy, string qz, string qw, string rad, bool press, string show)
    {
        //non-dynamic values
        //fixTime = Time.fixedTime.ToString();
        eyeTrackingLogger.Append(performaceTimer + ",");
        eyeTrackingLogger.Append(tStep + ",");
        eyeTrackingLogger.Append(tStamp + ",");
        eyeTrackingLogger.Append(LGazeO + ",");
        eyeTrackingLogger.Append(RGazeO + ",");
        eyeTrackingLogger.Append(LGazeD + ",");
        eyeTrackingLogger.Append(RGazeD + ",");
        eyeTrackingLogger.Append(RHorA + ",");
        eyeTrackingLogger.Append(LHorA + ",");
        eyeTrackingLogger.Append(RVerA + ",");
        eyeTrackingLogger.Append(LVerA + ",");
        eyeTrackingLogger.Append(RPupil + ",");
        eyeTrackingLogger.Append(LPupil + ",");
        eyeTrackingLogger.Append(IntPupil + ",");
        eyeTrackingLogger.Append(RIris + ",");
        eyeTrackingLogger.Append(LIris + ",");
        eyeTrackingLogger.Append(qx + ",");
        eyeTrackingLogger.Append(qy + ",");
        eyeTrackingLogger.Append(qz + ",");
        eyeTrackingLogger.Append(qw + ",");
        eyeTrackingLogger.Append(rad + ",");
        eyeTrackingLogger.Append(show + ",");
        eyeTrackingLogger.Append(press.ToString() + ",");
        eyeTrackingLogger.AppendLine();
    }

    IEnumerator logTrial()
    {
        StreamWriter file;
        try
        {
            // create log file if it does not already exist. Otherwise open it for appending new trial
            if (!File.Exists(trialLogFile))
            {
                trialLogFile = "EyeLog_3dFollowObject_"+ namestr + System.String.Format("{0:_yyyy_MM_dd_hh_mm_ss}", System.DateTime.Now) + ".txt";
                file = new StreamWriter(trialLogFile);
                file.WriteLine("PerformaceTimer,TimeStep,TimeStamp,LeftGazeOriginX,LeftGazeOriginY,LeftGazeOriginZ,RightGazeOriginX,RightGazeOriginY,RightGazeOriginZ,LeftGazeDirrectionX,LeftGazeDirrectionY,LeftGazeDirrectionZ,RightGazeDirrectionX,RightGazeDirrectionY,RightGazeDirrectionZ,RHorizontalAngle,LHorizontalAngle,RVerticalAngle,LVerticalAngle,RightPupilSize,LeftPupilSize,InterPupilaryDistance,RightIrisSize,LeftIrisSize," + readerHeaders);
                // file.WriteLine("TimeStep,TimeStamp,LeftGazeOriginX,LeftGazeOriginY,LeftGazeOriginZ,RightGazeOriginX,RightGazeOriginY,RightGazeOriginZ,LeftGazeDirrectionX,LeftGazeDirrectionY,LeftGazeDirrectionZ,RightGazeDirrectionX,RightGazeDirrectionY,RightGazeDirrectionZ,RHorizontalAngle,LHorizontalAngle,RVerticalAngle,LVerticalAngle,QuaternionX,QuaternionY,QuaternionZ,QuaternionW,Radius,ButtonPress,Active");
            }
            else
            {
                file = File.AppendText(trialLogFile);
                logFileCounter++;
            }
            file.WriteLine(eyeTrackingLogger.ToString());
            file.Close();
            eyeTrackingLogger = new StringBuilder();
            UnityEngine.Debug.Log("FinishedLogging");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.Log("Error in accessing file: " + e);
        }
        yield return new WaitForSeconds(.1f);
    }

    private void calculateFR(long newStamp)
    {
        long diff = newStamp - oldStamp;
        oldStamp = newStamp;
        if ((diff / 1000000) > 500)
        {
            UnityEngine.Debug.LogError("Extended delay between reads (ms) " + diff / 1000000);
        }
    }

    public void log(string str)
    {
        namestr = str;
        StartCoroutine(logTrial());
        UnityEngine.Debug.LogError("End of File");
    }

    //App specific information
    public string[] connectorAccess()
    {
        string[] returnArray = { "NaN", "NaN", "NaN", "NaN", "NaN", "NaN", "NaN", "NaN", "NaN", "NaN"};
        if(!errorFound)
        {
            returnArray[0] = RHorA;
            returnArray[1] = LHorA;
            returnArray[2] = RVerA;
            returnArray[3] = LVerA;
            returnArray[4] = vergence;
            returnArray[5] = RIrisD;
            returnArray[6] = LIrisD;
            returnArray[7] = RPupilD;
            returnArray[8] = LPupilD;
            returnArray[9] = InterPupilD;
        }
        return returnArray;
    }

    public string[] connectorFieldNames()
    {
        string[] returnArray = { "RX Angle", "LX Angle", "RY Angle", "LY Angle", "Vergence", "RIris", "LIris", "RPupil", "LPupil", "IPD" };
        return returnArray;
    }

    //              0         1        2    
    // dataOrder( distance, range, dynamic);
    public void storePath(string[] ReaderValues, string ReaderNames)
    {
        //QuaternionX,QuaternionY,QuaternionZ,QuaternionW,Radius,Active, Press
        readerHeaders = ReaderNames;
        Quaternionx = ReaderValues[0];
        Quaterniony = ReaderValues[1];
        Quaternionz= ReaderValues[2];
        Quaternionw = ReaderValues[3];
        showing = ReaderValues[5];
        r = ReaderValues[4];
    }

    public string[] outputValues() {
        string[] returnArray = { valueOnPress, "empty" };
        return returnArray;
    }

    private long HighResolutionDateTime()
    {
        DateTime currentDate = DateTime.Now;
        long filetime = currentDate.Ticks + 179995000000;
        return filetime;
    }
}