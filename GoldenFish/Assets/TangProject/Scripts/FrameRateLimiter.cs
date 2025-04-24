using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 72;
    }
}
