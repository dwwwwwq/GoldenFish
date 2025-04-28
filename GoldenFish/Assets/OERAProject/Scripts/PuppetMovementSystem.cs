using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using Unity.XR.CoreUtils;

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
    public Vector3 leftForwardOffset = new Vector3(0.3f, 0.2f, 0.5f);  // 左前方位置
    public Vector3 leftBackwardOffset = new Vector3(0.3f, 0.2f, -0.5f); // 左后方位置
    public float leftZoneRadius = 0.3f;
    public Color leftReadyColor = Color.cyan;
    public Color leftWaitingColor = Color.gray;

    [Header("右手触发区域设置")]
    public Vector3 rightForwardOffset = new Vector3(-0.3f, -0.2f, 0.5f);  // 右前方位置
    public Vector3 rightBackwardOffset = new Vector3(-0.3f, -0.2f, -0.5f); // 右后方位置
    public float rightZoneRadius = 0.3f;
    public Color rightReadyColor = Color.magenta;
    public Color rightWaitingColor = Color.gray;

    [Header("调试")]
    public bool showZones = true;

    private GameObject leftZone;
    private GameObject rightZone;
    private bool isMoving;
    private float lastStepTime;
    private bool isForwardPosition = true; // 当前是否是前方位置

    void Start()
    {
        CreateZones();
        UpdateZonePositions();
    }

    void CreateZones()
    {
        leftZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(leftZone.GetComponent<Collider>());
        leftZone.transform.localScale = Vector3.one * leftZoneRadius * 2;
        leftZone.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
        leftZone.GetComponent<Renderer>().material.color = leftWaitingColor;

        rightZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(rightZone.GetComponent<Collider>());
        rightZone.transform.localScale = Vector3.one * rightZoneRadius * 2;
        rightZone.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
        rightZone.GetComponent<Renderer>().material.color = rightWaitingColor;
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
        }
    }

    void UpdateZonePositions()
    {
        // 根据当前是前方还是后方位置来设置区域
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
        Vector3 targetPos = startPos + moveDir.normalized * stepDistance;

        float elapsed = 0f;
        while (elapsed < stepDuration)
        {
            xrOrigin.transform.position = Vector3.Lerp(
                startPos,
                targetPos,
                Mathf.SmoothStep(0, 1, elapsed / stepDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        xrOrigin.transform.position = targetPos;

        // 切换前后位置
        isForwardPosition = !isForwardPosition;

        // 重置区域颜色
        leftZone.GetComponent<Renderer>().material.color = leftWaitingColor;
        rightZone.GetComponent<Renderer>().material.color = rightWaitingColor;

        isMoving = false;
    }

    void OnDestroy()
    {
        if (leftZone) Destroy(leftZone);
        if (rightZone) Destroy(rightZone);
    }

    void OnDrawGizmos()
    {
        if (!showZones || head == null) return;

        // 绘制前方区域
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(head.position + leftForwardOffset, leftZoneRadius);
        Gizmos.DrawWireSphere(head.position + rightForwardOffset, rightZoneRadius);

        // 绘制后方区域
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(head.position + leftBackwardOffset, leftZoneRadius);
        Gizmos.DrawWireSphere(head.position + rightBackwardOffset, rightZoneRadius);

        // 绘制当前活动区域
        Gizmos.color = Color.green;
        Vector3 currentLeft = isForwardPosition ? leftForwardOffset : leftBackwardOffset;
        Vector3 currentRight = isForwardPosition ? rightForwardOffset : rightBackwardOffset;
        Gizmos.DrawWireSphere(head.position + currentLeft, leftZoneRadius * 1.1f);
        Gizmos.DrawWireSphere(head.position + currentRight, rightZoneRadius * 1.1f);
    }
}