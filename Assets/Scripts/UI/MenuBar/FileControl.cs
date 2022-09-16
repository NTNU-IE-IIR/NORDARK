using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class FileControl : MonoBehaviour
{
    [SerializeField]
    private SceneManager sceneManager;
    [SerializeField]
    private Button open;
    [SerializeField]
    private Button save;
    [SerializeField]
    private Button saveAs;
    [SerializeField]
    private Button exit;
    private Rect rectangle;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(open);
        Assert.IsNotNull(save);
        Assert.IsNotNull(saveAs);
        Assert.IsNotNull(exit);

        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        rectangle = new Rect(corners[0], corners[2]-corners[0]);
    }
    
    void Start()
    {
        open.onClick.AddListener(delegate {
            sceneManager.Load();
            gameObject.SetActive(false);
        });
        save.onClick.AddListener(delegate {
            sceneManager.Save();
            gameObject.SetActive(false);
        });
        saveAs.onClick.AddListener(delegate {
            sceneManager.SaveAs();
            gameObject.SetActive(false);
        });
        exit.onClick.AddListener(delegate {
            Application.Quit();
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
