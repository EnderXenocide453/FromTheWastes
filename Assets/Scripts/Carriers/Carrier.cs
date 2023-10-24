using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������� ����� ����������
/// </summary>
[RequireComponent(typeof(Storage))]
public abstract class Carrier : MonoBehaviour
{
    /// <summary>
    /// ��������� ����������
    /// </summary>
    public Storage storage { get; private set; }

    public int WorkSpeed { get => Mathf.RoundToInt(workSpeed * workSpeedModifier); }
    public int MoveSpeed { get => Mathf.RoundToInt(moveSpeed * speedModifier); }

    /// <summary>
    /// �������� ������ ���������� (��./���.)
    /// </summary>
    [SerializeField] private float workSpeed = 5;
    /// <summary>
    /// ����� ���� ��������, � �������� ��������� ����� �����������������
    /// </summary>
    [SerializeField] private LayerMask interactableMask;
    /// <summary>
    /// �������� �����������
    /// </summary>
    [SerializeField] private float moveSpeed = 5;
    /// <summary>
    /// ���������
    /// </summary>
    [SerializeField] private float acceleration = 2f;
    /// <summary>
    /// �������� �������� � ������� ��������
    /// </summary>
    [SerializeField] private float rotationSpeed = 10;

    protected float speedModifier = 1;
    protected float workSpeedModifier = 1;
    protected float capacityModifier = 1;

    /// <summary>
    /// ��������, � �������� ��������� ���������
    /// </summary>
    protected Quaternion targetRotation;
    /// <summary>
    /// ����������� ��������
    /// </summary>
    protected Vector3 moveDir;
    /// <summary>
    /// ���������� ��������
    /// </summary>
    protected GlobalValuesHandler valuesHandler;

    /// <summary>
    /// ������� ���� �������
    /// </summary>
    private Rigidbody _body;
    /// <summary>
    /// ������������� ������� ����� � �����������
    /// </summary>
    private Dictionary<int, InteractableObject> _nearInteractables;

    public delegate void CarrierHandler();
    /// <summary>
    /// ���������� ��� ����� � ���� ��������������
    /// </summary>
    public event CarrierHandler onEnterInteractable;
    /// <summary>
    /// ���������� ��� ������ �� ���� ��������������
    /// </summary>
    public event CarrierHandler onExitInteractable;
    /// <summary>
    /// ���������� ��� ������ �� ���� ��� ��������������
    /// </summary>
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

        //���� ������ ���������, �� �������������� � ������� ��������
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
        //���� � ������� ��� Rigidbody, ������ �� ������
        if (!other.attachedRigidbody)
            return;

        //���� ���� ������� ���� ����� ��������� ����������
        if (((1 << other.gameObject.layer) & interactableMask.value) > 0) {
            //� �� ������������
            if (!other.attachedRigidbody.TryGetComponent<InteractableObject>(out var obj)) return;
            //��������� ��� � ������
            AddInteractable(obj);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //���� � ������� ��� Rigidbody, ������ �� ������
        if (!other.attachedRigidbody)
            return;
        //������� ������� ������������� ������ �� ������
        RemoveInteractable(other.attachedRigidbody.gameObject.GetInstanceID());
    }

    /// <summary>
    /// ����������������� � ��������� �������������� ���������
    /// </summary>
    protected void StartInteract()
    {
        if (_nearInteractables.Count == 0) return;

        foreach (var id in _nearInteractables.Keys) {
            StartInteractWith(id);
        }
    }

    /// <summary>
    /// ���������� �������������� � �������������� ���������
    /// </summary>
    protected void StopInteract()
    {
        if (_nearInteractables.Count == 0) return;

        foreach (var id in _nearInteractables.Keys) {
            StopInteractWith(id);
        }
    }

    /// <summary>
    /// ��������� ����������� ��������
    /// </summary>
    protected abstract void GetDirection();

    /// <summary>
    /// ��������� � ����������� moveDir. ���������� ��������������� ������ GetDirection � ������ moveDir
    /// </summary>
    private void Move()
    {
        Vector3 velocity = new Vector3(_body.velocity.x, 0, _body.velocity.z);
        velocity = Vector3.MoveTowards(velocity, moveDir * MoveSpeed, acceleration);

        _body.velocity = new Vector3(velocity.x, _body.velocity.y, velocity.z);
    }

    /// <summary>
    /// ��������� � targetRotation
    /// </summary>
    private void Rotate()
    {
        //transform.Rotate(Vector3.MoveTowards(transform.rotation.eulerAngles, _targetRotation, rotationSpeed));
        _body.MoveRotation(Quaternion.RotateTowards(_body.rotation, targetRotation, rotationSpeed));
    }

    /// <summary>
    /// �������� ������������� ������ � ������
    /// </summary>
    /// <param name="obj">������������� ������</param>
    private void AddInteractable(InteractableObject obj)
    {
        _nearInteractables.TryAdd(obj.gameObject.GetInstanceID(), obj);

        onEnterInteractable?.Invoke();
    }

    /// <summary>
    /// ������ ������������� ������ �� ������
    /// </summary>
    /// <param name="id">InstanceID gameObject'� �������������� �������</param>
    private void RemoveInteractable(int id)
    {
        if (!_nearInteractables.ContainsKey(id)) return;

        StopInteractWith(id);
        _nearInteractables.Remove(id);

        onExitInteractable?.Invoke();
        if (_nearInteractables.Count == 0) 
            onExitAllInteractables?.Invoke();
    }

    /// <summary>
    /// ������ �������������� � ��������
    /// </summary>
    /// <param name="id">InstanceID gameObject'� �������������� �������</param>
    private void StartInteractWith(int id)
    {
        _nearInteractables[id].StartInteract(this);
    }

    /// <summary>
    /// ��������� �������������� � ��������
    /// </summary>
    /// <param name="id">InstanceID gameObject'� �������������� �������</param>
    private void StopInteractWith(int id)
    {
        _nearInteractables[id].StopInteract(this);
    }
}
