using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using Unity.XR.CoreUtils;
using FMODUnity;

public class FixedZonePuppetMovement : MonoBehaviour
{
    [Header("VR配置")]
    public XROrigin xrOrigin;
    public Transform leftHand;
    public Transform rightHand;
    public Transform head;

    [Header("移动参数")]
    public float stepDistance = 0.5f;
    public float stepDuration = 0.8f;
    public float cooldown = 0.5f;

    [Header("左手触发区域设置")]
    public Vector3 leftForwardOffset = new Vector3(0.3f, 0.2f, 0.5f);  // 前偏移位置
    public Vector3 leftBackwardOffset = new Vector3(0.3f, 0.2f, -0.5f); // 后偏移位置
    public float leftZoneRadius = 0.3f;
    public Color leftReadyColor = Color.cyan;
    public Color leftWaitingColor = Color.gray;

    [Header("右手触发区域设置")]
    public Vector3 rightForwardOffset = new Vector3(-0.3f, -0.2f, 0.5f);  // 前偏移位置
    public Vector3 rightBackwardOffset = new Vector3(-0.3f, -0.2f, -0.5f); // 后偏移位置
    public float rightZoneRadius = 0.3f;
    public Color rightReadyColor = Color.magenta;
    public Color rightWaitingColor = Color.gray;

    [Header("上下移动参数")]
    public float upwardDistance = 0.1f; // 向上移动的距离
    public float upwardDurationRatio = 0.3f; // 向上移动所占时间比例
    public float downwardDurationRatio = 0.7f; // 向下移动所占时间比例

    [Header("FMOD音效")]
    [EventRef] public string footstep;

    [Header("判定区域预制体")]
    public GameObject leftZonePrefab;
    public GameObject rightZonePrefab;

    [Header("步骤触发")]
    public GameObject targetObject; // 要启用的物体
    public int triggerStepIndex = 6; // 第几步时触发（从1开始）

    private GameObject leftZone;
    private GameObject rightZone;
    private bool isMoving;
    private float lastStepTime;
    private bool isForwardPosition = true; // 当前是否是前方位置
    private int stepCount = 0; // 当前已走的步数

    void Start()
    {
        CreateZones();
        UpdateZonePositions();
    }

    void CreateZones()
    {
        if (leftZonePrefab != null)
        {
            leftZone = Instantiate(leftZonePrefab);
            leftZone.transform.localScale = Vector3.one * leftZoneRadius * 2;
        }
        else
        {
            Debug.LogWarning("未指定左手判定区域预制体！");
        }

        if (rightZonePrefab != null)
        {
            rightZone = Instantiate(rightZonePrefab);
            rightZone.transform.localScale = Vector3.one * rightZoneRadius * 2;
        }
        else
        {
            Debug.LogWarning("未指定右手判定区域预制体！");
        }
    }

    void Update()
    {
        if (isMoving || Time.time < lastStepTime + cooldown) return;

        UpdateZonePositions();

        bool leftInZone = Vector3.Distance(leftHand.position, leftZone.transform.position) < leftZoneRadius;
        bool rightInZone = Vector3.Distance(rightHand.position, rightZone.transform.position) < rightZoneRadius;

        leftZone.GetComponent<Renderer>().material.color = leftInZone ? leftReadyColor : leftWaitingColor;
        rightZone.GetComponent<Renderer>().material.color = rightInZone ? rightReadyColor : rightWaitingColor;

        if (leftInZone && rightInZone)
        {
            StartCoroutine(PerformStep());
            RuntimeManager.PlayOneShot(footstep);
        }
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
        Vector3 moveDir = head.forward;
        moveDir.y = 0;
        Vector3 horizontalTarget = startPos + moveDir.normalized * stepDistance;

        float totalHeightChange = upwardDistance;
        float upwardDuration = stepDuration * upwardDurationRatio;
        float downwardDuration = stepDuration * downwardDurationRatio;

        float elapsed = 0f;
        Vector3 upwardTarget = startPos + moveDir.normalized * (stepDistance * upwardDurationRatio)
                              + Vector3.up * totalHeightChange;

        while (elapsed < upwardDuration)
        {
            xrOrigin.transform.position = Vector3.Lerp(
                startPos,
                upwardTarget,
                Mathf.SmoothStep(0, 1, elapsed / upwardDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        Vector3 finalTarget = new Vector3(horizontalTarget.x, startPos.y, horizontalTarget.z);

        while (elapsed < downwardDuration)
        {
            xrOrigin.transform.position = Vector3.Lerp(
                upwardTarget,
                finalTarget,
                Mathf.SmoothStep(0, 1, elapsed / downwardDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        xrOrigin.transform.position = finalTarget;
        isForwardPosition = !isForwardPosition;
        stepCount++;

        if (stepCount == triggerStepIndex && targetObject != null)
        {
            targetObject.SetActive(true);
        }

        leftZone.GetComponent<Renderer>().material.color = leftWaitingColor;
        rightZone.GetComponent<Renderer>().material.color = rightWaitingColor;
        isMoving = false;
    }

    void OnDestroy()
    {
        if (leftZone) Destroy(leftZone);
        if (rightZone) Destroy(rightZone);
    }
}