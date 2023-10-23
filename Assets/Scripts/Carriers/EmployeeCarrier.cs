using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EmployeeCarrier : Carrier
{
    [SerializeField] private ResourceArea areaA, areaB;

    private bool _stop;

    private void Start()
    {
        onEnterInteractable += StartWork;

        storage.onStorageFilled += FinishWork;
        storage.onStorageEmptied += FinishWork;
    }

    protected override void GetDirection()
    {
        moveDir = _stop ? Vector3.zero : (areaA.transform.position - transform.position).normalized;
    }

    private void StartWork()
    {
        _stop = true;

        StartInteract();
    }

    private void FinishWork(object obj)
    {
        _stop = false;
        (areaA, areaB) = (areaB, areaA);

        StopInteract();
    }
}
