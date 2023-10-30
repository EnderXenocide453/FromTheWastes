using UnityEngine;

public class BillBoard : MonoBehaviour
{
    private void Update()
    {
        LookAtCamera();
    }

    private void LookAtCamera()
    {
        Vector3 target = transform.position + transform.position - Camera.main.transform.position;
        target.x = transform.position.x;

        transform.LookAt(target);
    }
}