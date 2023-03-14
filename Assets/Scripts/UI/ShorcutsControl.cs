using UnityEngine;
using UnityEngine.Assertions;

public class ShorcutsControl : MonoBehaviour
{
    [SerializeField] private SceneManager sceneManager;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
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
    }
}
