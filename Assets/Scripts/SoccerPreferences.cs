using System.IO;
using TMPro;
using UnityEngine;

// Adapted from the Reader class by Luis Mesias Flores
public class SoccerPreferences : MonoBehaviour
{
    // This is mostly a placeholder class until I implement the settings screen in main menu.

    //reader variables
    private string dataPointsPath;
    public string docName;
    private StreamReader reader;
    private string line;
    private string[] lines;
    int i = 0;
    private Logger trackingScript;

    public GameObject logger;
    public GameObject percent;
    public GameObject endImage;
    public VRCameraData vrCamData;
    private TextMeshProUGUI percentTXT;

    //index: used to tell what each number in config files is related to
    private int arrowDirectionIndex = 0;
    private int optotypeDirectionIndex = 1;
    private int speedIndex = 2;
    private int leftGIndex = 3;
    private int rightGIndex = 4;
    private int sizeIndex = 5;
    private int correctPercentageIncreaseIndex = 6;
    private int correctPercentageDecreaseIndex = 7;
    private int lookBackUpperBoundIndex = 8;
    private int lookBackLowerBoundIndex = 9;
    private int lookBackFramesIndex = 10;
    private int optotypeChangeWindowIndex = 11;
    private int playerDistanceIndex = 12;
    private string[] array;

    private int arrowDirection = 0; // Integer that represents the direction that the arrow in the middle of the soccer ball will be in
                                    // 0 - Left
                                    // 1 - Right
                                    // Note that this is different from the rotation directions for the gloves, soccer ball, optotype, and integer input. 
    private int optotypeDirection; // Integer that represents the direction that the optotype will be rotated in
                                   // 0 - Up            4 - Down
                                   // 1 - Up Right      5 - Down Left
                                   // 2 - Right         6 - Left   
                                   // 3 - Down Right    7 - Up Left
                                   // Note that a similar numbering scheme is used for the glove vectors in GlovesController.cs' animation method. 
    private float speedThreshold = 200f; // How fast the head must rotate in order to count as 1 rotation
    private float leftGain = 1f;
    private float rightGain = 1f;
    private float optotypeSize = 1f; // Scaling for the optotype
    private float playerDistance = 10f; // Not sure what this is?     
    private float correctPercentageIncrease = 40f;
    private float correctPercentageDecrease = 60f;
    private float lookBackUpperBound; // Should these be floats?
    private float lookBackLowerBound;
    private float lookBackFrames;
    private Vector3 angularVelocity;
    private Quaternion angularQuat;

