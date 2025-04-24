using UnityEngine;
using System.Collections;

public class FrogJumpController : MonoBehaviour
{
    [Header("References")]
    public Transform headTransform;            // 头显 Transform（通常是 VR Camera）
    public Transform referenceObject;          // 另一个角色子物体（例如脚部等）
    public LayerMask groundLayer;              // 地面层

    [Header("Jump Settings")]
    public float upwardVelocityThreshold = 1.2f;   // 抬头速度阈值
    public float verticalJumpForce = 6.0f;         // 垂直跳跃力
    public float horizontalJumpForce = 2.0f;       // 水平跳跃力
    public float gravity = -9.81f;                 // 重力加速度
    public float landDelayTime = 0.5f;             // 落地后延迟时间（秒）

    private Rigidbody playerRigidbody;
    private Vector3 lastHeadPosition;
    private Vector3 lastReferencePosition;
    private bool isGrounded = true;
    private bool hasJumped = false;

    private bool isInitialized = false;  // 初始化完成标志
    private bool isFirstJump = true;     // 标记是否为第一次跳跃

    private bool isInLandDelay = false;  // 是否处于落地延迟阶段

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();

        if (!playerRigidbody || !headTransform || !referenceObject)
        {
            Debug.LogError("FrogJumpController requires headTransform, referenceObject and Rigidbody!");
            enabled = false;
            return;
        }

        lastHeadPosition = headTransform.position;
        lastReferencePosition = referenceObject.position;

        // 等待 1 秒后再开始跳跃检测
        StartCoroutine(InitializeAfterDelay(1f));  // 延迟1秒进入初始化完成阶段
    }

    private void Update()
    {
        if (!isInitialized)
            return;

        DetectHeadJump();
        lastHeadPosition = headTransform.position;
        lastReferencePosition = referenceObject.position;
    }

    private void FixedUpdate()
    {
        // 模拟重力（使用简单的 Y 轴速度处理）
        if (!isGrounded)
        {
            playerRigidbody.velocity += new Vector3(0, gravity * Time.deltaTime, 0);
        }
    }

    private void DetectHeadJump()
    {
        // 如果处于落地延迟阶段，或者初始化未完成，则不进行速度检测
        if (isInLandDelay || !isGrounded || !isInitialized || hasJumped) return;

        // 如果是第一次跳跃，则不检测
        if (isFirstJump)
        {
            isFirstJump = false;
            return;
        }

        // 计算头显与参考物体（如角色的脚部）之间的相对速度
        Vector3 velocityHead = (headTransform.position - lastHeadPosition) / Time.deltaTime;
        Vector3 velocityReference = (referenceObject.position - lastReferencePosition) / Time.deltaTime;
        Vector3 relativeVelocity = velocityHead - velocityReference;

        // 仅使用 Y 轴速度进行跳跃判断
        if (relativeVelocity.y > upwardVelocityThreshold)
        {
            PerformJump();
            hasJumped = true;
            Debug.Log("Jump Triggered!");
        }
    }

    private void PerformJump()
    {
        // 重置 Y 轴速度，应用垂直跳跃力
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, verticalJumpForce, playerRigidbody.velocity.z);

        // 获取头显的前向方向（跳跃方向）
        Vector3 jumpDirection = headTransform.forward;  // 获取头显的前方向
        jumpDirection.y = 0;  // 使跳跃方向只在水平面上

        // 确保方向向量的单位化
        if (jumpDirection.magnitude > 0.1f)
        {
            jumpDirection.Normalize();
            playerRigidbody.AddForce(jumpDirection * horizontalJumpForce, ForceMode.VelocityChange);  // 加水平力
        }
    }

    // 使用碰撞器检查是否站在地面
    private void OnCollisionEnter(Collision collision)
    {
        // 如果碰撞对象是地面层，则认为站在地面上
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
            hasJumped = false;
            Debug.Log("Grounded");

            // 在落地时启用延迟
            StartCoroutine(LandDelay());
        }
    }

    // 离开地面时更新地面状态
    private void OnCollisionExit(Collision collision)
    {
        // 如果离开地面层，则认为不再站在地面上
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
            Debug.Log("Left Ground");
        }
    }

    private void CompleteInitialization()
    {
        isInitialized = true;
        hasJumped = false; // 确保初始化完成时没有跳跃
        Debug.Log("Initialization Complete.");
    }

    // 延迟一段时间才开始初始化跳跃检测
    private IEnumerator InitializeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteInitialization();
    }

    // 落地后延迟一段时间才允许检测跳跃
    private IEnumerator LandDelay()
    {
        isInLandDelay = true;
        yield return new WaitForSeconds(landDelayTime);
        isInLandDelay = false;
        Debug.Log("Land delay over. Ready for next jump detection.");
    }
}
