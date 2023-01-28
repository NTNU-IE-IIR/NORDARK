using UnityEngine;
using UnityEngine.UI;

public class Heatmap : MaskableGraphic
{
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
            Vector2 bottomLeftCorner = -rectTransform.pivot;
            bottomLeftCorner.x *= rectTransform.rect.width;
            bottomLeftCorner.y *= rectTransform.rect.height;

            vh.Clear();
            for (int i=0; i<numberOfQuadsOnOneAxis; ++i) {
                for (int j=0; j<numberOfQuadsOnOneAxis; ++j) {
                    AddQuad(
                        vh,
                        bottomLeftCorner + new Vector2(i*widthStep, j*heightStep),
                        widthStep,
                        heightStep,
                        values[i*numberOfQuadsOnOneAxis + j],
                        values[(i+1)*numberOfQuadsOnOneAxis + j],
                        values[(i+1)*numberOfQuadsOnOneAxis + j+1],
                        values[i*numberOfQuadsOnOneAxis + j+1]
                    );
                }
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

        vertex.position = position + new Vector2(width, 0);
        vertex.uv0 = new Vector4(secondValue, 0);
        vh.AddVert(vertex);

        vertex.position = position + new Vector2(width, height);
        vertex.uv0 = new Vector4(thirdValue, 1);
        vh.AddVert(vertex);

        vertex.position = position + new Vector2(0, height);
        vertex.uv0 = new Vector4(fourthValue, 1);
        vh.AddVert(vertex);
            
        vh.AddTriangle(numberOfVertices + 0, numberOfVertices + 2, numberOfVertices + 1);
        vh.AddTriangle(numberOfVertices + 3, numberOfVertices + 2, numberOfVertices + 0);
    }
}