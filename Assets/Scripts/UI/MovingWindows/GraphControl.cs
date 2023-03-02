using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;

public class GraphControl : MonoBehaviour
{
    private const int SEPARATOR_COUNT = 10;
    private const float MIN_Y_DIFFERENCE = 5;
    private const float OFFSET_Y = 0.2f;
    [SerializeField] private Sprite dotSprite;
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private RectTransform labelTemplateX;
    [SerializeField] private RectTransform labelTemplateY;
    [SerializeField] private RectTransform dashTemplateX;
    [SerializeField] private RectTransform dashTemplateY;
    [SerializeField] private RectTransform highlightLine;
    [SerializeField] private Button barChartButton;
    [SerializeField] private Button lineGraphButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Transform legend;
    [SerializeField] private GameObject captionPrefab;
    private List<GameObject> gameObjects = new List<GameObject>();
    private List<IGraphVisualObject> graphVisualObjects = new List<IGraphVisualObject>();
    private List<IGraphVisual> graphVisuals = new List<IGraphVisual>();
    private List<GraphSet> graphSets;
    private System.Func<float, string> getAxisLabelX;
    private System.Func<float, string> getAxisLabelY;
    private float yMinimum = IComputationObject.EXTREMUM_DEFAULT_VALUE;
    private float yMaximum = IComputationObject.EXTREMUM_DEFAULT_VALUE;

    void Awake()
    {
        Assert.IsNotNull(dotSprite);
        Assert.IsNotNull(graphContainer);
        Assert.IsNotNull(labelTemplateX);
        Assert.IsNotNull(labelTemplateY);
        Assert.IsNotNull(dashTemplateX);
        Assert.IsNotNull(dashTemplateY);
        Assert.IsNotNull(highlightLine);
        Assert.IsNotNull(barChartButton);
        Assert.IsNotNull(lineGraphButton);
        Assert.IsNotNull(refreshButton);
        Assert.IsNotNull(legend);
        Assert.IsNotNull(captionPrefab);
    }

    public void CreateGraph(
        List<GraphSet> graphSets,
        System.Action onRefresh,
        float yMinimum = IComputationObject.EXTREMUM_DEFAULT_VALUE,
        float yMaximum = IComputationObject.EXTREMUM_DEFAULT_VALUE)
    {
        if (yMinimum > yMaximum && yMinimum != IComputationObject.EXTREMUM_DEFAULT_VALUE) {
            (yMinimum, yMaximum) = (yMaximum, yMinimum);
        }
        if (yMinimum == yMaximum && yMinimum != IComputationObject.EXTREMUM_DEFAULT_VALUE) {
            yMaximum += 0.001f;
        }
        this.yMinimum = yMinimum;
        this.yMaximum = yMaximum;

        List<IGraphVisual> lineGraphVisuals = new List<IGraphVisual>();
        for (int i=0; i<graphSets.Count; ++i) {
            lineGraphVisuals.Add(new LineGraphVisual(graphContainer, dotSprite, graphSets[i].Color, graphSets[i].Color));
        }

        List<IGraphVisual> barChartVisuals = new List<IGraphVisual>();
        for (int i=0; i<graphSets.Count; ++i) {
            barChartVisuals.Add(new BarChartVisual(graphContainer, graphSets[i].Color, 0.9f));
        }

        barChartButton.onClick.RemoveAllListeners();
        barChartButton.onClick.AddListener(() => {
            SetGraphVisual(barChartVisuals);
        });

        lineGraphButton.onClick.RemoveAllListeners();
        lineGraphButton.onClick.AddListener(() => {
            SetGraphVisual(lineGraphVisuals);
        });

        refreshButton.onClick.RemoveAllListeners();
        refreshButton.onClick.AddListener(() => {
            onRefresh();
        });
        
        ShowGraph(graphSets, lineGraphVisuals);
    }

    public void Show(bool display)
    {
        gameObject.SetActive(display);
    }

    public void Clear()
    {
        foreach (GameObject gameObject in gameObjects) {
            Destroy(gameObject);
        }
        gameObjects.Clear();
        foreach (IGraphVisualObject graphVisualObject in graphVisualObjects) {
            graphVisualObject.CleanUp();
        }
        graphVisualObjects.Clear();
        foreach (IGraphVisual graphVisual in graphVisuals) {
            graphVisual.CleanUp();
        }
    }

