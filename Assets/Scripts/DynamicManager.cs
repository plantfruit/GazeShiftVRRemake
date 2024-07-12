using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

// Controller script for the Dynamic Acuity game
public class DynamicManager : MonoBehaviour
{
    public GameObject background; // Soccer field
    public GameObject soccerBall; // Sprite for the soccer ball
    public GameObject arrowObject;
    public GameObject holdStillLabelObject; // Text that tells the user to hold their head still
    public GameObject feedbackLabelObject; // Text that tells the user if they got the right direction or not
    public GameObject savesLabelObject;
    public GameObject goalsLabelObject;
    public GameObject maxSpeedLabelObject; // Shows the angular speed of the head at the end of the trial display
    public GameObject finishedLabelObject; // Displays when the game is over
    public GameObject optotypeObject; // The optotype that flashes on soccer ball
    public GameObject inputOptotypeObject; // The green optotype that user rotates to view their input direction
    public GameObject glovesObject;
    public GameObject worldObject;
    public GameObject raycastSquare; // Square behind the background that is used for determining head fixation
    public VRCameraData vrCamData; // Class that does calculations based on VR Camera orientation
    public SoccerAudio soccerAudio; // Class that holds audioclips and plays them when called upon

    private SoccerWorldRotation world;
    private SoccerPreferences settings; // Contains the preferences from the menu screen
    private SoccerOptotype optotype; // Contains methods for controlling the optotype sprite
    private SoccerOptotype inputOptotype; // Similar as above, but for the green input optotype 
    private GlovesController gloves; // Animates the gloves and soccer ball
    private Quaternion savedGaze; // Save the orientation of the user's head at the start of each trial
    private Stopwatch stopwatch = new Stopwatch(); // Timer for resetting the center head stage if user doesn't hit the speed threshold in time
    private int state; // Represents the state that the game is currently in, e.g. the "Hold Still" or waiting for the user to turn their heard, etc. 
    private int directionInput; // Represents the direction that the user put in for their joystick
    private int goals = 0; // Number of times the user failed to get the right direction
    private int saves = 0; // Number of times the user did manage to get the right direction
    private int goalsInThisRange = 0; // Same as the above 2 variables, but the difference is that these 2 variables are for within each set of certain # of trials, rather than the total values
    private int savesInThisRange = 0; // These are within a RANGE, the other 2 are TOTAL
    private int trials;
    private int adjustmentTrials = 5; // SUPPOSED TO GET THIS VALUE FROM PREFERENCES, BUT I DON'T KNOW WHICH VARIABLE IT CORRESPONDS TO
                                      // Number of trials that passes before they adjust the size of the optotype
    private int tempControllerInput = -1;
    private float controllerThreshold = 0.5f; // How much the joystick should be moved in order to register it
    private float scaleChange = 1f; // Modifies the scale of the optotype. Changes based on percent correct.
    private float headStillThreshold = 5f; // Angle speed range that the head must be in to count as being stabilized
    private float headFixatedThreshold = 20f; // Angle range that the head must be within in order to count as being fixated
    private float verticalWeight = 0.25f; // The vertical head movement angle scale is much smaller for whatever reason. We have to adjust the headStillThreshold by a small decimal number in order to reduce it enough to detect up/down head movement.
    private float elapsedSeconds; // Each frame, records the amount of time that has passed for the stopwatch
    private float headImpulseTime = 0.25f; // Timer counts down for this long; if the user doesn't turn their head in the direction in this span of time, reset the state (Can adjust this later)
    private bool isHeadFixated; // Reads if the head is looking in the center
    private bool isHeadFixatedForRange; // Coroutine updates this value at the end, and then the Update() loop checks it to see if the head truly was fixated for the entire time span
    private bool isHeadStill; // If angular speed = 0, i.e. head is not moving
    private bool isHeadStillForRange; // Similar to isHeadFixatedForRange but we're checking the angular speed now
    private bool isCoroutineActive; // Are we running a timer focused thing right now?
    private bool isStateChangingRightNow = false; // Need this bool for when DelayedStateChange() is being used
    private bool cancelCoroutine = false; // Whenever an instant state change overrides a delayed state change, this is used to prevent a previous coroutine from activating
    private bool animationHappenedYet; 
    private float xDifference; // Track vertical movement of head between each frame, used in detecting ratcheting

