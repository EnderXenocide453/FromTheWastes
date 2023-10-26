using System;
using System.Collections.Generic;
using UnityEngine;

#region Улучшения
[System.Serializable]
public abstract class Upgrade
{
    [SerializeField] protected string name = "NaN";
    [SerializeField, TextArea] protected string description = "NaN";
    [SerializeField] protected int baseCost;
    [SerializeField] protected float costModifier;
    
    protected Upgrade _nextUpgrade;
    
    public delegate void UpgradeHandler();

    public event UpgradeHandler onUpgraded;

    public abstract int Cost { get; }
    public abstract string Description { get; }
    public abstract string Name { get; }
    public abstract Upgrade Next { get; }
    
    public void DoUpgrade()
    {
        EnableUpgrade();
        onUpgraded?.Invoke();
    }

    public void SetNext(Upgrade next)
    {
        _nextUpgrade = next;
    }

    public abstract void EnableUpgrade();
}

[Serializable]
public class CommonUpgrade : Upgrade
{
    [SerializeField] protected int currentLevel = 0;
    [SerializeField] protected int maxLevel = 10;
    [SerializeField] protected float storageCapacityMultiplier;
    [SerializeField] protected float workSpeedMultiplier;

    public event UpgradeHandler onMaxLevel;

    public override int Cost { get => Mathf.RoundToInt((currentLevel * costModifier + 1) * baseCost); }

    public override string Description
    {
        get => $"{description}\nРазмер хранилища + {storageCapacityMultiplier * 100}%\nСкорость работы + {workSpeedMultiplier * 100}%";
    }

    public override string Name
    {
        get => $"{name} уровень {currentLevel + 1}";
    }
    public override Upgrade Next
    {
        get => _nextUpgrade;
    }
    public float StorageCapacityMultiplier { get => 1 + storageCapacityMultiplier * currentLevel; }
    public float WorkSpeedMultiplier { get => 1 + workSpeedMultiplier * currentLevel; }

    public override void EnableUpgrade()
    {
        if (currentLevel + 1 < maxLevel) {
            currentLevel++;
            _nextUpgrade = this;
            return;
        }

        _nextUpgrade = null;
    }
}

[Serializable]
public class TierUpgrade : Upgrade
{
    public Transform[] tierParts;
    public override int Cost { get => baseCost; }
    public override string Description { get => description; }
    public override string Name { get => name; }
    public override Upgrade Next { get => _nextUpgrade; }
    public override void EnableUpgrade()
    {
        if (tierParts == null)
            return;

        foreach (var part in tierParts)
            part.gameObject.SetActive(true);
    }
}

[Serializable]
public class ConverterTierUpgrade : TierUpgrade
{
    public ConvertInfo[] tierConvertCost;
    public ConvertInfo[] tierConvertResult;
}
#endregion

#region Контроллеры улучшений
[System.Serializable]
public abstract class Upgrader
{
    public abstract Upgrade[] CurrentUpgrades { get; }

    public delegate void UpgraderHandler();

    /// <summary>
    /// Инициализация улучшений
    /// </summary>
    public abstract void Init();
}

[System.Serializable]
public class ConverterUpgrader : Upgrader
{
    [SerializeField] private CommonUpgrade converterUpgrade;
    [SerializeField] private ConverterTierUpgrade[] tierUpgrades;
    [SerializeField] private TierUpgrade hireEmployee;
    [SerializeField] private CommonUpgrade employeeUpgrade;

    private int _curTier;
    private bool _hired;

    public event UpgraderHandler onCommonUpgrade;
    public event UpgraderHandler onTierUpgrade;
    public event UpgraderHandler onHireEmployee;
    public event UpgraderHandler onEmployeeUpgrade;

    public ConverterTierUpgrade CurrentTier { get => tierUpgrades[_curTier]; }
    public CommonUpgrade ConverterUpgrade { get => converterUpgrade; }
    public CommonUpgrade EmployeeUpgrade { get => _hired ? employeeUpgrade : null; }

    public override Upgrade[] CurrentUpgrades
    {
        get
        {
            return new Upgrade[]
            {
                ConverterUpgrade,
                CurrentTier,
                _hired ? employeeUpgrade : hireEmployee
            };
        }
    }

    public override void Init()
    {
        converterUpgrade.onUpgraded += Upgrade;
        employeeUpgrade.onUpgraded += UpgradeEmployee;
        hireEmployee.onUpgraded += HireEmployee;

        ConverterTierUpgrade tier = tierUpgrades[tierUpgrades.Length - 1];
        tier.onUpgraded += () => UpgradeTier();

        for (int i = 0; i < tierUpgrades.Length - 1; i++) {
            tier = tierUpgrades[i];
            tier.onUpgraded += () => UpgradeTier();
            tier.SetNext(tierUpgrades[i + 1]);
        }
    }

    private void Upgrade()
    {
        if (converterUpgrade.Next == null)
            return;

        converterUpgrade = (CommonUpgrade)converterUpgrade.Next;
        
        converterUpgrade.onUpgraded += Upgrade;
        onCommonUpgrade?.Invoke();
    }

    private void UpgradeEmployee()
    {
        if (employeeUpgrade.Name == null)
            return;

        employeeUpgrade = (CommonUpgrade)employeeUpgrade.Next;

        employeeUpgrade.onUpgraded += UpgradeEmployee;
        onEmployeeUpgrade?.Invoke();
    }

    private void UpgradeTier()
    {
        onTierUpgrade?.Invoke();

        _curTier++;
    }

    private void HireEmployee()
    {
        hireEmployee.SetNext(employeeUpgrade);

        onHireEmployee?.Invoke();
        _hired = true;
    }
}
#endregion