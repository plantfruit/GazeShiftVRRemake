using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using TMPro;
//using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Varjo.XR;

public class ResetPosition : MonoBehaviour
{
    public Transform cameraM;
    public Transform anchor;
    public Transform reseter;

    public DynamicManager manager;
    private SoccerPreferences settings; // Contains the preferences from the menu screen

    public TextMeshProUGUI label;
    //public Transform worldFixedAnchor;
    //public Transform HeadFixedAnchor;
    //public Transform child;

    private bool reseted = false;
    private float rGain = 1f;
    private float lGain = 1f;
    private float prevAngle = 0f;
    private float currentAngle = 0f;
    private Quaternion savedPos;
    private Vector3 savedGaze;
    private bool firstSetGaze = false;

    // From Luis' new DVA script
    public Transform rotator;
    private Quaternion currentRotation;
    private Quaternion prevRotation;
    
    // Start is called before the first frame update
    void Start()
    {
        settings = GameObject.FindWithTag("Preferences").GetComponent<SoccerPreferences>(); // Settings is a DontDestroyOnLoad object, which only appears after accessing from menu
        firstSetGaze = false;
        VarjoRendering.SetFaceLocked(false);
        //SetWorld();
    }

    // Update is called once per frame
    void Update()
    {
        //currentAngle = cameraM.localEulerAngles.y; // Set current 
        //Debug.Log("camera" + cameraM.rotation.ToString());
        
        // After the arrow flashes, adjust the screen to match left and right gain. E.g. if the left side is suppressed, the world will "counter rotate" as the user performs a head impulse to their left.
        if (manager.GetState() >= 3) //deal with gain when arrow is up
        {
            reseted = false;

            // Grab the gain settings from instruction file
            rGain = settings.GetRightGain();
            lGain = settings.GetLeftGain();
    
        } 

        if (Input.GetKeyDown("r"))
        {
            SetWorld();
            label.text = "setWorld:" + "savedGaze " + savedGaze + " camera rot " + cameraM.rotation.eulerAngles.y + " world rot " + rotator.rotation.eulerAngles.y + " camera rot " + cameraM.rotation.eulerAngles.y + " anchor rot " + anchor.rotation.eulerAngles.y + " reseter rot " + reseter.rotation.eulerAngles.y;
            Debug.Log("setWorld:"+"savedGaze " + savedGaze + " camera rotation " + cameraM.rotation.eulerAngles.y + " world rotation " + rotator.rotation.eulerAngles.y + " camera rotation " + cameraM.rotation.eulerAngles.y + " anchor rotation " + anchor.rotation.eulerAngles.y + " reseter rotation " + reseter.rotation.eulerAngles.y); 
        }

        if (Input.GetKeyDown("t"))
        {
            label.text = "savedGaze " + savedGaze + " camera rotation " + cameraM.rotation.eulerAngles.y + " world rotation " + rotator.rotation.eulerAngles.y + " camera rotation " + cameraM.rotation.eulerAngles.y + " anchor rotation " + anchor.rotation.eulerAngles.y + " reseter rotation " + reseter.rotation.eulerAngles.y;
            Debug.Log("savedGaze " + savedGaze + " camera rotation " + cameraM.rotation.eulerAngles.y + " world rotation " + rotator.rotation.eulerAngles.y + " camera rotation " + cameraM.rotation.eulerAngles.y + " anchor rotation " + anchor.rotation.eulerAngles.y + " reseter rotation " + reseter.rotation.eulerAngles.y);
        }
        
        // If it's the first state, we should set a baseline, i.e. the initial orientation of the user's head in relation to the world.
        // This part stores the headset rotation at the beginning
        /* if (manager.GetState() == 0)
        {
            if (!reseted)
            {                                           // gain handled anytime before arrow, but only runs once. may catch on state 0/1
                // if(rGain != 0 || lGain != 0){
                //     child.parent = worldFixedAnchor;
                //     child.localPosition = new Vector3(0,0,settings.GetPlayerDistance());
                //     VarjoRendering.SetFaceLocked(false);
                // }

                reseted = true;
                if (!firstSetGaze) // Save initial position if it hasn't been saved yet
                {
                    SetWorld();
                    firstSetGaze = true;
                }
                else
                {
                    //ResetWorld();
                }
            }
        } */
        //prevAngle = currentAngle;
    }

    void FixedUpdate()
    {
        currentRotation = cameraM.rotation;
        currentAngle = cameraM.localEulerAngles.y;
        if (manager.GetState() >= 3)// && manager.GetState() < 5) //deal with gain when arrow is up or suppress for calibration
        {
            float diff = currentAngle - prevAngle;
            if (diff > 0) //turning right
            {
                supressRotation(rGain);
            }
            else //turning left
            {
                supressRotation(lGain);
            }
        }
        prevAngle = currentAngle;
        prevRotation = currentRotation;
    }



    /* Essentially, SetWorld() should be called at the beginning, to store the "baseline." Then, after each trial, presumably the world has been rotated because of the suppression effects. Therefore, ResetWorld() is called to shift the 
     * background, the world, etc. back to their initial positions/orientations in the baseline. */

    // Restore the world to the baseline rotation
    public void ResetWorld()
    {
        reseter.localEulerAngles = savedGaze;       // set current rotation to gaze
        rotator.rotation = Quaternion.identity;
        reseter.position += (anchor.position - cameraM.position);

        Debug.Log("RESETWORLD savedGaze " + savedGaze + " camera rotation " + cameraM.rotation.eulerAngles.y + " world rotation " + rotator.rotation.eulerAngles.y + " camera rotation " + cameraM.rotation.eulerAngles.y + " anchor rotation " + anchor.rotation.eulerAngles.y + " reseter rotation " + reseter.rotation.eulerAngles.y);
    }

    // Store baseline rotation
    public void SetWorld()
    {
        //savedPos = anchor.rotation * Quaternion.Inverse(cameraM.rotation) * reseter.rotation;
        savedGaze = reseter.rotation.eulerAngles + (anchor.rotation.eulerAngles - new Vector3(0, cameraM.rotation.eulerAngles.y, 0));
        reseter.localEulerAngles = savedGaze;
        rotator.rotation = Quaternion.identity;
        reseter.position += (anchor.position - cameraM.position);

        Debug.Log("reseter rot"+reseter.rotation.eulerAngles +"savedGaze " + savedGaze + " camera rotation " + cameraM.rotation.eulerAngles + " world rotation " + rotator.rotation.eulerAngles + " world pos " + rotator.position + " camera pos " + cameraM.position + " anchor pos " + anchor.position + " reseter pos " + reseter.position);
    }

    private void supressRotation(float gain)
    {
        rotator.rotation = Quaternion.SlerpUnclamped((currentRotation * Quaternion.Inverse(prevRotation)), Quaternion.identity, gain) * rotator.rotation;
    }

    public static void ResetRaycastSquare(GameObject raycastSquare, GameObject worldObject, Vector3 initialSquarePos, Vector3 initialSquareScale)
    {
        // The centering square was possibly "de-parented." This restores the square's coordinates to the center of the background again.
        if (raycastSquare.transform.parent == null)
        {
            raycastSquare.transform.SetParent(worldObject.transform);
            raycastSquare.transform.localPosition = initialSquarePos; 
            raycastSquare.transform.localRotation = Quaternion.identity; 
            raycastSquare.transform.localScale = initialSquareScale;
        }
    }

    void OnDestroy()
    {
        VarjoRendering.SetFaceLocked(false);
    }

}