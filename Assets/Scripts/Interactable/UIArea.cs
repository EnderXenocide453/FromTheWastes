using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIArea : InteractableObject
{
    [SerializeField] Transform UI;

    private bool _isActive;

    public override void StartInteract(Carrier carrier = null)
    {
        ToggleUI();
    }

    public override void StopInteract(Carrier carrier = null)
    {
        return;
    }

    private void ToggleUI()
    {
        _isActive = !_isActive;
        UI?.gameObject.SetActive(_isActive);
    }
}
