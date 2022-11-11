using UnityEngine;
using UnityEngine.Assertions;

public class ShorcutsControl : MonoBehaviour
{
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private LightsManager lightsManager;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(lightsManager);
    }

    void Update()
    {
        if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.O)) {
            sceneManager.Load();
        }
        if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.S)) {
            sceneManager.Save();
        }
        if (Input.GetKeyDown(KeyCode.F12)) {
            sceneManager.SaveAs();
        }
        if (Input.GetKeyDown(KeyCode.Delete)) {
            lightsManager.DeleteLight();
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            lightsManager.MoveLight();
        }
        if (Input.GetKeyDown(KeyCode.I)) {
            lightsManager.InsertLight();
        }
        if (Input.GetKeyDown(KeyCode.Insert)) {
            lightsManager.InsertLight();
        }
    }
}
