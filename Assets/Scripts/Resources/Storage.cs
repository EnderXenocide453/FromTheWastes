using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// ��������� ��������
/// </summary>
public class Storage : MonoBehaviour
{
    public bool handleEverything;
    public bool filled { get; private set; }

    /// <summary>
    /// ���� �������� ��������
    /// </summary>
    [SerializeField] private ResourceType[] resourceTypes;

    /// <summary>
    /// ����� ���������
    /// </summary>
    [SerializeField] private int capacity;

    /// <summary>
    /// ���������� �� ������
    /// </summary>
    [SerializeField] private bool endless;

    [SerializeField] private TMP_Text counter;
    [SerializeField] private ResourceDisplay resourceDisplay;

    /// <summary>
    /// ������� � ���������
    /// </summary>
    private Dictionary<ResourceType, Resource> _resources;

    /// <summary>
    /// ����� ���������� �������� ��������
    /// </summary>
    [SerializeField] private int _count = 0;

    private float _capacityMultiplier = 1;

    /// <summary>
    /// ������� ��� ������� ���������
    /// </summary>
    public delegate void StorageEventHandler(object obj);

    /// <summary>
    /// ������� ��������� ���������� ��������
    /// </summary>
    public event StorageEventHandler onCountChanged;

    /// <summary>
    /// ������� ������������ ���������
    /// </summary>
    public event StorageEventHandler onStorageFilled;

    /// <summary>
    /// ������� ����������� ���������
    /// </summary>
    public event StorageEventHandler onStorageEmptied;

    public event StorageEventHandler onSendEnds;

    public int Capacity
    {
        get => Mathf.RoundToInt(_capacityMultiplier * capacity);
    }

    public Dictionary<ResourceType, int> ResourcesCount { 
        get
        {
            if (_resources == null)
                return null;

            Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
            foreach (var res in _resources)
                resources.Add(res.Key, res.Value.count);

            return resources;
        }
    }

    private void Awake()
    {
        InitStorage();
    }

    /// <summary>
    /// ���������� ������ type � ��������� other � ���������� count
    /// </summary>
    /// <param name="other">���������-�������</param>
    /// <param name="type">��� ������������� �������</param>
    /// <param name="sendCount">���������� ������������� �������. ������ ���� ������ 0</param>
    /// <returns>���������� ������������� �������</return
    public int SendResource(Storage other, ResourceType type, int sendCount = 1)
    {
        //�������� ����������� ��������� � ������� � ������� ���������� �����
        if (!_resources.ContainsKey(type) || !other._resources.ContainsKey(type) || other.filled) {
            onSendEnds?.Invoke(null);
            return 0;
        }

        //���� ������� ����������, ��������� ���������� ��� ���������.
        //�����, ���� ������ ���� � ���������, ������������ ��� ������������ ���������. ���� ������� ���, �������� �� ��������������
        sendCount = endless ? sendCount : (_resources[type].count > 0 ? Mathf.Clamp(sendCount, 1, _resources[type].count) : 0);

        //���� ���������� ������ 1, �������� ����������
        if (sendCount < 1) {
            other.onSendEnds?.Invoke(null);
            onSendEnds?.Invoke(null);
            return 0;
        }

        AnimateSend(other.transform, _resources[type]);

        //�������� �������� � ��������� ���������� ���������� �������� �� �������� ����������-���������
        return -ChangeResource(type, -other.ChangeResource(type, sendCount));
    }

    public int GetCount() => _count;

    public int GetResourceCount(ResourceType type)
    {
        if (_resources.TryGetValue(type, out var res))
            return res.count;

        return 0;
    }

    public ResourceType[] GetResourceTypes()
    {
        ResourceType[] res = new ResourceType[resourceTypes.Length];
        resourceTypes.CopyTo(res, 0);

        return res;
    }

    public ResourceType[] FindIdentity(Storage other)
    {
        (ResourceType[] min, ResourceType[] max) = resourceTypes.Length > other.resourceTypes.Length ? (other.resourceTypes, resourceTypes) : (resourceTypes, other.resourceTypes);

        List<ResourceType> types = new List<ResourceType>();
        foreach (var type in min) {
            for (int i = 0; i < max.Length; i++) {
                if (type == max[i])
                    types.Add(type);
            }
        }

        return types.ToArray();
    }

