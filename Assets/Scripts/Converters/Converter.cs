using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ��������������� ��������
/// </summary>
[RequireComponent(typeof(Storage))]
public class Converter : MonoBehaviour
{
    #region ����
    [SerializeField] private string converterName = "��� �����";
    [SerializeField] private float workAmount = 1;
    [SerializeField] private float workSpeed = 1;

    [SerializeField] private ConvertInfo[] convertCost;
    [SerializeField] private ConvertInfo[] convertResult;

    [SerializeField] private EmployeeCarrier employee;

    [SerializeField] private Image progressBar;
    [SerializeField] private UpgradeArea upgradeArea;

    public Storage[] importStorages;
    public Storage[] exportStorages;
    public ConverterUpgrader upgrader;
    
    private float _workProgress;

    private float _workSpeedMultiplier = 1;
    private float _capacityMultiplier = 1;

    private int _level;
    private int _tier;

    /// <summary>
    /// ���� ������, ������ ��������� �������
    /// </summary>
    private bool _consumer = false;
    private bool _started;

    private Storage _converterStorage;

    private Dictionary<ResourceType, int> _readyResources;
    #endregion

    #region �������
    public delegate void ConverterHandler();
    public event ConverterHandler onConvertEnds;
    public event ConverterHandler onConvertStart;
    public event ConverterHandler onConverterReady;
    public event ConverterHandler onConvertChange;
    #endregion

    private void Start()
    {
        ValidateResources();
        InitStorage();
        InitUpgrades();
    }

    private void FixedUpdate()
    {
        if (_started)
            Convert(Time.fixedDeltaTime);
    }

    /// <summary>
    /// �������� ������������ �������� ������
    /// </summary>
    private void ValidateResources()
    {
        if (importStorages == null || importStorages.Length == 0) {
            Debug.Log($"���� ����� �������� ��������������� {converterName} �� ���������!");
            enabled = false;
            
            return;
        }  if (convertCost == null) {
            Debug.Log($"��������� �������������� � {converterName} �� ���������!");
            enabled = false;

            return;
        } 
        
        if (exportStorages == null || exportStorages.Length == 0) {
            Debug.Log($"���� ������ �������� ��������������� {converterName} �� ���������!");
            _consumer = true;
            exportStorages = new Storage[0];
        }
        if (convertResult == null || convertResult.Length == 0) {
            Debug.Log($"��������� �������������� � {converterName} �� ���������!");
            _consumer = true;
            convertResult = new ConvertInfo[0];
        }
    }

    /// <summary>
    /// ������������� ��������
    /// </summary>
    private void InitStorage()
    {
        _readyResources = new Dictionary<ResourceType, int>();
        _converterStorage = GetComponent<Storage>();

        foreach(var res in convertCost)
            _readyResources.TryAdd(res.type, 0);

        foreach (var storage in importStorages)
            storage.onCountChanged += (object obj) => HandleResourceChange(((ResourceType, int)) obj);

        foreach (var storage in exportStorages)
            storage.onCountChanged += (object obj) => {
                if (!_started)
                    CheckReadiness();
            };
    }

    /// <summary>
    /// ����� ������������ ��������� ��������
    /// </summary>
    /// <param name="change">��� ������� � �����, �� ������� ���������� ��� �����������</param>
    private void HandleResourceChange((ResourceType type, int count) change)
    {
        if (!_readyResources.ContainsKey(change.type))
            return;

        _readyResources[change.type] += change.count;
        CheckReadiness();
    }

    /// <summary>
    /// ��������� ���������� � ��������������
    /// </summary>
    private void CheckReadiness()
    {
        foreach (var cost in convertCost)
            if (_readyResources[cost.type] < cost.amount)
                return;

        foreach (var storage in exportStorages)
            if (storage.filled)
                return;

        onConverterReady?.Invoke();
        StartConvertation();
    }

    /// <summary>
    /// ������ ��������������
    /// </summary>
    private void StartConvertation()
    {
        if (_started) return;

        _workProgress = 0;
        _started = true;

        ConsumeCost();
        onConvertStart?.Invoke();
    }

