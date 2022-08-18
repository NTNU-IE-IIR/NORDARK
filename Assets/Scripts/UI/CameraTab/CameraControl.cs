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
        cameras.onValueChanged.AddListener(change =>  camerasManager.CameraChanged(cameras.value));
        resetCamera.onClick.AddListener(ResetCameraPosition);
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

    public int GetCameraIndex()
    {
        if (cameras.options.Count > 0) {
            return cameras.value;
        } else {
            return -1;
        }
    }

    public void CameraChanged(int cameraIndex)
    {
        cameras.value = cameraIndex;
    }

    private void ResetCameraPosition()
    {
        camerasManager.ResetCameraPosition(cameras.value);
    }

    private void UpdateCameraPosition()
    {
        camerasManager.UpdateCameraPosition(cameras.value);
    }

    private void AddCamera()
    {
        camerasManager.AddCamera(DetermineNewCameraName(1));
        cameras.value = cameras.options.Count - 1;
        camerasManager.CameraChanged(cameras.value);
    }

    private void DeleteCamera()
    {
        if (cameras.options.Count > 0) {
            string cameraName = cameras.options[cameras.value].text;

            cameras.options.RemoveAt(cameras.value);
            cameras.value = System.Math.Max(0, cameras.value-1);
            if (cameras.options.Count > 0) {
                cameras.captionText.text = cameras.options[cameras.value].text;
            } else {
                cameras.captionText.text = "";
            }

            camerasManager.DeleteCamera(cameraName);
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
