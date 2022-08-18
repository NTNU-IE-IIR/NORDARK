/*
https://assetstore.unity.com/packages/tools/utilities/photorealistic-lights-ies-59641#description
https://unity.com/how-to/getting-started-high-definition-render-pipeline-hdrp-games
https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@15.0/manual/index.html
*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.HighDefinition;
using GeoJSON.Net.Feature;

public class LightsManager : MonoBehaviour
{
    private const string LIGHTS_RESOURCES_FOLDER = "lights";
    private const string IES_RESOURCES_FOLDER = "IES";

    [SerializeField]
    private MapManager mapManager;
    [SerializeField]
    private LightControl lightControl;
    [SerializeField]
    private GameObject selectionPin;

    private List<IESLight> IESLights;
    List<LightNode> lightNodes;
    private LightNode selectedLightNode;
    private bool isMoving;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(lightControl);
        Assert.IsNotNull(selectionPin);

        IESLights = new List<IESLight>();
        Object[] IES = Resources.LoadAll(IES_RESOURCES_FOLDER);
        foreach (Object ies in IES) {
            if (ies.GetType().ToString() == "UnityEditor.Rendering.IESObject") {
                IESLights.Add(new IESLight(ies.name, GetIESCookie(ies.name)));
            }
        }
        
        lightNodes = new List<LightNode>();
        selectedLightNode = null;
        isMoving = false;
    }

    void Update()
    {
        if (selectedLightNode != null && isMoving)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, 10000, 1 << MapManager.UNITY_LAYER_MAP))
            {
                selectedLightNode.Light.transform.position = hit.point;
                selectionPin.transform.position = hit.point;
            }
        }
    }

    public void CreateLight(Feature feature)
    {
        GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;
        var coordinates = point.Coordinates;
        
        Vector2 latLong = new Vector2((float)(coordinates.Latitude), (float)(coordinates.Longitude));
        Vector3 position = mapManager.GetUnityPositionFromCoordinates(latLong);
        position.y = mapManager.GetElevationInUnityUnitsFromCoordinates(new Mapbox.Utils.Vector2d(latLong.x, latLong.y));

        string lightPrefabName;
        if (feature.Properties.ContainsKey("LightPrefabName")) {
            lightPrefabName = feature.Properties["LightPrefabName"] as string;
        } else {
            lightPrefabName = GetLightPrefabNames()[0];
        }

        Vector3 eulerAngles = new Vector3(0, 0);
        try
        {
            List<float> eulerAnglesList = (feature.Properties["eulerAngles"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();
            eulerAngles = new Vector3(eulerAnglesList[0], eulerAnglesList[1], eulerAnglesList[2]);
        }
        catch (System.Exception)
        {}

        GameObject lightObject = Instantiate(Resources.Load(LIGHTS_RESOURCES_FOLDER + "/" + lightPrefabName) as GameObject);
        lightObject.transform.name = "LightInfraS_" + lightNodes.Count;
        lightObject.transform.parent = transform;
        lightObject.transform.position = position;
        lightObject.transform.localScale = new Vector3(1, 1, 1);
        lightObject.transform.eulerAngles = eulerAngles;

        IESLight IES = null;
        if (feature.Properties.ContainsKey("IESfileName")) {
            string IESName = feature.Properties["IESfileName"] as string;
            IES = FindIESLightFromName(IESName);
        }
        if (IES == null) {
            IES = IESLights[0];
        }

        GameObject light = lightObject.transform.Find("Spot Light").gameObject;
        light.GetComponent<HDAdditionalLightData>().SetCookie(IES.Cookie);

        LightNode lightNode = new LightNode();
        lightNode.LatLong = latLong;
        lightNode.Light = lightObject;
        lightNode.IESLight = IES;
        lightNode.PrefabName = lightPrefabName;
        lightNodes.Add(lightNode);
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

    public void ChangeLightType(string newLightType)
    {
        if (selectedLightNode != null && newLightType != selectedLightNode.PrefabName)
        {
            GameObject lightObject = Instantiate(Resources.Load(LIGHTS_RESOURCES_FOLDER + "/" + newLightType) as GameObject);
            lightObject.transform.name = selectedLightNode.Light.transform.name;
            lightObject.transform.parent = selectedLightNode.Light.transform.parent;
            lightObject.transform.position = selectedLightNode.Light.transform.position;
            lightObject.transform.eulerAngles = selectedLightNode.Light.transform.eulerAngles;
            
            GameObject light = lightObject.transform.Find("Spot Light").gameObject;
            light.GetComponent<HDAdditionalLightData>().SetCookie(selectedLightNode.IESLight.Cookie);

            Destroy(selectedLightNode.Light);
            selectedLightNode.Light = lightObject;
            selectedLightNode.PrefabName = newLightType;
        }
    }

    public List<string> GetIESNames()
    {
        List<string> IESNames = new List<string>();
        foreach (IESLight IESLight in IESLights) {
            IESNames.Add(IESLight.Name);
        }
        return IESNames;
    }

    public void SetIESToAllLights(string IESName)
    {
        IESLight IES = FindIESLightFromName(IESName);
        if (IES != null) {
            for (int i = 0; i < lightNodes.Count; i++) {
                GameObject light = lightNodes[i].Light.transform.Find("Spot Light").gameObject;
                light.GetComponent<HDAdditionalLightData>().SetCookie(IES.Cookie);
            }
        }
    }

    public void UpdateLightsPositions()
    {
        for (int i = 0; i < lightNodes.Count; i++) {
            Vector2 latLong = lightNodes[i].LatLong;
            Vector3 position = mapManager.GetUnityPositionFromCoordinates(latLong);
            position.y = mapManager.GetElevationInUnityUnitsFromCoordinates(new Mapbox.Utils.Vector2d(latLong.x, latLong.y));

            lightNodes[i].Light.transform.position = position;
        }

        if (selectedLightNode != null)
        {
            selectionPin.transform.position = selectedLightNode.Light.transform.position;
        }
    }

    public void ClearLights()
    {
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }

        lightNodes.Clear();
    }

    public List<(Mapbox.Utils.Vector2d, Dictionary<string, object>)> GetLightFeatures()
    {
        List<(Mapbox.Utils.Vector2d, Dictionary<string, object>)> features = new List<(Mapbox.Utils.Vector2d, Dictionary<string, object>)>();
        foreach (LightNode lightNode in lightNodes) {
            Mapbox.Utils.Vector2d geopos = mapManager.GetCoordinatesFromUnityPosition(lightNode.Light.transform.position);
            lightNode.LatLong = new Vector2((float)geopos.x, (float)geopos.y);
            
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("name", lightNode.Name);
            properties.Add("eulerAngles", new List<float>{lightNode.Light.transform.eulerAngles.x, lightNode.Light.transform.eulerAngles.y, lightNode.Light.transform.eulerAngles.z});
            properties.Add("IESfileName", lightNode.IESLight.Name);
            properties.Add("LightPrefabName", lightNode.PrefabName);

            features.Add((geopos, properties));
        }
        return features;
    }

    public void RotateSelectedLight(float rotation)
    {
        if (selectedLightNode != null) { 
            Vector3 angles = selectedLightNode.Light.transform.eulerAngles;
            angles.y = rotation;
            selectedLightNode.Light.transform.eulerAngles = angles;
        }
    }

    public void ChangeLightSource(string newIESName)
    {
        if (selectedLightNode != null && newIESName != selectedLightNode.IESLight.Name)
        {
            IESLight newIES = FindIESLightFromName(newIESName);

            if (newIES != null) {
                GameObject light = selectedLightNode.Light.transform.Find("Spot Light").gameObject;
                light.GetComponent<HDAdditionalLightData>().SetCookie(newIES.Cookie);
                selectedLightNode.IESLight = newIES;
            }
        }
    }

    public void MoveLight()
    {
        if (selectedLightNode != null) {
            if (isMoving)
            {
                ClearSelectedLight();
            } else {
                isMoving = true;
            }
        } else {
            isMoving = false;
        }
    }

    public void DeleteLight()
    {
        if (selectedLightNode != null)
        {
            Destroy(selectedLightNode.Light);
            lightNodes.Remove(selectedLightNode);
            ClearSelectedLight();
        }
    }

    public void InsertLight()
    {
        GameObject cloneObj;
        if (selectedLightNode != null)
        {
            cloneObj = Instantiate(selectedLightNode.Light) as GameObject;
            cloneObj.name = "LightInfraS_" + lightNodes.Count;
            cloneObj.transform.parent = transform;

            LightNode lightNode = new LightNode();
            lightNode.LatLong = lightNodes[lightNodes.Count - 1].LatLong;
            lightNode.Light = cloneObj;
            lightNode.IESLight = lightNodes[lightNodes.Count - 1].IESLight;
            lightNodes.Add(lightNode);

            ClearSelectedLight();
            selectedLightNode = lightNode;
            selectionPin.SetActive(true);
            isMoving = true;
        }
    }

    public void SelectLight()
    {
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 10000))
        {
            GameObject selectedLight = GameObject.Find(hitInfo.collider.name);
            foreach (LightNode lightNode in lightNodes) {
                if (lightNode.Light == selectedLight) {
                    selectedLightNode = lightNode;
                }
            }

            if (selectedLightNode != null) {
                lightControl.LightSelected(selectedLightNode);
                
                selectionPin.SetActive(true);
                float scale = mapManager.GetWorldRelativeScale() * 4;
                selectionPin.transform.localScale = new Vector3(scale, scale, scale);
                selectionPin.transform.position = selectedLightNode.Light.transform.position;
            }            
        }

        if (isMoving)
        {
            ClearSelectedLight();
        }
    }

    public void ClearSelectedLight()
    {
        if (selectedLightNode != null)
        {
            selectedLightNode = null;
            selectionPin.SetActive(false);
        }
        isMoving = false;
        lightControl.ClearSelectedLight();
    }

    private UnityEngine.Texture GetIESCookie(string filename)
    {
        
        UnityEditor.Rendering.IESEngine engine = new UnityEditor.Rendering.IESEngine();
        UnityEditor.Rendering.IESMetaData iesMetaData = new UnityEditor.Rendering.IESMetaData();
        engine.TextureGenerationType = UnityEditor.TextureImporterType.Default;
        
        /*
        IESEngine engine = new IESEngine();
        IESMetaData iesMetaData = new IESMetaData();
        engine.TextureGenerationType = TextureImporterType.Default;
        */

        UnityEngine.Texture cookieTexture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);

        string path = @"D:\Nordark\Project\NORDARK\Assets\Resources\IES\";
        string iesFilePath = path + filename + ".ies";
        string errorMessage = engine.ReadFile(iesFilePath);

        if (string.IsNullOrEmpty(errorMessage))
        {
            iesMetaData.FileFormatVersion = "LM-63-2002";
            iesMetaData.IESPhotometricType = engine.GetPhotometricType();
            iesMetaData.Manufacturer = engine.GetKeywordValue("MANUFAC");
            iesMetaData.LuminaireCatalogNumber = engine.GetKeywordValue("LUMCAT");
            iesMetaData.LuminaireDescription = engine.GetKeywordValue("LUMINAIRE");
            iesMetaData.LampCatalogNumber = engine.GetKeywordValue("LAMPCAT");
            iesMetaData.LampDescription = engine.GetKeywordValue("LAMP");

            (iesMetaData.IESMaximumIntensity, iesMetaData.IESMaximumIntensityUnit) = engine.GetMaximumIntensity();

            string warningMessage;

            (warningMessage, cookieTexture2D) = engine.Generate2DCookie(iesMetaData.CookieCompression, iesMetaData.SpotAngle, (int)iesMetaData.iesSize, iesMetaData.ApplyLightAttenuation);
        }
        else
        {
            Debug.Log($"Cannot read IES file '{iesFilePath}': {errorMessage}");
        }

        return cookieTexture2D;
    }

    private IESLight FindIESLightFromName(string IESName)
    {
        IESLight IES = null;
        foreach (IESLight ies in IESLights) {
            if (ies.Name == IESName) {
                IES = ies;
            }
        }
        return IES;
    }
}
