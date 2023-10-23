using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Зона приема/выдачи ресурсов. Осуществляет взаимодействие с носильщиками и хранилищем
/// </summary>
[RequireComponent(typeof(Storage))]
public class ResourceArea : InteractableObject
{
    /// <summary>
    /// Хранилище этой зоны
    /// </summary>
    public Storage storage { get; private set; }

    [SerializeField] private bool isImport = true;

    /// <summary>
    /// Корутины, размещенные относительно id вызвавшего их носильщика
    /// </summary>
    private Dictionary<int, Coroutine> _coroutines;

    private void Start()
    {
        _coroutines = new Dictionary<int, Coroutine>();
        storage = GetComponent<Storage>();
    }

    public override void StartInteract(Carrier carrier = null)
    {
        if (!carrier) return;
        
        if (isImport)
            _coroutines.Add(carrier.GetInstanceID(), StartCoroutine(GlobalResourceTransporter.TransportResource(carrier.storage, storage, 1 / carrier.workSpeed)));
        else
            _coroutines.Add(carrier.GetInstanceID(), StartCoroutine(GlobalResourceTransporter.TransportResource(storage, carrier.storage, 1 / carrier.workSpeed)));
    }

    public override void StopInteract(Carrier carrier = null)
    {
        if (!carrier) return;

        int id = carrier.GetInstanceID();

        if (!_coroutines.ContainsKey(id)) return;

        StopCoroutine(_coroutines[id]);
        _coroutines.Remove(id);
    }
}
