using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// This class is pretty much deprecated. I think ResetToZero() is called once.
public class SoccerWorldRotation : MonoBehaviour
{
    public GameObject vrCamera;
    
    private Transform worldTransform; // The transform component for this parent game object
    private VRCameraData vrCamData;                                       
    private Vector3 vrCamAngles; // Euler angles for the VR Camera
    private bool leftSuppressionOn = false;
    private bool rightSuppressionOn = false;
    private bool suppressionStage = false; // Manager will enable this when it's during the head turning stage. This is a kind of general boolean.
    private float leftGain;
    private float rightGain;
    private float suppressionThreshold = 90f; // How fast the head needs to rotate before the world suppression will kick in

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        worldTransform = this.gameObject.transform;
        vrCamData = vrCamera.GetComponent<VRCameraData>();
        ResetToZero();        
    }

    // Update is called once per frame
    void Update()
    {
        // Old suppression code -- fully deprecated

        //vrCamAngles = vrCamera.transform.rotation.eulerAngles;
        ///* If we're running the VOR suppression test, then set the y rotation of the entire scene to (almost?) the same as what the user's head rotation is...
        // * but make sure to multiply it by the VOR suppression value (that was defined in Settings). */        
        //if (Mathf.Abs(vrCamData.GetAngularSpeed()) > suppressionThreshold) {
        //    if (leftSuppressionOn && suppressionStage) // Left turn
        //    {
        //        /* There's a special case when the head rotation is to the left. Whenever that happens, the VR Camara will register its y angle as a fairly large number--close to 360, because it's to the left. 
        //         * In that case, you can't simply multiply the euler angles by the VOR Suppression multiplier and set that as the new rotation of the World game object. You'll need to do some more calculations.
        //         * You need to find the "remaining angle," i.e. the angle between the angle of the VR Camera and the reference angle of 0/360 degrees. Then... */             
        //        float remainderAngle = 360f - vrCamAngles.y;
        //        // ...multiply that remaining angle by the suppression multiplier in order to find how much the World should be offset from the 0 degree reference. 
        //        float subtractedAngle = remainderAngle * leftGain;
        //        // Finally, subtract that from 360; this will be the angle the World will have to rotate in order to "adjust" for that leftward direction. 
        //        vrCamAngles.y = 360f - subtractedAngle;
        //        // If you don't do this, the world will flip around and point backwards. 
                
        //        // Do left suppression if we're turning left
        //        if (vrCamData.GetAngularSpeed() < 0) worldTransform.eulerAngles = new Vector3(0f, vrCamAngles.y, 0f); 
        //    }
        //    if (rightSuppressionOn && suppressionStage) // Right turn
        //    { 
        //        vrCamAngles = vrCamera.transform.rotation.eulerAngles; // If both left and right gains are back on, we'll need to reset this
        //        vrCamAngles *= rightGain; // Of course, if there's no leftward rotation, just multiply as normal
        //        if (vrCamData.GetAngularSpeed() > 0) worldTransform.eulerAngles = new Vector3(0f, vrCamAngles.y, 0f);
        //        //else if (!leftSuppressionOn) worldTransform.rotation = Quaternion.identity; // If there's no left suppression you can use this line as an extra measure to stop left side suppression during right gain testing only
        //    }
        //}
    }

    // Points the world forward. Usually called by Manager. 
    public void ResetToZero()
    {
        worldTransform.rotation = Quaternion.identity;
    }

    public void ActivateSuppression(float leftGainInput, float rightGainInput)
    {
        this.leftGain = leftGainInput;
        this.rightGain = rightGainInput;
        suppressionStage = true;
        
        leftSuppressionOn = DetermineLeftSuppressionOn();
        rightSuppressionOn = DetermineRightSuppressionOn();               
    }

    public void SetSuppression(bool input)
    {
        suppressionStage = input;        
    }

    public bool DetermineLeftSuppressionOn()
    {
        return (leftGain != 0f);
    }

    public bool DetermineRightSuppressionOn()
    {
        return (rightGain != 0f);
    }
}
