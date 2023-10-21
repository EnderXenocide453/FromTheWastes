using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Storage))]
public class ResourceArea : InteractableObject
{
    /// <summary>
    /// Хранилище этой зоны
    /// </summary>
    public Storage storage { get; private set; }

    [SerializeField] private bool isImport = true;
    [SerializeField] private float delay = 0.5f;

    /// <summary>
    /// Корутины, размещенные относительно id вызвавшего их носильщика
    /// </summary>
    private Dictionary<int, Coroutine> _coroutines;

    private void Start()
    {
        _coroutines = new Dictionary<int, Coroutine>();
        storage = GetComponent<Storage>();
    }

    protected override void StartInteract(Carrier carrier = null)
    {
        if (!carrier) return;
        
        if (isImport)
            _coroutines.Add(carrier.GetInstanceID(), StartCoroutine(CarryResource(carrier.storage, storage)));
        else
            _coroutines.Add(carrier.GetInstanceID(), StartCoroutine(CarryResource(storage, carrier.storage)));
    }

    protected override void StopInteract(Carrier carrier = null)
    {
        if (!carrier) return;

        StopCoroutine(_coroutines[carrier.GetInstanceID()]);
        _coroutines.Remove(carrier.GetInstanceID());
    }

    private IEnumerator CarryResource(Storage from, Storage to)
    {
        ResourceType[] types = from.FindIdentity(to);

        while (true) {
            foreach (var type in types) {
                from.SendResource(to, type);
            }

            yield return new WaitForSeconds(delay);
        }
    }
}