    private Vector3 initialSquarePos = new Vector3(0f, 0f, 19f); // Initial coordinates of the raycast square, which we will use to reset it after each trial
    private Vector3 initialSquareScale = new Vector3(5f, 5f, 0.1f); // Initial scale of the raycast square, which are also used for resetting

    // Start is called before the first frame update
    void Start()
    {
        // Instantiate the variables by grabbing script components from their corresponding Game Objects
        optotype = optotypeObject.GetComponent<SoccerOptotype>();
        inputOptotype = inputOptotypeObject.GetComponent<SoccerOptotype>();
        gloves = glovesObject.GetComponent<GlovesController>();
        world = worldObject.GetComponent<SoccerWorldRotation>();
        settings = GameObject.FindWithTag("Preferences").GetComponent<SoccerPreferences>();

        trials = 1;

        StateChange(0); // Start with first state of 0
    }

    // Update is called once per frame
    /* There's 2 components of the state machine. 
     * In Update(), there are "conditional checks," which wait for the conditions that lead to the next state; if those conditions are fulfilled, the game goes to 
     * the next state.
     * StateChange() "sets up" the next state, conducting whatever actions are necessary at the beginning of it.
     * You can think of the Update() code as the excitation equations. Or, you can think of it as Update() is "continuous," and StateChange() is "discrete." 
     * StateChange() performs concrete actions at the start of each state; Update() waits for certain conditions to move on to the next state, or it accepts user input.
     * The following comments detail what's going on in each state, and what the Update() function is looking for.
     */
    void Update()
    {
        // State 0 - Dark screen
        // At the beginning, the user can press the button on their controller to initiate the game. Or, the experimenter can press the keyboard key corresponding to Button A, which will do the same thing.
        if (state == 0 && !isStateChangingRightNow)
        {
            // If it's the first trial, require a button press in order to start the game. This makes it easier to debug when it's just 1 person working on the game.
            if (!settings.HasReachedEnd())
            {
                if (trials == 1 && Input.GetButtonDown("Button A"))
                {
                    worldObject.GetComponent<ResetPosition>().SetWorld(); // Script stores the initial orientation that the user is looking at 
                    StateChange(1);
                }
            }
        }
        // State 1 - Check for head centered
        else if (state == 1)
        {
            /* isHeadFixatedForRange starts off as false. As long as the user's head is away from the center of the screen, the game will continually start this coroutine. 
             * During WaitForHeadFixated(), it runs over a short timespan, checking for centering at short intervals. If the user's head is centered on the screen for the entire timespan, the coroutine will set isHeadFixatedForRange as true. */
            if (!isCoroutineActive && !isHeadFixatedForRange) { StartCoroutine(WaitForHeadFixated()); }
            // When that coroutine ends, and the bool is true, we can move on to next state. 
            if (isHeadFixatedForRange)
            {
                StateChange(2);
            }
        }
        // State 2 - Check for head to hold still
        else if (state == 2)
        {
            /* Similar function as in the previous state. The coroutine runs over a specified timespan; if the head speed is low enough, the corresponding field (isHeadStillForRange) will be true at the end of it. Otherwise, the coroutine
             * will restart, and the field will remain false. */
            if (!isCoroutineActive && !isHeadStillForRange) { StartCoroutine(WaitForHeadStabilized()); }
            // At the end of the coroutine, isHeadStillForRange would ideally be true. 
            if (isHeadStillForRange)
            {
                savedGaze = vrCamData.GetRotation();
                StateChange(3);
            }
        }
        // State 3 - Soccer ball with arrow displayed on it. Check if user turned their head in the correct direction.
        else if (state == 3 && !isStateChangingRightNow)
        {
            // Start the timer when user moves their head away from the center.
            if (!vrCamData.IsHeadFixated(headFixatedThreshold, Vector3.zero, soccerBall))
            {
                // Begin timer. If it exceeds a specified amount of time, and user has still not conducted a proper head impulse, then the Update() loop will reset back to center head.
                stopwatch.Start();
            }

            // If timer has exceeded the count, then reset back to the center head state
            elapsedSeconds = ((float)stopwatch.ElapsedMilliseconds) / 1000f; // Credit to "system" on Unity Discussions;
            if (elapsedSeconds >= headImpulseTime)
            {
                stopwatch.Stop(); stopwatch.Reset();
                TurnOff(new List<GameObject>() { soccerBall, arrowObject, background }); // Completely blank the screen (dark screen)
                StartCoroutine(DelayedStateChange(2f, 1)); // Go back to state 1 (head centering)
            }


            /* xDifference = Mathf.Abs((savedGaze * Quaternion.Inverse(vrCamData.GetRotation())).eulerAngles.x);
            //Debug.Log("outside z " + xDifference);

            // Check if the user moves their head too far up or down
            //if (Mathf.Abs(vrCamData.GetAngularVelocity().z) > headFixatedThreshold) 
            if ((xDifference > 360 - 2 * verticalWeight * headFixatedThreshold && xDifference < 360 - verticalWeight * headFixatedThreshold) || (xDifference < 360 - 2 * verticalWeight * headFixatedThreshold && xDifference > verticalWeight * headFixatedThreshold))
            {
                // Reset back to state 1 (head centering) when the user has shifted their head noticeably in a vertical direction (z axis).
                WrongDirection();
            } 
            // Prevent "ratcheting" during suppression mode by using a more sensitive threshold for wrong direction.
            // Check only for wrong direction of head turn and reset the state if so.
            // 0 - Left arrow, Velocity.y > 0 - Right head turn.
            // 1 - Right arrow, Velocity.y < 0 - Left head turn.
            //if ((settings.GetArrowDirection() == 0 && vrCamData.GetAngularVelocity().y > 0) || (settings.GetArrowDirection() == 1 && vrCamData.GetAngularVelocity().y < 0))
            if (!isCoroutineActive && (settings.GetArrowDirection() == 0 && vrCamData.GetAngularSpeed() > 0.5f * headStillThreshold) || (settings.GetArrowDirection() == 1 && vrCamData.GetAngularSpeed() < -0.5f * headStillThreshold))
            {
                WrongDirection();
            } */

            //Debug.Log("head speed " + vrCamData.GetAngularSpeed());

            /* There's two cases of head rotation that the program needs to check for: the "high speed" case and the "low speed" case. 
             * Generally, we are expecting the former case, where the user makes a quick head impulse to the left or right direction. To see if the direction is correct, the program checks the sign of the speed y-axis and compares it to the 
             * arrow direction.
             * For the latter case, the user might also slowly turn their head in either direction; this is bad because in suppression mode, the movement could result in "ratcheting," where the soccer game background slowly gets pushed off 
             * the screen. Therefore, there should also be an if statement detecting low speed head movements. But in this situation, it doesn't handle the "correct direction," it only has a response for the wrong direction (opposite of the 
             * arrow direction), as that is mostly likely to cause ratcheting. Anyways, if ratcheting is detected, the game resets to state 1.
             * */
            // Wait for the head to rotate, then check if it was in the right direction
            if (Mathf.Abs(vrCamData.GetAngularSpeed()) > settings.GetSpeedThreshold())
            {
                // Detects if the user turned their head in the correct direction. Remember that arrowDirection of 0 = left and 1 = right.
                if ((settings.GetArrowDirection() == 0 && vrCamData.GetAngularSpeed() < 0) || (settings.GetArrowDirection() == 1 && vrCamData.GetAngularSpeed() > 0))
                {
                    arrowObject.SetActive(false); // Turn off arrow when they turn the head                    

                    Debug.Log("right direction with speed of " + vrCamData.GetAngularSpeed() + " and scale change of " + scaleChange);

                    // Flash the optotype
                    optotype.CreateOptotype(settings.GetOptotypeDirection(), scaleChange);

                    // We need to delay the state change by the amount of time that the optotype is on
                    StartCoroutine(DelayedStateChange(0.2f, 4));
                }
                // User didn't turn their head in the right direction, so revert back to stage 1
                else
                {
                    WrongDirection();
                }
            }
        }
        // State 4 - Check if user inputted the correct direction on the optotype
        else if (state == 4)
        {
            // InputRotateOptotype() returns an integer representation of the joystick direction. This if statement just stores whatever direction the joystick is in right now.
            if (optotype.InputRotateOptotype(controllerThreshold) > -1)
            {
                tempControllerInput = optotype.InputRotateOptotype(controllerThreshold); // Change the direction of the optotype based on whatever the user inputs for the joystick
            }
            // When user presses the joystick button, save their last input into another field and move on to the next state. 
            if (Input.GetButtonDown("Primary") && tempControllerInput > -1)
            { // Remember that we already get the int representation of the user input from the InputRotateOptotype() method                
                directionInput = tempControllerInput;
                tempControllerInput = -1;
                StateChange(5);
            }
        }
        // State 5 - Activate gloves animation when user returns their head to center
        else if (state == 5)
        {
            // Let user recenter their head so they can watch the gloves animation
            /* if (!animationHappenedYet && vrCamData.IsHeadFixated(headFixatedThreshold, Vector3.zero, soccerBall))
            {
                Debug.Log("ANIMATIONYET???");
                animationHappenedYet = true; // To prevent this block from looping (it will get reset back to false at the start of next trial's stage 5)
            } */

            if (!gloves.GetIsIncomplete())// && animationHappenedYet) // Wait till animation is over, then move on to the next state
            {
                StateChange(6);
            }
        }
        // State 6 - Nothing really needs to happen in Update() during this state, the below code is redundant since every trial blanks the screen now.
        else if (state == 6) // Automatically proceeds to next trial after delay.   
        {

        }
    }

