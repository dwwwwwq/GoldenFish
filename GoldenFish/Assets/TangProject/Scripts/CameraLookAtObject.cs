using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform target;  // 目标物体（当前物体将朝向的目标）

    void Start()
    {
        if (target != null)
        {
            // 计算从当前物体到目标物体的方向
            Vector3 directionToTarget = target.position - transform.position;

            // 计算旋转，使当前物体朝向目标物体
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // 设置当前物体的旋转
            transform.rotation = targetRotation;
        }
        else
        {
            Debug.LogWarning("Target is not assigned!");
        }
    }
}
