using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Базовый класс носильщика
/// </summary>
[RequireComponent(typeof(Storage))]
public abstract class Carrier : MonoBehaviour
{
    /// <summary>
    /// Хранилище носильщика
    /// </summary>
    public Storage storage { get; private set; }

    public int WorkSpeed { get => Mathf.RoundToInt(workSpeed * workSpeedModifier); }
    public int MoveSpeed { get => Mathf.RoundToInt(moveSpeed * speedModifier); }

    /// <summary>
    /// Скорость работы носильщика (ед./сек.)
    /// </summary>
    [SerializeField] private float workSpeed = 5;
    /// <summary>
    /// Маска слоёв объектов, с которыми носильщик может взаимодействовать
    /// </summary>
    [SerializeField] private LayerMask interactableMask;
    /// <summary>
    /// Скорость перемещения
    /// </summary>
    [SerializeField] private float moveSpeed = 5;
    /// <summary>
    /// Ускорение
    /// </summary>
    [SerializeField] private float acceleration = 2f;
    /// <summary>
    /// Скорость поворота в сторону движения
    /// </summary>
    [SerializeField] private float rotationSpeed = 10;

    protected float speedModifier = 1;
    protected float workSpeedModifier = 1;
    protected float capacityModifier = 1;

    /// <summary>
    /// Вращение, к которому стремится носильщик
    /// </summary>
    protected Quaternion targetRotation;
    /// <summary>
    /// Направление движения
    /// </summary>
    protected Vector3 moveDir;
    /// <summary>
    /// Глобальные значения
    /// </summary>
    protected GlobalValuesHandler valuesHandler;

    /// <summary>
    /// Твердое тело объекта
    /// </summary>
    private Rigidbody _body;
    /// <summary>
    /// Интерактивные объекты рядом с носильщиком
    /// </summary>
    private Dictionary<int, InteractableObject> _nearInteractables;

    public delegate void CarrierHandler();
    /// <summary>
    /// Вызывается при входе в зону взаимодействия
    /// </summary>
    public event CarrierHandler onEnterInteractable;
    /// <summary>
    /// Вызывается при выходе из зоны взаимодействия
    /// </summary>
    public event CarrierHandler onExitInteractable;
    /// <summary>
    /// Вызывается при выходе из всех зон взаимодействия
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

        //Если объект двигается, то поворачивается в сторону движения
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
        //Если у объекта нет Rigidbody, ничего не делаем
        if (!other.attachedRigidbody)
            return;

        //Если слой объекта есть среди доступных носильщику
        if (((1 << other.gameObject.layer) & interactableMask.value) > 0) {
            //И он интерактивен
            if (!other.attachedRigidbody.TryGetComponent<InteractableObject>(out var obj)) return;
            //Добавляем его в список
            AddInteractable(obj);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Если у объекта нет Rigidbody, ничего не делаем
        if (!other.attachedRigidbody)
            return;
        //Пробуем удалить интерактивный объект из списка
        RemoveInteractable(other.attachedRigidbody.gameObject.GetInstanceID());
    }

    /// <summary>
    /// Взаимодействовать с активными интерактивными объектами
    /// </summary>
    protected void StartInteract()
    {
        if (_nearInteractables.Count == 0) return;

        foreach (var id in _nearInteractables.Keys) {
            StartInteractWith(id);
        }
    }

    /// <summary>
    /// Прекратить взаимодействие с интерактивными объектами
    /// </summary>
    protected void StopInteract()
    {
        if (_nearInteractables.Count == 0) return;

        foreach (var id in _nearInteractables.Keys) {
            StopInteractWith(id);
        }
    }

    /// <summary>
    /// Расчитать направление движения
    /// </summary>
    protected abstract void GetDirection();

    /// <summary>
    /// Двигаться в направлении moveDir. Необходимо переопределение метода GetDirection и расчет moveDir
    /// </summary>
    private void Move()
    {
        Vector3 velocity = new Vector3(_body.velocity.x, 0, _body.velocity.z);
        velocity = Vector3.MoveTowards(velocity, moveDir * MoveSpeed, acceleration);

        _body.velocity = new Vector3(velocity.x, _body.velocity.y, velocity.z);
    }

    /// <summary>
    /// Вращаться к targetRotation
    /// </summary>
    private void Rotate()
    {
        //transform.Rotate(Vector3.MoveTowards(transform.rotation.eulerAngles, _targetRotation, rotationSpeed));
        _body.MoveRotation(Quaternion.RotateTowards(_body.rotation, targetRotation, rotationSpeed));
    }

    /// <summary>
    /// Добавить интерактивный объект в список
    /// </summary>
    /// <param name="obj">Интерактивный объект</param>
    private void AddInteractable(InteractableObject obj)
    {
        _nearInteractables.TryAdd(obj.gameObject.GetInstanceID(), obj);

        onEnterInteractable?.Invoke();
    }

    /// <summary>
    /// Убрать интерактивный объект из списка
    /// </summary>
    /// <param name="id">InstanceID gameObject'а интерактивного объекта</param>
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
    /// Начать взаимодействие с объектом
    /// </summary>
    /// <param name="id">InstanceID gameObject'а интерактивного объекта</param>
    private void StartInteractWith(int id)
    {
        _nearInteractables[id].StartInteract(this);
    }

    /// <summary>
    /// Завершить взаимодействие с объектом
    /// </summary>
    /// <param name="id">InstanceID gameObject'а интерактивного объекта</param>
    private void StopInteractWith(int id)
    {
        _nearInteractables[id].StopInteract(this);
    }
}
