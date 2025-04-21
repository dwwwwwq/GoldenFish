using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class lookThroughGlass : MonoBehaviour
{
    public Camera vrCamera;             // XR 相机
    public string glassTag = "Glass";   // 玻璃球的 Tag
    public string targetTag = "Target"; // 目标物体的 Tag
    public float maxDistance = 200f;
    public string triggerObjectTag = "Glass"; 
    public float triggerDistance = 0.2f;

    public GameObject frosted;  // 要隐藏的对象
    public GameObject glass;   // 要显示的对象

    public string nextSceneName = "testVR"; 

    bool wasSeeingThroughGlass = false; // 上一帧的状态
    bool hitTarget = false;
    bool hitGlass = false;
    bool isSeeingThroughGlass = false;
    

    void Update()
    {
        hitGlass = false;
        hitTarget = false;
        // 从相机正前方发出射线
        Ray ray = new Ray(vrCamera.transform.position, vrCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green);

        // 检测所有击中的碰撞体（按距离排序）
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance);
        
        if (hits.Length == 0) return;

        
        foreach (RaycastHit hit in hits)
        {
            
            if (hit.collider.CompareTag(glassTag))
            {
                hitGlass = true;
                Debug.Log("glass");
            }
            else if (hit.collider.CompareTag(targetTag))
            {
                hitTarget = true;
                Debug.Log("target");

            }
        }

        isSeeingThroughGlass = hitGlass && hitTarget;
        
        // 判断状态是否发生了变化
        if (isSeeingThroughGlass != wasSeeingThroughGlass)
        {
            wasSeeingThroughGlass = isSeeingThroughGlass; // 更新状态

            if (isSeeingThroughGlass)
            {
                Debug.Log("✅ 刚刚开始透过玻璃看到目标了！");
                if (frosted != null) frosted.SetActive(false);
                if (glass != null) glass.SetActive(true);
            }
            else
            {
                Debug.Log("❌ 不再透过玻璃看到目标了！");
                if (frosted != null) frosted.SetActive(true);
                if (glass != null) glass.SetActive(false);
            }
        }

        if (isSeeingThroughGlass)
        {
            GameObject targetObject = GameObject.FindGameObjectWithTag(glassTag);
            if (targetObject != null)
            {
                float distance = Vector3.Distance(vrCamera.transform.position, targetObject.transform.position);
                if (distance <= triggerDistance) // 如果距离小于设定的阈值
                {
                    Debug.Log("🌟 目标物体足够近，切换场景！");
                    SceneManager.LoadScene(nextSceneName); // 记得换成你的目标场景名
                }
            }
        }
    }

    
            
        
}
