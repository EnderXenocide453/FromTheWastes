using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Storage))]
public class Carrier : MonoBehaviour
{
    public Storage storage { get; private set; }

    void Start()
    {
        storage = GetComponent<Storage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
