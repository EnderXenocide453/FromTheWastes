using UnityEngine;

public class GlobalValuesHandler : MonoBehaviour
{
    public const string HorAxis = "Horizontal";
    public const string VertAxis = "Vertical";

    public KeyCode interactKey = KeyCode.E;

    public Transform upgradePanel;
    public UpgradeUI upgradeUI;

    public AlertPanel alertPanel;

    public delegate void KeyHandler();

    public event KeyHandler onInteractKeyDown;
    public event KeyHandler onInteractKeyUp;

    [SerializeField] private TMPro.TMP_Text cashCounter;

    private int _cash;

    private void Awake()
    {
        GlobalValues.handler = this;
    }

    public int Cash
    {
        get => _cash;
        set
        {
            if (value < 0)
                value = 0;

            _cash = value;
            cashCounter.text = $"{_cash}";
        }
    }
    public Vector3 GetAxis() => new Vector3(Input.GetAxis(HorAxis), 0, Input.GetAxis(VertAxis));

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
            onInteractKeyDown?.Invoke();
        else if (Input.GetKeyUp(interactKey))
            onInteractKeyUp?.Invoke();
    }
}

public static class GlobalValues
{
    public static GlobalValuesHandler handler;

    public static int Cash
    {
        get => handler.Cash;
        set => handler.Cash = value;
    }

    public static void Alert(string text, Color color) => handler?.alertPanel.Alert(text, color);

}