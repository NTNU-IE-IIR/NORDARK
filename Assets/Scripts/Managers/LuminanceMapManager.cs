using UnityEngine;
using UnityEngine.Assertions;

public class LuminanceMapManager : MonoBehaviour
{
    [SerializeField] private GameObject luminanceMapPass;
    [SerializeField] private GameObject legend;
    
    void Awake()
    {
        Assert.IsNotNull(luminanceMapPass);
        Assert.IsNotNull(legend);
    }

    public void DisplayLuminanceMap(bool display)
    {
        luminanceMapPass.SetActive(display);
        legend.SetActive(display);
    }
}