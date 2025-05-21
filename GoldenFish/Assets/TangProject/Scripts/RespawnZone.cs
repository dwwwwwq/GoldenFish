using UnityEngine;

public class RespawnZone : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform respawnPoint;         // 玩家将被传送回的位置
    public string playerTag = "Player";    // 用于识别玩家对象的标签

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // 重置玩家位置
            other.transform.position = respawnPoint.position;

            // 可选：重置速度（如果有 Rigidbody）
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log("Player respawned at start point.");
        }
    }
}
