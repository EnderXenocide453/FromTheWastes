using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    void Update()
    {
        LookAtCamera();
    }

    private void LookAtCamera()
    {
        Vector3 target = Camera.main.transform.position - transform.position;
        target = -target;
        target.x = transform.position.x;

        transform.LookAt(target);
    }
}
