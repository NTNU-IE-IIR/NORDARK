using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    private CamerasManager camerasManager;
    [SerializeField]
    private Dropdown cameras;
    [SerializeField]
    private Button resetCamera;
    [SerializeField]
    private Button updateCamera;
    [SerializeField]
    private Button addCamera;
    [SerializeField]
    private Button deleteCamera;

    void Awake()
    {
        Assert.IsNotNull(camerasManager);
        Assert.IsNotNull(cameras);
        Assert.IsNotNull(resetCamera);
        Assert.IsNotNull(updateCamera);
        Assert.IsNotNull(addCamera);
        Assert.IsNotNull(deleteCamera);
    }
    
    void Start()
    {
        cameras.onValueChanged.AddListener(change =>  camerasManager.ChangeCurrentCamera(cameras.value));
        resetCamera.onClick.AddListener(ResetMainCameraPosition);
        updateCamera.onClick.AddListener(UpdateCameraPosition);
        addCamera.onClick.AddListener(AddCamera);
        deleteCamera.onClick.AddListener(DeleteCamera);
    }

    public void AddCameraToList(string cameraName)
    {
        cameras.AddOptions(new List<string> { cameraName });
    }

    public void ClearCameras()
    {
        cameras.ClearOptions();
    }

    public void CameraChanged(int cameraIndex)
    {
        cameras.value = cameraIndex;
    }

    private void ResetMainCameraPosition()
    {
        camerasManager.ResetMainCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        camerasManager.UpdateCameraPosition();
    }

    private void AddCamera()
    {
        camerasManager.AddCamera(DetermineNewCameraName(1));
    }

    private void DeleteCamera()
    {
        if (cameras.options.Count > 1) {
            int cameraIndex = cameras.value;
            cameras.options.RemoveAt(cameras.value);
            camerasManager.DeleteCamera(cameraIndex);
        }
    }

    private string DetermineNewCameraName(int index)
    {
        foreach (Dropdown.OptionData option in cameras.options) {
            if (option.text == "New Camera " + index.ToString()) {
                return DetermineNewCameraName(index + 1);
            }
        }
        return "New Camera " + index.ToString();
    }
}