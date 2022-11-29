using UnityEngine;

abstract public class TabControl: MonoBehaviour
{
    public abstract void OnTabOpened();
    public abstract void OnTabClosed();
}