    // Discrete actions that occur at the start of each state. Typically actions that "set up" the scene.
    public void StateChange(int nextState)
    {
        state = nextState; // Update the field
        Debug.Log("Reached state " + state);

        // State 0 - Blank the screen, end the game if it's the last line of instruction file, or otherwise pass on to the next state after a 2 second delay.
        if (state == 0)
        {
            // Load next instruction from the instruction file
            settings.NextInstruction();

            // Make the screen dark/blank
            // Turn off all the sprites and labels in the beginning to clear the screen
            TurnOff(new List<GameObject>() { background, soccerBall, arrowObject, optotypeObject, inputOptotypeObject, glovesObject, holdStillLabelObject, feedbackLabelObject, savesLabelObject, goalsLabelObject, maxSpeedLabelObject, finishedLabelObject });
            //worldObject.GetComponent<SoccerWorldRotation>().ResetToZero();

            {/* Originally, the soccer game had a feature where it automatically adjusted the size of the optotype images in response to user performance. So if the user was doing well, then the optotype scale would be shrunk. If
             * the user was doing poorly, then the optotype scale would be increased. However, we switched to an instruction file-based program, so the auto-scaling is no longer relevant. */

                // Note that the trials variable starts with 1 and not 0
                // adjustmentTrials represents the range we're working in. See below comment
                /* if (trials != 1 && (trials % adjustmentTrials == 1)) // e.g. if the threshold was 10 trials, then on the 11th trial (aka. after 10 trials have finished), we would adjust the optotype size
                {
                    int total = goalsInThisRange + savesInThisRange;
                    float percentCorrect = savesInThisRange / total; // Should I do this for all trials, or just for the past 10 or so?
                    percentCorrect *= 100; // To convert into percentage
                    Debug.Log("percent correct " + percentCorrect + " saves " + savesInThisRange + " goals " + goalsInThisRange);
                    if (percentCorrect > settings.GetCorrectPercentageDecrease()) // Decrease the size of the optotype if user is doing well
                    {
                        scaleChange -= 0.1f; // TENTATIVE VALUES, REPLACE LATER
                    }
                    else if (percentCorrect < settings.GetCorrectPercentageIncrease()) // Increase the size of the optotype is user is not doing well
                    {
                        scaleChange += 0.1f;
                    }

                    savesInThisRange = 0; // Reset variable values for the next range of trials
                    goalsInThisRange = 0;
                } */
                //Debug.Log("percentage decrease " + settings.GetCorrectPercentageDecrease() + " percent increase " + settings.GetCorrectPercentageIncrease());
            }
            // Check if we've reached the last line of the instruction file (aka. experiment is over)
            if (settings.HasReachedEnd())
            {   // Activate the text label that reads "Finished"
                TurnOff(new List<GameObject>() { raycastSquare });
                finishedLabelObject.SetActive(true);
            }
            // Change to state 1 after the passing of 2 seconds, and if suppression is not on
            else if (trials > 1)// && !(world.DetermineRightSuppressionOn() || world.DetermineLeftSuppressionOn()))
            {
                StartCoroutine(DelayedStateChange(2f, 1));
            }
            //StartCoroutine(DelayedStateChange(2f, 1, new List<GameObject>() { background, soccerBall, arrowObject, optotypeObject, glovesObject, holdStillLabelObject, feedbackLabelObject, savesLabelObject, goalsLabelObject })); 
            // UNCOMMENT THIS LATER
        }
        // State 1 - Turn the background back on. Activate label telling the user to center their head.
        else if (state == 1)
        {
            // Move optotype and gloves to their initial positions
            optotype.resetSprite();            
            gloves.ResetPositions();

            // If it's suppression mode, set the "baseline" for the world to wherever the user is currently looking
            if (settings.GetLeftGain() != 1 || settings.GetRightGain() != 1)
            {
                //Debug.Log("set baseline");
                //world.GetComponent<ResetPosition>().SetWorld();
            }

            // Just for redundancy, place the world's orientation at the baseline set a few lines up
            worldObject.GetComponent<ResetPosition>().ResetWorld();
            // Show the background
            background.SetActive(true);
            background.GetComponent<SpriteRenderer>().enabled = true;
            // The raycast square is what the game uses to determine if the user's head is centered; it shoots a raycast from the position of the head in the forward direction, and we check if this raycast intersects with the square.
            raycastSquare.SetActive(true);
            //ResetPosition.ResetRaycastSquare(raycastSquare, worldObject, initialSquarePos, initialSquareScale);
            // Show the label telling user to center their head
            feedbackLabelObject.SetActive(true);
            feedbackLabelObject.GetComponent<TextMeshPro>().text = "Center head"; // Activate label that tells the user to center their head

            // Reset these fields to false, before Update() checks the centering conditions
            isHeadFixated = false;
            isHeadFixatedForRange = false;
            isCoroutineActive = false;
        }
        // State 2 - // "Hold still" aka. wait for the user to stop moving their head
        else if (state == 2)
        {
            feedbackLabelObject.SetActive(false); // Turn off label from previous stage
            holdStillLabelObject.SetActive(true); // Turn on label for that instructs the user to hold still

            // Reset necessary fields to false; they'll become true later if the conditions are fulfilled
            isHeadStill = false;
            isHeadStillForRange = false;
            isCoroutineActive = false;
        }
        // State 3- Show soccer ball arrow and wait for the user to rotate their head
        else if (state == 3)
        {
            vrCamData.SetMaxSpeed(0f); // Before the rotation occurs, reset max speed
            soccerBall.SetActive(true); // Show soccer ball on the screen
            soccerBall.GetComponent<SpriteRenderer>().enabled = true; 
            arrowObject.SetActive(true); // Show the arrow object on the screen
            holdStillLabelObject.SetActive(false); // Hide label from the previous stage
            arrowObject.GetComponent<SpriteRenderer>().enabled = true; // Initialize the arrow object
            arrowObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

           /*  Vector3 tempPos = raycastSquare.transform.position; Quaternion tempRot = raycastSquare.transform.rotation;
            // "De-parent" the centering target so it's no longer affected by world suppression. We do this so that we can check if the user has moved their head away from the center, in the "arrow" stage, without being affected by suppression.
            raycastSquare.transform.SetParent(null, true); 
            // Reset all of the square's characteristics because the "de-parenting" might have changed its coordinates or something. I saw it on Unity forums
            raycastSquare.transform.position = tempPos; raycastSquare.transform.localScale = initialSquareScale; raycastSquare.transform.rotation = tempRot; */

            // Switch arrow to the left or right direction depending on instruction file data
            switch (settings.GetArrowDirection())
            {
                case 0: // Left
                    arrowObject.transform.rotation = Quaternion.identity;
                    break;
                case 1: // Right
                    arrowObject.transform.localScale = -1f * arrowObject.transform.localScale; // Flip it horizontally
                    break;
                default:
                    break;
            }
        }
        // State 4 - Show optotype for user input
        else if (state == 4)
        {
            // Clear stopwatch from previous stage. This fixes a bug where if you don't clear it, it will still run and instantly trigger a blank screen at the middle of a new trial.
            stopwatch.Stop();
            stopwatch.Reset();

            //ResetPosition.ResetRaycastSquare(raycastSquare, worldObject, initialSquarePos, initialSquareScale);

            //optotype.ShowOptotype();
            optotype.ShowOptotype(); // Show the green optotype that user controls
        }
        // State 5 - Animate gloves and soccer ball based on user input. Determine if the user's input in previous state was correct.
        else if (state == 5)
        {
            inputOptotype.HideOptotype(); // Hide the optotype from previous stage
            inputOptotype.resetSprite();
            optotype.HideOptotype();
            optotype.resetSprite();

            bool isCorrectResponse = true; // Needs to be defined before if statement
            string text; // For the feedback label
            float maxSpeed = vrCamData.GetMaxSpeed(); // Grab the largest speed from the user's head impulse
            maxSpeedLabelObject.SetActive(true); // Show this largest speed

            // Compare the direction the user inputted to the correct direction from the instruction file. Since they're both represented as integers, this makes comparison easy
            if (directionInput == settings.GetOptotypeDirection())
            {
                text = "Correct!";
                saves++;
                savesInThisRange++;
                isCorrectResponse = true;
            }
            else
            {
                text = "Incorrect!";
                goals++;
                goalsInThisRange++;
                isCorrectResponse = false;
            }
            maxSpeedLabelObject.GetComponent<TextMeshPro>().text = "Maximum Velocity: " + maxSpeed;

            // Animate it so that the gloves and soccer ball go in their proper directions
            glovesObject.GetComponent<SpriteRenderer>().enabled = true;
            gloves.Animate(isCorrectResponse, directionInput, settings.GetOptotypeDirection(), saves, goals); // Note that this same script also turns on the saves and the goals labels
                                                                                                              // Now turn on the feedback label too, after 1 second (same delay as the saves and goals labels)
            StartCoroutine(ShowLabel(feedbackLabelObject, 0.7f, text));

            animationHappenedYet = false;
        }
        // State 6 - Pause on the score screen. Go to the next state after a short delay.
        else if (state == 6)
        {
            trials++;
            StartCoroutine(DelayedStateChange(0.5f, 0));
        }
    }

