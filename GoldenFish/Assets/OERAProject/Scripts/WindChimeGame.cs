using UnityEngine;
using UnityEngine.XR;

public class WindChimeGame : MonoBehaviour
{
    [Header("风粒子设置")]
    public GameObject windParticlePrefab;
    public Transform windSpawnPlane; // 生成风的平面
    public Vector2 spawnSize = new Vector2(5f, 3f); // 生成区域大小
    public Vector2 windSpeedRange = new Vector2(1f, 3f); // 风速范围
    public Vector2 spawnIntervalRange = new Vector2(0.5f, 2f); // 生成间隔范围

    [Header("玩家设置")]
    public Transform leftHand;
    public Transform rightHand;
    public float catchRadius = 0.3f; // 捕捉半径

    [Header("效果设置")]
    public AudioClip catchSound;
    public ParticleSystem catchEffect;

    private float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time + Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
    }

    void Update()
    {
        // 生成新的风粒子
        if (Time.time >= nextSpawnTime)
        {
            SpawnWindParticle();
            nextSpawnTime = Time.time + Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
        }

        // 检测捕捉
        DetectCatches();
    }

    void SpawnWindParticle()
    {
        // 在生成平面上随机位置
        Vector3 spawnPos = windSpawnPlane.position +
                          windSpawnPlane.right * Random.Range(-spawnSize.x / 2, spawnSize.x / 2) +
                          windSpawnPlane.up * Random.Range(-spawnSize.y / 2, spawnSize.y / 2);

        GameObject wind = Instantiate(windParticlePrefab, spawnPos, Quaternion.identity);
        WindParticle wp = wind.AddComponent<WindParticle>();

        // 设置风速和方向(朝向玩家)
        wp.speed = Random.Range(windSpeedRange.x, windSpeedRange.y);
        wp.direction = -windSpawnPlane.forward;

        // 自动销毁
        Destroy(wind, 10f);
    }

    void DetectCatches()
    {
        // 获取场景中所有风粒子
        WindParticle[] winds = FindObjectsOfType<WindParticle>();

        foreach (WindParticle wind in winds)
        {
            // 检测左手
            if (Vector3.Distance(leftHand.position, wind.transform.position) < catchRadius)
            {
                CatchWind(wind.gameObject, leftHand.position);
                break;
            }

            // 检测右手
            if (Vector3.Distance(rightHand.position, wind.transform.position) < catchRadius)
            {
                CatchWind(wind.gameObject, rightHand.position);
                break;
            }
        }
    }

    void CatchWind(GameObject wind, Vector3 catchPosition)
    {
        // 播放音效
        if (catchSound != null)
            AudioSource.PlayClipAtPoint(catchSound, catchPosition);

        // 播放粒子效果
        if (catchEffect != null)
        {
            ParticleSystem effect = Instantiate(catchEffect, catchPosition, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }

        // 销毁风粒子
        Destroy(wind);
    }
}

// 风粒子行为脚本
public class WindParticle : MonoBehaviour
{
    public float speed;
    public Vector3 direction;

    void Update()
    {
        // 沿指定方向移动
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
}