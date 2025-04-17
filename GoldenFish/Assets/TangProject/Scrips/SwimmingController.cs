using UnityEngine;

public class SwimmingController : MonoBehaviour
{
    [Header("Controllers")]
    public Transform leftController;
    public Transform rightController;
    public Transform playerCamera;

    [Header("Physics Settings")]
    public float flapForce = 1.1f;
    public float upForce = 0.3f;
    public float gravity = 0.9f;
    public float maxSpeed = 4.5f;
    public float forceMagnitude = 0.6f;

    [Header("Swing Detection")]
    public float xRotationThreshold = 12f;
    public float swingDelay = 0.6f;

    private Rigidbody playerRigidbody;
    private float lastSwingTime;
    private Quaternion lastLeftRotation;
    private Quaternion lastRightRotation;
    private Vector3 flapForceDirection;
    private Vector3 upWard = new Vector3(0f, 2f, 0f).normalized;

    private bool hasFirstFlapOccurred = false;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        if (!playerRigidbody || !leftController || !rightController || !playerCamera)
        {
            Debug.LogError("FlappingController requires proper components!");
            enabled = false;
            return;
        }

        lastLeftRotation = leftController.rotation;
        lastRightRotation = rightController.rotation;
    }

    private void Update()
    {
        flapForceDirection = playerCamera.forward;
        DetectSwing();
    }

    private void FixedUpdate()
    {
        playerRigidbody.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        // Apply drag and limit speed
        Vector3 velocity = playerRigidbody.velocity;
        playerRigidbody.AddForce(-velocity.normalized * forceMagnitude, ForceMode.Force);
        if (velocity.magnitude > maxSpeed)
        {
            playerRigidbody.velocity = velocity.normalized * maxSpeed;
        }
    }

    private void DetectSwing()
    {
        float leftRotationDelta = Quaternion.Angle(lastLeftRotation, leftController.rotation);
        float rightRotationDelta = Quaternion.Angle(lastRightRotation, rightController.rotation);

        if (leftRotationDelta > xRotationThreshold && rightRotationDelta > xRotationThreshold && Time.time - lastSwingTime >= swingDelay)
        {
            if (!hasFirstFlapOccurred)
            {
                // 屏蔽第一次触发
                hasFirstFlapOccurred = true;
            }
            else
            {
                Flap();
            }
            lastSwingTime = Time.time;
        }

        lastLeftRotation = leftController.rotation;
        lastRightRotation = rightController.rotation;
    }

    private void Flap()
    {
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0.8f, playerRigidbody.velocity.z);
        playerRigidbody.AddForce(flapForceDirection * flapForce + upWard * upForce, ForceMode.VelocityChange);
    }

    public void TriggerFlap()
    {
        Flap();
    }

    // 可选：重置第一次触发状态（例如角色重新下水时调用）
    public void ResetFirstFlap()
    {
        hasFirstFlapOccurred = false;
    }
}
