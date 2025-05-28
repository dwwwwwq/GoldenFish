using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using Unity.XR.CoreUtils;
using FMODUnity;

public class FixedZonePuppetMovement : MonoBehaviour
{
    [Header("VR References")]
    public XROrigin xrOrigin;
    public Transform leftHand;
    public Transform rightHand;
    public Transform head;

    [Header("Movement Settings")]
    public float stepDistance = 0.5f;
    public float stepDuration = 0.8f;
    public float cooldown = 0.5f;

    [Header("Left Hand Zone")]
    public Vector3 leftForwardOffset = new Vector3(0.3f, 0.2f, 0.5f);
    public Vector3 leftBackwardOffset = new Vector3(0.3f, 0.2f, -0.5f);
    public float leftZoneRadius = 0.3f;
    public Color leftReadyColor = Color.cyan;
    public Color leftWaitingColor = Color.gray;

    [Header("Right Hand Zone")]
    public Vector3 rightForwardOffset = new Vector3(-0.3f, -0.2f, 0.5f);
    public Vector3 rightBackwardOffset = new Vector3(-0.3f, -0.2f, -0.5f);
    public float rightZoneRadius = 0.3f;
    public Color rightReadyColor = Color.magenta;
    public Color rightWaitingColor = Color.gray;

    [Header("Step Animation")]
    public float upwardDistance = 0.1f;
    public float upwardDurationRatio = 0.3f;
    public float downwardDurationRatio = 0.7f;

    [Header("Visualization")]
    public bool showZones = true;

    [Header("FMOD")]
    [EventRef] public string footstep;

    [Header("Prefabs - Required")]
    public GameObject leftZonePrefab;
    public GameObject rightZonePrefab;

    [Header("Step Trigger")]
    public GameObject targetObject;
    public int triggerStepIndex = 6;

    private GameObject leftZone;
    private GameObject rightZone;
    private bool isMoving;
    private float lastStepTime;
    private bool isForwardPosition = true;
    private int stepCount = 0;

    void Start()
    {
        if (!leftZonePrefab || !rightZonePrefab)
        {
            Debug.LogError("必须指定左右手区域预制体！");
            enabled = false;
            return;
        }

        leftZone = Instantiate(leftZonePrefab);
        rightZone = Instantiate(rightZonePrefab);

        // 设置初始缩放
        leftZone.transform.localScale = Vector3.one * leftZoneRadius * 2;
        rightZone.transform.localScale = Vector3.one * rightZoneRadius * 2;

        UpdateZonePositions();
    }

    void Update()
    {
        if (isMoving || Time.time < lastStepTime + cooldown) return;

        UpdateZonePositions();

        bool leftInZone = CheckHandInZone(leftHand, leftZone, leftZoneRadius);
        bool rightInZone = CheckHandInZone(rightHand, rightZone, rightZoneRadius);

        UpdateZoneColor(leftZone, leftInZone, leftReadyColor, leftWaitingColor);
        UpdateZoneColor(rightZone, rightInZone, rightReadyColor, rightWaitingColor);

        if (leftInZone && rightInZone)
        {
            StartCoroutine(PerformStep());
            RuntimeManager.PlayOneShot(footstep);
        }
    }

    bool CheckHandInZone(Transform hand, GameObject zone, float radius)
    {
        return Vector3.Distance(hand.position, zone.transform.position) < radius;
    }

    void UpdateZoneColor(GameObject zone, bool isReady, Color readyColor, Color waitingColor)
    {
        zone.GetComponent<Renderer>().material.color = isReady ? readyColor : waitingColor;
    }

    void UpdateZonePositions()
    {
        Vector3 currentLeftOffset = isForwardPosition ? leftForwardOffset : leftBackwardOffset;
        Vector3 currentRightOffset = isForwardPosition ? rightForwardOffset : rightBackwardOffset;

        leftZone.transform.position = head.position + currentLeftOffset;
        rightZone.transform.position = head.position + currentRightOffset;
    }

    IEnumerator PerformStep()
    {
        isMoving = true;
        lastStepTime = Time.time;

        Vector3 startPos = xrOrigin.transform.position;
        Vector3 moveDir = new Vector3(head.forward.x, 0, head.forward.z).normalized;
        Vector3 horizontalTarget = startPos + moveDir * stepDistance;

        // 上升阶段
        yield return MoveToPosition(startPos,
                                  startPos + moveDir * (stepDistance * upwardDurationRatio) + Vector3.up * upwardDistance,
                                  stepDuration * upwardDurationRatio);

        // 下降阶段
        yield return MoveToPosition(xrOrigin.transform.position,
                                  new Vector3(horizontalTarget.x, startPos.y, horizontalTarget.z),
                                  stepDuration * downwardDurationRatio);

        xrOrigin.transform.position = horizontalTarget;
        isForwardPosition = !isForwardPosition;
        stepCount++;

        if (stepCount == triggerStepIndex && targetObject)
        {
            targetObject.SetActive(true);
        }

        UpdateZoneColor(leftZone, false, leftReadyColor, leftWaitingColor);
        UpdateZoneColor(rightZone, false, rightReadyColor, rightWaitingColor);

        isMoving = false;
    }

    IEnumerator MoveToPosition(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            xrOrigin.transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void OnDestroy()
    {
        if (leftZone) Destroy(leftZone);
        if (rightZone) Destroy(rightZone);
    }

    void OnDrawGizmos()
    {
        if (!showZones || head == null) return;

        Gizmos.color = Color.blue;
        DrawZoneGizmos(leftForwardOffset, rightForwardOffset);

        Gizmos.color = Color.red;
        DrawZoneGizmos(leftBackwardOffset, rightBackwardOffset);

        Gizmos.color = Color.green;
        Vector3 currentLeft = isForwardPosition ? leftForwardOffset : leftBackwardOffset;
        Vector3 currentRight = isForwardPosition ? rightForwardOffset : rightBackwardOffset;
        Gizmos.DrawWireSphere(head.position + currentLeft, leftZoneRadius * 1.1f);
        Gizmos.DrawWireSphere(head.position + currentRight, rightZoneRadius * 1.1f);
    }

    void DrawZoneGizmos(Vector3 leftOffset, Vector3 rightOffset)
    {
        Gizmos.DrawWireSphere(head.position + leftOffset, leftZoneRadius);
        Gizmos.DrawWireSphere(head.position + rightOffset, rightZoneRadius);
    }
}