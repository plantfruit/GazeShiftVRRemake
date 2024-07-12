using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerOptotype : MonoBehaviour
{
    public Sprite diagonalSprite; // When the rotation of the optotype is at an offset
    public Sprite rightAngleSprite; // When the rotation of the optotype is at 90, 180, etc.

    private GameObject optotypeObject;
    private SpriteRenderer optotype;
    private Quaternion initialRotation; // Where the open side of the "C" points up
    private ScaleChanger scaler;
    private float flashOptotypeScale = 0.25f;

    // Start is called before the first frame update
    void Start()
    {
        scaler = this.gameObject.GetComponent<ScaleChanger>();
    }

    void Awake()
    {
        optotypeObject = this.gameObject; // Grab object the script is attached to
        initialRotation = optotypeObject.transform.rotation; // Store the initial rotatino of the optotype (for resetting purposes later)
        optotype = optotypeObject.GetComponent<SpriteRenderer>(); // Also grab the sprite renderer the boject is attached to
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Change the rotation of the green input optotype to match the direction that the user rotates their joystick. Also find the integer representation of this input joystick direction
    // Taken from the DynamicAcuityController class in the original project
    public int InputRotateOptotype(float controllerThreshold)
    {
        int directionInput = -1; // Tells the manager what the integer representation of the joystick direction is
        scaler.ResetScale();
        directionInput = axisInput(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), controllerThreshold);
        return directionInput;
    }

    /* Take in the data from the instructions/preferences and rotate the optotype in the direction that it should be in. This is separate from the other methods, which deal with the "input optotype," which is a visual representation
     * of the joystick direction. This method deals with the optotype that is flashed in the center of the soccer ball. */
    public void CreateOptotype(int settingsDirection, float scaleChange)
    {
        ShowOptotype(); // Reveal the optotype picture
        //Debug.Log("Creating optotype with direction of " + settingsDirection);
                
        int direction = settingsDirection; // Grab direction from the instructino file (integer representation)
        optotypeObject.transform.rotation = initialRotation; // Reset orientation of optotype, preparing it for the rotation in next step

        Debug.Log("direction " + direction + " angles " + optotypeObject.transform.rotation.eulerAngles);
        // Load different sprites depending on whether the optotype is in an orthogonal direction (0, 90, 180, etc) or if it's in a diagonal direction (in between)
        if ((direction % 2 == 0) || (direction == 0)) 
        {
            optotype.sprite = Resources.Load<Sprite>("11");//rightAngleSprite;
            Debug.Log("Hit"); 
            optotypeObject.transform.Rotate(new Vector3(0f, 0f, 90f)); // Rotate it back twice because the optotype is pointing to the right in default sprite but all of our sprites assume default is pointing up
        } 
        else {
            optotype.sprite = optotype.sprite = Resources.Load<Sprite>("11r");//diagonalSprite;
            optotypeObject.transform.Rotate(new Vector3(0f, 0f, 45f)); 
        } // Rotate it back once because for the diagonal directions, starting rotation is up and to the right

        scaler.SetScale(scaleChange * flashOptotypeScale); // Note that the optotypes we use in flash are much more high res and need to be scaled down in order to remain smaller than the soccer ball
        //Debug.Log("opto scale " + optotypeObject.transform.localScale);

        // Rotate optotype based on the integer representation (we can just multiply it by -45, which converts the integer representation to degrees)
        optotypeObject.transform.Rotate(new Vector3(0f, 0f, -45f * direction));

        //Debug.Log("Optotype sprite name " + optotype.sprite);

        // Show and then hide the optotype for a specified period of time
        StartCoroutine(FlashOptotype());
    }

    // Flashes the optotype for 100 milliseconds
    private IEnumerator FlashOptotype()
    {
        yield return new WaitForSeconds(0.1f);
        HideOptotype();
        resetSprite();
        scaler.ResetScale();
        optotypeObject.transform.rotation = initialRotation;
    }

    // Hide and reveal the optotype. These methods seem kind of redundant, but they're called by manager or other classes. 
    public void HideOptotype()
    {
        optotype.enabled = false;
    }

    // Opposite of the previous method; it turns the optotype back on
    public void ShowOptotype()
    {
        optotype.enabled = true;
    }

    /* Maps joystick directions to an integer value
     * Clumsy way of doing it: a massive if-else block that checks each possible combination of horizontal-vertical axes (a joystick direction), and returns an integer corresponding to that.
     * Also, we set the green optotype (the input optotype) in the center of the soccer ball to the corresponding joystick direction. */
    private int axisInput(float x, float y, float stickThreshold)
    {
        if (validInput(x, y, stickThreshold))
        {
            if (y > stickThreshold && Mathf.Abs(x) < stickThreshold)
            {
                optotype.sprite = Resources.Load<Sprite>("DecisionMaking/Up");
                return 0;
            }
            else if (y > stickThreshold && x > stickThreshold)
            {
                optotype.sprite = Resources.Load<Sprite>("DecisionMaking/UpRight");
                return 1;
            }
            else if (Mathf.Abs(y) < stickThreshold && x > stickThreshold)
            {
                optotype.sprite = Resources.Load<Sprite>("DecisionMaking/Right");
                return 2;
            }
            else if (y < stickThreshold * -1 && x > stickThreshold)
            {
                optotype.sprite = Resources.Load<Sprite>("DecisionMaking/DownRight");
                return 3;
            }
            else if (y < stickThreshold * -1 && Mathf.Abs(x) < stickThreshold)
            {
                optotype.sprite = Resources.Load<Sprite>("DecisionMaking/Down");
                return 4;
            }
            else if (y < stickThreshold * -1 && x < stickThreshold * -1)
            {
                optotype.sprite = Resources.Load<Sprite>("DecisionMaking/DownLeft");
                return 5;
            }
            else if (Mathf.Abs(y) < stickThreshold && x < stickThreshold * -1)
            {
                optotype.sprite = Resources.Load<Sprite>("DecisionMaking/Left");
                return 6;
            }
            else if (y > stickThreshold && x < stickThreshold * -1)
            {
                optotype.sprite = Resources.Load<Sprite>("DecisionMaking/UpLeft");
                return 7;
            }
            else
            {
                return -1;
            }

        }
        else
        {
            return -1; // Return -1 if a direction isn't found (unlikely)
        }
    }

    // Clear the sprite of the optotype
    public void resetSprite()
    {
        optotype.sprite = Resources.Load<Sprite>("DecisionMaking/Empty");
    }

    // Check that the joystick moved a significant amount and that it isn't still/neutral
    private bool validInput(float x, float y, float stickThreshold)
    {
        return (Mathf.Abs(x) > stickThreshold || Mathf.Abs(y) > stickThreshold);
    }
}

