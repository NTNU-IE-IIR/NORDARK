using UnityEngine;
using UnityEngine.Assertions;

public class MinimapUI : MonoBehaviour
{
    [SerializeField] private Minimap minimap;

    void Awake()
    {
        Assert.IsNotNull(minimap);
    }

    public void OnClicked()
    {
        minimap.ChangeViewType();
    }
}
