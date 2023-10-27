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
    
    public bool enabled = true;
    
    public delegate void UpgradeHandler();
    public event UpgradeHandler onUpgraded;

    public abstract int Cost { get; }
    public abstract string Description { get; }
    public abstract string Name { get; }
    public abstract Upgrade Next { get; }
    
    public void DoUpgrade()
    {
        PreUpgrade();
        onUpgraded?.Invoke();
        PostUpgrade();
    }

    public void SetNext(Upgrade next)
    {
        _nextUpgrade = next;
    }

    /// <summary>
    /// Удаление слушателей события
    /// </summary>
    public void RemoveListeners()
    {
        onUpgraded = null;
    }

    public abstract void PreUpgrade();
    public abstract void PostUpgrade();
}

[Serializable]
public class CommonUpgrade : Upgrade
{
    [SerializeField] protected int maxLevel = 10;
    [SerializeField] protected float storageCapacityMultiplier;
    [SerializeField] protected float workSpeedMultiplier;
    public int currentLevel { get; private set; } = 0;

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

    public override void PreUpgrade()
    {
        if (currentLevel < maxLevel) {
            currentLevel++;
            _nextUpgrade = this;
            return;
        }
    }

    public override void PostUpgrade()
    {
        if (currentLevel < maxLevel)
            return;

        RemoveListeners();
        enabled = false;
        _nextUpgrade = null;
        onMaxLevel?.Invoke();
    }

    public void UpgradeTo(int level)
    {
        currentLevel = Mathf.Clamp(level, 0, maxLevel);
        _nextUpgrade = this;

        PostUpgrade();
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

    public override void PostUpgrade() { }

    public override void PreUpgrade()
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
    public abstract int[] SaveInfo { get; }

    public delegate void UpgraderHandler();

    /// <summary>
    /// Инициализация улучшений
    /// </summary>
    public abstract void Init();
    public abstract void UpgradeTo(int[] saveInfo);
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

    public event UpgraderHandler onConverterUpgrade;
    public event UpgraderHandler onTierUpgrade;
    public event UpgraderHandler onHireEmployee;
    public event UpgraderHandler onEmployeeUpgrade;

    public ConverterTierUpgrade CurrentTier { get => _curTier >= tierUpgrades.Length ? null : tierUpgrades[_curTier]; }
    public CommonUpgrade ConverterUpgrade { get => converterUpgrade.enabled ? converterUpgrade : null; }
    public CommonUpgrade EmployeeUpgrade { get => employeeUpgrade.enabled ? employeeUpgrade : null; }

    public override Upgrade[] CurrentUpgrades
    {
        get
        {
            return new Upgrade[]
            {
                ConverterUpgrade,
                CurrentTier,
                _hired ? EmployeeUpgrade : hireEmployee
            };
        }
    }

    public override int[] SaveInfo {
        get => new int[]
        {
            converterUpgrade.currentLevel,
            _curTier,
            _hired ? employeeUpgrade.currentLevel : -1
        };
    }

    public override void Init()
    {
        converterUpgrade.onUpgraded += () => onConverterUpgrade?.Invoke();

        employeeUpgrade.onUpgraded += () => onEmployeeUpgrade?.Invoke();

        hireEmployee.onUpgraded += HireEmployee;

        ConverterTierUpgrade tier = tierUpgrades[tierUpgrades.Length - 1];
        tier.onUpgraded += () => UpgradeTier();

        for (int i = 0; i < tierUpgrades.Length - 1; i++) {
            tier = tierUpgrades[i];
            tier.onUpgraded += () => UpgradeTier();
            tier.SetNext(tierUpgrades[i + 1]);
        }
    }

    public override void UpgradeTo(int[] saveInfo)
    {
        Init();

        converterUpgrade.UpgradeTo(saveInfo[0]);

        int targetTier = saveInfo[1];

        for (int i = 0; i < targetTier; i++)
            UpgradeTier();

        //Если работник не нанят выходим из метода
        if (saveInfo[2] < 0)
            return;

        HireEmployee();
        employeeUpgrade.UpgradeTo(saveInfo[2]);
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

[System.Serializable]
public class PlayerUpgrader : Upgrader
{
    [SerializeField] private CommonUpgrade upgrade;

    public event UpgraderHandler onUpgraded;

    public CommonUpgrade PlayerUpgrade { get => upgrade.enabled ? upgrade : null; }

    public override Upgrade[] CurrentUpgrades { get => new Upgrade[] { PlayerUpgrade }; }

    public override int[] SaveInfo { get => new int[] { upgrade.currentLevel }; }

    public override void Init()
    {
        upgrade.onUpgraded += () => onUpgraded?.Invoke();
    }

    public override void UpgradeTo(int[] saveInfo)
    {
        upgrade.UpgradeTo(saveInfo[0]);
    }
}
#endregion