using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceDisplay : MonoBehaviour
{
    [SerializeField] private GameObject rowPrefab;

    private Dictionary<ResourceType, int> _resources;
    private Dictionary<ResourceType, (Transform row, Image icon, TMP_Text counter)> _rows;

    private void Awake()
    {
        _resources = new Dictionary<ResourceType, int>();
        _rows = new Dictionary<ResourceType, (Transform row, Image icon, TMP_Text counter)>();
    }

    public void SetResource(ResourceType type, int count)
    {
        if (!_resources.ContainsKey(type)) {
            _resources.Add(type, 0);
            _rows.Add(type, AddRow(type));
        }

        _resources[type] = count;
        UpdateRow(type);
    }

    private (Transform, Image, TMP_Text) AddRow(ResourceType type)
    {
        Transform row = Instantiate(rowPrefab, transform).transform;

        Image icon = row.GetComponentInChildren<Image>();
        icon.sprite = Resources.Load<Sprite>(ResourcesCollection.GetResource(type).iconPath);

        return (row, icon, row.GetComponentInChildren<TMP_Text>());
    }

    private void UpdateRow(ResourceType type)
    {
        if (!_resources.ContainsKey(type))
            return;

        _rows[type].row.gameObject.SetActive(_resources[type] > 0);
        _rows[type].counter.text = _resources[type].ToString();
    }
}