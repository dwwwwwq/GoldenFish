using UnityEngine;

public class GlassControl : MonoBehaviour
{
    public Transform target; // 目标物品
    public Material glassMaterial; // 玻璃球材质
    public float transparentValue = 0.05f; // 透明度阈值
    public float frostedValue = 0.3f; // 磨砂透明度

    public Camera mainCamera;

    void Start()
    {
        if (target == null) 
    {
        target = GameObject.Find("111")?.transform;
        Debug.Log("找到");
    }
    }

    void Update()
    {
        // 从摄像机到玻璃球中心发射射线
        Vector3 direction = (target.position - mainCamera.transform.position).normalized;
        Ray ray = new Ray(mainCamera.transform.position, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // 如果射线击中了目标物品
            if (hit.transform == target)
            {
                Debug.Log("看到");
                glassMaterial.SetFloat("_Transparency", transparentValue); // 变透明
            }
            else
            {
                glassMaterial.SetFloat("_Transparency", frostedValue); // 变磨砂
            }
        }
        else
        {
            glassMaterial.SetFloat("_Transparency", frostedValue); // 变磨砂
        }
    }
}
