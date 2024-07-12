using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Grapher : MonoBehaviour
{
    public RectTransform container;

    public Texture point1;
    public Texture point2;
    public Texture point3;
    public Texture point4;

    private GameObject[] points1;
    private GameObject[] points2;
    private GameObject[] points3;
    private GameObject[] points4;

    private float graph1Value = 0;
    private float graph2Value = 0;
    private float graph3Value = 0;
    private float graph4Value = 0;

    private string graph1Name = "";
    private string graph2Name = "";
    private string graph3Name = "";
    private string graph4Name = "";

    public GameObject lblGraph1;
    public GameObject lblGraph2;
    public GameObject lblGraph3;
    public GameObject lblGraph4;

    private TextMeshProUGUI graph1TXT;
    private TextMeshProUGUI graph2TXT;
    private TextMeshProUGUI graph3TXT;
    private TextMeshProUGUI graph4TXT;

    private int counter = 0;
    private int slowerCounter = 0;
    public int slowerFactor = 1;
    private int testWaveIndex = 0;
    private float horConv = 0;
    private float verConv = 0;
    private float containerX = 0;
    private float containerY = 0;
    public int numPoints = 1000;

    // Start is called before the first frame update
    void Start()
    {
        points1 = new GameObject[numPoints];
        points2 = new GameObject[numPoints];
        points3 = new GameObject[numPoints];
        points4 = new GameObject[numPoints];

        graph1TXT = lblGraph1.GetComponent<TextMeshProUGUI>();
        graph2TXT = lblGraph2.GetComponent<TextMeshProUGUI>();
        graph3TXT = lblGraph3.GetComponent<TextMeshProUGUI>();
        graph4TXT = lblGraph4.GetComponent<TextMeshProUGUI>();

        Canvas.ForceUpdateCanvases();
        // positioning of graphs
        containerX = Mathf.Abs(container.rect.width);
        containerY = Mathf.Abs(container.rect.height);
        horConv = containerX/ numPoints;
        verConv = containerY / 300.0f;
        Debug.Log(horConv.ToString());

        for(int i = 0; i<numPoints; i++)
        {
            points1[i] = CreatePoint(i * horConv, 0, point1);
        }
        for(int i = 0; i<numPoints; i++)
        {
            points2[i] = CreatePoint(i * horConv, 0, point2);
        }
        for(int i = 0; i<numPoints; i++)
        {
            points3[i] = CreatePoint(i * horConv, 0, point3);
        }
        for(int i = 0; i<numPoints; i++)
        {
            points4[i] = CreatePoint(i * horConv, 0, point4);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // setGraph1(testWave());
        // setGraph2(testWave());
        // setGraph3(testWave());
        // setGraph4(testWave());
        UpdateNames();
        slowerCounter++;
        if (slowerCounter >= slowerFactor)
        {
            UpdateGraph(points1, 0f, graph1Value);
            UpdateGraph(points2, 2f, graph2Value);
            UpdateGraph(points3, 4f, graph3Value);
            UpdateGraph(points4, 6f, graph4Value);
            UpdateCounter();
            slowerCounter = 0;
        }
    }

    private void UpdateGraph(GameObject[] points, float offset, float value)  
    {
        points[counter].GetComponent<RectTransform>().anchoredPosition = new Vector2((counter*horConv)-(containerX/2.0f), (value * verConv) - (containerY/2.0f)+offset);
        points[counter].SetActive(true);
        if (counter + 50 > numPoints-1)
        {
            points[(counter - (numPoints-50))].SetActive(false);
        }
        else
        {
            points[counter + 50].SetActive(false);
        }
    }

    private void UpdateCounter() {
        counter++;  //
        if (counter > numPoints -1) //
        {
            counter = 0; //
        }
    }

    private GameObject CreatePoint(float x, float y, Texture tex)
    {
        GameObject circle = new GameObject("circle", typeof(RawImage));
        circle.transform.SetParent(container , false);
        circle.GetComponent<RawImage>().texture = tex;
        circle.GetComponent<RectTransform>().anchoredPosition = new Vector2((x * horConv) - (containerX / 2.0f), (y * verConv) - (containerY / 2.0f));
        circle.SetActive(false);
        circle.GetComponent<RectTransform>().sizeDelta = new Vector2(0.05f, 0.05f);
        return circle;
    }

    private void UpdateNames()
    {
        graph1TXT.text = graph1Name;
        graph2TXT.text = graph2Name;
        graph3TXT.text = graph3Name;
        graph4TXT.text = graph4Name;
    }

    public void setGraph1(float num)
    {
        graph1Value = num;
    }

    public void setGraph2(float num)
    {
        graph2Value = num;
    }

    public void setGraph3(float num)
    {
        graph3Value = num;
    }

    public void setGraph4(float num)
    {
        graph4Value = num;
    }

    public void setGraph1Name(string name)
    {
        graph1Name = name;
    }

    public void setGraph2Name(string name)
    {
        graph2Name = name;
    }

    public void setGraph3Name(string name)
    {
        graph3Name = name;
    }

    public void setGraph4Name(string name)
    {
        graph4Name = name;
    }

    private int testWave()
    {
        testWaveIndex++;
        return (int) ((Mathf.Sin(0.2f * testWaveIndex) * 200) + 200);
    }

    public float verTransform(float y)
    {
        return (y * verConv) - (containerY / 2.0f);
    }
}
