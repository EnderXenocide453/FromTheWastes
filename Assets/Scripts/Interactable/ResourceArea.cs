using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� ������/������ ��������. ������������ �������������� � ������������ � ����������
/// </summary>
[RequireComponent(typeof(Storage))]
public class ResourceArea : InteractableObject
{
    [SerializeField] private bool isImport = true;
    [SerializeField] private float workAmount = 1;

    /// <summary>
    /// ��������, ����������� ������������ id ���������� �� ����������
    /// </summary>
    private Dictionary<int, Coroutine> _coroutines;

    /// <summary>
    /// ��������� ���� ����
    /// </summary>
    public Storage storage { get; private set; }

    protected override string Description { 
        get
        {
            string action = isImport ? "��������" : "�����";

            string names = "";
            foreach (var type in storage.GetResourceTypes())
                names += $"|{ResourcesCollection.GetResource(type).name}| ";

            return $"{action} �������: {names}";
        }
    }

    private void Start()
    {
        _coroutines = new Dictionary<int, Coroutine>();
        storage = GetComponent<Storage>();
    }

    public override void StartInteract(Carrier carrier = null)
    {
        if (!carrier) return;
        
        if (isImport)
            _coroutines.Add(carrier.GetInstanceID(), StartCoroutine(TransportResource(carrier.storage, storage, workAmount / carrier.WorkSpeed)));
        else
            _coroutines.Add(carrier.GetInstanceID(), StartCoroutine(TransportResource(storage, carrier.storage, workAmount / carrier.WorkSpeed)));
    }

    public override void StopInteract(Carrier carrier = null)
    {
        if (!carrier) return;

        int id = carrier.GetInstanceID();

        if (!_coroutines.ContainsKey(id)) return;

        StopCoroutine(_coroutines[id]);
        _coroutines.Remove(id);
    }

    public IEnumerator TransportResource(Storage from, Storage to, float delay)
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
