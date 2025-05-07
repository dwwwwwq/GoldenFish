using UnityEngine;
using FMODUnity;

public class SwimmingController : MonoBehaviour
{
    [Header("Controllers")]
    public Transform leftController;
    public Transform rightController;
    public Transform playerCamera;

    [Header("Physics Settings")]
    public float upForce = 0f;
    public float gravity = 0f;
    public float maxSpeed = 6f;
    public float forceMagnitude = 2.7f;

    [Header("Flap Forces")]
    public float lightFlapForce = 5f;
    public float strongFlapForce = 9f;

    [Header("Swing Detection")]
    public float xRotationThreshold = 5f;
    public float strongFlapThreshold = 20f;
    public float swingDelay = 0.5f;

    private Rigidbody playerRigidbody;
    private float lastSwingTime;
    private Quaternion lastLeftRotation;
    private Quaternion lastRightRotation;
    private Vector3 flapForceDirection;
    private Vector3 upWard = new Vector3(0f, 2f, 0f).normalized;

    private bool hasFirstFlapOccurred = false;

    [EventRef] public string catchSoundEvent;
    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        if (!playerRigidbody || !leftController || !rightController || !playerCamera)
        {
            Debug.LogError("SwimmingController requires proper components!");
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
                hasFirstFlapOccurred = true;
            }
            else
            {
                // Determine flap strength
                if (leftRotationDelta > strongFlapThreshold && rightRotationDelta > strongFlapThreshold)
                {
                    Flap(strongFlapForce); // Strong flap
                    RuntimeManager.PlayOneShot(catchSoundEvent);
                }
                else
                {
                    Flap(lightFlapForce); // Light flap
                    RuntimeManager.PlayOneShot(catchSoundEvent);
                }
            }
            lastSwingTime = Time.time;
        }

        lastLeftRotation = leftController.rotation;
        lastRightRotation = rightController.rotation;
    }

    private void Flap(float flapPower)
    {
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0.8f, playerRigidbody.velocity.z);
        playerRigidbody.AddForce(flapForceDirection * flapPower + upWard * upForce, ForceMode.VelocityChange);
    }

    public void TriggerFlap()
    {
        Flap(lightFlapForce);
    }

    public void ResetFirstFlap()
    {
        hasFirstFlapOccurred = false;
    }
}
