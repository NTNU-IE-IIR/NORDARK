using UnityEngine;
using UnityEngine.Assertions;

public class LightComputationManager : MonoBehaviour
{
    [SerializeField]
    private GameObject fullscreenPass;
    [SerializeField]
    private GameObject legend;

    void Awake()
    {
        Assert.IsNotNull(fullscreenPass);
        Assert.IsNotNull(legend);
    }

    public void DisplayLightResults(bool display)
    {
        fullscreenPass.SetActive(display);
        legend.SetActive(display);
    }
}