    public void SetCapacityMultiplier(float multiplier)
    {
        _capacityMultiplier = multiplier;

        UpdateCounter();
    }

    /// <summary>
    /// �������� �������� ������� ���������
    /// </summary>
    /// <param name="other">������ �����������</param>
    public void SetCount(Dictionary<ResourceType, int> count)
    {
        InitStorage();

        foreach (var res in count)
            ChangeResource(res.Key, res.Value);
    }

    /// <summary>
    /// ����� ������������� ���������
    /// </summary>
    private void InitStorage()
    {
        _resources = new Dictionary<ResourceType, Resource>();
        _count = 0;

        if (handleEverything) {
            resourceTypes = new ResourceType[ResourcesCollection.Resources.Count];
            int id = 0;

            foreach (var res in ResourcesCollection.Resources) {
                resourceTypes[id] = res.Key;

                id++;
            }
        } else if (resourceTypes == null || resourceTypes.Length == 0) {
            Debug.Log($"�� �������� ������ �������� � ��������� ������� {gameObject.name}");
            return;
        }

        foreach (var type in resourceTypes) {
            if (!_resources.TryAdd(type, ResourcesCollection.GetResource(type)))
                Debug.Log($"������ ���� {_resources[type].name} ��� �������� � ��������� ������� {gameObject.name}");
            UpdateCounter();
        }
    }

    /// <summary>
    /// �������� ���������� ������� type �� �������� count
    /// </summary>
    /// <param name="type">��� ����������� �������</param>
    /// <param name="count">�����, �� ������� ��������� ���������� �������</param>
    /// <returns>��������, �� ������� ���������� ����������</returns>
    private int ChangeResource(ResourceType type, int count)
    {
        //���� ������ ����������, ���������� ����� ��� ���������
        if (endless) return count;

        int verified = VerifyResourceChange(type, count);

        //���� ������ �� ����� ���� �������, ���������� 0
        if (verified == 0) {
            onSendEnds?.Invoke(null);
            return 0;
        }

        _count += verified;
        _resources[type].count += verified;

        CheckCount();
        UpdateUI(type);

        onCountChanged?.Invoke((type, count));

        return verified;
    }

    /// <summary>
    /// �������� ���������� ��� ������ �������
    /// </summary>
    private void CheckCount()
    {
        if (endless)
            return;

        filled = false;
        if (_count == Capacity) {
            filled = true;
            onStorageFilled?.Invoke(null);
        } else if (_count == 0)
            onStorageEmptied?.Invoke(null);
    }

    /// <summary>
    /// ����� �������� ��������� �������
    /// </summary>
    /// <param name="type">��� �������</param>
    /// <param name="count">�����, �� ������� ���������� ���������� �������</param>
    /// <returns>�����, �� ������� ����� ���������� ����������</returns>
    private int VerifyResourceChange(ResourceType type, int count)
    {
        //�������� ����������� �������� �������
        if (!_resources.ContainsKey(type)) {
            return 0;
        }

        //���� ����� ���������� ����� ���������� �������� ���������������, ���������� �������� ��������������� � �������� ������ ����������
        if (_count + count > Capacity) {
            return Capacity - _count;
        }

        //���� ��� ���������� ����� � ���������� ������� ��� ������ �������������, ���������� ������������� ���������� �������
        if (_resources[type].count + count < 0) {
            return -_resources[type].count;
        }

        //���� ������ �� ����� �� �����������, ���������� ����� ����������
        return count;
    }

    private void UpdateUI(ResourceType type)
    {
        UpdateCounter();
        resourceDisplay?.SetResource(type, GetResourceCount(type));
    }

    private void UpdateCounter()
    {
        if (counter)
            counter.text = $"{_count}/{Capacity}";
    }

    private void AnimateSend(Transform target, Resource sended)
    {
        ResourceAnimation res = Instantiate(Resources.Load<GameObject>(sended.prefabPath), transform.position, Quaternion.identity).GetComponent<ResourceAnimation>();
        res.StartFollow(target);
    }
}