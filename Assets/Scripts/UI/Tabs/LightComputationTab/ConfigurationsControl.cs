using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;

public class ConfigurationsControl : MonoBehaviour
{
    public const int MAX_NUMBER_OF_CONFIGURATIONS = 4;
    [SerializeField] private LightConfigurationsManager lightConfigurationsManager;
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private TMP_Dropdown numberOfConfigurations;
    [SerializeField] private Toggle splitScreen;
    [SerializeField] private GameObject configurationPrefab;
    [SerializeField] private RectTransform configurationsHolder;

    void Awake()
    {
        Assert.IsNotNull(lightConfigurationsManager);
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(numberOfConfigurations);
        Assert.IsNotNull(splitScreen);
        Assert.IsNotNull(configurationPrefab);
        Assert.IsNotNull(configurationsHolder);
    }

    void Start()
    {
        numberOfConfigurations.AddOptions(
            Enumerable.Range(1, MAX_NUMBER_OF_CONFIGURATIONS).Select(i => i.ToString()).ToList()
        );
        numberOfConfigurations.onValueChanged.AddListener(value => {
            SetConfigurations(int.Parse(numberOfConfigurations.options[value].text));
        });

        splitScreen.onValueChanged.AddListener(isOn => {
            sceneCamerasManager.SplitScreen(isOn ?
                int.Parse(numberOfConfigurations.options[numberOfConfigurations.value].text) :
                1
            );
        });

        SetConfigurations(1);
    }

    public void ResetConfigurations()
    {
        numberOfConfigurations.value = 0;
    }

    public int GetCurrentNumberOfConfigurations()
    {
        return configurationsHolder.childCount;
    }

    private void SetConfigurations(int numberOfConfigs)
    {
        configurationsHolder.sizeDelta = new Vector2(0, 0);
        foreach (Transform configuration in configurationsHolder) {
            Destroy(configuration.gameObject);
        }
        lightConfigurationsManager.DeleteConfigurations();

        for (int i=0; i<numberOfConfigs; ++i) {
            Configuration configuration = Instantiate(configurationPrefab, configurationsHolder).GetComponent<Configuration>();

            if (i == 0) {
                configuration.Create("Scene config", () => {}, false);
            } else {
                int configurationIndex = i;
                configuration.Create("No file selected", () => {
                    string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Import configuration file", "", "nordark", false);
                    if (paths.Length > 0) {
                        configuration.SetName(paths[0]);
                        lightConfigurationsManager.SetConfiguration(paths[0], configurationIndex);
                    }
                });
            }

            configurationsHolder.sizeDelta += new Vector2(0, configuration.GetHeight());
        }
        
        sceneCamerasManager.SplitScreen(splitScreen.isOn ? numberOfConfigs : 1);
    }
}