using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SclerDisplay : MonoBehaviour
{
    public Connector connector;
    public GameObject textHolder;
    private GameObject canvas;
    private TextMeshProUGUI scaleTXT;
    private RectTransform rect;
    private bool app2D = false;
    // Start is called before the first frame update
    void Start()
    {
        scaleTXT = textHolder.GetComponent<TextMeshProUGUI>();
        canvas = connector.getScaleCanvas();
        if (canvas == null ){
            app2D = false;
        } else {
            app2D = true;
            rect = canvas.GetComponent<RectTransform>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(app2D){
            scaleTXT.text = rect.localScale.x.ToString();
        } else {
            scaleTXT.text = "NaN";
        }
        
    }
}
