using UnityEngine;

public class GazeFadeController : MonoBehaviour
{
    public float maxDistance = 10f;
    public LayerMask interactableLayer;

    private Camera _vrCamera;

    void Start()
    {
        _vrCamera = Camera.main; // 获取VR主摄像头
    }

    void Update()
    {
        // 从摄像头中心发射射线
        Ray ray = new Ray(_vrCamera.transform.position, _vrCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, interactableLayer))
        {
            GazeFadeObject gazeObject = hit.collider.GetComponent<GazeFadeObject>();
            if (gazeObject != null)
            {
                gazeObject.OnGazeStart();
            }
        }
    }
}