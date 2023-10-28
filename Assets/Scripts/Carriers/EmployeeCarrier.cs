using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EmployeeCarrier : Carrier
{
    [SerializeField] private Transform area;
    [SerializeField] private Vector3 target;

    private Vector3 _targetA, _targetB;

    private bool _stop;

    private void Start()
    {
        _targetA = target;
        _targetB = area.position;

        onEnterInteractable += StartWork;

        storage.onStorageFilled += (object obj) => FinishWork();
        storage.onStorageEmptied += (object obj) => FinishWork();
    }

    protected override void GetDirection()
    {
        moveDir = _stop ? Vector3.zero : (_targetA - transform.position).normalized;
    }

    private void StartWork()
    {
        _stop = true;

        StartInteract();
    }

    private void FinishWork()
    {
        _stop = false;
        (_targetA, _targetB) = (_targetB, _targetA);

        StopInteract();
    }
}
