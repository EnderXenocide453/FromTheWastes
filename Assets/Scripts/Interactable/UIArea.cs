using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIArea : InteractableObject
{
    [SerializeField, TextArea] protected string UIName;
    [SerializeField] protected Transform UI;

    private bool _isActive;

    protected override string Description { get => $"Открыть {UIName}"; }

    private void Start()
    {
        GlobalValues.handler.onPauseKeyDown += DisableUI;
    }

    public override void StartInteract(Carrier carrier = null)
    {
        ToggleUI();
    }

    public override void StopInteract(Carrier carrier = null)
    {
        return;
    }

    protected override void OnPlayerExit()
    {
        DisableUI();
        base.OnPlayerExit();
    }

    private void ToggleUI()
    {
        _isActive = !_isActive;
        UI?.gameObject.SetActive(_isActive);
    }

    private void DisableUI()
    {
        _isActive = false;
        UI?.gameObject.SetActive(false);
    }
}
