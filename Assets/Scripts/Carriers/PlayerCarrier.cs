using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCarrier : Carrier
{
    public PlayerUpgrader upgrader;

    [SerializeField] private UpgradeArea area;

    void Start()
    {
        GlobalValues.handler.onInteractKeyDown += StartInteract;
        GlobalValues.handler.onInteractKeyUp += StopInteract;

        upgrader.Init();
        upgrader.onUpgraded += Upgrade;
        area.upgrader = upgrader;
    }

    protected override void GetDirection()
    {
        moveDir = GlobalValues.handler.GetAxis().normalized;
    }

    private void Upgrade()
    {
        SetCapacityModifier(upgrader.PlayerUpgrade.StorageCapacityMultiplier);
        SetWorkSpeedModifier(upgrader.PlayerUpgrade.WorkSpeedMultiplier);
    }
}
