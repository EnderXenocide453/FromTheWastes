using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalKeyHandler : MonoBehaviour
{
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
}
