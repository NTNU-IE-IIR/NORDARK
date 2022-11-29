using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class LightsManager : MonoBehaviour, IObjectsManager
{
    private const string LIGHTS_RESOURCES_FOLDER = "Lights";
    [SerializeField] private MapManager mapManager;
    [SerializeField] private IESManager iesManager;
    [SerializeField] private LightControl lightControl;
    [SerializeField] private SelectionPin selectionPin;
    [SerializeField] private Material highlightMaterial;
    List<LightPole> lightPoles;
    private LightPole selectedLightPole;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(iesManager);
        Assert.IsNotNull(lightControl);
        Assert.IsNotNull(selectionPin);
        Assert.IsNotNull(highlightMaterial);
        
        lightPoles = new List<LightPole>();
        selectedLightPole = null;
    }

    public void Create(Feature feature)
    {
        LightPole lightPole = new LightPole(feature.Coordinates[0]);
        lightPole.Name = feature.Properties["name"] as string;
        lightPole.PrefabName = feature.Properties["prefabName"] as string;
        List<float> eulerAngles = feature.Properties["eulerAngles"] as List<float>;
        CreateLight(lightPole, new Vector3(eulerAngles[0], eulerAngles[1], eulerAngles[2]), feature.Properties["IESfileName"] as string);
    }

    public void Clear()
    {
        ClearSelectedLight();
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.Destroy();
        }
        lightPoles.Clear();
    }

    public void OnLocationChanged()
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.SetPosition(mapManager.GetUnityPositionFromCoordinates(lightPole.Coordinates, true));
            lightPole.Light.MultiplyScale(mapManager.GetWorldRelativeScale());
        }

        if (selectedLightPole != null) {
            selectionPin.SetPosition(selectedLightPole.Light.GetTransform().position);
            selectionPin.MultiplyScale(mapManager.GetWorldRelativeScale());
        }
    }

    public List<Feature> GetFeatures()
    {
        List<Feature> features = new List<Feature>();
        foreach (LightPole lightPole in lightPoles) {
            Feature feature = new Feature();
            Vector3 eulerAngles = lightPole.Light.GetTransform().eulerAngles;
            feature.Properties.Add("type", "light");
            feature.Properties.Add("name", lightPole.Name);
            feature.Properties.Add("eulerAngles", new List<float>{eulerAngles.x, eulerAngles.y, eulerAngles.z});
            feature.Properties.Add("IESfileName", lightPole.Light.GetIESLight().Name);
            feature.Properties.Add("prefabName", lightPole.PrefabName);
            feature.Coordinates = new List<Vector3d> {lightPole.Coordinates};
            features.Add(feature);
        }
        return features;
    }

    public void Create()
    {
        LightPole lightPole = new LightPole();
        CreateLight(lightPole, new Vector3(0, 0), "");
        SelectLight(lightPole);
        MoveCurrentLight();
    }

    public void DeleteLight()
    {
        if (selectedLightPole != null)
        {
            selectedLightPole.Light.Destroy();
            lightPoles.Remove(selectedLightPole);
            ClearSelectedLight();
        }
    }

    public void ShowLights(bool show)
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.Show(show);
        }
    }

    public void ChangeLightType(string newLightType)
    {
        if (selectedLightPole != null && newLightType != selectedLightPole.PrefabName) {
            Vector3 eulerAngles = selectedLightPole.Light.GetTransform().eulerAngles;
            IESLight iesLight = selectedLightPole.Light.GetIESLight();
            selectedLightPole.Light.Destroy();

            selectedLightPole.PrefabName = newLightType;
            selectedLightPole.Light = Instantiate(Resources.Load<GameObject>(LIGHTS_RESOURCES_FOLDER + "/" + newLightType)).GetComponent<LightPrefab>();
            selectedLightPole.Light.Create(selectedLightPole, transform, eulerAngles, mapManager);
            selectedLightPole.Light.SetIESLight(iesLight);
        }
    }

    public List<string> GetLightPrefabNames()
    {
        List<string> lightPrefabNames = new List<string>();

        Object[] lights = Resources.LoadAll(LIGHTS_RESOURCES_FOLDER);
        foreach (Object light in lights) {
            lightPrefabNames.Add(light.name);
        }

        return lightPrefabNames;
    }

    public void MoveLight()
    {
        if (selectedLightPole != null) {
            if (selectedLightPole.Light.IsMoving()) {
                ClearSelectedLight();
            } else {
                MoveCurrentLight();
            }
        }
    }

    public void RotateSelectedLight(float rotation)
    {
        if (selectedLightPole != null) {
            selectedLightPole.Light.Rotate(rotation);
        }
    }

    public void ChangeLightSource(string newIESName)
    {
        if (selectedLightPole != null && newIESName != selectedLightPole.Light.GetIESLight().Name) {
            IESLight newIES = iesManager.GetIESLightFromName(newIESName);

            if (newIES != null) {
                selectedLightPole.Light.SetIESLight(newIES);
            }
        }
    }

    public void SelectLight()
    {
        RaycastHit hitInfo = new RaycastHit();
        bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        if (!isOverUI && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 10000)) {
            LightPrefab lightPrefab = hitInfo.transform.gameObject.GetComponent<LightPrefab>();
            if (lightPrefab != null) {
                foreach (LightPole lightPole in lightPoles) {
                    if (lightPole.Light == lightPrefab) {
                        selectedLightPole = lightPole;
                        SelectLight(selectedLightPole);
                    }
                }
            }
        }
    }

    public void ClearSelectedLight()
    {
        if (selectedLightPole != null) {
            selectedLightPole.Light.SetMoving(false);

            Vector3 lightPosition = selectedLightPole.Light.GetTransform().position;
            selectedLightPole.Coordinates = mapManager.GetCoordinatesFromUnityPosition(lightPosition);

            selectedLightPole = null;
            selectionPin.SetActive(false);
        }
        lightControl.ClearSelectedLight();
    }

    public void HighlightLights(bool hightlight)
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.Hightlight(hightlight, highlightMaterial);
        }
    }

    private void CreateLight(LightPole lightPole, Vector3 eulerAngles, string IESName)
    {
        if (lightPole.Name == "") {
            lightPole.Name = Utils.DetermineNewName(lightPoles.Select(light => light.Name).ToList(), "Light");
        }
        if (lightPole.PrefabName == "") {
            lightPole.PrefabName = GetLightPrefabNames()[0];
        }
        
        lightPole.Light = Instantiate(Resources.Load<GameObject>(LIGHTS_RESOURCES_FOLDER + "/" + lightPole.PrefabName)).GetComponent<LightPrefab>();
        lightPole.Light.Create(lightPole, transform, eulerAngles, mapManager);
        lightPole.Light.SetIESLight(iesManager.GetIESLightFromName(IESName));

        lightPoles.Add(lightPole);
    }

    private void SelectLight(LightPole lightPole)
    {
        ClearSelectedLight();
        selectedLightPole = lightPole;

        lightControl.LightSelected(selectedLightPole);
        
        selectionPin.SetActive(true);
        selectionPin.SetPosition(selectedLightPole.Light.GetTransform().position);
        selectionPin.MultiplyScale(mapManager.GetWorldRelativeScale());
    }

    private void MoveCurrentLight()
    {
        selectedLightPole.Light.SetMoving(true);
        selectionPin.SetMoving(true);
    }
}