using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class AutoFade : MonoBehaviour
{
    public GameObject model;
    private List<Material> materials = new List<Material>();
    public float fadeDuration = 2f;

    private void Start()
    {
        GetMaterials();       // 获取并处理材质
        FadeIn();             // 开始淡入
    }

private void GetMaterials()
{
    // 处理 MeshRenderer
    MeshRenderer[] meshRenderers = model.GetComponentsInChildren<MeshRenderer>();
    foreach (var renderer in meshRenderers)
    {
        AddMaterials(renderer.materials);
    }

    // 处理 SkinnedMeshRenderer
    SkinnedMeshRenderer[] skinnedRenderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
    foreach (var renderer in skinnedRenderers)
    {
        AddMaterials(renderer.materials);
    }
}

private void AddMaterials(Material[] mats)
{
    foreach (var mat in mats)
    {
        if (!materials.Contains(mat))
        {
            materials.Add(mat);

            // 设置材质为 Fade 模式
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            // 初始化为透明（可选）
            Color c = mat.color;
            mat.color = new Color(c.r, c.g, c.b, 0f);
        }
    }
}


    private void FadeIn()
    {
        foreach (var mat in materials)
        {
            Color color = mat.color;
            mat.DOColor(new Color(color.r, color.g, color.b, 1f), fadeDuration);
        }

        // 等待淡入完成后再开始淡出
        DOVirtual.DelayedCall(fadeDuration + 2f, FadeOut); // 等待2秒再执行淡出
    }

    private void FadeOut()
    {
        foreach (var mat in materials)
        {
            Color color = mat.color;
            mat.DOColor(new Color(color.r, color.g, color.b, 0f), fadeDuration);
        }
    }
}
