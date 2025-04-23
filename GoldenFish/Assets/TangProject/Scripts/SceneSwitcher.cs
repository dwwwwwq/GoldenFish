using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Header("要切换到的下一个场景名称")]
    public string nextSceneName;

    // 这个函数可以在 Signal Receiver 中被调用
    public void SwitchToNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("请在 Inspector 中设置下一个场景名称！");
        }
    }
}
