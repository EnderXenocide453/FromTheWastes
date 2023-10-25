using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeArea : UIArea
{
    [SerializeField] public Upgrader upgrader;

    private void Awake()
    {
        UI = GlobalValues.handler.upgradePanel;
    }

    public override void StartInteract(Carrier carrier = null)
    {
        base.StartInteract(carrier);
        GlobalValues.handler.upgradeUI.ClearUI();
        GlobalValues.handler.upgradeUI.AddUpgrades(upgrader.CurrentUpgrades);
    }
}