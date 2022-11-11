using UnityEngine;

public class Rotation : MonoBehaviour
{
    private Material material;
    private int count = 0;
    private int step = 1;
    
    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    void FixedUpdate()
    {
        if (count <= 0) {
            step = 1;
        } else if (count >= 40) {
            step = -1;
        }
        count += step;

        transform.Rotate(0, -25 * Time.deltaTime, 0, Space.World);
        material.SetFloat("_OutLineWidth", 1f + 0.015f * count);
    }
}
