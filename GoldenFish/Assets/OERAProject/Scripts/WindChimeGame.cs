using UnityEngine;
using UnityEngine.XR;
using FMODUnity;
using System.Collections;

public class WindChimeGame : MonoBehaviour
{
    [Header("生成相关设置")]
    public GameObject windParticlePrefab;
    public Transform windSpawnPlane; // 生成风的平面
    public Vector2 windSpeedRange = new Vector2(1f, 3f); // 风速范围
    public Vector2 spawnIntervalRange = new Vector2(0.5f, 2f); // 生成间隔范围

    [Header("双倍生成设置")]
    public int doubleSpawnAtCatchCount = 3; // 接住多少个后开始双倍生成
    private bool doubleSpawnActive = false; // 是否启用双倍生成

    [Header("捕捉相关设置")]
    public Transform leftHand;
    public Transform rightHand;
    public float catchRadius = 0.3f; // 捕捉半径

    [Header("特效相关")]
    public ParticleSystem catchEffect;

    [Header("FMOD音效")]
    [EventRef] public string catchSoundEvent; // FMOD事件路径

    [Header("游戏进度")]
    public int maxCatches = 10; // 最大捕捉数
    private int currentCatches = 0; // 当前捕捉数
    private bool gameActive = true; // 游戏是否进行中

    [Header("游戏进度")]
    public GameObject unlockableItem; // 想要启用的物体
    public int unlockAtCatchCount = 6; // 达到几次抓取后启用
    private bool itemUnlocked = false; // 只启用一次

    [Header("第二阶段解锁物体")]
    public GameObject secondUnlockableItem;
    public int secondUnlockAtCatchCount = 8;
    public Vector3 secondUnlockTargetScale = Vector3.one;
    public float scaleDuration = 1f;
    private bool secondItemUnlocked = false;

    private float nextSpawnTime;

    void Start()
    {
        // 确保生成平面有MeshRenderer组件以便可见
        if (windSpawnPlane != null)
        {
            var renderer = windSpawnPlane.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = windSpawnPlane.gameObject.AddComponent<MeshRenderer>();
            }
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(0, 1, 0, 0.3f); // 半透明绿色
        }

        nextSpawnTime = Time.time + Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
    }

    void Update()
    {
        if (!gameActive) return;

        // 控制风粒子生成逻辑
        if (Time.time >= nextSpawnTime)
        {
            if (!doubleSpawnActive)
            {
                // 单粒子生成模式
                SpawnWindParticle();
                nextSpawnTime = Time.time + Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            }
            else
            {
                // 双粒子生成模式
                SpawnWindParticle();
                SpawnWindParticle();
                float shorterInterval = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y) * 0.5f;
                nextSpawnTime = Time.time + shorterInterval;
            }
        }

        // 检测是否有粒子被抓住
        DetectCatches();
    }

    void SpawnWindParticle()
    {
        if (windSpawnPlane == null)
        {
            Debug.LogError("Wind spawn plane not assigned!");
            return;
        }

        // 获取平面的大小
        Vector3 planeSize = windSpawnPlane.localScale;
        float width = planeSize.x * 10f; // Unity的plane默认是10单位宽
        float height = planeSize.z * 10f; // Unity的plane默认是10单位长

        // 生成位置在生成平面上的一个随机点
        Vector3 spawnPos = windSpawnPlane.position +
                          windSpawnPlane.right * Random.Range(-width / 2, width / 2) +
                          windSpawnPlane.up * Random.Range(-height / 2, height / 2);

        GameObject wind = Instantiate(windParticlePrefab, spawnPos, Quaternion.identity);
        WindParticle wp = wind.AddComponent<WindParticle>();

        wp.speed = Random.Range(windSpeedRange.x, windSpeedRange.y);
        wp.direction = -windSpawnPlane.forward;

        // 自动销毁
        Destroy(wind, 10f);
    }

    void DetectCatches()
    {
        WindParticle[] winds = FindObjectsOfType<WindParticle>();

        foreach (WindParticle wind in winds)
        {
            if (Vector3.Distance(leftHand.position, wind.transform.position) < catchRadius)
            {
                CatchWind(wind.gameObject, leftHand.position);
                break;
            }

            if (Vector3.Distance(rightHand.position, wind.transform.position) < catchRadius)
            {
                CatchWind(wind.gameObject, rightHand.position);
                break;
            }
        }
    }

    void CatchWind(GameObject wind, Vector3 catchPosition)
    {
        // 播放FMOD音效
        if (!string.IsNullOrEmpty(catchSoundEvent))
        {
            RuntimeManager.PlayOneShot(catchSoundEvent, catchPosition);
        }

        // 生成抓取特效
        if (catchEffect != null)
        {
            ParticleSystem effect = Instantiate(catchEffect, catchPosition, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }

        currentCatches++;

        // 检查是否激活双倍生成
        if (!doubleSpawnActive && currentCatches >= doubleSpawnAtCatchCount)
        {
            doubleSpawnActive = true;
            Debug.Log("双倍生成模式激活！");
        }

        // 检查是否需要启用物品
        if (!itemUnlocked && currentCatches >= unlockAtCatchCount)
        {
            if (unlockableItem != null)
            {
                unlockableItem.SetActive(true);
                Debug.Log("物品已启用！");
            }
            itemUnlocked = true;
        }

        // 检查是否达到最大抓取次数，结束游戏
        if (currentCatches >= maxCatches)
        {
            gameActive = false;
            Debug.Log("游戏结束，已达到最大抓取数。");
        }

        // 删除被抓到的风
        Destroy(wind);

        // 检查是否需要启用第二个物品
        if (!secondItemUnlocked && currentCatches >= secondUnlockAtCatchCount)
        {
            if (secondUnlockableItem != null)
            {
                secondUnlockableItem.SetActive(true);
                StartCoroutine(ScaleUpObject(secondUnlockableItem, secondUnlockTargetScale, scaleDuration));
                Debug.Log("第二个物品已启用并开始缩放！");
            }
            secondItemUnlocked = true;
        }
    }

    IEnumerator ScaleUpObject(GameObject obj, Vector3 targetScale, float duration)
    {
        Transform t = obj.transform;
        Vector3 initialScale = Vector3.zero;
        float time = 0f;

        t.localScale = initialScale;

        while (time < duration)
        {
            t.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        t.localScale = targetScale; // 保证最终为目标大小
    }

    void OnDrawGizmos()
    {
        if (windSpawnPlane != null)
        {
            Gizmos.color = Color.green;

            // 获取平面的大小
            Vector3 planeSize = windSpawnPlane.localScale;
            float width = planeSize.x * 10f; // Unity的plane默认是10单位宽
            float height = planeSize.z * 10f; // Unity的plane默认是10单位长

            // 绘制平面边界
            Vector3 center = windSpawnPlane.position;
            Vector3 right = windSpawnPlane.right * width / 2;
            Vector3 up = windSpawnPlane.up * height / 2;

            Vector3 topRight = center + right + up;
            Vector3 topLeft = center - right + up;
            Vector3 bottomRight = center + right - up;
            Vector3 bottomLeft = center - right - up;

            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
        }
    }
}

public class WindParticle : MonoBehaviour
{
    public float speed;
    public Vector3 direction;

    void Update()
    {
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
}