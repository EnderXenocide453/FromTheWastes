using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EmployeeCarrier : Carrier
{
    [SerializeField] private Transform targetA, targetB;

    private bool _stop;

    private void Start()
    {
        onEnterInteractable += StartWork;

        storage.onStorageFilled += (object obj) => FinishWork();
        storage.onStorageEmptied += (object obj) => FinishWork();
        storage.onSendEnds += (object obj) => FinishWork();
    }

    protected override void GetDirection()
    {
        moveDir = _stop ? Vector3.zero : (targetA.position - transform.position).normalized;
    }

    private void StartWork()
    {
        _stop = true;

        StartInteract();
    }

    private void FinishWork()
    {
        _stop = false;
        (targetA, targetB) = (targetB, targetA);

        StopInteract();
    }
}
