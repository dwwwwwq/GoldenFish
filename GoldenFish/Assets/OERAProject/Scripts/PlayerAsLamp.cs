using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Light))]
public class PlayerAsLamp : MonoBehaviour
{
    [Header("光源设置")]
    [Range(1, 30)] public float lightRange = 10f;       // 光照范围
    [Range(1, 180)] public float lightAngle = 60f;      // 光照角度
    [Range(0, 10)] public float lightIntensity = 3f;    // 光照强度
    public Color lightColor = new Color(1f, 0.95f, 0.8f); // 暖白色

    [Header("光线效果")]
    public float flickerAmount = 0.05f;  // 灯光闪烁强度
    public float flickerSpeed = 3f;      // 闪烁速度

    private Light lampLight;
    private Transform headTransform;
    private float baseIntensity;
    private float randomSeed;

    void Start()
    {
        // 获取VR头部（主相机）的变换
        headTransform = Camera.main.transform;

        // 初始化光源组件
        lampLight = GetComponent<Light>();
        lampLight.type = LightType.Spot;
        lampLight.range = lightRange;
        lampLight.spotAngle = lightAngle;
        lampLight.intensity = lightIntensity;
        lampLight.color = lightColor;
        lampLight.shadows = LightShadows.Soft;

        baseIntensity = lightIntensity;
        randomSeed = Random.Range(0f, 100f);

        // 将光源附加到头部（保持相对位置）
        transform.SetParent(headTransform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    void Update()
    {
        // 使光源完全跟随头部旋转
        transform.rotation = headTransform.rotation;

        // 模拟灯光闪烁效果
        if (flickerAmount > 0)
        {
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, randomSeed);
            lampLight.intensity = baseIntensity * (1f + (noise - 0.5f) * flickerAmount);
        }

        // 调试用：按键调整光照参数
        if (Input.GetKey(KeyCode.UpArrow)) lightRange += Time.deltaTime * 5f;
        if (Input.GetKey(KeyCode.DownArrow)) lightRange -= Time.deltaTime * 5f;
    }

    // 供外部调用的参数设置方法
    public void SetLightParameters(float range, float angle, float intensity)
    {
        lightRange = Mathf.Clamp(range, 1, 30);
        lightAngle = Mathf.Clamp(angle, 1, 180);
        lightIntensity = Mathf.Clamp(intensity, 0, 10);

        lampLight.range = lightRange;
        lampLight.spotAngle = lightAngle;
        baseIntensity = lightIntensity;
    }
}