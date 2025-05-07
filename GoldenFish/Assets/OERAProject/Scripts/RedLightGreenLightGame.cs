using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using Unity.XR.CoreUtils;
using FMODUnity;

public class RedLightGreenLightGame : MonoBehaviour
{
    [Header("玩具熊设置")]
    public GameObject toyBear;
    public float activationDistance = 5f;
    public float safeZoneDistance = 2f;
    public float turnDuration = 2f;

    [Header("时间设置")]
    public float warningDuration = 1f;
    public float[] possibleRedDurations = { 2.0f, 2.3f, 2.7f, 3.0f, 3.3f, 3.7f, 4.0f };
    public float[] possibleGreenDurations = { 3.0f, 3.5f, 4.0f, 4.5f, 5.0f, 5.5f, 6.0f };

    [Header("玩家检测设置")]
    public float headMovementThreshold = 0.05f;
    public float handMovementThreshold = 0.1f;
    public float checkInterval = 0.2f;

    [Header("视觉效果")]
    public Light directionalLight;
    public float dimIntensity = 0.3f;
    private float originalIntensity;
    public Color warningColor = Color.yellow;
    public Color redLightColor = Color.red;
    public Color greenLightColor = Color.green;

    [Header("玩家设置")]
    public Transform xrOrigin;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Vector3 startPosition;

    [Header("安全区域设置")]
    public float safeZoneBuffer = 0.5f;

    private bool isInSafeZone = false;
    private enum GameState { Inactive, GreenLight, RedLight, Warning }
    private GameState currentState = GameState.Inactive;
    private Vector3 lastHeadLocalPosition;
    private Vector3 lastLeftHandLocalPosition;
    private Vector3 lastRightHandLocalPosition;
    private float stateTimer;
    private bool isTurning = false;
    private bool isFacingPlayer = false;
    private float currentGreenLightDuration;
    private float currentRedLightDuration;

    [Header("FMOD设置")]
    [EventRef] public string alarm;

    void Start()
    {
        originalIntensity = directionalLight.intensity;
        startPosition = xrOrigin.position;
        SetBearFacingPlayer(false);
        directionalLight.color = Color.white;
        directionalLight.intensity = originalIntensity;
        RecordCurrentPositions();
        GenerateRandomDurations();
    }

    void GenerateRandomDurations()
    {
        currentRedLightDuration = possibleRedDurations[Random.Range(0, possibleRedDurations.Length)];
        currentGreenLightDuration = possibleGreenDurations[Random.Range(0, possibleGreenDurations.Length)];
    }

    void Update()
    {
        float distanceToBear = Vector3.Distance(xrOrigin.position, toyBear.transform.position);

        if (currentState != GameState.Inactive)
        {
            if (distanceToBear <= safeZoneDistance && !isInSafeZone)
            {
                isInSafeZone = true;
                ExitGame();
                return;
            }
            else if (distanceToBear > (safeZoneDistance + safeZoneBuffer))
            {
                isInSafeZone = false;
            }
        }

        if (currentState == GameState.Inactive &&
            distanceToBear <= activationDistance &&
            distanceToBear > (safeZoneDistance + safeZoneBuffer))
        {
            StartGame();
        }

        if (currentState != GameState.Inactive && !isTurning)
        {
            stateTimer -= Time.deltaTime;

            switch (currentState)
            {
                case GameState.GreenLight:
                    if (stateTimer <= 0) StartCoroutine(TransitionToRedLight());
                    break;
                case GameState.RedLight:
                    if (stateTimer <= 0) StartCoroutine(TransitionToGreenLight());
                    break;
                case GameState.Warning:
                    if (stateTimer <= 0) StartCoroutine(TurnToFacePlayer());
                    break;
            }
        }
    }

    void ExitGame()
    {
        if (currentState == GameState.Inactive) return;
        Debug.Log("进入安全区域，游戏结束");
        currentState = GameState.Inactive;
        directionalLight.intensity = originalIntensity;
        directionalLight.color = Color.white;
        StopAllCoroutines();
    }

    void StartGame()
    {
        GenerateRandomDurations();
        currentState = GameState.GreenLight;
        stateTimer = currentGreenLightDuration;
        directionalLight.color = greenLightColor;
        directionalLight.intensity = originalIntensity;
        StartCoroutine(MonitorPlayerMovement());
        Debug.Log($"游戏开始! 绿灯时长:{currentGreenLightDuration:F1}秒 - 现在可以移动");
    }

