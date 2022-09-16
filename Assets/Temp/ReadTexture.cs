using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadTexture : MonoBehaviour
{
    public RenderTexture lightTexture;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) {
            Debug.Log("Started");

            int m = lightTexture.width;
            int n = lightTexture.height;
            RenderTexture tmpTexture = RenderTexture.GetTemporary(m, n, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(lightTexture, tmpTexture);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmpTexture;

            Texture2D temp2DTexture = new Texture2D(m, n, TextureFormat.RGBA32, -1, true);
            temp2DTexture.ReadPixels(new Rect(0, 0, tmpTexture.width, tmpTexture.height), 0, 0);
            temp2DTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmpTexture);

            Color32[] colors = temp2DTexture.GetPixels32();
            float gray = 0f;

            for (int i=0; i<colors.Length; i++) {
                gray = (0.2989f * colors[i].r) + (0.5870f * colors[i].g) + (0.1140f * colors[i].b);
                if (gray != 0) {
                    Debug.Log(gray);
                }
            }
            
            Debug.Log("Finished");
        }
    }
}