    public void HighlightXLine(float x)
    {
        if (graphSets == null) {
            return;
        }
        
        float xMinimum, xMaximum;
        CalculateXScale(out xMinimum, out xMaximum);
        float xGraph = (x - xMinimum) / (xMaximum - xMinimum) * graphContainer.sizeDelta.x;

        highlightLine.anchoredPosition = new Vector2(xGraph, highlightLine.anchoredPosition.y);
    }

    private void SetGraphVisual(List<IGraphVisual> graphVisuals)
    {
        ShowGraph(graphSets, graphVisuals, getAxisLabelX, getAxisLabelY);
    }

    private void ShowGraph(List<GraphSet> graphSets, List<IGraphVisual> graphVisuals, System.Func<float, string> getAxisLabelX = null, System.Func<float, string> getAxisLabelY = null)
    {
        if (graphSets == null) {
            return;
        }

        if (getAxisLabelX == null) {
            getAxisLabelX = f => f.ToString("0.0");
        }
        if (getAxisLabelY == null) {
            getAxisLabelY = f => {
                if (f < 10) {
                    return f.ToString("0.000");
                } else {
                    return f.ToString("0.");
                }
            };
        }

        this.graphSets = graphSets;
        this.graphVisuals = graphVisuals;
        this.getAxisLabelX = getAxisLabelX;
        this.getAxisLabelY = getAxisLabelY;

        Clear();

        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;
        int maxNumberOfAbscissas = graphSets.Select(graphSet => graphSet.Ordinates.Count).Max();
        float maxXSize = graphWidth / maxNumberOfAbscissas;

        float xMinimum, xMaximum, yMinimum, yMaximum;
        CalculateXScale(out xMinimum, out xMaximum);
        CalculateYScale(out yMinimum, out yMaximum);
        
        for (int i=0; i<graphSets.Count; ++i) {
            for (int j=0; j<graphSets[i].Ordinates.Count; ++j) {
                float yPositionUnscaled = Mathf.Max(Mathf.Min(graphSets[i].Ordinates[j], yMaximum), yMinimum);

                float xPosition = ((graphSets[i].Abscissas[j] - xMinimum) / (xMaximum - xMinimum)) * graphWidth;
                float yPosition = ((yPositionUnscaled - yMinimum) / (yMaximum - yMinimum)) * graphHeight;

                string tooltipText = graphSets[i].Title + "\n" + graphSets[i].Ordinates[j].ToString("0.0000");
                graphVisualObjects.Add(graphVisuals[i].CreateGraphVisualObject(new Vector2(xPosition, yPosition), maxXSize, tooltipText));
            }
        }

        for (int i=0; i<=SEPARATOR_COUNT; ++i) {
            float normalizedValue = (float) i / SEPARATOR_COUNT;

            RectTransform labelX = Instantiate(labelTemplateX, graphContainer);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(normalizedValue * graphWidth, labelX.anchoredPosition.y);
            labelX.GetComponent<TMP_Text>().text = getAxisLabelX(xMinimum + normalizedValue * (xMaximum - xMinimum));
            gameObjects.Add(labelX.gameObject);

            RectTransform dashX = Instantiate(dashTemplateX, graphContainer);
            dashX.gameObject.SetActive(true);
            dashX.anchoredPosition = new Vector2(normalizedValue * graphWidth, dashX.anchoredPosition.y);
            dashX.GetComponent<Image>().raycastTarget = false;
            gameObjects.Add(dashX.gameObject);
        }

        for (int i=0; i<=SEPARATOR_COUNT; ++i) {
            float normalizedValue = (float) i / SEPARATOR_COUNT;

            RectTransform labelY = Instantiate(labelTemplateY, graphContainer);
            labelY.gameObject.SetActive(true);
            labelY.anchoredPosition = new Vector2(labelY.anchoredPosition.x, normalizedValue * graphHeight);
            labelY.GetComponent<TMP_Text>().text = getAxisLabelY(yMinimum + normalizedValue * (yMaximum - yMinimum));
            gameObjects.Add(labelY.gameObject);

            RectTransform dashY = Instantiate(dashTemplateY, graphContainer);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(dashY.anchoredPosition.x, normalizedValue * graphHeight);
            dashY.GetComponent<Image>().raycastTarget = false;
            gameObjects.Add(dashY.gameObject);
        }

        for (int i=0; i<graphVisuals.Count; ++i) {
            if (graphSets[i].Ordinates.Count > 0) {
                Caption caption = Instantiate(captionPrefab, legend).GetComponent<Caption>();
                caption.Create(graphVisuals[i].GetColor(), graphSets[i].Title);
                gameObjects.Add(caption.gameObject);
            }
        }
    }

