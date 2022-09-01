using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class LightsManager : MonoBehaviour
{
    private const string LIGHTS_RESOURCES_FOLDER = "Lights";

    [SerializeField]
    private EnvironmentManager environmentManager;
    [SerializeField]
    private IESManager iesManager;
    [SerializeField]
    private LightControl lightControl;
    [SerializeField]
    private SelectionPin selectionPin;

    List<LightNode> lightNodes;
    private LightNode selectedLightNode;

    void Awake()
    {
        Assert.IsNotNull(environmentManager);
        Assert.IsNotNull(iesManager);
        Assert.IsNotNull(lightControl);
        Assert.IsNotNull(selectionPin);
        
        lightNodes = new List<LightNode>();
        selectedLightNode = null;
    }

    public void CreateLight(LightNode lightNode, Vector3 eulerAngles, string IESName)
    {
        if (lightNode.Name == "") {
            lightNode.Name = "LightInfraS_" + lightNodes.Count;
        }
        if (lightNode.PrefabName == "") {
            lightNode.PrefabName = GetLightPrefabNames()[0];
        }
        
        lightNode.Light = Instantiate(Resources.Load<GameObject>(LIGHTS_RESOURCES_FOLDER + "/" + lightNode.PrefabName)).GetComponent<LightPrefab>();
        lightNode.Light.Create(lightNode, transform, eulerAngles, environmentManager);

        IESLight IES = iesManager.GetIESLightFromName(IESName);
        lightNode.Light.SetIESLight(IES);

        lightNodes.Add(lightNode);
    }

    public void InsertLight()
    {
        LightNode lightNode = new LightNode();
        CreateLight(lightNode, new Vector3(0, 0), "");
        SelectLight(lightNode);
        MoveCurrentLight();
    }

    public void DeleteLight()
    {
        if (selectedLightNode != null)
        {
            selectedLightNode.Light.Destroy();
            lightNodes.Remove(selectedLightNode);
            ClearSelectedLight();
        }
    }

    public void ClearLights()
    {
        ClearSelectedLight();
        foreach (LightNode lightNode in lightNodes) {
            lightNode.Light.Destroy();
        }
        lightNodes.Clear();
    }

    public void ShowLights(bool show)
    {
        foreach (LightNode lightNode in lightNodes) {
            lightNode.Light.Show(show);
        }
    }

    public void ChangeLightType(string newLightType)
    {
        if (selectedLightNode != null && newLightType != selectedLightNode.PrefabName) {
            Vector3 eulerAngles = selectedLightNode.Light.GetTransform().eulerAngles;
            IESLight iesLight = selectedLightNode.Light.GetIESLight();
            selectedLightNode.Light.Destroy();

            selectedLightNode.PrefabName = newLightType;
            selectedLightNode.Light = Instantiate(Resources.Load<GameObject>(LIGHTS_RESOURCES_FOLDER + "/" + newLightType)).GetComponent<LightPrefab>();
            selectedLightNode.Light.Create(selectedLightNode, transform, eulerAngles, environmentManager);
            selectedLightNode.Light.SetIESLight(iesLight);
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

    public void UpdateLightsPositions()
    {
        for (int i = 0; i < lightNodes.Count; i++) {
            lightNodes[i].Light.SetPosition(environmentManager.GetUnityPositionFromCoordinatesAndAltitude(lightNodes[i].LatLong, lightNodes[i].Altitude, true));
            lightNodes[i].Light.MultiplyScale(environmentManager.GetWorldRelativeScale());
        }

        if (selectedLightNode != null) {
            selectionPin.SetPosition(selectedLightNode.Light.GetTransform().position);
            selectionPin.MultiplyScale(environmentManager.GetWorldRelativeScale());
        }
    }

    public void MoveLight()
    {
        if (selectedLightNode != null) {
            if (selectionPin.IsMoving()) {
                ClearSelectedLight();
            } else {
                MoveCurrentLight();
            }
        }
    }

    public void RotateSelectedLight(float rotation)
    {
        if (selectedLightNode != null) {
            selectedLightNode.Light.Rotate(rotation);
        }
    }

    public List<Feature> GetFeatures()
    {
        List<Feature> features = new List<Feature>();
        foreach (LightNode lightNode in lightNodes) {
            Feature feature = new Feature();
            Vector3 eulerAngles = lightNode.Light.GetTransform().eulerAngles;
            feature.Properties.Add("name", lightNode.Name);
            feature.Properties.Add("eulerAngles", new List<float>{eulerAngles.x, eulerAngles.y, eulerAngles.z});
            feature.Properties.Add("IESfileName", lightNode.Light.GetIESLight().Name);
            feature.Properties.Add("LightPrefabName", lightNode.PrefabName);
            feature.Coordinates = new Vector3d(lightNode.LatLong, lightNode.Altitude);
            features.Add(feature);
        }
        return features;
    }

    public void ChangeLightSource(string newIESName)
    {
        if (selectedLightNode != null && newIESName != selectedLightNode.Light.GetIESLight().Name) {
            IESLight newIES = iesManager.GetIESLightFromName(newIESName);

            if (newIES != null) {
                selectedLightNode.Light.SetIESLight(newIES);
            }
        }
    }

    public void SelectLight()
    {
        RaycastHit hitInfo = new RaycastHit();
        bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        if (!isOverUI && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 10000))
        {
            LightPrefab lightPrefab = hitInfo.transform.gameObject.GetComponent<LightPrefab>();
            if (lightPrefab != null) {
                foreach (LightNode lightNode in lightNodes) {
                    if (lightNode.Light == lightPrefab) {
                        selectedLightNode = lightNode;
                    }
                }
            }

            if (selectedLightNode != null) {
                SelectLight(selectedLightNode);
            }            
        }
    }

    public void ClearSelectedLight()
    {
        if (selectedLightNode != null)
        {
            selectedLightNode.Light.SetMoving(false);

            Vector3 lightPosition = selectedLightNode.Light.GetTransform().position;
            selectedLightNode.LatLong = environmentManager.GetCoordinatesFromUnityPosition(lightPosition);
            selectedLightNode.Altitude = environmentManager.GetAltitudeFromUnityPosition(lightPosition);

            selectedLightNode = null;
            selectionPin.SetActive(false);
        }
        lightControl.ClearSelectedLight();
    }

    private void SelectLight(LightNode lightNode)
    {
        ClearSelectedLight();
        selectedLightNode = lightNode;

        lightControl.LightSelected(selectedLightNode);
        
        selectionPin.SetActive(true);
        selectionPin.SetPosition(selectedLightNode.Light.GetTransform().position);
        selectionPin.MultiplyScale(environmentManager.GetWorldRelativeScale());
    }

    private void MoveCurrentLight()
    {
        selectedLightNode.Light.SetMoving(true);
        selectionPin.SetMoving(true);
    }
}