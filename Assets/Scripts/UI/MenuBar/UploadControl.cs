using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class UploadControl : MonoBehaviour
{
    [SerializeField]
    private IESManager iesManager;
    [SerializeField]
    private Button iesFile;
    private Rect rectangle;

    void Awake()
    {
        Assert.IsNotNull(iesManager);
        Assert.IsNotNull(iesFile);

        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        rectangle = new Rect(corners[0], corners[2]-corners[0]);
    }
    
    void Start()
    {
        iesFile.onClick.AddListener(delegate {
            iesManager.Upload();
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