    /// <summary>
    /// ��������� ������� �� ������� �������� 
    /// </summary>
    private void ConsumeCost()
    {
        foreach (var cost in convertCost) {
            int remains = cost.amount;

            foreach(var storage in importStorages) {
                remains -= storage.SendResource(_converterStorage, cost.type, remains);

                if (remains == 0)
                    break;
            }
        }
    }

    /// <summary>
    /// ������� ��� ��������������
    /// </summary>
    /// <param name="delta">����� ����</param>
    private void Convert(float delta)
    {
        _workProgress += workSpeed * _workSpeedMultiplier * delta;

        if (_workProgress >= workAmount) {
            _workProgress = 0;
            OnConvertEnds();
        }

        if (progressBar)
            progressBar.fillAmount = _workProgress / workAmount;
    }

    /// <summary>
    /// ����������, ����� ����������� ��������
    /// </summary>
    private void OnConvertEnds()
    {
        SendResult();
        _started = false;

        CheckReadiness();

        onConvertEnds?.Invoke();
    }

    /// <summary>
    /// ��������� ��������� ����������� � �������� ���������
    /// </summary>
    private void SendResult()
    {
        if (_consumer)
            return;

        foreach(var result in CalculateResult()) {
            int remains = result.amount;

            foreach (var storage in exportStorages) {
                remains -= _converterStorage.SendResource(storage, result.type, remains);
                
                if (remains == 0)
                    break;
            }
        }
    }

    /// <summary>
    /// ���������� ��������� ��������������
    /// </summary>
    /// <returns>��������� ��������������</returns>
    protected virtual ConvertInfo[] CalculateResult()
    {
        return convertResult;
    }

    /// <summary>
    /// ������������� ���������
    /// </summary>
    private void InitUpgrades()
    {
        upgrader.Init();
        upgradeArea.upgrader = upgrader;

        upgrader.onConverterUpgrade += Upgrade;
        upgrader.onTierUpgrade += () => UpgradeTier(upgrader.CurrentTier);
        upgrader.onEmployeeUpgrade += UpgradeEmployee;
    }

    /// <summary>
    /// ��������� ���������� ���������������
    /// </summary>
    private void Upgrade()
    {
        _workSpeedMultiplier = upgrader.ConverterUpgrade.WorkSpeedMultiplier;
        _capacityMultiplier = upgrader.ConverterUpgrade.StorageCapacityMultiplier;
        
        foreach (var storage in importStorages)
            storage.SetCapacityMultiplier(_capacityMultiplier);

        foreach (var storage in exportStorages)
            storage.SetCapacityMultiplier(_capacityMultiplier);

        _level++;
    }

    /// <summary>
    /// ��������� ��������� �����
    /// </summary>
    /// <param name="tier">���������</param>
    private void UpgradeTier(ConverterTierUpgrade tier)
    {
        convertCost = tier.tierConvertCost;
        convertResult = tier.tierConvertResult;

        //���� ���������� ���, ������� ������ �����������
        _consumer = convertResult == null;

        onConvertChange?.Invoke();

        _tier++;
    }

    /// <summary>
    /// ��������� ��������� ���������
    /// </summary>
    private void UpgradeEmployee()
    {
        employee.SetCapacityModifier(upgrader.EmployeeUpgrade.StorageCapacityMultiplier);
        employee.SetWorkSpeedModifier(upgrader.EmployeeUpgrade.WorkSpeedMultiplier);
    }
}

/// <summary>
/// ��� � ���������� ������� �� ������ / ������������ ��� ������������
/// </summary>
[System.Serializable]
public struct ConvertInfo
{
    /// <summary>
    /// ��� �������
    /// </summary>
    public ResourceType type;
    /// <summary>
    /// ���������� ������� �� ������ / ������������ ��� ������������
    /// </summary>
    public int amount;

    public ConvertInfo(ResourceType type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
}
