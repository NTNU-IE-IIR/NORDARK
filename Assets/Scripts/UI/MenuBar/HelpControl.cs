public class HelpControl : MenuBarItemControl
{
    void Awake()
    {
        base.SetUp();
    }

    void Update()
    {
        base.DeactivateIfCursorOutside();
    }
}
