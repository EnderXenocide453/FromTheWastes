using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��������� ��������
/// </summary>
public class Storage : MonoBehaviour
{
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

    /// <summary>
    /// ������� � ���������
    /// </summary>
    private Dictionary<ResourceType, Resource> _resources;
    /// <summary>
    /// ����� ���������� �������� ��������
    /// </summary>
    private int _count = 0;

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

    private void Awake()
    {
        InitStorage();
        onCountChanged += CheckCount;
    }

    /// <summary>
    /// ���������� ������ type � ��������� other � ���������� count
    /// </summary>
    /// <param name="other">���������-�������</param>
    /// <param name="type">��� ������������� �������</param>
    /// <param name="sendCount">���������� ������������� �������. ������ ���� ������ 0</param>
    public void SendResource(Storage other, ResourceType type, int sendCount = 1)
    {
        //�������� ����������� ��������� � �������
        if (!_resources.ContainsKey(type))
            return;

        //���� ������� ����������, ��������� ���������� ��� ���������.
        //�����, ���� ������ ���� � ���������, ������������ ��� ������������ ���������. ���� ������� ���, �������� �� ��������������
        sendCount = endless ? sendCount : (_resources[type].count > 0 ? Mathf.Clamp(sendCount, 1, _resources[type].count) : 0);

        //���� ���������� ������ 1, �������� ����������
        if (sendCount < 1) 
            return;

        //�������� �������� � ��������� ���������� ���������� �������� �� �������� ����������-���������
        ChangeResource(type, -other.ChangeResource(type, sendCount));
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

    /// <summary>
    /// ����� ������������� ���������
    /// </summary>
    private void InitStorage()
    {
        _resources = new Dictionary<ResourceType, Resource>();

        if (resourceTypes == null || resourceTypes.Length == 0) {
            Debug.Log($"�� �������� ������ �������� � ��������� ������� {gameObject.name}");
            return;
        }

        foreach (var type in resourceTypes)
            if (!_resources.TryAdd(type, ResourcesCollection.GetResource(type)))
                Debug.Log($"������ ���� {_resources[type].name} ��� �������� � ��������� ������� {gameObject.name}");
    }

    /// <summary>
    /// �������� ���������� ��� ������ �������
    /// </summary>
    private void CheckCount(object obj)
    {
        if (_count == capacity)
            onStorageFilled?.Invoke(obj);
        else if (_count == 0)
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

        Debug.Log($"���������� ������� {_resources[type].name} ������ ����� {_resources[type].count}");

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
        if (_count + count > capacity) {
            return capacity - _count;
        }

        //���� ��� ���������� ����� � ���������� ������� ��� ������ �������������, ���������� ������������� ���������� �������
        if (_resources[type].count + count < 0) {
            return -_resources[type].count;
        }

        //���� ������ �� ����� �� �����������, ���������� ����� ����������
        return count;
    }
}