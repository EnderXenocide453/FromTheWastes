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
            Debug.Log("��������� 2 ���������");
            StopCoroutine(coroutine);
            coroutine = StartCoroutine(StorageTest2());
        };
        storage3.onStorageFilled += () => { Debug.Log("��������� 3 ���������"); };
        storage2.onStorageEmptied += () => { Debug.Log("��������� 2 ����������"); };
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
