using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ViewControl : MonoBehaviour
{
    [SerializeField]
    private Button fullscreen;
    [SerializeField]
    private Button windowed;
    private Rect rectangle;

    void Awake()
    {
        Assert.IsNotNull(fullscreen);
        Assert.IsNotNull(windowed);

        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        rectangle = new Rect(corners[0], corners[2]-corners[0]);
    }
    
    void Start()
    {
        fullscreen.onClick.AddListener(delegate {
            Screen.fullScreen = true;
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            gameObject.SetActive(false);
        });
        windowed.onClick.AddListener(delegate {
            Resolution currentResolution = Screen.currentResolution;
            Screen.SetResolution(currentResolution.width, currentResolution.height, false);
            gameObject.SetActive(false);
        });
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) {
            if (!rectangle.Contains(Input.mousePosition)) {
                gameObject.SetActive(false);
            }
        }
    }
}
