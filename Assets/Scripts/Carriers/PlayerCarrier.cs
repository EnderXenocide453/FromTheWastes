using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCarrier : Carrier
{
    void Start()
    {
        valuesHandler.onInteractKeyDown += StartInteract;
        valuesHandler.onInteractKeyUp += StopInteract;
    }

    protected override void GetDirection()
    {
        moveDir = valuesHandler.GetAxis().normalized;
    }
}
