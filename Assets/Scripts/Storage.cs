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
    [SerializeField] private ResourceType[] resouceTypes;
    /// <summary>
    /// ����� ���������
    /// </summary>
    [SerializeField] private int capacity;
    /// <summary>
    /// ���������� �� ������
    /// </summary>
    [SerializeField] private bool endless;

    /// <summary>
    /// ������� � ��������� � �� ����������
    /// </summary>
    private Dictionary<ResourceType, int> _resources;
    /// <summary>
    /// ����� ���������� �������� ��������
    /// </summary>
    private int _count = 0;

    /// <summary>
    /// ������� ��� ������� ���������
    /// </summary>
    public delegate void StorageEventHandler();
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

    public int GetResourceCount(ResourceType type)
    {
        if (_resources.ContainsKey(type))
            return _resources[type];

        return 0;
    }

    /// <summary>
    /// ���������� ������ type � ��������� other � ���������� count
    /// </summary>
    /// <param name="other">���������-�������</param>
    /// <param name="type">��� ������������� �������</param>
    /// <param name="sendCount">���������� ������������� �������. ������ ���� ������ 0</param>
    public void SendResource(Storage other, ResourceType type, int sendCount = 1)
    {
        //���� ���������� ������ 1, �������� ����������
        if (sendCount < 1) 
            return;
        //�������� ����������� ��������� � �������
        if (!_resources.ContainsKey(type))
            return;

        //���� ������ �� ���������� � ���� � ���������
        if (_resources[type] > 0 && !endless)
            //����������� ������������� ���������� ��������� �� 1 �� ���������� �������� � ���������
            sendCount = Mathf.Clamp(sendCount, 1, _resources[type]);

        //�������� �������� � ��������� ���������� ���������� �������� �� �������� ����������-���������
        ChangeResource(type, -other.ChangeResource(type, sendCount));
    }

    /// <summary>
    /// ����� ������������� ���������
    /// </summary>
    private void InitStorage()
    {
        _resources = new Dictionary<ResourceType, int>();

        if (resouceTypes == null || resouceTypes.Length == 0) {
            Debug.Log($"�� �������� ������ �������� � ��������� ������� {gameObject.name}");
            return;
        }

        foreach (var type in resouceTypes)
            if (!_resources.TryAdd(type, 0))
                Debug.Log($"������ ���� {type} ��� �������� � ��������� ������� {gameObject.name}");
    }

    /// <summary>
    /// �������� ���������� ��� ������ �������
    /// </summary>
    private void CheckCount()
    {
        if (_count == capacity)
            onStorageFilled?.Invoke();
        else if (_count == 0)
            onStorageEmptied?.Invoke();
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
        if (endless) return 0;

        int verified = VerifyResourceChange(type, count);

        //���� ������ �� ����� ���� �������, ���������� 0
        if (verified == 0)
            return 0;

        _count += verified;
        _resources[type] += verified;

        onCountChanged?.Invoke();

        Debug.Log($"���������� ������� {type} ������ ����� {_resources[type]}");

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
        if (_resources[type] + count < 0) {
            return -_resources[type];
        }

        //���� ������ �� ����� �� �����������, ���������� ����� ����������
        return count;
    }
}