    // Deactivate the sprite renderer or the game object for the specified list of objects
    private void TurnOff(List<GameObject> disableThese)
    {
        foreach (GameObject obj in disableThese)
        {
            // Some of these items in the list are 2D objects, so it's more prudent to turn off their SpriteRenderers instead fo the entire GameObject. That way, we can still access some of its functions, even when "deactivated."
            if (obj.GetComponent<SpriteRenderer>())
            {
                obj.GetComponent<SpriteRenderer>().enabled = false;
            }
            else
            {
                obj.SetActive(false);
            }
        }
    }

    // Detects if head is still
    private IEnumerator WaitForHeadStabilized()
    {
        isCoroutineActive = true; // Tells other parts of the program that 
        bool checker = true; // Temp variable that immediately switches off if the head moves away from center

        // Sample for a timespan of 2.5 seconds, at timesteps of 0.1 seconds. In each sample, check if the head speed is very low/almost 0 (below a certain threshold).
        for (int i = 0; i < 15; i++) // For 2.5 seconds, every 0.1 seconds check if the head is not moving
        {
            yield return new WaitForSeconds(0.05f); // Wait 0.1 seconds
            isHeadStill = vrCamData.GetAngularSpeed() < headStillThreshold; // Check that the head rotational speed is very low
            // Reset timer whenever user moves their head too much during the loop
            if (!isHeadStill)
            {
                checker = false;
                break;
            }
        }

        /* At the end of the loop, the checker variable will be true if the user kept their head still for the entire timespan.
         * Thus, set isHeadStillForRange to the checker if it has a true value.
         * Otherwise, go back to the head centering stage because if the head moved, then that means it's probably not centered anymore. */
        if (checker)
        {
            isHeadStillForRange = checker;
        }
        else
        {
            StateChange(1);
            TurnOff(new List<GameObject>() { holdStillLabelObject });
        }

        isCoroutineActive = false;
    }