    private void CalculateXScale(out float xMinimum, out float xMaximum)
    {
        xMinimum = Mathf.Infinity;
        xMaximum = -Mathf.Infinity;
        foreach (GraphSet graphSet in graphSets) {
            foreach (float ordinate in graphSet.Abscissas) {
                xMinimum = Mathf.Min(xMinimum, ordinate);
                xMaximum = Mathf.Max(xMaximum, ordinate);
            }
        }
    }

    private void CalculateYScale(out float yMinimum, out float yMaximum)
    {
        yMinimum = Mathf.Infinity;
        yMaximum = -Mathf.Infinity;
        foreach (GraphSet graphSet in graphSets) {
            foreach (float ordinate in graphSet.Ordinates) {
                yMinimum = Mathf.Min(yMinimum, ordinate);
                yMaximum = Mathf.Max(yMaximum, ordinate);
            }
        }
        float yDifference = yMaximum - yMinimum;
        if (yDifference <= 0) {
            yDifference = MIN_Y_DIFFERENCE;
        }
        yMaximum += yDifference * OFFSET_Y;
        yMinimum -= yDifference * OFFSET_Y;

        if (this.yMinimum != IComputationObject.EXTREMUM_DEFAULT_VALUE) {
            yMinimum = this.yMinimum;
        }
        if (this.yMaximum != IComputationObject.EXTREMUM_DEFAULT_VALUE) {
            yMaximum = this.yMaximum;
        }
    }

