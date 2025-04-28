using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class CharacterFadeController : MonoBehaviour
{
    public GameObject model;
    private List<Material> materials = new List<Material>();

    void Start()
    {
        MeshRenderer[] renderers = model.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials)
            {
                if (!materials.Contains(mat))
                {
                    materials.Add(mat);
                    mat.SetFloat("_Alpha", 0f); // 初始化为透明
                }
            }
        }

        FadeInOutSequence();
    }

    void FadeInOutSequence()
    {
        foreach (var mat in materials)
        {
            mat.DOFloat(1f, "_Alpha", 2f).OnComplete(() =>
            {
                mat.DOFloat(0f, "_Alpha", 2f);
            });
        }
    }
}
