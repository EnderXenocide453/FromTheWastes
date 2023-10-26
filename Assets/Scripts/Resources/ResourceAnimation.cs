using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceAnimation : MonoBehaviour
{
    [SerializeField] private float speed = 2;
    [SerializeField] private float height = 1;

    private Vector3 _startPos;
    private Transform _target;

    public void StartFollow(Transform target)
    {
        _target = target;
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        while (Vector3.Distance(transform.position, _target.position) > 0.05f) {
            yield return new WaitForEndOfFrame();

            transform.position = Vector3.MoveTowards(transform.position, _target.position, speed * Time.deltaTime);
        }

        Destroy(gameObject);
    }
}
