using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class LightComputationControl : MonoBehaviour
{
    [SerializeField] private LightComputationManager lightComputationManager;
    [SerializeField] private Toggle lightResults;

    void Awake()
    {
        Assert.IsNotNull(lightComputationManager);
        Assert.IsNotNull(lightResults);
    }

    void Start()
    {
        lightResults.onValueChanged.AddListener(lightComputationManager.DisplayLightResults);
    }
}
