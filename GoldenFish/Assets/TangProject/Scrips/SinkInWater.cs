using System.Collections;
using UnityEngine;

public class SinkInWater : MonoBehaviour
{
    public GameObject prefab; // 要生成的预制体
    public GameObject referenceObject; // 用来规定生成范围的物体
    public GameObject character; // 角色 GameObject，用来获取角色高度
    public float sinkSpeed = 2f; // 沉入水中的速度
    public float waterLevel = 0f; // 水面高度
    public int numberOfObjects = 10; // 生成物体的数量
    public float spawnInterval = 1f; // 生成间隔时间（秒）

    private float groundHeight; // 物体下落的最低点，角色的 Y 坐标

    private void Start()
    {
        // 获取 referenceObject 的碰撞箱
        Collider referenceCollider = referenceObject.GetComponent<Collider>();
        
        if (referenceCollider == null)
        {
            Debug.LogError("Reference object does not have a collider.");
            return;
        }

        // 获取角色的 Y 坐标（作为地面高度）
        groundHeight = character.transform.position.y;

        // 启动协程按间隔生成物体
        StartCoroutine(SpawnObjectsWithInterval(referenceCollider));
    }

    private IEnumerator SpawnObjectsWithInterval(Collider referenceCollider)
    {
        // 获取碰撞箱的范围
        Vector3 boundsMin = referenceCollider.bounds.min;
        Vector3 boundsMax = referenceCollider.bounds.max;

        // 生成物体，按间隔生成
        for (int i = 0; i < numberOfObjects; i++)
        {
            // 随机生成位置，确保在碰撞箱的范围内
            float x = Random.Range(boundsMin.x, boundsMax.x);
            float z = Random.Range(boundsMin.z, boundsMax.z);
            float y = boundsMax.y; // 生成位置的 y 坐标位于碰撞箱的顶部

            Vector3 spawnPosition = new Vector3(x, y, z);

            GameObject spawnedObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
            StartCoroutine(SinkObject(spawnedObject)); // 启动物体沉入水中的协程

            // 等待指定的时间间隔再生成下一个物体
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator SinkObject(GameObject obj)
    {
        // 沉入水中的过程
        while (obj.transform.position.y > groundHeight)
        {
            obj.transform.position += Vector3.down * sinkSpeed * Time.deltaTime;
            yield return null;
        }

        // 到达地面高度后停止
        obj.transform.position = new Vector3(obj.transform.position.x, groundHeight, obj.transform.position.z);
    }
}
