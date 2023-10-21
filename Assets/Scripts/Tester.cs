using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    #if UNITY_EDITOR
    public Storage storage1, storage2, storage3;

    // Start is called before the first frame update
    void Start()
    {
        Coroutine coroutine = StartCoroutine(StorageTest());
        storage2.onStorageFilled += () => { 
            Debug.Log("Хранилище 2 заполнено");
            StopCoroutine(coroutine);
            coroutine = StartCoroutine(StorageTest2());
        };
        storage3.onStorageFilled += () => { Debug.Log("Хранилище 3 заполнено"); };
        storage2.onStorageEmptied += () => { Debug.Log("Хранилище 2 опустошено"); };
    }

    private IEnumerator StorageTest()
    {
        while (true) {
            storage1.SendResource(storage2, ResourceType.Waste, 10);
            yield return new WaitForSeconds(2);
        }
    }

    private IEnumerator StorageTest2()
    {
        while (true) {
            storage2.SendResource(storage3, ResourceType.Waste, 2);
            yield return new WaitForSeconds(2);
        }
    }
    #endif
}
