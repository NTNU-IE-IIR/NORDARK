using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

public class ConfigurationsControl : MonoBehaviour
{
    private const int MAX_NUMBER_OF_CONFIGURATIONS = 4;
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private TMP_Dropdown numberOfConfigurations;
    [SerializeField] private GameObject configurationPrefab;
    [SerializeField] private RectTransform configurationsHolder;

    void Awake()
    {
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(numberOfConfigurations);
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

        SetConfigurations(1);
    }

    private void SetConfigurations(int numberOfConfigs)
    {
        configurationsHolder.sizeDelta = new Vector2(0, 0);
        foreach (Transform configuration in configurationsHolder) {
            Destroy(configuration.gameObject);
        }

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
                        sceneCamerasManager.SetConfiguration(paths[0], configurationIndex);
                    }
                });
            }

            configurationsHolder.sizeDelta += new Vector2(0, configuration.GetHeight());
        }

        sceneCamerasManager.SplitScreen(numberOfConfigs, MAX_NUMBER_OF_CONFIGURATIONS);
    }
}