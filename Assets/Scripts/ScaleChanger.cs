using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleChanger : MonoBehaviour
{
    // Set of methods to change the scaling of whatever object (usually one of the 2D ones) this script is attached to 

    // Start is called before the first frame update
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Increase scale by input percent
    public void IncreaseScale(float input)
    {
        this.gameObject.transform.localScale = new Vector3(1f + input / 100, 1f + input / 100, 1f); // By the way we can set z scale to 1 because these are 2D objects
    }

    // Decrease scale by input percent
    public void DecreaseScale(float input)
    {
        this.gameObject.transform.localScale = new Vector3(1f - input / 100, 1f - input / 100, 1f);
    }

    // Set scale back to initial (read: 1) values
    public void ResetScale() // So far, this is still called in the SoccerOptotype class. Might move their method calls to the DynamicManager script?
    {
        this.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    // Unused method 
    public void SetScale(float input)
    {
        this.gameObject.transform.localScale = new Vector3(input, input, 1f);
    }
}
