using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonCreator : MonoBehaviour
{
    private const int IMAGE_SIZE = 128;
    [SerializeField] private ComputeShader polygonCreatorShader;
    private int indexOfPolygonCreatorKernel;

    void Start()
    {
        indexOfPolygonCreatorKernel = polygonCreatorShader.FindKernel("CSMain");

        List<Vector2> polygon = new List<Vector2> {
            new Vector2(-20, 20),
            new Vector2(20, 50),
            new Vector2(50, 50),
            new Vector2(35, 35),
            new Vector2(50, 20)
        };

        //CreatePolygonInImageCPU(polygon);
        CreatePolygonInImageGPU(polygon);
    }

    private void CreatePolygonInImageGPU(List<Vector2> polygon)
    {
        RenderTexture old = RenderTexture.active;

        float[] points = new float[polygon.Count*4];
        for (int i=0; i<polygon.Count; ++i) {
            points[4*i + 0] = polygon[i].x;
            points[4*i + 1] = polygon[i].y;
        }

        RenderTexture renderTexture = RenderTexture.GetTemporary(IMAGE_SIZE, IMAGE_SIZE, 0, RenderTextureFormat.ARGB32);
        renderTexture.enableRandomWrite = true;

        polygonCreatorShader.SetInt("polygonLength", polygon.Count);
        polygonCreatorShader.SetFloats("polygon", points);
        polygonCreatorShader.SetTexture(indexOfPolygonCreatorKernel, "Result", renderTexture);
        polygonCreatorShader.Dispatch(indexOfPolygonCreatorKernel, renderTexture.width / 32, renderTexture.height / 32, 1);

        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        System.IO.File.WriteAllBytes(@"C:\Users\leole\Desktop\" + "image.png", texture.EncodeToPNG());
        
        RenderTexture.active = old;
        RenderTexture.ReleaseTemporary(renderTexture);
    }

    private void CreatePolygonInImageCPU(List<Vector2> polygon)
    {
        Texture2D texture = new Texture2D(IMAGE_SIZE, IMAGE_SIZE, TextureFormat.ARGB32, false);

        for (int i=0; i<IMAGE_SIZE; ++i) {
            for (int j=0; j<IMAGE_SIZE; ++j) {
                if (PolyContainsPoint(polygon, new Vector2(i, j))) {
                    texture.SetPixel(i, j, new Color(1.0f, 0.0f, 0.0f, 1.0f));
                } else {
                    texture.SetPixel(i, j, new Color(0.0f, 0.0f, 0.0f, 1.0f));
                }
            }
        }

        // Apply all SetPixel calls
        texture.Apply();

        System.IO.File.WriteAllBytes(@"C:\Users\leole\Desktop\" + "image.png", texture.EncodeToPNG());
    }

    private bool PolyContainsPoint(List<Vector2> points, Vector2 p)
    {
        bool inside = false;

        // An imaginary closing segment is implied,
        // so begin testing with that.
        Vector2 v1 = points[points.Count - 1];

        foreach (Vector2 v0 in points)
        {
            double d1 = (p.y - v0.y) * (v1.x - v0.x);
            double d2 = (p.x - v0.x) * (v1.y - v0.y);

            if (p.y < v1.y)
            {
                // V1 below ray
                if (v0.y <= p.y)
                {
                    // V0 on or above ray
                    // Perform intersection test
                    if (d1 > d2)
                    {
                        inside = !inside; // Toggle state
                    }
                }
            }
            else if (p.y < v0.y)
            {
                // V1 is on or above ray, V0 is below ray
                // Perform intersection test
                if (d1 < d2)
                {
                    inside = !inside; // Toggle state
                }
            }

            v1 = v0; //Store previous endpoint as next startpoint
        }

        return inside;
    }
}
