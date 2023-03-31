using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private CamerasManager camerasManager;
    [SerializeField] private TMP_Dropdown cameras;
    [SerializeField] private Button addCamera;
    [SerializeField] private Button deleteCamera;
    [SerializeField] private Button updateCamera;
    [SerializeField] private Button moveToCamera;

    void Awake()
    {
        Assert.IsNotNull(camerasManager);
        Assert.IsNotNull(cameras);
        Assert.IsNotNull(addCamera);
        Assert.IsNotNull(deleteCamera);
        Assert.IsNotNull(updateCamera);
        Assert.IsNotNull(moveToCamera);
    }
    
    void Start()
    {
        cameras.onValueChanged.AddListener(change =>  camerasManager.ChangeCurrentCamera(cameras.value));
        addCamera.onClick.AddListener(delegate {
            camerasManager.CreateCamera();
            EventSystem.current.SetSelectedGameObject(null);
        });
        deleteCamera.onClick.AddListener(delegate {
            camerasManager.DeleteCamera();
            EventSystem.current.SetSelectedGameObject(null);
        });
        updateCamera.onClick.AddListener(delegate {
            camerasManager.SetCurrentCameraPositionFromMainCamera();
            EventSystem.current.SetSelectedGameObject(null);
        });
        moveToCamera.onClick.AddListener(delegate {
            camerasManager.SetMainCameraPositionFromCurrentCamera();
            EventSystem.current.SetSelectedGameObject(null);
        });
    }

    public void AddCameraToList(string cameraName)
    {
        cameras.AddOptions(new List<string> { cameraName });
    }

    public void RemoveCamera()
    {
        cameras.options.RemoveAt(cameras.value);
        if (cameras.options.Count < 1) {
            cameras.captionText.text = "";
        }
    }

    public void ClearCameras()
    {
        cameras.ClearOptions();
    }

    public void CameraChanged(int cameraIndex)
    {
        cameras.SetValueWithoutNotify(cameraIndex);
        cameras.RefreshShownValue();    // needed to update the selected value of the dropdown
    }
}