using UnityEngine;
using UnityEngine.Rendering; // 新增的命名空间
using UnityEngine.Rendering.Universal;

public class SceneURPFeatureController : MonoBehaviour
{
    [Header("URP Renderer 配置")]
    public UniversalRendererData rendererData;

    [Header("本场景需要的 Features")]
    public string[] featuresToEnable;

    [Header("操作模式")]
    public bool disableOtherFeatures = true;

    void Start()
    {
        if (rendererData == null)
        {
            Debug.LogError("未分配 URP Renderer Data！", this);
            return;
        }
        ApplyFeatures();
    }

    public void ApplyFeatures()
    {
        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature == null) continue;

            bool shouldEnable = System.Array.Exists(featuresToEnable, name => name == feature.name);
            feature.SetActive(disableOtherFeatures ? shouldEnable : feature.isActive || shouldEnable);
        }

        rendererData.SetDirty();
        // 替换原来的 GraphicsSettings 刷新方式
        ForcePipelineUpdate();
        Debug.Log($"已应用场景 {gameObject.scene.name} 的配置");
    }

    // 新的管线刷新方法
    private void ForcePipelineUpdate()
    {
        var pipeline = GraphicsSettings.renderPipelineAsset;
        GraphicsSettings.renderPipelineAsset = null;
        GraphicsSettings.renderPipelineAsset = pipeline;
    }

    [ContextMenu("打印当前所有 Features")]
    void PrintFeatures()
    {
        if (rendererData == null) return;
        foreach (var f in rendererData.rendererFeatures)
            if (f != null) Debug.Log($"{f.name} : {(f.isActive ? "启用" : "禁用")}");
    }
}