using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private GameObject upgradeRow;

    private int _counter;
    private Dictionary<int, UpgradeRow> _rows;

    /// <summary>
    /// �������� UI
    /// </summary>
    public void ClearUI()
    {
        _counter = 0;

        if (_rows != null)
            foreach (var row in _rows.Values)
                Destroy(row.gameObject);

        _rows = new Dictionary<int, UpgradeRow>();
    }

    /// <summary>
    /// �������� ��������� ���������
    /// </summary>
    /// <param name="upgrades">������ ���������</param>
    public void AddUpgrades(Upgrade[] upgrades)
    {
        foreach (var upgrade in upgrades)
            AddUpgrade(upgrade);
    }

    /// <summary>
    /// �������� ���������
    /// </summary>
    /// <param name="upgrade">���������</param>
    public void AddUpgrade(Upgrade upgrade)
    {
        if (upgrade == null)
            return;

        UpgradeRow row = Instantiate(upgradeRow, transform).GetComponent<UpgradeRow>();
        _rows.Add(_counter, row);

        row.title.text = upgrade.Name;
        row.description.text = upgrade.Description;
        row.cost.text = upgrade.Cost.ToString();

        int id = _counter;

        row.button.onClick.AddListener(delegate {
            DoUpgrade(id, upgrade);
        });

        _counter++;
    }

    /// <summary>
    /// ����������� ���������
    /// </summary>
    /// <param name="id">ID ���������</param>
    /// <param name="upgrade">������ �� ���������</param>
    private void DoUpgrade(int id, Upgrade upgrade)
    {
        if (GlobalValues.Cash < upgrade.Cost) {
            GlobalValues.Alert("������������ �������", Color.red);
            return;
        }

        GlobalValues.Cash -= upgrade.Cost;

        upgrade.DoUpgrade();
        
        RemoveUpgrade(id);
        AddUpgrade(upgrade.Next);
    }

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="id">ID ���������</param>
    private void RemoveUpgrade(int id)
    {
        Destroy(_rows[id].gameObject);
        _rows.Remove(id);
    }
}
