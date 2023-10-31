using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Объект сохранения простого объекта
/// </summary>
public class SaveSimpleObject : SaveItem
{
    public override SaveInfo SaveInfo
    {
        get
        {
            SaveInfo info = new SaveInfo();
            info.position = transform.position;
            info.rotation = transform.rotation;
            info.prefabPath = path;

            return info;
        }
    }

    protected override void Load(SaveInfo info)
    {
        transform.position = info.position;
        transform.rotation = info.rotation;
    }
}
