using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCarrier : Carrier
{
    void Start()
    {
        GlobalValues.handler.onInteractKeyDown += StartInteract;
        GlobalValues.handler.onInteractKeyUp += StopInteract;
    }

    protected override void GetDirection()
    {
        moveDir = GlobalValues.handler.GetAxis().normalized;
    }
}
