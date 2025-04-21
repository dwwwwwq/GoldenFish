using System.Collections.Generic;
using UnityEngine;

public class SignalActivator : MonoBehaviour
{
    [Header("Signal触发时启用的物品")]
    public List<GameObject> objectsToEnable;

    [Header("Signal触发时禁用的物品")]
    public List<GameObject> objectsToDisable;

    // 这个函数可以在Timeline的Signal中通过Signal Receiver调用
    public void ActivateObjects()
    {
        // 启用指定物品
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // 禁用指定物品
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
