using UnityEngine;
using TMPro;
using System.Collections;

public class TextFadeIn : MonoBehaviour
{
    public float fadeInSpeed = 0.5f; // 淡入速度
    public float fadeOutSpeed = 0.5f; // 淡出速度
    public float displayDuration = 2f; // 显示时间
    private TextMeshPro textMeshPro;
    private BoxCollider boxCollider;

    void Start()
    {
        textMeshPro = GetComponent<TextMeshPro>();
        boxCollider = GetComponent<BoxCollider>();
        textMeshPro.alpha = 0f; // 将文字的透明度初始化为0
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeIn());  // 触发淡入
        }
    }

    // 淡入
    IEnumerator FadeIn()
    {
        while (textMeshPro.alpha < 1f)
        {
            textMeshPro.alpha += Time.deltaTime * fadeInSpeed;
            yield return null;
        }

        // 显示文本一段时间
        yield return new WaitForSeconds(displayDuration);

        // 启动淡出
        StartCoroutine(FadeOut());
    }

    // 淡出
    IEnumerator FadeOut()
    {
        while (textMeshPro.alpha > 0f)
        {
            textMeshPro.alpha -= Time.deltaTime * fadeOutSpeed;
            boxCollider.enabled = false;  // 关闭触发器
            yield return null;
        }
    }

    // 可由Signal Receiver调用的函数：启动淡入
    public void TriggerFadeIn()
    {
        StartCoroutine(FadeIn());
    }

    // 可由Signal Receiver调用的函数：启动淡出
    public void TriggerFadeOut()
    {
        StartCoroutine(FadeOut());
    }
}
