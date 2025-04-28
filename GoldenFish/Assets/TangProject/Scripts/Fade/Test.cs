using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject model;
    private FadeModel fadeModel;

    void Start()
    {
        fadeModel = new FadeModel(model);
        fadeModel.FadeInModel(); // 淡入模型

        // 等待2秒后再淡出
        Invoke("FadeOut", 2f);
    }

    void FadeOut()
    {
        fadeModel.HideModel(); // 淡出模型
    }
}
