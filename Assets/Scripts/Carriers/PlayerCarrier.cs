using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Carrier))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerCarrier : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float rotationSpeed = 10;

    private Quaternion _targetRotation;

    private Vector3 _moveDir;
    private Rigidbody _body;

    private GlobalValuesHandler _valuesHandler;
    private Carrier _carrier;

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        
        _carrier = GetComponent<Carrier>();

        _valuesHandler = GameObject.FindWithTag("Global")?.GetComponent<GlobalValuesHandler>();
        _valuesHandler.onInteractKeyDown += _carrier.StartInteract;
        _valuesHandler.onInteractKeyUp += _carrier.StopInteract;
    }

    void Update()
    {
        _moveDir = _valuesHandler.GetAxis().normalized;

        if (_moveDir.magnitude != 0)
            _targetRotation = Quaternion.LookRotation(_moveDir);
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
    }

    private void Move()
    {
        Vector3 velocity = new Vector3(_body.velocity.x, 0, _body.velocity.z);
        velocity = Vector3.MoveTowards(velocity, _moveDir * moveSpeed, acceleration);

        _body.velocity = new Vector3(velocity.x, _body.velocity.y, velocity.z);
    }

    private void Rotate()
    {
        //transform.Rotate(Vector3.MoveTowards(transform.rotation.eulerAngles, _targetRotation, rotationSpeed));
        _body.MoveRotation(Quaternion.RotateTowards(_body.rotation, _targetRotation, rotationSpeed));
    }
}
