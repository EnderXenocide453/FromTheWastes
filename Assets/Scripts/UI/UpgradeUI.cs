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

    public void ClearUI()
    {
        _counter = 0;

        if (_rows != null)
            foreach (var row in _rows.Values)
                Destroy(row.gameObject);

        _rows = new Dictionary<int, UpgradeRow>();
    }

    public void AddUpgrades(Upgrade[] upgrades)
    {
        foreach (var upgrade in upgrades)
            AddUpgrade(upgrade);
    }

    public void AddUpgrade(Upgrade upgrade)
    {
        Debug.Log(upgrade);
        if (upgrade == null)
            return;

        UpgradeRow row = Instantiate(upgradeRow, transform).GetComponent<UpgradeRow>();
        _rows.Add(_counter, row);

        row.title.text = upgrade.Name;
        row.description.text = upgrade.Description;

        int id = _counter;

        row.button.onClick.AddListener(delegate {
            upgrade.DoUpgrade();
            RemoveUpgrade(id);
            AddUpgrade(upgrade.Next);
        });

        _counter++;
    }

    private void RemoveUpgrade(int id)
    {
        Destroy(_rows[id].gameObject);
        _rows.Remove(id);
    }
}
