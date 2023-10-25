using UnityEngine;

public class GlobalValuesHandler : MonoBehaviour
{
    public const string HorAxis = "Horizontal";
    public const string VertAxis = "Vertical";

    public KeyCode interactKey = KeyCode.E;

    private int _cash;

    [SerializeField] private TMPro.TMP_Text cashCounter;

    public Transform upgradePanel;
    public UpgradeUI upgradeUI;

    public delegate void KeyHandler();

    public event KeyHandler onInteractKeyDown;

    public event KeyHandler onInteractKeyUp;

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
}