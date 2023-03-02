using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Heatmap : MaskableGraphic
{
    private const float MAX_NUMBER_OF_POINTS_ON_ONE_AXIS = 10;
    [SerializeField] private HeatmapControl heatmapControl;
    [SerializeField] private Texture heatmapTexture;
    [SerializeField] private GameObject heatmapPointPrefab;
    public Texture texture
    {
        get
        {
            return heatmapTexture;
        }
        set
        {
            if (heatmapTexture == value) {
                return;
            }
 
            heatmapTexture = value;
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }
    public override Texture mainTexture
    {
        get
        {
            return heatmapTexture == null ? s_WhiteTexture : heatmapTexture;
        }
    }
    private float[] valuesNormalized;
    private bool isMouseOver = false;
    private bool arePointsVisible = true;

    protected override void Awake()
    {
        Assert.IsNotNull(heatmapControl);
        Assert.IsNotNull(heatmapPointPrefab);

        base.Awake();

        SetUpEventTrigger();
    }

    void Update()
    {
        if (isMouseOver) {
            heatmapControl.OnMouseOver(new Vector2(
                (rectTransform.position.x - Input.mousePosition.x + rectTransform.rect.width / 2) / rectTransform.rect.width,
                (Input.mousePosition.y - rectTransform.position.y + rectTransform.rect.height / 2) / rectTransform.rect.height
            ));
        }
    }

    public void DisplayValues(bool display)
    {
        foreach (Transform heatmapPoint in transform) {
            heatmapPoint.gameObject.SetActive(display);
        }
        arePointsVisible = display;
    }

    public void SetValues(float[] values, float minValue, float maxValue)
    {
        valuesNormalized = new float[values.Length];
        for (int i=0; i<values.Length; ++i) {
            valuesNormalized[i] = 1 - (values[i] - minValue) / (maxValue - minValue);
        }

        SetVerticesDirty();
        SetMaterialDirty();

        SetHeatmapPoints(values);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (valuesNormalized != null) {
            int numberOfQuadsOnOneAxis = (int) Mathf.Sqrt(valuesNormalized.Length) - 1;
            float widthStep = rectTransform.rect.width / numberOfQuadsOnOneAxis;
            float heightStep = rectTransform.rect.height / numberOfQuadsOnOneAxis;

            vh.Clear();
            for (int j=0; j<numberOfQuadsOnOneAxis; ++j) {
                for (int i=0; i<numberOfQuadsOnOneAxis; ++i) {
                    AddQuad(
                        vh,
                        GetPositionFromIndices(i, j, numberOfQuadsOnOneAxis),
                        widthStep,
                        heightStep,
                        valuesNormalized[(numberOfQuadsOnOneAxis+1)*j + i],
                        valuesNormalized[(numberOfQuadsOnOneAxis+1)*j + i+1],
                        valuesNormalized[(numberOfQuadsOnOneAxis+1)*(j+1) + i+1],
                        valuesNormalized[(numberOfQuadsOnOneAxis+1)*(j+1) + i]
                    );
                }
            }
        }
    }

    private void SetUpEventTrigger()
    {
        EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryPointerEnter = new EventTrigger.Entry();
        entryPointerEnter.eventID = EventTriggerType.PointerEnter;
        entryPointerEnter.callback.AddListener((data) => {
            isMouseOver = true;
        });
        eventTrigger.triggers.Add(entryPointerEnter);

        EventTrigger.Entry entryPointerExit = new EventTrigger.Entry();
        entryPointerExit.eventID = EventTriggerType.PointerExit;
        entryPointerExit.callback.AddListener((data) => {
            isMouseOver = false;
            heatmapControl.OnMouseExit();
        });
        eventTrigger.triggers.Add(entryPointerExit);
    }

    private void SetHeatmapPoints(float[] values)
    {
        foreach (Transform heatmapPoint in transform) {
            Destroy(heatmapPoint.gameObject);
        }

        int numberOfQuadsOnOneAxis = (int) Mathf.Sqrt(values.Length) - 1;
        float scaleFactor = Mathf.Min(MAX_NUMBER_OF_POINTS_ON_ONE_AXIS / numberOfQuadsOnOneAxis, 1);

        for (int j=0; j<numberOfQuadsOnOneAxis+1; ++j) {
            for (int i=0; i<numberOfQuadsOnOneAxis+1; ++i) {
                Vector2 position = GetPositionFromIndices(i, j, numberOfQuadsOnOneAxis) + new Vector2(transform.position.x, transform.position.y);
                HeatmapPoint heatmapPoint = Instantiate(
                    heatmapPointPrefab,
                    position,
                    Quaternion.identity,
                    transform
                ).GetComponent<HeatmapPoint>();

                HeatmapPoint.TextPositionHorizontal textPositionHorizontal;
                HeatmapPoint.TextPositionVertical textPositionVertical;

                if (i == 0) {
                    textPositionHorizontal = HeatmapPoint.TextPositionHorizontal.Left;
                } else if (i == numberOfQuadsOnOneAxis) {
                    textPositionHorizontal = HeatmapPoint.TextPositionHorizontal.Right;
                } else {
                    textPositionHorizontal = HeatmapPoint.TextPositionHorizontal.Middle;
                }

                if (j == numberOfQuadsOnOneAxis) {
                    textPositionVertical = HeatmapPoint.TextPositionVertical.Bottom;
                } else {
                    textPositionVertical = HeatmapPoint.TextPositionVertical.Top;
                }
                
                heatmapPoint.Create(values[(numberOfQuadsOnOneAxis+1)*j + i], textPositionHorizontal, textPositionVertical, scaleFactor);
                heatmapPoint.gameObject.SetActive(arePointsVisible);
            }
        }
    }

    private void AddQuad(VertexHelper vh, Vector2 position, float width, float height, float firstValue, float secondValue, float thirdValue, float fourthValue)
    {
        int numberOfVertices = vh.currentVertCount;

        UIVertex vertex = new UIVertex();
        vertex.color = this.color;

        vertex.position = position;
        vertex.uv0 = new Vector4(firstValue, 0);
        vh.AddVert(vertex);

        vertex.position = position + new Vector2(-width, 0);
        vertex.uv0 = new Vector4(secondValue, 0);
        vh.AddVert(vertex);

        vertex.position = position + new Vector2(-width, height);
        vertex.uv0 = new Vector4(thirdValue, 1);
        vh.AddVert(vertex);

        vertex.position = position + new Vector2(0, height);
        vertex.uv0 = new Vector4(fourthValue, 1);
        vh.AddVert(vertex);
            
        vh.AddTriangle(numberOfVertices + 0, numberOfVertices + 2, numberOfVertices + 1);
        vh.AddTriangle(numberOfVertices + 3, numberOfVertices + 2, numberOfVertices + 0);
    }

    private Vector2 GetPositionFromIndices(int i, int j, int numberOfDivisions)
    {
        float widthStep = rectTransform.rect.width / numberOfDivisions;
        float heightStep = rectTransform.rect.height / numberOfDivisions;

        Vector2 bottomRightCorner = rectTransform.pivot;
        bottomRightCorner.x *= rectTransform.rect.width;
        bottomRightCorner.y *= -rectTransform.rect.height;

        return bottomRightCorner + new Vector2(-i*widthStep, j*heightStep);
    }
}