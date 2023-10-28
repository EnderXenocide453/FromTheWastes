using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePlayer : SaveItem
{
    [SerializeField] private PlayerCarrier player;

    public override SaveInfo SaveInfo
    {
        get
        {
            SaveInfo info = new SaveInfo();

            info.position = transform.position;
            info.rotation = transform.rotation;
            info.prefabPath = path;
            info.upgraderInfo = player.upgrader.SaveInfo;

            info.storagesInfo = new Dictionary<ResourceType, int>[]
            {
                player.storage.ResourcesCount
            };

            return info;
        }
    }

    public override void Load(SaveInfo info)
    {
        transform.position = info.position;
        transform.rotation = info.rotation;

        player.upgrader.UpgradeTo(info.upgraderInfo);

        player.storage.SetCount(info.storagesInfo[0]);
    }
}
