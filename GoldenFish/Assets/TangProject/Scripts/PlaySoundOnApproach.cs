using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlaySoundOnApproach : MonoBehaviour
{
    public string playerTag = "Player";          // 目标对象的Tag
    public float triggerRadius = 5f;             // 检测半径
    public AudioClip audioClip;                  // 要播放的音频
    public bool playOnce = true;                 // 是否只播放一次

    private AudioSource audioSource;
    private bool hasPlayed = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (audioClip != null)
            audioSource.clip = audioClip;
    }

    void Update()
    {
        if (playOnce && hasPlayed) return;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= triggerRadius)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
                hasPlayed = true;
            }
        }
    }

    // 可视化触发范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
