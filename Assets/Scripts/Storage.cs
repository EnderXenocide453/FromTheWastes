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
    private int _count = 0;

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

    public int Capacity
    {
        get => Mathf.RoundToInt(_capacityMultiplier * capacity);
    }

    private void Awake()
    {
        InitStorage();
        onCountChanged += (object obj) =>
        {
            (ResourceType type, int count) = ((ResourceType, int))obj;

            CheckCount(obj);
            UpdateUI(type);
        };
    }

    private void OnEnable()
    {
        if (_resources == null)
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
        //�������� ����������� ��������� � �������
        if (!_resources.ContainsKey(type))
            return 0;

        //���� ������� ����������, ��������� ���������� ��� ���������.
        //�����, ���� ������ ���� � ���������, ������������ ��� ������������ ���������. ���� ������� ���, �������� �� ��������������
        sendCount = endless ? sendCount : (_resources[type].count > 0 ? Mathf.Clamp(sendCount, 1, _resources[type].count) : 0);

        //���� ���������� ������ 1, �������� ����������
        if (sendCount < 1)
            return 0;

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
    /// ����� ������������� ���������
    /// </summary>
    private void InitStorage()
    {
        _resources = new Dictionary<ResourceType, Resource>();
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
            UpdateUI(type);
        }
    }

    /// <summary>
    /// �������� ���������� ��� ������ �������
    /// </summary>
    private void CheckCount(object obj = null)
    {
        filled = false;
        if (_count == Capacity) {
            filled = true;
            onStorageFilled?.Invoke(obj);
        } else if (_count == 0)
            onStorageEmptied?.Invoke(obj);
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
        if (verified == 0)
            return 0;

        _count += verified;
        _resources[type].count += verified;

        onCountChanged?.Invoke((type, verified));

        return verified;
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
            Debug.Log($"������ {type} �� ����� ��������� � {gameObject.name}");

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
}