using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using TMPro;
//using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Varjo.XR;

public class ResetPositionNoVR : MonoBehaviour
{
    public Transform cameraM;
    public Transform anchor;
    public Transform reseter;

    private SoccerPreferences settings; // Contains the preferences from the menu screen
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
        firstSetGaze = false;
        VarjoRendering.SetFaceLocked(false);
        //SetWorld();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("r"))
        {
            SetWorld();          
            Debug.Log("setWorld:" + "savedGaze " + savedGaze + " camera rotation " + cameraM.rotation.eulerAngles.y + " world rotation " + rotator.rotation.eulerAngles.y + " camera rotation " + cameraM.rotation.eulerAngles.y + " anchor rotation " + anchor.rotation.eulerAngles.y + " reseter rotation " + reseter.rotation.eulerAngles.y);
        }

        if (Input.GetKeyDown("t"))
        {
            Debug.Log("savedGaze " + savedGaze + " camera rotation " + cameraM.rotation.eulerAngles.y + " world rotation " + rotator.rotation.eulerAngles.y + " camera rotation " + cameraM.rotation.eulerAngles.y + " anchor rotation " + anchor.rotation.eulerAngles.y + " reseter rotation " + reseter.rotation.eulerAngles.y);
        }
    }

    void FixedUpdate()
    {

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

        Debug.Log("reseter rot" + reseter.rotation.eulerAngles + "savedGaze " + savedGaze + " camera rotation " + cameraM.rotation.eulerAngles + " world rotation " + rotator.rotation.eulerAngles + " world pos " + rotator.position + " camera pos " + cameraM.position + " anchor pos " + anchor.position + " reseter pos " + reseter.position);
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