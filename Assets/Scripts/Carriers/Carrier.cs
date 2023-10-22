using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Storage))]
public class Carrier : MonoBehaviour
{
    public Storage storage { get; private set; }
    [SerializeField] private LayerMask interactableMask;

    private Dictionary<int, InteractableObject> _nearInteractables;

    public delegate void CarrierHandler();
    public event CarrierHandler onEnterInteractable;
    public event CarrierHandler onExitInteractable;
    public event CarrierHandler onExitAllInteractables;

    private void Start()
    {
        _nearInteractables = new Dictionary<int, InteractableObject>();
        storage = GetComponent<Storage>();

        onEnterInteractable += Interact;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & interactableMask.value) > 0) {
            if (!other.TryGetComponent<InteractableObject>(out var obj)) return;

            AddInteractable(obj);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        RemoveInteractable(other.gameObject.GetInstanceID());
    }

    public void Interact()
    {
        if (_nearInteractables.Count == 0) return;

        foreach (var id in _nearInteractables.Keys) {
            InteractWith(id);
        }
    }

    public void StopInteract()
    {
        if (_nearInteractables.Count == 0) return;

        foreach (var id in _nearInteractables.Keys) {
            StopInteractWith(id);
        }
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
