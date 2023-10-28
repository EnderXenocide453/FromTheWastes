using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Converter))]
public class SaveConverter : SaveItem
{
    private Converter _converter;

    public override SaveInfo SaveInfo
    {
        get
        {
            SaveInfo info = new SaveInfo();

            info.position = _converter.transform.position;
            info.rotation = _converter.transform.rotation;
            info.prefabPath = path;
            info.upgraderInfo = _converter.upgrader.SaveInfo;

            info.storagesInfo = new Dictionary<ResourceType, int>[_converter.importStorages.Length + _converter.exportStorages.Length];
            for (int i = 0; i < _converter.importStorages.Length; i++)
                info.storagesInfo[i] = _converter.importStorages[i].ResourcesCount;

            for (int i = 0; i < _converter.exportStorages.Length; i++)
                info.storagesInfo[i + _converter.importStorages.Length] = _converter.exportStorages[i].ResourcesCount;
            
            return info;
        }
    }

    protected override void Start()
    {
        base.Start();

        _converter = GetComponent<Converter>();
    }

    public override void Load(SaveInfo info)
    {
        _converter.transform.position = info.position;
        _converter.transform.rotation = info.rotation;
        _converter.upgrader.UpgradeTo(info.upgraderInfo);

        for (int i = 0; i < _converter.importStorages.Length; i++) {
            if (info.storagesInfo[i] == null)
                continue;

            _converter.importStorages[i].SetCount(info.storagesInfo[i]);
        }

        for (int i = 0; i < _converter.exportStorages.Length; i++) {
            if (info.storagesInfo[i + _converter.importStorages.Length] == null)
                continue;

            _converter.exportStorages[i].SetCount(info.storagesInfo[i + _converter.importStorages.Length]);
        }
    }
}
