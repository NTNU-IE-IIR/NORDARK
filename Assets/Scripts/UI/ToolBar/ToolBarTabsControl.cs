using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ToolBarTabsControl : MonoBehaviour
{
    [SerializeField] private Transform tabsParent;
    private List<Button> buttons;
    private List<TabControl> tabs;

    void Awake()
    {
        Assert.IsNotNull(tabsParent);

        buttons = new List<Button>();
        foreach (Transform child in transform) {
            Button button = child.GetComponent<Button>();
            Assert.IsNotNull(button);
            buttons.Add(button);
        }

        tabs = new List<TabControl>();
        foreach (Transform child in tabsParent) {
            TabControl tab = child.GetComponent<TabControl>();
            Assert.IsNotNull(tab);
            tabs.Add(tab);
        }

        Assert.IsTrue(buttons.Count == tabs.Count);
    }

    void Start()
    {
        for (int i=0; i<buttons.Count; ++i) {
            Button button = buttons[i];
            TabControl tab = tabs[i];

            button.onClick.AddListener(() => ActivateTab(button, tab));
        }
    }

    public void ActivateDefaultTab()
    {
        if (buttons.Count > 0) {
            ActivateTab(buttons[0], tabs[0]);
        }
    }

    private void ActivateTab(Button button, TabControl tab)
    {
        Clear();

        tab.gameObject.SetActive(true);
        tab.OnTabOpened();

        ColorBlock cb = button.colors;
        cb.normalColor = Color.black;
        button.colors = cb;
    }

    private void Clear()
    {
        for (int i=0; i<tabs.Count; ++i) {
            tabs[i].OnTabClosed();
            tabs[i].gameObject.SetActive(false);

            ColorBlock colorBlock = buttons[i].colors;
            colorBlock.normalColor = Color.clear;
            buttons[i].colors = colorBlock;
        }
    }
}