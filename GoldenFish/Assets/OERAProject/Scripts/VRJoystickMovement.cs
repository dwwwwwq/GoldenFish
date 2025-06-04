using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(CharacterController))]
public class VRJoystickMovement : MonoBehaviour
{
    [Header("VR References")]
    public XROrigin xrOrigin;
    public Transform head; // 头显位置（通常是XR Origin下的Main Camera）

    [Header("Movement Settings")]
    [Tooltip("移动速度 (米/秒)")]
    public float moveSpeed = 1.5f;
    [Tooltip("每一步的距离 (米)")]
    public float stepDistance = 0.5f;
    [Tooltip("每一步的持续时间 (秒)")]
    public float stepDuration = 0.4f;
    [Tooltip("步与步之间的间隔时间 (秒)")]
    public float stepCooldown = 0.1f;
    [Tooltip("摇杆移动阈值 (0-1)")]
    [Range(0.1f, 0.9f)] public float joystickThreshold = 0.3f;

    [Header("Step Animation")]
    [Tooltip("步伐起伏高度 (米)")]
    public float stepHeight = 0.05f;
    [Tooltip("上升阶段占步持续时间的比例")]
    [Range(0.1f, 0.9f)] public float riseRatio = 0.4f;
    [Tooltip("起伏曲线")]
    public AnimationCurve stepCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Collision Settings")]
    [Tooltip("碰撞检测距离 (米)")]
    public float collisionCheckDistance = 0.5f;
    [Tooltip("碰撞检测半径 (米)")]
    public float collisionRadius = 0.2f;
    [Tooltip("检测高度偏移 (米)")]
    public float collisionHeightOffset = 0.5f;

    private CharacterController characterController;
    private Vector2 joystickInput;
    private bool isMoving;
    private float lastStepTime;
    private Vector3 moveDirection;
    private bool canMove = true;
    private InputDevice leftHandDevice;

    // 步行动画相关变量
    private Vector3 stepStartPosition;  // 步伐起始位置（水平）
    private Vector3 stepTargetPosition; // 步伐目标位置（水平）
    private float stepProgress;         // 步伐进度 (0-1)
    private float currentVerticalOffset; // 当前垂直偏移量

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        if (xrOrigin == null)
            xrOrigin = FindObjectOfType<XROrigin>();

        if (head == null && xrOrigin != null)
            head = xrOrigin.Camera.transform;

        // 初始化左手设备
        InitializeLeftHandDevice();
    }

    void InitializeLeftHandDevice()
    {
        // 尝试获取左手设备
        var leftHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);

        if (leftHandDevices.Count > 0)
        {
            leftHandDevice = leftHandDevices[0];
            Debug.Log("找到左手设备: " + leftHandDevice.name);
        }
        else
        {
            Debug.LogWarning("未找到左手设备！");
        }
    }

    void Update()
    {
        if (!canMove) return;

        // 如果没有有效的左手设备，尝试重新初始化
        if (!leftHandDevice.isValid)
        {
            InitializeLeftHandDevice();
            return;
        }

        // 获取左手手柄摇杆输入
        if (leftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickInput))
        {
            // 检查是否在移动中或冷却中
            bool inCooldown = Time.time < lastStepTime + stepCooldown;

            // 如果有摇杆输入且不在移动中或冷却中，开始移动
            if (joystickInput.magnitude > joystickThreshold && !isMoving && !inCooldown)
            {
                StartMovement();
            }
        }

        // 更新移动和动画
        if (isMoving)
        {
            UpdateMovement();
        }
    }

    private void StartMovement()
    {
        isMoving = true;
        lastStepTime = Time.time;
        stepProgress = 0f;
        currentVerticalOffset = 0f;

        // 计算移动方向（基于头部朝向）
        Vector3 horizontalDirection = new Vector3(head.forward.x, 0f, head.forward.z).normalized;
        Vector3 rightDirection = new Vector3(head.right.x, 0f, head.right.z).normalized;

        // 根据摇杆输入确定移动方向
        moveDirection = (horizontalDirection * joystickInput.y + rightDirection * joystickInput.x).normalized;

        // 检查碰撞
        if (CheckCollision(moveDirection))
        {
            // 如果有碰撞，取消移动
            isMoving = false;
            return;
        }

        // 设置起始和目标位置
        stepStartPosition = transform.position;
        stepTargetPosition = stepStartPosition + moveDirection * stepDistance;
    }

    private void UpdateMovement()
    {
        // 更新步伐进度
        stepProgress += Time.deltaTime / stepDuration;

        if (stepProgress >= 1f)
        {
            // 完成移动
            transform.position = stepTargetPosition;
            isMoving = false;
            return;
        }

        // 计算水平位置（使用平滑步进）
        float horizontalProgress = Mathf.SmoothStep(0f, 1f, stepProgress);
        Vector3 horizontalPosition = Vector3.Lerp(stepStartPosition, stepTargetPosition, horizontalProgress);

        // 计算垂直偏移（步伐动画）
        UpdateVerticalOffset();

        // 组合最终位置
        Vector3 finalPosition = horizontalPosition + Vector3.up * currentVerticalOffset;

        // 应用移动
        characterController.Move(finalPosition - transform.position);
    }

    private void UpdateVerticalOffset()
    {
        if (stepProgress < riseRatio)
        {
            // 上升阶段
            float riseProgress = stepProgress / riseRatio;
            currentVerticalOffset = stepCurve.Evaluate(riseProgress) * stepHeight;
        }
        else
        {
            // 下降阶段
            float fallProgress = (stepProgress - riseRatio) / (1 - riseRatio);
            currentVerticalOffset = (1 - stepCurve.Evaluate(fallProgress)) * stepHeight;
        }
    }

    private bool CheckCollision(Vector3 direction)
    {
        // 执行球体投射检测碰撞
        Vector3 checkPosition = transform.position + Vector3.up * collisionHeightOffset;

        if (Physics.SphereCast(
            checkPosition,
            collisionRadius,
            direction,
            out RaycastHit hit,
            collisionCheckDistance))
        {
            // 如果检测到碰撞体，且不是触发器，则返回有碰撞
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                Debug.Log($"检测到碰撞: {hit.collider.gameObject.name}");
                return true;
            }
        }
        return false;
    }

    // 用于调试的可视化
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // 绘制碰撞检测范围
        Vector3 checkPosition = transform.position + Vector3.up * collisionHeightOffset;
        Vector3 displayDirection = moveDirection != Vector3.zero ? moveDirection : Vector3.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(checkPosition, collisionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            checkPosition,
            checkPosition + displayDirection * collisionCheckDistance
        );

        // 绘制碰撞检测终点
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(checkPosition + displayDirection * collisionCheckDistance, collisionRadius);

        // 绘制步伐动画信息
        if (isMoving)
        {
            // 绘制起始位置和目标位置
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(stepStartPosition, 0.05f);
            Gizmos.DrawLine(stepStartPosition, stepTargetPosition);

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(stepTargetPosition, 0.05f);

            // 绘制当前垂直偏移
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * currentVerticalOffset);
        }
    }

    // 外部调用禁用/启用移动
    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }
}