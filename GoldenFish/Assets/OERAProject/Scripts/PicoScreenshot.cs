using UnityEngine;
using System.Collections;
using UnityEngine.XR;

public class PicoScreenshot : MonoBehaviour
{
    [Header("截图设置")]
    public GameObject screenshotPrefab;  // 照片预制体（建议使用Quad）
    public float photoWidth = 0.3f;      // 单张照片宽度（米）

    [Header("照片墙设置")]
    public Transform photoWall;          // 照片墙的父物体
    public Vector2Int gridSize = new Vector2Int(4, 3); // 行列数量
    public Vector2 spacing = new Vector2(0.05f, 0.05f); // 照片间距

    private Texture2D[] savedTextures;   // 存储所有截图
    private int currentPhotoIndex = 0;   // 当前照片索引
    private Vector3[,] photoPositions;   // 预计算的照片位置网格
    private bool isTriggerPressed = false;

    void Start()
    {
        InitializePhotoWall();
    }

    void Update()
    {
        CheckControllerInput();
    }

    void InitializePhotoWall()
    {
        savedTextures = new Texture2D[gridSize.x * gridSize.y];
        photoPositions = new Vector3[gridSize.x, gridSize.y];

        // 计算照片墙左下角起点位置（按4:3比例调整）
        Vector3 startPos = photoWall.position
                         - photoWall.right * (gridSize.x - 1) * (photoWidth + spacing.x) / 2
                         - photoWall.up * (gridSize.y - 1) * (photoWidth * 0.75f + spacing.y) / 2;

        // 预计算所有照片位置
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                photoPositions[x, y] = startPos
                                     + photoWall.right * x * (photoWidth + spacing.x)
                                     + photoWall.up * y * (photoWidth * 0.75f + spacing.y);
            }
        }
    }

    void CheckControllerInput()
    {
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue))
        {
            if (triggerValue && !isTriggerPressed)
            {
                isTriggerPressed = true;
                TakeScreenshot();
            }
            else if (!triggerValue && isTriggerPressed)
            {
                isTriggerPressed = false;
            }
        }
    }

    void TakeScreenshot()
    {
        StartCoroutine(CaptureAndCrop());
    }

    IEnumerator CaptureAndCrop()
    {
        yield return new WaitForEndOfFrame();

        // 原始截图
        Texture2D fullTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
        fullTex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        fullTex.Apply();

        // 计算裁剪区域（保持宽度不变，裁剪高度）
        int targetHeight = Screen.width * 3 / 4; // 计算4:3所需高度
        int cropY = (Screen.height - targetHeight) / 2; // 上下各裁掉的部分

        // 创建4:3纹理
        Texture2D croppedTex = new Texture2D(Screen.width, targetHeight, TextureFormat.RGBA32, false);

        // 裁剪上下部分
        Color[] pixels = fullTex.GetPixels(0, cropY, Screen.width, targetHeight);
        croppedTex.SetPixels(pixels);
        croppedTex.Apply();
        Destroy(fullTex);

        // 保存到照片墙
        SavePhotoToWall(croppedTex);
    }

    void SavePhotoToWall(Texture2D newPhoto)
    {
        // 存储新照片
        savedTextures[currentPhotoIndex] = newPhoto;
        currentPhotoIndex = (currentPhotoIndex + 1) % (gridSize.x * gridSize.y);

        // 更新照片墙显示
        UpdatePhotoWallDisplay();
    }

    void UpdatePhotoWallDisplay()
    {
        // 清除现有照片
        foreach (Transform child in photoWall)
        {
            Destroy(child.gameObject);
        }

        // 重新排列所有照片
        for (int i = 0; i < savedTextures.Length; i++)
        {
            if (savedTextures[i] == null) continue;

            int x = i % gridSize.x;
            int y = i / gridSize.x;
            DisplayPhotoOnWall(savedTextures[i], photoPositions[x, y]);
        }
    }

    void DisplayPhotoOnWall(Texture2D texture, Vector3 position)
    {
        GameObject photo = Instantiate(screenshotPrefab, position, Quaternion.identity, photoWall);
        photo.transform.rotation = photoWall.rotation;

        // 固定4:3显示比例
        photo.transform.localScale = new Vector3(photoWidth, photoWidth * 0.75f, 0.01f);

        // 应用标准材质
        Material newMat = new Material(Shader.Find("Unlit/Texture"));
        newMat.mainTexture = texture;
        photo.GetComponent<Renderer>().material = newMat;
    }
}