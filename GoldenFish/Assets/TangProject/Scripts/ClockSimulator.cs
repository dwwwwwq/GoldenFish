using UnityEngine;

public class ClockSimulator : MonoBehaviour
{
    public Transform controller;                // 控制器的Transform（左手或右手均可）
    public GameObject[] switchObjects;          // 要切换的物体数组

    public enum RotationDirection { Clockwise, CounterClockwise, Both }
    public RotationDirection rotationDirection = RotationDirection.Clockwise;

    private int currentIndex = 0;
    private float totalAngle = 0f;
    private float lastZAngle = 0f;

    void Start()
    {
        if (controller != null)
            lastZAngle = controller.eulerAngles.z;
    }

    void Update()
    {
        if (controller == null) return;

        float currentZ = controller.eulerAngles.z;
        float delta = Mathf.DeltaAngle(lastZAngle, currentZ);
        lastZAngle = currentZ;

        // 根据设定方向累加角度
        switch (rotationDirection)
        {
            case RotationDirection.Clockwise:
                if (delta > 0) totalAngle += delta;
                break;
            case RotationDirection.CounterClockwise:
                if (delta < 0) totalAngle += Mathf.Abs(delta);
                break;
            case RotationDirection.Both:
                totalAngle += Mathf.Abs(delta);
                break;
        }

        if (totalAngle >= 360f)
        {
            OnFullRotation();
            totalAngle = 0f;
        }
    }

    void OnFullRotation()
    {
        if (switchObjects.Length == 0) return;

        switchObjects[currentIndex].SetActive(false);
        currentIndex = (currentIndex + 1) % switchObjects.Length;
        switchObjects[currentIndex].SetActive(true);
    }
}
