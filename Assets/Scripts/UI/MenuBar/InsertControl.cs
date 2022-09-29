using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class InsertControl : MonoBehaviour
{
    [SerializeField]
    private SceneManager sceneManager;
    [SerializeField]
    private Button lights;
    private Rect rectangle;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(lights);

        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        rectangle = new Rect(corners[0], corners[2]-corners[0]);
    }
    
    void Start()
    {
        lights.onClick.AddListener(delegate {
            sceneManager.AddLights();
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
