using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Rotation : MonoBehaviour
{
    private int count = 0;
    private int step = 1;
    

    void FixedUpdate()
    {
        if (count <= 0) {
            step = 1;
        } else if (count >= 40) {
            step = -1;
        }
        count += step;

        transform.Rotate(0, -25 * Time.deltaTime, 0, Space.World);
    }
}
