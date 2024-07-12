using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlovesController : MonoBehaviour
{
    public GameObject soccerBall;
    public GameObject savesLabelObject;
    public GameObject goalsLabelObject;
    private GameObject glovesObject;

    private bool isAnimating = false; // Used in Update() loop for enabling/disabling movement
    private bool isIncomplete = true;
    private bool labelsSuppressed = false;
    private int directionInput; // Direction that the gloves will go to (user controls the glove movement)
    private int saves; // Displayed in their respective labels
    private int goals;
    private float ballSpeed = 10f;
    private float glovesSpeed = 10f;
    private float distance = 3f; // Multiply this by the direction to get the final coordinate position of the ball and gloves objects
    private float distanceFromPlayer = 18f; // Distance from the player to the background plane
    private Vector3 ballStartPos; // The startpos variables represent the location to which the objects are reset to after each trial (where they start at)
    private Vector3 glovesStartPos;
    private Vector3 ballEndPos; // The endpos variables represent the location to which the objects must go to at the end of their animation
    private Vector3 glovesEndPos;    
    
    private Vector3[] movementArray = new[] { // This is a list of direction vectors that is used for the gloves and soccer ball lerp    
            new Vector3 (0f, 1f, 0f),
            new Vector3 (1f, 1f, 0f),
            new Vector3 (1f, 0f, 0f),
            new Vector3 (1f, -1f, 0f),
            new Vector3 (0f, -1f, 0f),
            new Vector3 (-1f, -1f, 0f),
            new Vector3 (-1f, 0f, 0f),
            new Vector3 (-1f, 1f, 0f),
        }; 
    /* Direction guide: 
         * 0 - up
         * 1 - up right
         * 2 - right
         * 3 - down right
         * 4 - down
         * 5 - down left
         * 6 - left
         * 7 - up left
         */

    // Start is called before the first frame update
    void Start()
    {
        // Set the fields for the ball and gloves' initial positions. Later on, we can use these fields for resetting their location.
        ballStartPos = soccerBall.transform.localPosition;
        glovesStartPos = glovesObject.transform.localPosition;
    }

    void Awake()
    {
        // This script is attached to the gloves object in game
        glovesObject = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (isAnimating)
        {
            // Iteratively move the soccer and gloves objects to their end coordinates (which were calculated in the Animate() method)
            soccerBall.transform.localPosition = Vector3.MoveTowards(soccerBall.transform.localPosition, ballEndPos, ballSpeed * Time.deltaTime);
            glovesObject.transform.localPosition = Vector3.MoveTowards(glovesObject.transform.localPosition, glovesEndPos, glovesSpeed * Time.deltaTime);
            //Debug.Log("gloves " + Vector3.Distance(glovesObject.transform.position, glovesEndPos) + " ball " + Vector3.Distance(soccerBall.transform.position, ballEndPos));    
            
            if (Vector3.Distance(glovesObject.transform.localPosition, glovesEndPos) < 0.001f && Vector3.Distance(soccerBall.transform.localPosition, ballEndPos) < 0.001f) // Unity documentation 
            {
                isAnimating = false; // Stop animation on the next frame 
                StartCoroutine(ShowLabels()); 
            }
        }
    }

    public void Animate(bool isCorrectResponse, int directionInput, int optotypeDirection, int saves, int goals)
    {
        // Since the animation needs to be gradual, its actual movement steps are handled in Update(). These fields "toggle on" and let the Update() loop know that it's time to move the ball and gloves.
        isAnimating = true;
        isIncomplete = true;

        // Save method inputs into this class' fields
        this.directionInput = directionInput;
        this.saves = saves;
        this.goals = goals;        

        // Calculate the final coordinates for the soccer ball and gloves. We can select the general direction vector that the ball and gloves need to be in, using the movementArray, which maps direction integers to their corresponding direction vectors.
        ballEndPos = distance * (movementArray[optotypeDirection]) + ballStartPos; // Add the (0, 0, 10) at the end because this is the distance that the vectors must be from the plane
        if (isCorrectResponse)
        {
            glovesEndPos = ballEndPos;
        }
        else
        {
            glovesEndPos = distance * (movementArray[directionInput]) + ballStartPos; // Use the ball start pos because it's the center of the screen            
        }

        // Need to adjust the end coordinates for an edge case
        if (glovesEndPos.y >= distance)
        { // Gloves sprite are off-centered and won't make it to the soccer ball if it's on upper part of screen
            glovesEndPos = new Vector3(glovesEndPos.x, glovesEndPos.y * 1.1f, glovesEndPos.z);
        }

        // Calculate the necessary speed of the gloves animation, which is based off the predetermined ball speed. 
        float soccerBallDistance = Vector3.Distance(ballStartPos, ballEndPos);
        float time = soccerBallDistance / ballSpeed;
        float glovesDistance = Vector3.Distance(glovesStartPos, glovesEndPos);
        glovesSpeed = glovesDistance / time;

        Debug.Log("isCorrectResponse " + isCorrectResponse + " w directionInput " + directionInput + " and optotypeDirection of " + optotypeDirection + " ballstartpos " + ballStartPos + " ballendpos " + ballEndPos + " glovesendpos " + glovesEndPos);
                
        glovesObject.SetActive(true); // Show the gloves (ball was already revealed in a previous stage)
    }

    // Reveals the labels that tell game stats so far 
    private IEnumerator ShowLabels()
    {
        yield return new WaitForSeconds(0.25f);
        savesLabelObject.SetActive(true);
        goalsLabelObject.SetActive(true);
        savesLabelObject.GetComponent<TextMeshPro>().text = "Saves: " + saves;
        goalsLabelObject.GetComponent<TextMeshPro>().text = "Goals: " + goals;
        isIncomplete = false;
    }

    // Called by Manager.
    public void ResetPositions()
    {
        soccerBall.transform.localPosition = ballStartPos;
        glovesObject.transform.localPosition = glovesStartPos;
    }

    // Called by Manager in state 5. Idiosyncrasy with this method is that it returns false when it's done. 
    public bool GetIsIncomplete()
    {
        return isIncomplete;
    }
}