    // Detects if head is looking at the center of the screen/the soccer ball
    private IEnumerator WaitForHeadFixated()
    {
        isCoroutineActive = true; // Alerts other parts of the program that this method is measuring something for a certain amount of time
        bool checker = true; // Temp variable that immediately switches off if the head moves away from center

        // Sample for a timespan of 2 seconds, at timesteps of 0.1 seconds. In each sample, check if the head is looking at the center of the screen
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(0.05f); // Wait 0.1 seconds
            /* The funny part about this isHeadFixated() method is that it doesn't use any of these params.
             * What it does is that it raycasts a line straight ahead of where the user is looking.
             * At the same time, we have an invisible square that is positioned in the center of the picture.
             * So if the raycast hits that square, then that means the user is pointing their gaze at the center of the picture. */
            isHeadFixated = vrCamData.IsHeadFixated(headFixatedThreshold, Vector3.zero, soccerBall);

            if (!isHeadFixated) // Consequence of this part is that the 2-second timer (loop) will reset whenever user moves their gaze away from the center.
            {
                checker = false;
                break;
            }
            else // When the user has centered their head, alert them to this by playing a certain sound
            {
                if (!soccerAudio.GetIsPlaying()) soccerAudio.HeadCenterAudio();
            }
        }

        // isHeadFixatedForRange is going to be checked in Update(), so set its value accordingly
        // We only have to set isHeadFixated when it's true because the state after head centering stage resets this variable to false
        if (checker) isHeadFixatedForRange = checker;
        isCoroutineActive = false; // Coroutine is over       
    }

    /* Pauses the program for a specified time before moving on to the next state in the state machine
     * @delay           Time (in seconds) of the delay
     * @nextState       The next state that the method should move to
     * @disableThese    List of GameObjects that will be deactivated (hidden from view) after the delay
     * */
    private IEnumerator DelayedStateChange(float delay, int nextState, List<GameObject> disableThese)
    {
        // Bool variable that alerts other parts of the program to pause
        isStateChangingRightNow = true;
        //Debug.Log("Changing state with bool of " + isStateChangingRightNow);

        // Wait for the specified amount of time
        yield return new WaitForSeconds(delay);

        /* Sometimes, the game might want to cancel the delayed state change or switch to a state instantaneously. When it does so, it will set cancelCoroutine to true.
         * After the delay, the method will not do anything if cancelCoroutine is set to true.
         * Otherwise, when there's no override active, the method will proceed as normal. It hides the objects listed in disableThese and calls stateChange with the inputted number.
         * */
        if (!cancelCoroutine)
        {
            TurnOff(disableThese);
            StateChange(nextState);
        }
        else
        {
            cancelCoroutine = false;
        }

        // Let rest of program know that the delay is over
        isStateChangingRightNow = false;
    }

    // Overload of the previous method. It functions the same, but does not include the ability to hide a list of game objects.
    private IEnumerator DelayedStateChange(float delay, int nextState)
    {
        isStateChangingRightNow = true;
        //Debug.Log("Changing state with bool of " + isStateChangingRightNow);
        yield return new WaitForSeconds(delay);
        // If the override variable (cancelCoroutine) is on, do nothing. If the override is false, do the state change as normal
        if (!cancelCoroutine)
        {
            StateChange(nextState);
        }
        else
        {
            cancelCoroutine = false;
        }
        isStateChangingRightNow = false;
    }

    /* Coroutine to show text label after a specified delay time. 
     * @label   GameObject representation of the label
     * @delay   Waits this amount of time to display the label
     * @text    Text that goes in the label */
    private IEnumerator ShowLabel(GameObject label, float delay, string text)
    {
        yield return new WaitForSeconds(delay);
        label.SetActive(true);
        label.GetComponent<TextMeshPro>().text = text;
    }

    /* When the user turns their head in the wrong direction, we should reset back to state 1, as well as show the corresponding feedback text label.
     * This block of code is called pretty frequently, so I made a separate method for it. */
    private void WrongDirection()
    {
        stopwatch.Stop(); stopwatch.Reset();

        Debug.Log("wrong direction with speed " + vrCamData.GetAngularSpeed());
        TurnOff(new List<GameObject>() { soccerBall, arrowObject });

        // Activate the text label that informs the user they turned their head in the wrong direction
        feedbackLabelObject.SetActive(true);
        feedbackLabelObject.GetComponent<TextMeshPro>().text = "Wrong direction!";

        StartCoroutine(DelayedStateChange(2f, 1, new List<GameObject>() { feedbackLabelObject })); // Wait 2 seconds, change back to state 1, turn off the feedback label
    }

    // Called by other objects in the game, to detect what stage of the state machine we're in.
    public int GetState()
    {
        return state;
    }
}