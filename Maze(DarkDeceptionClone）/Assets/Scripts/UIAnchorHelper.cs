using UnityEngine;
using UnityEngine.UI;

public class UIAnchorHelper : MonoBehaviour
{
    [System.Serializable]
    public class UIElement
    {
        public RectTransform rectTransform;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public TextAnchor alignment;
    }

    [Header("UI元素列表")]
    public UIElement[] uiElements;

    [Header("参考分辨率")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    [Header("边缘边距")]
    public float topMargin = 50f;
    public float bottomMargin = 50f;
    public float leftMargin = 50f;
    public float rightMargin = 50f;

    void Start()
    {
        UpdateUIAnchors();
    }

    void UpdateUIAnchors()
    {
        // 计算当前屏幕与参考分辨率之间的比例
        float widthRatio = Screen.width / referenceResolution.x;
        float heightRatio = Screen.height / referenceResolution.y;

        // 根据Canvas Scaler的模式计算缩放因子
        CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
        float scaleFactor = 1f;

        if (canvasScaler != null && canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            if (canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            {
                float match = canvasScaler.matchWidthOrHeight;
                scaleFactor = Mathf.Pow(widthRatio, 1 - match) * Mathf.Pow(heightRatio, match);
            }
        }

        // 更新每个UI元素
        foreach (UIElement element in uiElements)
        {
            if (element.rectTransform != null)
            {
                // 根据屏幕比例调整位置
                Vector2 adjustedPosition = new Vector2(
                    element.anchoredPosition.x * scaleFactor,
                    element.anchoredPosition.y * scaleFactor
                );

                element.rectTransform.anchoredPosition = adjustedPosition;

                // 根据屏幕比例调整大小
                if (element.sizeDelta != Vector2.zero)
                {
                    Vector2 adjustedSize = new Vector2(
                        element.sizeDelta.x * scaleFactor,
                        element.sizeDelta.y * scaleFactor
                    );
                    element.rectTransform.sizeDelta = adjustedSize;
                }
            }
        }
    }

    // 获取安全区域，确保UI不会被遮挡
    public Rect GetSafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // 转换为Canvas坐标
        RectTransform canvasRect = GetComponent<RectTransform>();
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        return new Rect(anchorMin.x, anchorMin.y, anchorMax.x - anchorMin.x, anchorMax.y - anchorMin.y);
    }

    // 编辑器功能：捕获当前UI元素位置
    [ContextMenu("捕获当前UI位置")]
    void CaptureCurrentUIPositions()
    {
        foreach (UIElement element in uiElements)
        {
            if (element.rectTransform != null)
            {
                element.anchoredPosition = element.rectTransform.anchoredPosition;
                element.sizeDelta = element.rectTransform.sizeDelta;
            }
        }
        Debug.Log("UI位置已捕获");
    }

    // 屏幕分辨率改变时自动更新
    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            UpdateUIAnchors();
        }
    }

    private int lastScreenWidth;
    private int lastScreenHeight;
}