using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class VegetationObjectsControl : MonoBehaviour
{
    [SerializeField] private VegetationObjectsManager vegetationObjectsManager;
    [SerializeField] private Slider rotation;
    [SerializeField] private TMP_Text rotationValue;
    [SerializeField] private TMP_Dropdown object3DModel;
    [SerializeField] private Button add;
    [SerializeField] private Button move;
    [SerializeField] private Button delete;
    [SerializeField] private Toggle display;

    void Awake()
    {
        Assert.IsNotNull(vegetationObjectsManager);
        Assert.IsNotNull(rotation);
        Assert.IsNotNull(rotationValue);
        Assert.IsNotNull(object3DModel);
        Assert.IsNotNull(add);
        Assert.IsNotNull(move);
        Assert.IsNotNull(delete);
        Assert.IsNotNull(display);
    }

    void Start()
    {
        rotation.onValueChanged.AddListener(value => {
            vegetationObjectsManager.RotateSelectedObjects(value);
            SetRotation(value);
        });

        object3DModel.AddOptions(vegetationObjectsManager.GetObjectPrefabNames());
        object3DModel.onValueChanged.AddListener(value => {
            vegetationObjectsManager.Change3DModelOfSelectedObjects(object3DModel.options[value].text);
        });

        add.onClick.AddListener(vegetationObjectsManager.AddObject);
        move.onClick.AddListener(vegetationObjectsManager.MoveSelectedObjects);
        delete.onClick.AddListener(() => {
            vegetationObjectsManager.DeleteSelectedObjects();
            EventSystem.current.SetSelectedGameObject(null);
        });

        display.onValueChanged.AddListener(vegetationObjectsManager.ShowObjects);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            vegetationObjectsManager.StopObjectsMovement();
            vegetationObjectsManager.SelectObjectPointedByCursor(Input.GetKey(KeyCode.LeftControl));
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            vegetationObjectsManager.ClearSelectedObjects();
        }
        if (Input.GetKeyDown(KeyCode.I)) {
            vegetationObjectsManager.AddObject();
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            vegetationObjectsManager.MoveSelectedObjects();
        }
        if (Input.GetKeyDown(KeyCode.Delete)) {
            vegetationObjectsManager.DeleteSelectedObjects();
        }
    }

    public void ClearSelectedObjects()
    {
        rotation.SetValueWithoutNotify(0);
        rotationValue.text = "";
    }

    public void ObjectSelected(VegetationObject vegetationObject, bool multipleSelectedObjects)
    {
        float rotationSelected = System.Math.Max(0, vegetationObject.GameObject.transform.eulerAngles.y);
        int prefabIndex = object3DModel.options.FindIndex(i => i.text.Equals(vegetationObject.PrefabName));
        
        rotation.SetValueWithoutNotify(rotationSelected);
        SetRotation(rotationSelected);

        if (prefabIndex >= 0) {
            object3DModel.SetValueWithoutNotify(prefabIndex);
        }
    }

    private void SetRotation(float rotation)
    {
        rotationValue.text = rotation.ToString("0.00") + "°";
    }
}
