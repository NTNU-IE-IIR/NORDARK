using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class LuminanceMapControl : MonoBehaviour
{
    [SerializeField] private LuminanceMapManager luminanceMapManager;
    [SerializeField] private Toggle luminanceMap;

    void Awake()
    {
        Assert.IsNotNull(luminanceMapManager);
        Assert.IsNotNull(luminanceMap);
    }

    void Start()
    {
        luminanceMap.onValueChanged.AddListener(luminanceMapManager.DisplayLuminanceMap);
    }
}
