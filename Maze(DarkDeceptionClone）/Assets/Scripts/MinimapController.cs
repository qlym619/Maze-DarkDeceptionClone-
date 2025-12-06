using UnityEngine;

public class MinimapController : MonoBehaviour
{
    [Header("小地图设置")]
    public Camera minimapCamera;
    public RectTransform playerIcon;
    public Transform player;
    public float cameraHeight = 20f;

    [Header("小地图显示范围")]
    public float minimapSize = 15f;

    void Start()
    {
        // 如果没有手动赋值，尝试自动查找
        if (!minimapCamera)
            minimapCamera = GameObject.Find("MinimapCamera")?.GetComponent<Camera>();

        if (!player)
            player = GameObject.Find("Player")?.transform;

        // 设置小地图大小
        if (minimapCamera)
        {
            minimapCamera.orthographicSize = minimapSize;
        }
    }

    void Update()
    {
        UpdateMinimap();
    }

    void UpdateMinimap()
    {
        if (!player || !minimapCamera || !playerIcon) return;

        // 更新小地图相机位置（跟随玩家，保持固定高度）
        Vector3 newCameraPos = player.position;
        newCameraPos.y = cameraHeight;
        minimapCamera.transform.position = newCameraPos;

        // 更新玩家图标在小地图上的位置
        // 由于小地图相机是正交投影居中，玩家图标应该始终在中心
        playerIcon.anchoredPosition = Vector2.zero;

        // 更新玩家图标旋转以指向正确的方向
        float playerRotation = player.eulerAngles.y;
        playerIcon.rotation = Quaternion.Euler(0, 0, -playerRotation);
    }

    // 设置小地图大小
    public void SetMinimapSize(float newSize)
    {
        minimapSize = newSize;
        if (minimapCamera)
        {
            minimapCamera.orthographicSize = minimapSize;
        }
    }
}