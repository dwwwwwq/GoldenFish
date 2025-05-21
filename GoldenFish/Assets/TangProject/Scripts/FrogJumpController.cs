using UnityEngine;
using System.Collections;

public class FrogJumpController : MonoBehaviour
{
    [Header("References")]
    public Transform headTransform;            // 头显 Transform（通常是 VR Camera）
    public Transform referenceObject;          // 另一个角色子物体（例如脚部等）
    public LayerMask groundLayer;              // 地面层

    [Header("Jump Settings")]
    public float upwardVelocityThreshold = 1.2f;   // 大跳抬头速度阈值
    public float verticalJumpForce = 6.0f;         // 大跳垂直力
    public float horizontalJumpForce = 2.0f;       // 大跳水平力
    public float gravity = -9.81f;                 // 重力加速度
    public float landDelayTime = 0.5f;             // 落地后延迟时间（秒）

    [Header("Mini Jump Settings")]
    public float miniJumpVelocityThreshold = 0.5f; // 小跳抬头速度阈值
    public float miniJumpForce = 2.5f;             // 小跳垂直力
    public float miniHorizontalForce = 1.0f;       // 小跳水平力

    [Header("Advanced Jump Timing")]
    public float jumpBufferTime = 0.1f;            // 跳跃缓冲时间（秒）

    private Rigidbody playerRigidbody;
    private Vector3 lastHeadPosition;
    private Vector3 lastReferencePosition;
    private bool isGrounded = true;
    private bool hasJumped = false;
    private bool isInitialized = false;
    private bool isFirstJump = true;
    private bool isInLandDelay = false;

    // 缓冲逻辑
    private float jumpBufferTimer = 0f;
    private bool wantsBigJump = false;

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
        if (!isGrounded)
        {
            playerRigidbody.velocity += new Vector3(0, gravity * Time.deltaTime, 0);
        }
    }

    private void DetectHeadJump()
    {
        if (isInLandDelay || !isGrounded || !isInitialized || hasJumped)
            return;

        if (isFirstJump)
        {
            isFirstJump = false;
            return;
        }

        Vector3 velocityHead = (headTransform.position - lastHeadPosition) / Time.deltaTime;
        Vector3 velocityReference = (referenceObject.position - lastReferencePosition) / Time.deltaTime;
        Vector3 relativeVelocity = velocityHead - velocityReference;
        float relY = relativeVelocity.y;

        Debug.Log($"Relative Y Velocity: {relY:F3}");

        // 大跳缓冲检测
        if (relY > upwardVelocityThreshold)
        {
            wantsBigJump = true;
            jumpBufferTimer = jumpBufferTime;
        }

        if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer -= Time.deltaTime;

            if (wantsBigJump)
            {
                PerformJump(verticalJumpForce, horizontalJumpForce);
                hasJumped = true;
                wantsBigJump = false;
                jumpBufferTimer = 0f;
                Debug.Log("Buffered Big Jump Triggered!");
                return;
            }
        }
        // 小跳仅在大跳未触发时启用
        else if (relY > miniJumpVelocityThreshold)
        {
            PerformJump(miniJumpForce, miniHorizontalForce);
            hasJumped = true;
            Debug.Log("Mini Jump Triggered!");
        }
    }

    private void PerformJump(float jumpForce, float horizontalForce)
    {
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, jumpForce, playerRigidbody.velocity.z);

        Vector3 jumpDirection = headTransform.forward;
        jumpDirection.y = 0;

        if (jumpDirection.magnitude > 0.1f)
        {
            jumpDirection.Normalize();
            playerRigidbody.AddForce(jumpDirection * horizontalForce, ForceMode.VelocityChange);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
            hasJumped = false;
            wantsBigJump = false;
            jumpBufferTimer = 0f;
            Debug.Log("Grounded");

            StartCoroutine(LandDelay());
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
            Debug.Log("Left Ground");
        }
    }

    private void CompleteInitialization()
    {
        isInitialized = true;
        hasJumped = false;
        Debug.Log("Initialization Complete.");
    }

    private IEnumerator InitializeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteInitialization();
    }

    private IEnumerator LandDelay()
    {
        isInLandDelay = true;
        yield return new WaitForSeconds(landDelayTime);
        isInLandDelay = false;
        Debug.Log("Land delay over. Ready for next jump detection.");
    }
}
