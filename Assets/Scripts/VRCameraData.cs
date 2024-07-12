using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class VRCameraData : MonoBehaviour
{
    public Camera vrCamera;

    // Variables for calculating if the head is centered
    private Vector3 eulerForm; // Temporary variable to store the euler angle form of the VR Camera's rotation (so I don't have to type it all out)    
    private Vector3 centerDirection; // Angle from the origin to the soccer ball, used in calculating if the head is centered
    private Ray centerRay; // Ray for the above var
    private Quaternion centerDirectionQuat; // Quaternion of the centerDirection variable
    private Quaternion diffAngle; // Difference in angle between the rotation of the vrCamera and the normal angle to the center
    private DynamicManager manager; 

    // Variables for calculating the angular velocity
    private Vector3 angularVelocity; // Rotational speed of head, in euler angles    
    private float angularSpeed; // Just the y component of previous variable
    private float maxSpeed = 0f; // Records the largest speed, reset by manager each trial
    private Quaternion oldAngle; // Quaternion of head rotation in previous frame
    private Quaternion newAngle; // Quaternion of head rotation in current frame
    private Stopwatch stopWatch = new Stopwatch(); // Records time in between each frame
    private float prevTime; // Elapsed milliseconds in previous frame
    private float currTime; // Elapsed milliseconds in current frame
    private float elapsedSeconds; // Total elapsed seconds (all frames, for the entire run of the program)

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<DynamicManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Determines angle between where the head is pointing and the line to the center of the canvas (as represented by the Main Camera's forward direction). It doesn't matter whether leftCam or rightCam is used because both have the same transform.forward as the parent VR Camera
        newAngle = vrCamera.transform.rotation;

        //angularVelocity = new Vector3(0f, 0f, 0f); 

        prevTime = currTime;

        // If this is the first frame, speed is 0
        if (oldAngle == null)
        {
            angularSpeed = 0f;
            stopWatch.Start();
        } // But if it's not the first frame, speed involves a change in angle. 

        // Angular velocity calculations
        // Credit to Dr. Walker for the equations and user BrightBit in the Unity forums
        else
        {
            // Record the time elapsed between 2 frames
            stopWatch.Stop();
            //float elapsedSeconds = ((float)stopWatch.ElapsedMilliseconds) / 1000f; // Credit to "system" on Unity Discussions
            currTime = ((float)stopWatch.ElapsedMilliseconds) / 1000f; // Update it with the current point in time
            float elapsedSeconds = currTime - prevTime;
            if (elapsedSeconds == 0f) { elapsedSeconds = Time.deltaTime; }
            //stopWatch.Reset();
            stopWatch.Start();

            Quaternion deltaRotation = Quaternion.Inverse(oldAngle) * newAngle;
            deltaRotation.ToAngleAxis(out var angle, out var axis);
            //angle *= Mathf.Deg2Rad;
            angularVelocity = (1.0f / elapsedSeconds) * angle * axis;
            //UnityEngine.Debug.Log("angular velocity " + angularVelocity);
            angularSpeed = angularVelocity.y; // Assume that we're only looking at the "horizontal" rotations (the yaw)
                                              //Debug.Log("speed " + angularSpeed);
        }

        // Filter out the lower speeds and check if it's the maximum speed recorded so far
        if (Mathf.Abs(angularSpeed) > 100f && Mathf.Abs(angularSpeed) > maxSpeed && manager.GetState() == 3)
        {
            maxSpeed = angularSpeed; 
        }
        
        oldAngle = newAngle; // Helps reset the angular speed calculations
    }

    // Called by Manager in Update() loop (or more specifically, the WaitForHeadFixated() method)
    // Determines if the head is centered/pointed at the center of the screen
    public bool IsHeadFixated(float threshold, Vector3 startingPoint, GameObject endingPoint)
    { 
        /* The funny part about this isHeadFixated() method is that it doesn't use any of these params.
             * What it does is that it raycasts a line straight ahead of where the user is looking.
             * At the same time, we have an invisible square that is positioned in the center of the picture.
             * So if the raycast hits that square, then that means the user is pointing their gaze at the center of the picture. */
        bool isFixated = false;

        // Shoot the raycast ahead of wherever the user is looking (hence, the transform.forward)
        if (Physics.Raycast(vrCamera.transform.position, vrCamera.transform.forward, 25f)) { // The 25 distance is an arbitrary value (magic number)
            isFixated = true;
        }
        return isFixated; // Maybe change this method later to account for the user looking in a direction besides in front...
    }

    public Vector3 GetAngularVelocity()
    {
        return angularVelocity;
    }

    public float GetAngularSpeed()
    {
        //Debug.Log("the angular speed is " + angularSpeed);
        return angularSpeed;
    }

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }
    
    public void SetMaxSpeed(float input)
    {
        maxSpeed = input;
    }

    public Quaternion GetRotation()
    {
        return newAngle;
    }

    public Quaternion GetLocalRotation()
    {
        return vrCamera.transform.localRotation;
    }

    // True = Left. False = Right.
    // Called by Manager in the state machine in Update()
    public bool IsHeadTurnedLeft()
    {
        UnityEngine.Debug.Log("The rotation of camera is " + vrCamera.transform.rotation.eulerAngles.y);
        return (vrCamera.transform.rotation.eulerAngles.y > 180f);
    }
}
