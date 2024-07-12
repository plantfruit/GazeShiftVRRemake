using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoccerMenu : MonoBehaviour
{
    public SoccerPreferences settings; 

    public Dropdown directionDropdown;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 3 methods to load the various scenes. Called by menu buttons.
    public void LoadDynamic()
    {
        SceneManager.LoadSceneAsync("Soccer_Dynamic");
    }

    public void LoadStatic()
    {
        SceneManager.LoadSceneAsync("Soccer_Static");
    }

    public void LoadGame()
    {
        SceneManager.LoadSceneAsync("Soccer_Game");
    }

    // Configuration Screen Methods

    public void HeadDirectionDropdown() // Called everytime the value in the dropdown changes. Revises the direction that the arrow is in.
    {
         settings.SetArrowDirection(directionDropdown.GetComponent<Dropdown>().value);
    }
}