    private interface IGraphVisual {
        IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText);
        void CleanUp();
        Color GetColor();
    }

    private interface IGraphVisualObject {
        void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string tooltipText);
        void CleanUp();
    }

    private class BarChartVisual: IGraphVisual {
        private RectTransform graphContainer;
        private Color barColor;
        private float barWidthMultiplier;

        public BarChartVisual(RectTransform graphContainer, Color barColor, float barWidthMultiplier)
        {
            this.graphContainer = graphContainer;
            this.barColor = barColor;
            this.barWidthMultiplier = barWidthMultiplier;
        }

        public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
        {
            GameObject bar = CreateBar(graphPosition, graphPositionWidth);

            BarChartVisualObject barChartVisualObject = new BarChartVisualObject(bar, barWidthMultiplier);
            barChartVisualObject.SetGraphVisualObjectInfo(graphPosition, graphPositionWidth, tooltipText);

            return barChartVisualObject;
        }

        public void CleanUp()
        {}

        public Color GetColor()
        {
            return barColor;
        }

        private GameObject CreateBar(Vector2 barPosition, float barWidth)
        {
            GameObject bar = new GameObject("bar", typeof(Image));
            bar.transform.SetParent(graphContainer, false);
            bar.GetComponent<Image>().color = barColor;
            bar.AddComponent<TooltipDisplayerUI>();
            
            RectTransform rectTransform = bar.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(barPosition.x, 0);
            rectTransform.sizeDelta = new Vector2(barWidth * barWidthMultiplier, barPosition.y);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);

            return bar;
        }

        public class BarChartVisualObject: IGraphVisualObject
        {
            private GameObject barGameObject;
            private float barWidthMultiplier;

            public BarChartVisualObject(GameObject barGameObject, float barWidthMultiplier)
            {
                this.barGameObject = barGameObject;
                this.barWidthMultiplier = barWidthMultiplier;
            }

            public void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
            {
                RectTransform rectTransform = barGameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(graphPosition.x, 0);
                rectTransform.sizeDelta = new Vector2(graphPositionWidth * barWidthMultiplier, graphPosition.y);
                
                barGameObject.GetComponent<TooltipDisplayerUI>().SetText(tooltipText);
            }

            public void CleanUp()
            {
                Destroy(barGameObject);
            }
        }
    }

    private class LineGraphVisual: IGraphVisual {
        private const float DOT_SIZE = 11;
        private const float LINE_WIDTH = 3;
        private RectTransform graphContainer;
        private Sprite dotSprite;
        private Color dotColor;
        private Color dotConnectionColor;
        private LineGraphVisualObject lastLineGraphVisualObject;

        public LineGraphVisual(RectTransform graphContainer, Sprite dotSprite, Color dotColor, Color dotConnectionColor)
        {
            this.graphContainer = graphContainer;
            this.dotSprite = dotSprite;
            this.dotColor = dotColor;
            this.dotConnectionColor = dotConnectionColor;
            lastLineGraphVisualObject = null;
        }

        public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
        {
            GameObject dot = CreateDot(graphPosition);

            GameObject dotConnection = null;
            if (lastLineGraphVisualObject != null) {
                dotConnection = CreateDotConnection(lastLineGraphVisualObject.GetGraphPosition(), dot.GetComponent<RectTransform>().anchoredPosition);
            }

            LineGraphVisualObject lineGraphVisualObject = new LineGraphVisualObject(dot, dotConnection, lastLineGraphVisualObject);
            lineGraphVisualObject.SetGraphVisualObjectInfo(graphPosition, graphPositionWidth, tooltipText);
            lastLineGraphVisualObject = lineGraphVisualObject;
            
            return lineGraphVisualObject;
        }

        public void CleanUp()
        {
            lastLineGraphVisualObject = null;
        }

        public Color GetColor()
        {
            return dotColor;
        }

        private GameObject CreateDot(Vector2 anchoredPosition)
        {
            GameObject dot = new GameObject("dot", typeof(Image));
            dot.AddComponent<TooltipDisplayerUI>();
            dot.transform.SetParent(graphContainer, false);

            Image image = dot.GetComponent<Image>();
            image.sprite = dotSprite;
            image.color = dotColor;
            
            RectTransform rectTransform = dot.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(DOT_SIZE, DOT_SIZE);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            return dot;
        }

        private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
        {
            GameObject dotConnection = new GameObject("dotConnection", typeof(Image));
            dotConnection.transform.SetParent(graphContainer, false);
            Image image = dotConnection.GetComponent<Image>();
            image.color = dotConnectionColor;
            image.raycastTarget = false;

            RectTransform rectTransform = dotConnection.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            return dotConnection;
        }

        public class LineGraphVisualObject: IGraphVisualObject
        {
            public event System.EventHandler OnChangedGraphVisualObjectInfo;
            private GameObject dotGameObject;
            private GameObject dotConnectionGameObject;
            private LineGraphVisualObject lastVisualObject;

            public LineGraphVisualObject(GameObject dotGameObject, GameObject dotConnectionGameObject, LineGraphVisualObject lastVisualObject)
            {
                this.dotGameObject = dotGameObject;
                this.dotConnectionGameObject = dotConnectionGameObject;
                this.lastVisualObject = lastVisualObject;

                if (lastVisualObject != null) {
                    lastVisualObject.OnChangedGraphVisualObjectInfo += LastVisualObject_OnChangedGraphVisualObjectInfo;
                }
            }

            private void LastVisualObject_OnChangedGraphVisualObjectInfo(object sender, System.EventArgs e)
            {
                UpdateDotConnection();
            }

            public void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
            {
                dotGameObject.GetComponent<RectTransform>().anchoredPosition = graphPosition;

                UpdateDotConnection();

                dotGameObject.GetComponent<TooltipDisplayerUI>().SetText(tooltipText);

                if (OnChangedGraphVisualObjectInfo != null) {
                    OnChangedGraphVisualObjectInfo(this, System.EventArgs.Empty);
                }
            }
            
            public void CleanUp()
            {
                Destroy(dotGameObject);
                Destroy(dotConnectionGameObject);
            }

            public Vector2 GetGraphPosition()
            {
                return dotGameObject.GetComponent<RectTransform>().anchoredPosition;
            }

            private void UpdateDotConnection()
            {
                if (dotConnectionGameObject != null) {
                    Vector2 direction = (lastVisualObject.GetGraphPosition() - GetGraphPosition()).normalized;
                    float distance = Vector2.Distance(GetGraphPosition(), lastVisualObject.GetGraphPosition());
                    float angle = Utils.GetAngleBetweenPositions(lastVisualObject.GetGraphPosition(), GetGraphPosition());

                    RectTransform dotConnectionRectTransform = dotConnectionGameObject.GetComponent<RectTransform>();
                    dotConnectionRectTransform.sizeDelta = new Vector2(distance, LINE_WIDTH);
                    dotConnectionRectTransform.anchoredPosition = GetGraphPosition() + direction * distance / 2;
                    dotConnectionRectTransform.localEulerAngles = new Vector3(0, 0, angle);
                }
            }
        }
    }
}