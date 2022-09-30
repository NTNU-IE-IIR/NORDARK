using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ReadTexture : MonoBehaviour
{
    public RenderTexture lightTexture;
    private string path = @"C:\Users\leole\Desktop\";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) {
            Debug.Log("Started");

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            int m = lightTexture.width;
            int n = lightTexture.height;
            RenderTexture tmpTexture = RenderTexture.GetTemporary(m, n, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(lightTexture, tmpTexture);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmpTexture;
            
            Debug.Log("1:" + stopwatch.ElapsedMilliseconds);

            Texture2D temp2DTexture = new Texture2D(m, n, TextureFormat.RGBA32, -1, true);
            temp2DTexture.ReadPixels(new Rect(0, 0, tmpTexture.width, tmpTexture.height), 0, 0);
            temp2DTexture.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmpTexture);
            
            Debug.Log("2:" + stopwatch.ElapsedMilliseconds);

            Color32[] colors = temp2DTexture.GetPixels32();
            
            Debug.Log("3:" + stopwatch.ElapsedMilliseconds);

            string colorsContent = "";
            for (int i=0; i<colors.Length; i++) {
                float gray = (0.2989f * colors[i].r) + (0.5870f * colors[i].g) + (0.1140f * colors[i].b);
                colorsContent += gray.ToString("0.000") + "; ";
            }
            
            Debug.Log("4:" + stopwatch.ElapsedMilliseconds);

            Debug.Log("width: " + m.ToString());
            Debug.Log("height: " + n.ToString());
            File.WriteAllText(path + "colorfile.txt", colorsContent);

            Debug.Log("5:" + stopwatch.ElapsedMilliseconds);

            byte[] bytes = temp2DTexture.EncodeToPNG();
            File.WriteAllBytes(path + "Image.png", bytes);
            
            stopwatch.Stop();
            Debug.Log("6:" + stopwatch.ElapsedMilliseconds);

            Debug.Log("Finished");
        }
    }
}
