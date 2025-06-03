using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class distanceAbsorption : MonoBehaviour
{
    public Transform objectA;
    public Transform objectB;
    public float stickDistance = 1.0f;
    public float moveSpeed = 5.0f; // 控制吸附速度

    private bool isStuck = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isStuck) return;

        float distanceToA = Vector3.Distance(transform.position, objectA.position);
        float distanceToB = Vector3.Distance(transform.position, objectB.position);

        if (distanceToA < stickDistance)
        {
            StartCoroutine(MoveToTarget(objectA));
        }
        else if (distanceToB < stickDistance)
        {
            StartCoroutine(MoveToTarget(objectB));
        }
    }

    IEnumerator MoveToTarget(Transform target)
    {
        isStuck = true; // 防止重复触发

        // 平滑移动
        while (Vector3.Distance(transform.position, target.position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 最终对齐
        transform.position = target.position;
        transform.SetParent(target); // 设置为子物体
    }
}
