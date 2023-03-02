using UnityEngine;
using UnityEngine.Assertions;

public class ShorcutsControl : MonoBehaviour
{
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private LightPolesSelectionManager lightPolesSelectionManager;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(lightPolesSelectionManager);
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
        if (Input.GetKeyDown(KeyCode.O)) {
            lightPolesSelectionManager.StartDrawing();
        }
        if (Input.GetKeyDown(KeyCode.I)) {
            lightPolesManager.AddLightPole();
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            lightPolesManager.MoveSelectedLightPoles();
        }
        if (Input.GetKeyDown(KeyCode.Delete)) {
            lightPolesManager.DeleteSelectedLightPoles();
        }
    }
}
