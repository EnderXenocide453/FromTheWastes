using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Storage))]
public abstract class Carrier : MonoBehaviour
{
    public Storage storage { get; private set; }
    public float workSpeed = 5;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float rotationSpeed = 10;

    protected Quaternion targetRotation;
    protected Vector3 moveDir;
    protected GlobalValuesHandler valuesHandler;

    private Rigidbody _body;
    private Dictionary<int, InteractableObject> _nearInteractables;

    public delegate void CarrierHandler();
    public event CarrierHandler onEnterInteractable;
    public event CarrierHandler onExitInteractable;
    public event CarrierHandler onExitAllInteractables;

    private void Awake()
    {
        storage = GetComponent<Storage>();
        valuesHandler = GameObject.FindWithTag("Global")?.GetComponent<GlobalValuesHandler>();
        _body = GetComponent<Rigidbody>();

        _nearInteractables = new Dictionary<int, InteractableObject>();
    }

    private void Update()
    {
        GetDirection();

        if (moveDir.magnitude != 0)
            targetRotation = Quaternion.LookRotation(_body.velocity);
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.attachedRigidbody)
            return;

        if (((1 << other.gameObject.layer) & interactableMask.value) > 0) {
            if (!other.attachedRigidbody.TryGetComponent<InteractableObject>(out var obj)) return;

            AddInteractable(obj);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.attachedRigidbody)
            return;

        RemoveInteractable(other.attachedRigidbody.gameObject.GetInstanceID());
    }

    protected void StartInteract()
    {
        if (_nearInteractables.Count == 0) return;

        foreach (var id in _nearInteractables.Keys) {
            InteractWith(id);
        }
    }

    protected void StopInteract()
    {
        if (_nearInteractables.Count == 0) return;

        foreach (var id in _nearInteractables.Keys) {
            StopInteractWith(id);
        }
    }

    protected abstract void GetDirection();

    private void Move()
    {
        Vector3 velocity = new Vector3(_body.velocity.x, 0, _body.velocity.z);
        velocity = Vector3.MoveTowards(velocity, moveDir * moveSpeed, acceleration);

        _body.velocity = new Vector3(velocity.x, _body.velocity.y, velocity.z);
    }

    private void Rotate()
    {
        //transform.Rotate(Vector3.MoveTowards(transform.rotation.eulerAngles, _targetRotation, rotationSpeed));
        _body.MoveRotation(Quaternion.RotateTowards(_body.rotation, targetRotation, rotationSpeed));
    }

    private void AddInteractable(InteractableObject obj)
    {
        _nearInteractables.TryAdd(obj.gameObject.GetInstanceID(), obj);

        onEnterInteractable?.Invoke();
    }

    private void RemoveInteractable(int id)
    {
        if (!_nearInteractables.ContainsKey(id)) return;

        StopInteractWith(id);
        _nearInteractables.Remove(id);

        onExitInteractable?.Invoke();
        if (_nearInteractables.Count == 0) 
            onExitAllInteractables?.Invoke();
    }

    private void InteractWith(int id)
    {
        _nearInteractables[id].StartInteract(this);
    }

    private void StopInteractWith(int id)
    {
        _nearInteractables[id].StopInteract(this);
    }
}
