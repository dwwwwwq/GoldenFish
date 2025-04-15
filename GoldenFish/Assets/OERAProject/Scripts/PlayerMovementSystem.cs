using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using Unity.XR.CoreUtils;

public class PlayerMovementSystem : MonoBehaviour
{
    [Header("VR配置")]
    public XROrigin xrOrigin;
    public Collider leftHandCollider;
    public Collider rightHandCollider;
    public Transform head;

    [Header("移动参数")]
    public float moveDistance = 1f;
    public float moveDuration = 0.5f;
    public int maxMoveCount = 0;
    public bool resetOnNewSession = true;
    [Range(0f, 45f)] public float maxAngleDeviation = 45f; // 新增：最大偏离角度

    [Header("触发区域")]
    public float triggerRadius = 0.3f;
    public float triggerHeight = 0.5f;

    // 运行时状态
    private bool isMoving;
    private int currentMoveCount;
    private bool handsWereInTrigger;
    private bool requireExit;

    void Start()
    {
        if (resetOnNewSession) currentMoveCount = 0;
    }

    void Update()
    {
        if (maxMoveCount > 0 && currentMoveCount >= maxMoveCount) return;

        Vector3 triggerPos = head.position + Vector3.up * triggerHeight;
        Collider[] hits = Physics.OverlapSphere(triggerPos, triggerRadius);

        bool leftHandIn = System.Array.Exists(hits, col => col == leftHandCollider);
        bool rightHandIn = System.Array.Exists(hits, col => col == rightHandCollider);
        bool handsInTrigger = leftHandIn && rightHandIn;

        if (!isMoving)
        {
            if (requireExit)
            {
                if (!handsInTrigger) requireExit = false;
            }
            else if (handsInTrigger)
            {
                if (!handsWereInTrigger)
                {
                    // 计算基于头部朝向的移动方向
                    Vector3 moveDirection = CalculateMoveDirection();
                    StartCoroutine(MovePlayer(moveDirection));
                    requireExit = true;
                }
            }
        }

        handsWereInTrigger = handsInTrigger;
    }

    Vector3 CalculateMoveDirection()
    {
        // 获取头部前向向量，但忽略旋转的X和Z轴（只保留Y轴旋转）
        Vector3 headForward = head.forward;
        headForward.y = 0f; // 保持水平方向
        headForward.Normalize();

        // 计算允许的偏移方向（基于头部水平朝向）
        Vector3 deviationDirection = headForward;

        // 限制偏移角度不超过maxAngleDeviation
        float angle = Vector3.Angle(Vector3.up, deviationDirection);
        if (angle > maxAngleDeviation)
        {
            // 如果角度超过限制，使用球面插值限制在最大角度内
            deviationDirection = Vector3.Slerp(Vector3.up, deviationDirection, maxAngleDeviation / angle);
        }

        return deviationDirection.normalized;
    }

    IEnumerator MovePlayer(Vector3 direction)
    {
        isMoving = true;
        currentMoveCount++;

        Vector3 startPos = xrOrigin.transform.position;
        Vector3 targetPos = startPos + direction * moveDistance;

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            xrOrigin.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        xrOrigin.transform.position = targetPos;
        isMoving = false;

        Debug.Log($"移动完成 ({currentMoveCount}/{maxMoveCount}) 方向: {direction}");
    }

    public void ResetMoveCount() => currentMoveCount = 0;

    void OnDrawGizmosSelected()
    {
        if (head == null) return;

        Gizmos.color = requireExit ? Color.yellow :
                      (maxMoveCount > 0 && currentMoveCount >= maxMoveCount) ? Color.red : Color.cyan;

        Vector3 triggerPos = head.position + Vector3.up * triggerHeight;
        Gizmos.DrawWireSphere(triggerPos, triggerRadius);

        // 绘制移动方向
        Vector3 moveDir = CalculateMoveDirection();
        Gizmos.DrawLine(triggerPos, triggerPos + moveDir * moveDistance * 0.5f);
    }
}