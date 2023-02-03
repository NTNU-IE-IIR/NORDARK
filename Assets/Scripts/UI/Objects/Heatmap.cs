using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Heatmap : MaskableGraphic
{
    [SerializeField] private HeatmapControl heatmapControl;
    [SerializeField] private Texture heatmapTexture;
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
    private float[] values;
    private bool isMouseOver;

    protected override void Awake()
    {
        Assert.IsNotNull(heatmapControl);

        base.Awake();

        isMouseOver = false;
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

    public void SetValues(float[] values)
    {
        this.values = values;
        SetVerticesDirty();
        SetMaterialDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (values != null) {
            int numberOfQuadsOnOneAxis = (int) Mathf.Sqrt(values.Length) - 1;

            float widthStep = rectTransform.rect.width / numberOfQuadsOnOneAxis;
            float heightStep = rectTransform.rect.height / numberOfQuadsOnOneAxis;
            Vector2 bottomRightCorner = rectTransform.pivot;
            bottomRightCorner.x *= rectTransform.rect.width;
            bottomRightCorner.y *= -rectTransform.rect.height;

            vh.Clear();
            for (int j=0; j<numberOfQuadsOnOneAxis; ++j) {
                for (int i=0; i<numberOfQuadsOnOneAxis; ++i) {
                    AddQuad(
                        vh,
                        bottomRightCorner + new Vector2(-i*widthStep, j*heightStep),
                        widthStep,
                        heightStep,
                        values[(numberOfQuadsOnOneAxis+1)*j + i],
                        values[(numberOfQuadsOnOneAxis+1)*j + i+1],
                        values[(numberOfQuadsOnOneAxis+1)*(j+1) + i+1],
                        values[(numberOfQuadsOnOneAxis+1)*(j+1) + i]
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
}