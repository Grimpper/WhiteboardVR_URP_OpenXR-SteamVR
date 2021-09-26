using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    [SerializeField] private Material circleMaterial;
    [SerializeField] private float circleDiameter = 11;
    [SerializeField] private bool startAtZero = false;

    [SerializeField] private float minDistanceBetweenPoints = 0;
    [SerializeField] private int maxVisibleAmount = -1;
    [SerializeField] [FixEnumNames] private NumberUtils.Unit xMagnitude = NumberUtils.Unit.Unitary;
    [SerializeField] [FixEnumNames] private NumberUtils.Unit yMagnitude = NumberUtils.Unit.Unitary;

    [SerializeField] private Vector2Int gridSize;

    #region GraphObjects
    private RectTransform canvas;
    private RectTransform graphContainer;
    private RectTransform labelsBackground;
    private RectTransform titlesBackground;
    private RectTransform graphBackground;
    private RectTransform title;
    private RectTransform xAxisTitle;
    private RectTransform yAxisTitle;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;
    private List<GameObject> gameObjectsList;

    private UIGridRenderer uiGridRenderer;
    private UILineRenderer uiLineRenderer;
    #endregion

    #region GraphData
    private float graphWidth;
    private float graphHeight;
    
    private float xMin;
    private float xMax;
    private float yMin;
    private float yMax;

    private float xStep;
    private float yStep;

    private int startIndex;
    #endregion
    
    private void Awake()
    {
        labelsBackground = transform.Find("Labels background").GetComponent<RectTransform>();
        titlesBackground = transform.Find("Titles background").GetComponent<RectTransform>();
        graphBackground = transform.Find("Graph background").GetComponent<RectTransform>();
        graphContainer = graphBackground.Find("Graph container").GetComponent<RectTransform>();
        canvas = transform.parent.GetComponent<RectTransform>();

        graphWidth = canvas.sizeDelta.x + GetComponent<RectTransform>().sizeDelta.x + graphBackground.sizeDelta.x; // sizeDelta.x from stretched components is negative
        graphHeight = canvas.sizeDelta.y + GetComponent<RectTransform>().sizeDelta.y + graphBackground.sizeDelta.y; // sizeDelta.y from stretched components is negative

        title = titlesBackground.Find("Title").GetComponent<RectTransform>();
        xAxisTitle = titlesBackground.Find("X axis title").GetComponent<RectTransform>();
        yAxisTitle = titlesBackground.Find("Y axis title").GetComponent<RectTransform>();
        
        labelTemplateX = graphContainer.Find("Label template X").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("Label template Y").GetComponent<RectTransform>();
        
        gameObjectsList = new List<GameObject>();

        uiGridRenderer = graphBackground.Find("UIGridRenderer").GetComponent<UIGridRenderer>();
        uiLineRenderer = graphBackground.Find("UILineRenderer").GetComponent<UILineRenderer>();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(330, 10, 150, 50), "Show graph"))
            ShowGraph(2, SpiceParser.Variables, maxVisibleAmount);
    }

    private void ShowGraph(int variableIndex, in Dictionary<int, SpiceVariable> variables,  int visibleAmount = -1, 
        Func<float, float, string> getAxisLabelX = null, Func<float, float,  string> getAxisLabelY = null)
    {
        getAxisLabelX ??= (number, diff) => 
            Math.Round(number, Mathf.Abs(NumberUtils.GetSignificantFigurePos(diff)) + 2).ToString(CultureInfo.InvariantCulture);
        getAxisLabelY ??= (number, diff) => 
            Math.Round(number, Mathf.Abs(NumberUtils.GetSignificantFigurePos(diff)) + 2).ToString(CultureInfo.InvariantCulture);

        EmptyGameObjectList(gameObjectsList);

        if (!variables.TryGetValue(0, out var xVariable) || !variables.TryGetValue(variableIndex, out var yVariable))
            return;
        List<float> xValues = xVariable.GetValues(xMagnitude);
        List<float> yValues = yVariable.GetValues(yMagnitude);
        
        if (visibleAmount < 0) 
            visibleAmount = yVariable.GetValues().Count;

        (yMin, yMax, yStep, startIndex) = GetAxisValues(yValues, visibleAmount);
        (xMin, xMax, xStep, _) = GetAxisValues(xValues);

        SetTitles(xVariable, yVariable);
        CreateLabels(getAxisLabelX, getAxisLabelY);
        CreateDots(xValues, yValues);

    }
    
    private void EmptyGameObjectList(List<GameObject> list)
    {
        foreach (GameObject element in list)
        {
            Destroy(element);
        }
        list.Clear();
    }

    private void SetTitles(SpiceVariable xVariable, SpiceVariable yVariable)
    {
        title.GetComponent<TextMeshProUGUI>().text = SpiceParser.Title;
        title.anchoredPosition = new Vector2(0, labelsBackground.offsetMax.y / 2f);
        title.gameObject.SetActive(true);

        string xUnit = NumberUtils.GetUnit(xVariable.Name, xMagnitude);
        xAxisTitle.GetComponent<TextMeshProUGUI>().text = xVariable.DisplayName + xUnit;
        xAxisTitle.anchoredPosition = new Vector2(0, labelsBackground.offsetMin.y / 2f);
        xAxisTitle.gameObject.SetActive(true);
        
        string yUnit = NumberUtils.GetUnit(yVariable.Name, yMagnitude);
        yAxisTitle.GetComponent<TextMeshProUGUI>().text = yVariable.DisplayName + yUnit;
        yAxisTitle.anchoredPosition = new Vector2(labelsBackground.offsetMin.x / 2f, 0);
        yAxisTitle.gameObject.SetActive(true);
    }

    private void CreateLabels(Func<float, float, string> getAxisLabelX, Func<float, float, string> getAxisLabelY)
    {
        int xLabelCount = 0;
        for (float xSeparatorPos = xMin; xSeparatorPos < xMax + xStep; xSeparatorPos += xStep, xLabelCount++)
        {
            float diff = xMax - xMin;
            float graphPosX = GetGraphPosX(xSeparatorPos);
            float yLabelPos = (labelsBackground.offsetMin.y - graphBackground.offsetMin.y) / 2f;
            CreateLabel(labelTemplateX, new Vector2(graphPosX, yLabelPos), getAxisLabelX(xSeparatorPos, diff));
        }

        int yLabelCount = 0;
        for (float ySeparatorPos = yMin; ySeparatorPos < yMax + yStep; ySeparatorPos += yStep, yLabelCount++)
        {
            float diff = yMax - yMin;
            float graphPosY = GetGraphPosY(ySeparatorPos);
            float xLabelPos = -(labelsBackground.offsetMin.x - graphBackground.offsetMin.x) / 2f;
            CreateLabel(labelTemplateY, new Vector2(-xLabelPos, graphPosY), getAxisLabelY(ySeparatorPos, diff));
        }
        
        int xDivisions = --xLabelCount;
        int yDivisions = --yLabelCount;

        gridSize = new Vector2Int(xDivisions, yDivisions);
        uiGridRenderer.GridSize = gridSize;
        uiLineRenderer.GridSize = gridSize;
    }
    
    private void CreateDots(in List<float> xValues, in List<float> yValues)
    {
        GameObject lastCircle = null;
        List<Vector2> graphPoints = new List<Vector2>();

        for (int i = startIndex; i < yValues.Count; i++)
        {
            Vector2 dataPoint = new Vector2(GetGraphPosX(xValues[i]), GetGraphPosY(yValues[i]));
            graphPoints.Add(dataPoint);

            if (!lastCircle || !((dataPoint - lastCircle.GetComponent<RectTransform>().anchoredPosition).magnitude <
                                 minDistanceBetweenPoints))
            {
                GameObject circle = CreateCircle(dataPoint);
                gameObjectsList.Add(circle);

                lastCircle = circle;
            }
        }

        uiLineRenderer.Points = graphPoints;
    }

    private void CreateDotsAndConnections(in List<float> xValues, in List<float> yValues)
    {
        GameObject lastCircle = null;
        Vector2? lastDataPoint = null;
        List<Vector2> graphPoints = new List<Vector2>();

        for (int i = startIndex; i < yValues.Count; i++)
        {
            Vector2 dataPoint = new Vector2(GetGraphPosX(xValues[i]), GetGraphPosY(yValues[i]));
            graphPoints.Add(dataPoint);

            if (!lastCircle || !((dataPoint - lastCircle.GetComponent<RectTransform>().anchoredPosition).magnitude <
                                 minDistanceBetweenPoints))
            {
                GameObject circle = CreateCircle(dataPoint);
                gameObjectsList.Add(circle);

                lastCircle = circle;
            }

            // if (lastDataPoint != null)
            // {
            //     GameObject connection = CreateConnection((Vector2) lastDataPoint,
            //         dataPoint);
            //     gameObjectsList.Add(connection);
            // }

             lastDataPoint = dataPoint;
        }

        uiLineRenderer.Points = graphPoints;
    }
    
    private GameObject CreateCircle(Vector2 anchoredPos)
    {
        GameObject dot = new GameObject("circle", typeof(Image));
        dot.transform.SetParent(graphContainer, false);
        dot.GetComponent<Image>().material = circleMaterial;

        RectTransform rectTransform = dot.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPos;
        rectTransform.sizeDelta = new Vector2(circleDiameter, circleDiameter);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return dot;
    }
    
    private float GetGraphPosX(float xPos) => (xPos - xMin) / (xMax - xMin) * graphWidth;
    private float GetGraphPosY(float yPos) => (yPos - yMin) / (yMax - yMin) * graphHeight;

    private (float min, float max, float step, int startIndex) GetAxisValues(IReadOnlyList<float> valueList, 
        int? visibleAmount = null)
    {
    int startValueIndex = Mathf.Max(valueList.Count - visibleAmount ?? 0, 0);
        
        float max = valueList[0];
        float min = valueList[0];
        
        for (int i = startValueIndex; i < valueList.Count; i++)
        {
            float value = valueList[i];
            if (value > max)
                max = value;
            else if (value < min)
                min = value;
        }

        float diff = max - min;
        
        int significantFigurePos = NumberUtils.GetSignificantFigurePos(diff);
        float significantFigure = diff * Mathf.Pow(10, significantFigurePos);

        float step = Mathf.Pow(10, significantFigurePos) / (significantFigure > 5f ? 1f : 2f);
        
        if (startAtZero)
            min = 0;

        return (min , max, step, startValueIndex);
    }   
    
    private void CreateLabel(RectTransform labelTemplate, Vector2 position, string labelText)
    {
        RectTransform label = Instantiate(labelTemplate, graphContainer, false);
        label.gameObject.SetActive(true);
        label.anchoredPosition = position;
        label.GetComponent<TextMeshProUGUI>().text = labelText;
        
        gameObjectsList.Add(label.gameObject);
    }

    private GameObject CreateConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject connection = new GameObject("dotConnection", typeof(Image));
        connection.transform.SetParent(graphContainer, false);
        connection.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);

        RectTransform rectTransform = connection.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = dotPositionA + 0.5f * distance * dir;
        rectTransform.localEulerAngles = 
            new Vector3(0, 0, NumberUtils.GetAngleFromVector(dir, NumberUtils.Degrees));

        return connection;
    }
}   
