using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Storage))]
public class Converter : MonoBehaviour
{
    [SerializeField] private string converterName = "Ѕез имени";
    [SerializeField] private float workAmount = 1;
    [SerializeField] private float workSpeed = 1;

    [SerializeField] private Storage[] importStorages;
    [SerializeField] private Storage[] exportStorages;
    [SerializeField] private ConvertInfo[] convertCost;
    [SerializeField] private ConvertInfo[] convertResult;

    [SerializeField] private Image progressBar;
    [SerializeField] private UpgradeArea upgradeArea;
    [SerializeField] private ConverterUpgrader upgrader;
    
    private float _workProgress;

    private float _workSpeedMultiplier = 1;
    private float _capacityMultiplier = 1;

    private int _level;
    private int _tier;

    private bool _started;

    private Storage _converterStorage;

    private Dictionary<ResourceType, int> _readyResources;

    public delegate void ConverterHandler();
    public event ConverterHandler onConvertEnds;
    public event ConverterHandler onConvertStart;
    public event ConverterHandler onConverterReady;
    public event ConverterHandler onConvertChange;

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

    private void ValidateResources()
    {
        if (importStorages == null || importStorages.Length == 0) {
            Debug.Log($"«оны входа ресурсов преобразовател€ {converterName} не назначены!");
            enabled = false;
            
            return;
        } if (exportStorages == null || exportStorages.Length == 0) {
            Debug.Log($"«оны выхода ресурсов преобразовател€ {converterName} не назначены!");
            enabled = false;

            return;
        } if (convertCost == null) {
            Debug.Log($"—тоимость преобразовани€ в {converterName} не назначена!");
            enabled = false;

            return;
        } if (convertResult == null) {
            Debug.Log($"–езультат преобразовани€ в {converterName} не назначена!");
            enabled = false;

            return;
        }
    }

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

    private void HandleResourceChange((ResourceType type, int count) change)
    {
        if (!_readyResources.ContainsKey(change.type))
            return;

        _readyResources[change.type] += change.count;
        CheckReadiness();
    }

    private void CheckReadiness()
    {
        foreach (var cost in convertCost)
            if (_readyResources[cost.type] < cost.amount)
                return;

        foreach (var storage in exportStorages)
            if (storage.filled) {
                Debug.Log("јјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјјј");
                return;
            }

        onConverterReady?.Invoke();
        StartConvertation();
    }

    private void StartConvertation()
    {
        if (_started) return;

        _workProgress = 0;
        _started = true;

        ConsumeCost();
        onConvertStart?.Invoke();
    }

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

    private void Convert(float delta)
    {
        _workProgress += workSpeed * _workSpeedMultiplier * delta;

        if (_workProgress >= workAmount) {
            _workProgress = 0;
            OnConvertEnds();
        }

        progressBar.fillAmount = _workProgress / workAmount;
    }

    private void OnConvertEnds()
    {
        SendResult();
        _started = false;

        CheckReadiness();

        onConvertEnds?.Invoke();
    }

    private void SendResult()
    {
        foreach(var result in CalculateResult()) {
            int remains = result.amount;

            foreach (var storage in exportStorages) {
                remains -= _converterStorage.SendResource(storage, result.type, remains);
                
                if (remains == 0)
                    break;
            }
        }
    }

    protected virtual ConvertInfo[] CalculateResult()
    {
        return convertResult;
    }

    private void InitUpgrades()
    {
        upgrader.Init();
        upgradeArea.upgrader = upgrader;

        upgrader.onCommonUpgrade += Upgrade;
        upgrader.onTierUpgrade += () => UpgradeTier(upgrader.CurrentTier);
    }

    private void Upgrade()
    {
        _workSpeedMultiplier = upgrader.CommonUpgrade.WorkSpeedMultiplier;
        _capacityMultiplier = upgrader.CommonUpgrade.StorageCapacityMultiplier;
        
        foreach (var storage in importStorages)
            storage.SetCapacityMultiplier(_capacityMultiplier);
        foreach (var storage in exportStorages)
            storage.SetCapacityMultiplier(_capacityMultiplier);

        _level++;
    }

    private void UpgradeTier(ConverterTierUpgrade tier)
    {
        convertCost = tier.tierConvertCost;
        convertResult = tier.tierConvertResult;

        onConvertChange?.Invoke();

        _tier++;
    }
}

/// <summary>
/// “ип и количество ресурса на выходе / необходимого дл€ производства
/// </summary>
[System.Serializable]
public struct ConvertInfo
{
    /// <summary>
    /// “ип ресурса
    /// </summary>
    public ResourceType type;
    /// <summary>
    ///  оличество ресурса на выходе / необходимого дл€ производства
    /// </summary>
    public int amount;

    public ConvertInfo(ResourceType type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
}