    IEnumerator TransitionToRedLight()
    {
        currentState = GameState.Warning;
        stateTimer = warningDuration;
        directionalLight.intensity = dimIntensity;
        directionalLight.color = warningColor;
        RuntimeManager.PlayOneShot(alarm);
        Debug.Log("警告! 玩具熊即将转身");
        yield return new WaitForSeconds(warningDuration);
        StartCoroutine(TurnToFacePlayer());
        
    }

    IEnumerator TurnToFacePlayer()
    {
        if (isTurning || isFacingPlayer) yield break;
        isTurning = true;

        float elapsed = 0f;
        Quaternion startRotation = toyBear.transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 180, 0);

        while (elapsed < turnDuration)
        {
            toyBear.transform.rotation = Quaternion.Slerp(
                startRotation,
                endRotation,
                elapsed / turnDuration
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        toyBear.transform.rotation = endRotation;
        isFacingPlayer = true;
        GenerateRandomDurations(); // 新增：红灯开始时重新生成时间
        currentState = GameState.RedLight;
        directionalLight.color = redLightColor;
        directionalLight.intensity = originalIntensity;
        stateTimer = currentRedLightDuration;
        isTurning = false;
        Debug.Log($"玩具熊转身了! 红灯时长:{currentRedLightDuration:F1}秒 - 现在不能移动");
        RecordCurrentPositions();
    }

    IEnumerator TransitionToGreenLight()
    {
        if (isTurning || !isFacingPlayer) yield break;
        isTurning = true;

        float elapsed = 0f;
        Quaternion startRotation = toyBear.transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 180, 0);

        while (elapsed < turnDuration)
        {
            toyBear.transform.rotation = Quaternion.Slerp(
                startRotation,
                endRotation,
                elapsed / turnDuration
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        toyBear.transform.rotation = endRotation;
        isFacingPlayer = false;
        GenerateRandomDurations(); // 新增：绿灯开始时重新生成时间
        currentState = GameState.GreenLight;
        directionalLight.color = greenLightColor;
        stateTimer = currentGreenLightDuration;
        isTurning = false;
        Debug.Log($"玩具熊背对你了! 绿灯时长:{currentGreenLightDuration:F1}秒 - 现在可以移动");
    }

    IEnumerator MonitorPlayerMovement()
    {
        while (currentState != GameState.Inactive)
        {
            if (currentState == GameState.RedLight && isFacingPlayer)
            {
                Vector3 headLocalPos = head.localPosition;
                Vector3 leftHandLocalPos = leftHand.localPosition;
                Vector3 rightHandLocalPos = rightHand.localPosition;

                float headMovement = Vector3.Distance(headLocalPos, lastHeadLocalPosition);
                float leftHandMovement = Vector3.Distance(leftHandLocalPos, lastLeftHandLocalPosition);
                float rightHandMovement = Vector3.Distance(rightHandLocalPos, lastRightHandLocalPosition);

                if (headMovement > headMovementThreshold ||
                    leftHandMovement > handMovementThreshold ||
                    rightHandMovement > handMovementThreshold)
                {
                    PlayerCaughtMoving();
                }
            }
            RecordCurrentPositions();
            yield return new WaitForSeconds(checkInterval);
        }
    }

    void RecordCurrentPositions()
    {
        lastHeadLocalPosition = head.localPosition;
        lastLeftHandLocalPosition = leftHand.localPosition;
        lastRightHandLocalPosition = rightHand.localPosition;
    }

    void PlayerCaughtMoving()
    {
        Debug.Log("你被抓到移动了! 游戏失败");
        xrOrigin.position = startPosition;
        SetBearFacingPlayer(false);
        currentState = GameState.Inactive;
        directionalLight.intensity = originalIntensity;
        directionalLight.color = Color.white;
    }

    void SetBearFacingPlayer(bool facing)
    {
        if (facing)
        {
            toyBear.transform.rotation = Quaternion.LookRotation(
                new Vector3(
                    xrOrigin.position.x,
                    toyBear.transform.position.y,
                    xrOrigin.position.z
                ) - toyBear.transform.position
            );
        }
        else
        {
            toyBear.transform.rotation = Quaternion.LookRotation(
                toyBear.transform.position - new Vector3(
                    xrOrigin.position.x,
                    toyBear.transform.position.y,
                    xrOrigin.position.z
                )
            );
        }
        isFacingPlayer = facing;
    }

    void OnDrawGizmos()
    {
        if (toyBear != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(toyBear.transform.position, activationDistance);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(toyBear.transform.position, safeZoneDistance);
        }
    }
}