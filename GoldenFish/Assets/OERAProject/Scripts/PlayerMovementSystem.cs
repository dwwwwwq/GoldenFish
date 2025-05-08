using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using FMODUnity;

public class PlayerMovementSystem : MonoBehaviour
{
    [Header("VR配置")]
    public XROrigin xrOrigin;
    public Collider leftHandCollider;
    public Collider rightHandCollider;
    public Transform head;
    public Camera vrCamera;

    [Header("移动参数")]
    public float moveDistance = 1f;
    public float moveDuration = 0.5f;
    public int maxMoveCount = 0;
    public bool resetOnNewSession = true;
    [Range(0f, 45f)] public float maxAngleDeviation = 45f;

    [Header("URP Renderer Feature控制")]
    public UniversalRendererData rendererData;
    public string[] featuresToDisable;
    public int disableAfterMoves = 3;
    private bool hasDisabledFeatures = false;

    [Header("触发区域")]
    public float triggerRadius = 0.3f;
    public float triggerHeight = 0.5f;

    [Header("阶段触发物体")]
    public GameObject objectToEnableOn2ndMove;
    public GameObject objectToEnableOn5thMove;

    [Header("植物生长")]
    public Transform plantModel;        // 🌱 新增：要生长的植物模型
    public float growthPerMove = 0.1f;  // 🌱 新增：每次生长的 Y 轴增量

    private bool isMoving;
    private int currentMoveCount;
    private bool handsWereInTrigger;
    private bool requireExit;

    [EventRef] public string catchSoundEvent;
    [EventRef] public string blossom;

    [Header("动画控制")]
    public Animator targetAnimator;
    public string bloomParameterName = "bloom";

    private float lastMoveTime = -Mathf.Infinity;

    void Start()
    {
        if (resetOnNewSession) currentMoveCount = 0;
    }

    void Update()
    {
        if (maxMoveCount > 0 && currentMoveCount >= maxMoveCount) return;

        Vector3 triggerPos = head.position + head.up * triggerHeight;
        bool leftHandIn = Vector3.Distance(leftHandCollider.transform.position, triggerPos) < triggerRadius;
        bool rightHandIn = Vector3.Distance(rightHandCollider.transform.position, triggerPos) < triggerRadius;
        bool handsInTrigger = leftHandIn && rightHandIn;

        if (!isMoving && Time.time - lastMoveTime >= moveDuration)
        {
            if (requireExit)
            {
                if (!handsInTrigger) requireExit = false;
            }
            else if (handsInTrigger)
            {
                if (!handsWereInTrigger)
                {
                    RuntimeManager.PlayOneShot(catchSoundEvent);
                    Vector3 moveDirection = CalculateMoveDirection();
                    StartCoroutine(MovePlayer(moveDirection));
                    requireExit = true;
                    lastMoveTime = Time.time;
                }
            }
        }

        handsWereInTrigger = handsInTrigger;
    }

    Vector3 CalculateMoveDirection()
    {
        Vector3 headUp = head.up;
        float angle = Vector3.Angle(Vector3.up, headUp);

        if (angle > maxAngleDeviation)
            headUp = Vector3.Slerp(Vector3.up, headUp, maxAngleDeviation / angle);

        return headUp.normalized;
    }

IEnumerator MovePlayer(Vector3 direction)
{
    isMoving = true;
    currentMoveCount++;

    // 阶段触发
    if (currentMoveCount == 3 && objectToEnableOn2ndMove != null)
    {
        objectToEnableOn2ndMove.SetActive(true);
        Debug.Log("第2次移动时启用了指定物体");
    }
    else if (currentMoveCount == 8 && objectToEnableOn5thMove != null)
    {
        objectToEnableOn5thMove.SetActive(true);
        Debug.Log("第5次移动时启用了指定物体");
    }

    // 禁用 Renderer Features
    if (currentMoveCount >= disableAfterMoves && !hasDisabledFeatures)
        DisableRendererFeatures();

    // 插值移动 + 植物同步增长
    Vector3 startPos = xrOrigin.transform.position;
    Vector3 targetPos = startPos + direction * moveDistance;

    Vector3 plantStartScale = plantModel != null ? plantModel.localScale : Vector3.zero;
    Vector3 plantTargetScale = plantModel != null ? new Vector3(
        plantStartScale.x,
        plantStartScale.y + growthPerMove,
        plantStartScale.z
    ) : Vector3.zero;

    float elapsed = 0f;
    while (elapsed < moveDuration)
    {
        float t = elapsed / moveDuration;
        xrOrigin.transform.position = Vector3.Lerp(startPos, targetPos, t);

        if (plantModel != null)
            plantModel.localScale = Vector3.Lerp(plantStartScale, plantTargetScale, t);

        elapsed += Time.deltaTime;
        yield return null;
    }

    xrOrigin.transform.position = targetPos;

    if (plantModel != null)
        plantModel.localScale = plantTargetScale;

    isMoving = false;

    Debug.Log($"移动完成 ({currentMoveCount}/{maxMoveCount}) 方向: {direction}");
}

    void DisableRendererFeatures()
    {
        if (rendererData == null)
        {
            Debug.LogError("Renderer Data未分配！");
            return;
        }

        bool anyFeatureDisabled = false;
        foreach (var featureName in featuresToDisable)
        {
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature != null && feature.name == featureName)
                {
                    feature.SetActive(false);
                    Debug.Log($"已关闭Renderer Feature: {featureName}");
                    anyFeatureDisabled = true;
                    break;
                }
            }
        }

        if (anyFeatureDisabled)
        {
            rendererData.SetDirty();
            GraphicsSettings.renderPipelineAsset = GraphicsSettings.renderPipelineAsset;

            if (targetAnimator != null)
            {
                targetAnimator.SetBool(bloomParameterName, true);
                Debug.Log($"已设置Animator参数 {bloomParameterName} = true");
            }
            else Debug.LogWarning("未分配目标Animator，无法设置bloom参数");

            RuntimeManager.PlayOneShot(blossom);
            hasDisabledFeatures = true;
            Debug.Log($"第{currentMoveCount}次移动后，已关闭指定Renderer Features");
        }
    }

    public void ResetMoveCount()
    {
        currentMoveCount = 0;
        hasDisabledFeatures = false;

        if (rendererData != null && featuresToDisable != null)
        {
            foreach (var featureName in featuresToDisable)
            {
                foreach (var feature in rendererData.rendererFeatures)
                {
                    if (feature != null && feature.name == featureName)
                    {
                        feature.SetActive(true);
                        break;
                    }
                }
            }
            rendererData.SetDirty();
            GraphicsSettings.renderPipelineAsset = GraphicsSettings.renderPipelineAsset;
            Debug.Log("重置移动计数并重新启用所有Renderer Features");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (head == null) return;

        Gizmos.color = requireExit ? Color.yellow :
                      (maxMoveCount > 0 && currentMoveCount >= maxMoveCount) ? Color.red : Color.cyan;

        Vector3 triggerPos = head.position + head.up * triggerHeight;
        Gizmos.DrawWireSphere(triggerPos, triggerRadius);
        Gizmos.DrawLine(triggerPos, triggerPos + CalculateMoveDirection() * moveDistance * 0.5f);
    }
}
