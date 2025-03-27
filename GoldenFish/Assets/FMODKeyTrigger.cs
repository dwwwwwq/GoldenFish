using UnityEngine;
using FMODUnity;

public class FMODKeyTrigger : MonoBehaviour
{
    [SerializeField] private string eventPath = "event:/SoundFX/FX_GlassBall";

    private void PlayBallSound()
    {
        try
        {
            // 方法1：绑定到当前物体（跟随移动）
            RuntimeManager.PlayOneShotAttached(eventPath, gameObject);

            // 方法2：在当前位置播放（不跟随移动）
            // RuntimeManager.PlayOneShot(eventPath, transform.position);

            Debug.Log($"播放声音: {eventPath} 位置: {transform.position}");
        }
        catch (EventNotFoundException e)
        {
            Debug.LogError($"FMOD事件未找到: {eventPath}\n{e.Message}");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            PlayBallSound();
        }
    }
}