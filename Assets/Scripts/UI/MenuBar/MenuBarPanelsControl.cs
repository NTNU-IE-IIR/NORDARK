using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class MenuBarPanelsControl : MonoBehaviour
{
    [SerializeField] private Transform buttonsHolder;
    [SerializeField] private Transform panelsParent;
    private List<Button> buttons;
    private List<GameObject> panels;

    void Awake()
    {
        Assert.IsNotNull(buttonsHolder);
        Assert.IsNotNull(panelsParent);
        
        buttons = new List<Button>();
        foreach (Transform child in buttonsHolder) {
            Button button = child.GetComponent<Button>();
            Assert.IsNotNull(button);
            buttons.Add(button);
        }

        panels = new List<GameObject>();
        foreach (Transform panel in panelsParent) {
            panels.Add(panel.gameObject);
        }
        Assert.IsTrue(buttons.Count == panels.Count);
    }

    void Start()
    {
        for (int i=0; i<buttons.Count; ++i) {
            Button button = buttons[i];
            GameObject panel = panels[i];

            button.onClick.AddListener(() => OpenPanel(button, panel));

            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => { 
                if (OnePanelOpened()) {
                    OpenPanel(button, panel);
                }
            });
            trigger.triggers.Add(entry);
        }
    }

    private void OpenPanel(Button button, GameObject panel) {
        ClearPanels();
        panel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    private bool OnePanelOpened()
    {
        return panels.Any(panel => panel.activeSelf);
    }

    private void ClearPanels()
    {
        foreach (GameObject panel in panels) {
            panel.SetActive(false);
        }
        EventSystem.current.SetSelectedGameObject(null);
    }
}
