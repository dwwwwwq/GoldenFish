using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
  
public class FadeModel {
  
 private GameObject model;//传入的模型
 private float fadeTime = 2f;//默认淡入时间为2s
 private List<Material> materials = new List<Material>();
  
 public FadeModel(GameObject model,float fadeTime=2f)
 {
 this.model = model;
 this.fadeTime = fadeTime;
 MeshRenderer[] meshRenderers = model.GetComponentsInChildren<MeshRenderer>();
 foreach(MeshRenderer mr in meshRenderers)
 {
 Material[] materals = mr.materials;
 foreach(Material m in materals)
 {
 if (!materials.Contains(m))
 {
 materials.Add(m);
 }
 }
 }
 }
//隐藏模型的淡隐效果
public void FadeInModel()
{
    for (int i = 0; i < materials.Count; i++)
    {
        Material m = materials[i];
        Color color = m.color;
        setMaterialRenderingMode(m, RenderingMode.Fade);
        m.color = new Color(color.r, color.g, color.b, 0); // 先设置透明
        m.DOColor(new Color(color.r, color.g, color.b, 1), fadeTime); // 淡入
    }
}
 public void HideModel()
 {
 for(int i=0;i< materials.Count;i++)
 {
 Material m = materials[i];
 Color color = m.color;
 m.color = new Color(color.r, color.g, color.b, 1);//这里一定要重新设置下Fade模式下的color a值 为1 不然 经过一次显示他会一直显示为0
 setMaterialRenderingMode(m,RenderingMode.Fade);
 m.DOColor(new Color(color.r, color.g, color.b, 0), fadeTime);
 }
 }
//当我们隐藏完后还需要设置回来 不然他下次显示使用就是透明状态
 public void ShowModel()
 {
 for (int i = 0; i < materials.Count; i++)
 {
 Material m = materials[i];
 Color color = m.color;
 setMaterialRenderingMode(m, RenderingMode.Opaque);
 }
 }
 public enum RenderingMode
 {
 Opaque,
 Cutout,
 Fade,
 Transparent
 }
 //设置材质的渲染模式 
 private void setMaterialRenderingMode(Material material, RenderingMode renderingMode)
 {
 switch (renderingMode)
 {
 case RenderingMode.Opaque:
 material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
 material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
 material.SetInt("_ZWrite", 1);
 material.DisableKeyword("_ALPHATEST_ON");
 material.DisableKeyword("_ALPHABLEND_ON");
 material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
 material.renderQueue = -1;
 break;
 case RenderingMode.Cutout:
 material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
 material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
 material.SetInt("_ZWrite", 1);
 material.EnableKeyword("_ALPHATEST_ON");
 material.DisableKeyword("_ALPHABLEND_ON");
 material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
 material.renderQueue = 2450;
 break;
 case RenderingMode.Fade:
 material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
 material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
 material.SetInt("_ZWrite", 0);
 material.DisableKeyword("_ALPHATEST_ON");
 material.EnableKeyword("_ALPHABLEND_ON");
 material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
 material.renderQueue = 3000;
 //material.SetFloat("" _Mode & quot;", 2); 
 break;
 case RenderingMode.Transparent:
 material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
 material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
 material.SetInt("_ZWrite", 0);
 material.DisableKeyword("_ALPHATEST_ON");
 material.DisableKeyword("_ALPHABLEND_ON");
 material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
 material.renderQueue = 3000;
 break;
 }
 }
  
  
}