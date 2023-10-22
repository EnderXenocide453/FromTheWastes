using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalValuesHandler : MonoBehaviour
{
    public const string HorAxis = "Horizontal";
    public const string VertAxis = "Vertical";

    public KeyCode interactKey = KeyCode.E;

    public delegate void KeyHandler();
    public event KeyHandler onInteractKeyDown;
    public event KeyHandler onInteractKeyUp;

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
            onInteractKeyDown?.Invoke();
        else if (Input.GetKeyUp(interactKey))
            onInteractKeyUp?.Invoke();
    }

    public Vector3 GetAxis() => new Vector3(Input.GetAxis(HorAxis), 0, Input.GetAxis(VertAxis));
}
