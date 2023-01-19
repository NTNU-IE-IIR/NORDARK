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
    [SerializeField] private Button barChartButton;
    [SerializeField] private Button lineGraphButton;
    [SerializeField] private Transform legend;
    [SerializeField] private GameObject captionPrefab;
    private List<GameObject> gameObjects = new List<GameObject>();
    private List<IGraphVisualObject> graphVisualObjects = new List<IGraphVisualObject>();
    private List<RectTransform> yLabels = new List<RectTransform>();
    private List<IGraphVisual> graphVisuals = new List<IGraphVisual>();
    private List<List<float>> valuesList;
    private List<string> labels;
    private int maxVisibleValueAmount;
    private System.Func<int, string> getAxisLabelX;
    private System.Func<float, string> getAxisLabelY;
    private float xSize;
    private bool startYScaleAtZero = true;

    void Awake()
    {
        Assert.IsNotNull(dotSprite);
        Assert.IsNotNull(graphContainer);
        Assert.IsNotNull(labelTemplateX);
        Assert.IsNotNull(labelTemplateY);
        Assert.IsNotNull(dashTemplateX);
        Assert.IsNotNull(dashTemplateY);
        Assert.IsNotNull(barChartButton);
        Assert.IsNotNull(lineGraphButton);
        Assert.IsNotNull(legend);
        Assert.IsNotNull(captionPrefab);
    }

    public void CreateGraph(List<List<float>> valuesList, List<Color> colors, List<string> labels, System.Func<int, string> getAxisLabelX)
    {
        List<IGraphVisual> lineGraphVisuals = new List<IGraphVisual>();
        for (int i=0; i<valuesList.Count; ++i) {
            lineGraphVisuals.Add(new LineGraphVisual(graphContainer, dotSprite, colors[i], new Color(1, 1, 1, 0.5f)));
        }

        List<IGraphVisual> barChartVisuals = new List<IGraphVisual>();
        for (int i=0; i<valuesList.Count; ++i) {
            barChartVisuals.Add(new BarChartVisual(graphContainer, colors[i], 0.9f));
        }

        barChartButton.onClick.AddListener(() => {
            SetGraphVisual(barChartVisuals);
        });
        lineGraphButton.onClick.AddListener(() => {
            SetGraphVisual(lineGraphVisuals);
        });
        
        ShowGraph(valuesList, lineGraphVisuals, labels, -1, getAxisLabelX);
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
        yLabels.Clear();
        foreach (IGraphVisualObject graphVisualObject in graphVisualObjects) {
            graphVisualObject.CleanUp();
        }
        graphVisualObjects.Clear();
        foreach (IGraphVisual graphVisual in graphVisuals) {
            graphVisual.CleanUp();
        }
    }

    private void SetGraphVisual(List<IGraphVisual> graphVisuals)
    {
        ShowGraph(valuesList, graphVisuals, labels, maxVisibleValueAmount, getAxisLabelX, getAxisLabelY);
    }

    private void ShowGraph(List<List<float>> valuesList, List<IGraphVisual> graphVisuals, List<string> labels, int maxVisibleValueAmount = -1, System.Func<int, string> getAxisLabelX = null, System.Func<float, string> getAxisLabelY = null)
    {
        if (valuesList == null) {
            return;
        }

        if (getAxisLabelX == null) {
            getAxisLabelX = i => i.ToString();
        }
        if (getAxisLabelY == null) {
            getAxisLabelY = f => f.ToString("0.00");
        }
        if (maxVisibleValueAmount <= 0 || maxVisibleValueAmount > valuesList.Count) {
            maxVisibleValueAmount = valuesList.Select(v => v.Count).Max();
        }

        this.valuesList = valuesList;
        this.labels = labels;
        this.graphVisuals = graphVisuals;
        this.getAxisLabelX = getAxisLabelX;
        this.getAxisLabelY = getAxisLabelY;
        this.maxVisibleValueAmount = maxVisibleValueAmount;

        Clear();

        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;
        xSize = graphWidth / (maxVisibleValueAmount+1);

        float yMinimum, yMaximum;
        CalculateYScale(out yMinimum, out yMaximum);
        
        for (int i=0; i<valuesList.Count; ++i) {
            List<float> values = valuesList[i];
            int xIndex = 0;

            for (int j=Mathf.Max(values.Count - maxVisibleValueAmount, 0); j<values.Count; ++j) {
                float xPosition = (xIndex+1) * xSize;
                float yPosition = ((values[j] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;

                string tooltipText = getAxisLabelY(values[j]);
                graphVisualObjects.Add(graphVisuals[i].CreateGraphVisualObject(new Vector2(xPosition, yPosition), xSize, tooltipText));

                RectTransform labelX = Instantiate(labelTemplateX, graphContainer);
                labelX.gameObject.SetActive(true);
                labelX.anchoredPosition = new Vector2(xPosition, labelX.anchoredPosition.y);
                labelX.GetComponent<TMP_Text>().text = getAxisLabelX(j);
                gameObjects.Add(labelX.gameObject);

                RectTransform dashX = Instantiate(dashTemplateX, graphContainer);
                dashX.gameObject.SetActive(true);
                dashX.anchoredPosition = new Vector2(xPosition, dashX.anchoredPosition.y);
                dashX.GetComponent<Image>().raycastTarget = false;
                gameObjects.Add(dashX.gameObject);

                xIndex++;
            }
        }

        for (int i=0; i<=SEPARATOR_COUNT; ++i) {
            float normalizedValue = (float) i / SEPARATOR_COUNT;

            RectTransform labelY = Instantiate(labelTemplateY, graphContainer);
            labelY.gameObject.SetActive(true);
            labelY.anchoredPosition = new Vector2(labelY.anchoredPosition.x, normalizedValue * graphHeight);
            labelY.GetComponent<TMP_Text>().text = getAxisLabelY(yMinimum + normalizedValue * (yMaximum - yMinimum));
            yLabels.Add(labelY);
            gameObjects.Add(labelY.gameObject);

            RectTransform dashY = Instantiate(dashTemplateY, graphContainer);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(dashY.anchoredPosition.x, normalizedValue * graphHeight);
            dashY.GetComponent<Image>().raycastTarget = false;
            gameObjects.Add(dashY.gameObject);
        }

        for (int i=0; i<graphVisuals.Count; ++i) {
            if (valuesList[i].Count > 0) {
                Caption caption = Instantiate(captionPrefab, legend).GetComponent<Caption>();
                caption.Create(graphVisuals[i].GetColor(), labels[i]);
                gameObjects.Add(caption.gameObject);
            }
        }
    }

    private void CalculateYScale(out float yMinimum, out float yMaximum)
    {
        yMinimum = Mathf.Infinity;
        yMaximum = -Mathf.Infinity;
        foreach (List<float> values in valuesList) {
            foreach (float value in values) {
                yMinimum = Mathf.Min(yMinimum, value);
                yMaximum = Mathf.Max(yMaximum, value);
            }
        }
        float yDifference = yMaximum - yMinimum;
        if (yDifference <= 0) {
            yDifference = MIN_Y_DIFFERENCE;
        }
        yMaximum += yDifference * OFFSET_Y;
        yMinimum -= yDifference * OFFSET_Y;

        if (startYScaleAtZero) {
            yMinimum = 0;
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