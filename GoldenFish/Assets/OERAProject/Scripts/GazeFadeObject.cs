using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GazeFadeObject : MonoBehaviour
{
    public float fadeSpeed = 2f;

    private Material _materialInstance;
    private bool _isGazed;
    private float _currentAlpha;

    void Start()
    {
        // 创建材质实例（重要！避免修改原始材质）
        _materialInstance = new Material(GetComponent<Renderer>().material);
        GetComponent<Renderer>().material = _materialInstance;

        // 设置初始透明度
        SetMaterialTransparency(0f);
    }

    void Update()
    {
        UpdateAlpha();
        HandleFadeEffect();
    }

    void UpdateAlpha()
    {
        float targetAlpha = _isGazed ? 1f : 0f;
        _currentAlpha = Mathf.MoveTowards(_currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        SetMaterialTransparency(_currentAlpha);
    }

    void HandleFadeEffect()
    {
        // 控制物体显隐状态（此功能也会影响能否被相机拍到）
        gameObject.SetActive(_currentAlpha > 0.01f || _isGazed);
        _isGazed = false; // 重置注视状态
    }

    public void OnGazeStart()
    {
        _isGazed = true;
    }

    void SetMaterialTransparency(float alpha)
    {
        // 设置材质的透明属性（适合Standard Shader）
        _materialInstance.SetFloat("_Mode", 2);  // Fade模式
        _materialInstance.SetOverrideTag("RenderType", "Transparent");
        _materialInstance.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _materialInstance.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _materialInstance.SetInt("_ZWrite", 0);
        _materialInstance.DisableKeyword("_ALPHATEST_ON");
        _materialInstance.EnableKeyword("_ALPHABLEND_ON");
        _materialInstance.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        Color color = _materialInstance.color;
        color.a = alpha;
        _materialInstance.color = color;
    }
}