    // Have not made the getters and setters for the below variables yet
    private int optotypeChangeWindow = 10; 
    private bool reachedEnd = false; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake() // Using Awake() because the previous project claimed that Start() could produce synchronicity issues
    {
        //DontDestroyOnLoad(this.gameObject); why need dont destroy ?
        percentTXT = percent.GetComponent<TextMeshProUGUI>();
        //docName = Config.fileName;
        docName = "soccer.txt"; //temp to be replaced with config file onse implemented into eytracking apps package
        dataPointsPath = Path.Combine("C:/data/" + docName);
        trackingScript = logger.GetComponent<Logger>();

        reader = new StreamReader(dataPointsPath);
        string readContents = reader.ReadToEnd();
        lines = readContents.Split('\n');
        if (lines.Length < 2)
        {
            lines = readContents.Split('\r');
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        angularVelocity = vrCamData.GetAngularVelocity();        
        angularQuat = vrCamData.GetRotation();
        string[] storeValues = { arrowDirection.ToString(), speedThreshold.ToString(), leftGain.ToString(), rightGain.ToString(), optotypeSize.ToString(), correctPercentageDecrease.ToString(), correctPercentageIncrease.ToString(), optotypeChangeWindow.ToString(), playerDistance.ToString(), angularVelocity.x.ToString(), angularVelocity.y.ToString(), angularVelocity.z.ToString(), angularQuat.w.ToString(), angularQuat.x.ToString(), angularQuat.y.ToString(), angularQuat.z.ToString() };
        string names = "ArrowDirection,SpeedThreshold,LeftGain,RightGain,OptotypeSize,CorrectPercentageDecrease,CorrectPercentageIncrease,OptotypeChangeWindow,PlayerDistance,AngularVelocityX,AngularVelocityY,AngularVelocityZ,QuaternionW,QuaternionX,QuaternionY,QuaternionZ";
        trackingScript.storePath(storeValues, names); //sends data to the logger
    }

    // Methods called in the manager.

    // Gets all of the directions and specifications needed for each trial
    public void NextInstruction()
    {
        // Note that is called in state 0 of the manager.
        // Also called right away as the scene starts. 
        UnityEngine.Debug.Log("next");
        if (i < lines.Length)
        {
            line = lines[i];
            percentTXT.text = (100 * i / lines.Length).ToString("0.#");
            Debug.Log(line.ToString());
            array = line.Split(',');
            if (array[0].ToString() == "End")
            {
                //quit current app and save data
                endImage.SetActive(true);
                reachedEnd = true;
                //trackingScript.log(array[1].ToString());
            }
            else
            {
                if (float.Parse(array[optotypeDirectionIndex]) <8)
                {
                    optotypeDirection = int.Parse(array[optotypeDirectionIndex]);
                }
                else
                {
                    optotypeDirection = Random.Range(0, 8);// rnadomely assign a number from 0 to 7 
                }
                arrowDirection = int.Parse(array[arrowDirectionIndex]);
                speedThreshold = float.Parse(array[speedIndex]);
                leftGain = float.Parse(array[leftGIndex]);
                rightGain = float.Parse(array[rightGIndex]);
                optotypeSize = float.Parse(array[sizeIndex]);
                correctPercentageDecrease = float.Parse(array[correctPercentageDecreaseIndex]);
                correctPercentageIncrease = float.Parse(array[correctPercentageIncreaseIndex]);
                lookBackLowerBound = float.Parse(array[lookBackLowerBoundIndex]);
                lookBackUpperBound = float.Parse(array[lookBackUpperBoundIndex]);
                lookBackFrames = float.Parse(array[lookBackFramesIndex]);
                optotypeChangeWindow = int.Parse(array[optotypeChangeWindowIndex]);
                playerDistance = float.Parse(array[playerDistanceIndex]);                          
            }
        }
        i = i + 1;
    }    

    // Getter method
    public bool HasReachedEnd()
    {
        return reachedEnd;
    }

    // Bunch of miscallaneous getters and setters
    // Get is usually called by the manager/controller class
    // Set is usually called by the menu config screen, which sets the values here whenever the fields/dropdowns are changed

    // Direction that the user must rotate their head
    // 0 - Left and 1 - Right, maybe include up or down? Is that necessary? I saw it in the code for the original project, but in the demos I only saw Left or Right
    // Revision note - Up and down is not necessary, read the paper.
    public int GetArrowDirection()
    {
        return arrowDirection;
    }

    // Bunch of miscallaneous getters and setters

    // Direction that the optotype should be rotated
    public int GetOptotypeDirection()
    {
        return optotypeDirection;
    }

    public float GetSpeedThreshold()
    {
        return speedThreshold;
    }

    public float GetLeftGain()
    {
        return leftGain;
    }

    public float GetRightGain()
    {
        return rightGain;
    }

    public float GetOptotypeSize()
    {
        return optotypeSize;
    }
    
    public float GetPlayerDistance()
    {
        return playerDistance;
    }

    public float GetCorrectPercentageIncrease()
    {
        return correctPercentageIncrease;
    }

    public float GetCorrectPercentageDecrease()
    {
        return correctPercentageDecrease;
    }

    public float GetLookBackUpperBound()
    {
        return lookBackUpperBound;
    }

    public float GetLookBackLowerBound()
    {
        return lookBackLowerBound;
    }

    public float GetLookBackFrames()
    {
        return lookBackFrames;
    }

    // Setter methods, generally called by the Menu buttons
    public void SetArrowDirection(int arrowDirection)
    {
        this.arrowDirection = arrowDirection;
    }

    public void SetOptotypeDirection(int input)
    {
        optotypeDirection = input;
    }

    public void SpeedSpeedThreshold(int input)
    {
        speedThreshold = input;
    }

    public void SetLeftGain(float input)
    {
        leftGain = input;
    }

    public void SetRightGain(float input)
    {
        rightGain = input;
    }

    public void SetOptotypeSize(float input)
    {
        optotypeSize = input;
    }

    public void SetPlayerDistance(float input)
    {
        playerDistance = input;
    }

    public void SetCorrectPercentageIncrease(float input)
    {
        correctPercentageIncrease = input;
    }

    public void SetCorrectPercentageDecrease(float input)
    {
        correctPercentageDecrease = input;
    }

    public void SetLookBackUpperBound(float input)
    {
        lookBackUpperBound = input;
    }

    public void SetLookBackLowerBound(float input)
    {
        lookBackLowerBound = input;
    }

    public void SetLookBackFrames(float input)
    {
        lookBackFrames = input;
    }

    private void OnDestroy()
    {
        reader.Close();
    }

}
