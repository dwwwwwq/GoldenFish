using UnityEngine;
using FMODUnity;

public class HandProximityEmitterController : MonoBehaviour
{
    [Header("距离设置")]
    public float activationDistance = 0.3f;
    public float cooldownTime = 0.2f;

    [Header("参考对象")]
    public Transform headTransform;
    public StudioEventEmitter proximityEmitter;

    private bool isInRange = false;
    private float lastToggleTime;
    private FMOD.ATTRIBUTES_3D attributes;

    void Update()
    {
        if (headTransform == null || proximityEmitter == null)
            return;

        // 计算当前距离
        float currentDistance = Vector3.Distance(transform.position, headTransform.position);
        bool shouldBePlaying = currentDistance <= activationDistance;

        // 只在状态改变时操作(带冷却时间)
        if (shouldBePlaying != isInRange && Time.time > lastToggleTime + cooldownTime)
        {
            isInRange = shouldBePlaying;
            lastToggleTime = Time.time;

            if (isInRange)
            {
                proximityEmitter.Play();
                Debug.Log("开始播放3D声音");
            }
            else
            {
                proximityEmitter.AllowFadeout = true;
                proximityEmitter.Stop();
                Debug.Log("停止播放3D声音");
            }
        }

        // 更新3D属性
        if (proximityEmitter.IsPlaying())
        {
            attributes = RuntimeUtils.To3DAttributes(transform);
            proximityEmitter.EventInstance.set3DAttributes(attributes);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (headTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(headTransform.position, activationDistance);
        }
    }
}