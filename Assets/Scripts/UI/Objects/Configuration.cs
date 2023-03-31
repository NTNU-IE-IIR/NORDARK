using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;

public class Configuration : MonoBehaviour
{
    [SerializeField] private TMP_Text fileName;
    [SerializeField] private Button browse;
    private RectTransform rectTransform;

    void Awake()
    {
        Assert.IsNotNull(fileName);
        
        // When the location is changed, Create() might be called before Awake() and so browse may be destroyed
        // So we don't check if it's null
    }
    
    public void Create(string name, System.Action onBrowse, bool browsable = true)
    {
        rectTransform = GetComponent<RectTransform>();
        fileName.text = name;
        browse.onClick.AddListener(() => {
            onBrowse();
        });
        if (!browsable) {
            Destroy(browse.gameObject);
        }
    }

    public float GetHeight()
    {
        return rectTransform.sizeDelta.y;
    }

    public void SetName(string name)
    {
        fileName.text = name;
    }
}