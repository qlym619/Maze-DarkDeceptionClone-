using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
    [Header("金币设置")]
    public int value = 1;
    public float rotationSpeed = 100f;
    public float floatHeight = 0.5f;
    public float floatSpeed = 1f;

    [Header("收集效果")]
    public float collectEffectDuration = 1f;
    public float fadeOutDuration = 0.3f;
    public float shrinkDuration = 0.2f;

    [Header("视觉效果")]
    public bool useGlowEffect = true;
    public float glowIntensity = 1.5f;
    public Color glowColor = new Color(1, 0.8f, 0, 1);

    // 组件引用
    private Renderer coinRenderer;
    private Collider coinCollider;
    private Light glowLight; // 可选
    private Vector3 originalScale;
    private Vector3 startPosition; // 添加：浮动动画的起始位置
    private bool isCollected = false;
    private Material originalMaterial;
    private Material glowMaterial;

    void Start()
    {
        // 记录起始位置（用于浮动动画）
        startPosition = transform.position;
        
        // 获取组件引用
        coinRenderer = GetComponent<Renderer>();
        coinCollider = GetComponent<Collider>();
        originalScale = transform.localScale;
    
        // 创建发光材质（如果启用）
        if (useGlowEffect && coinRenderer)
        {
            SetupGlowEffect();
        }
    
        // 可选：添加点光源
        if (useGlowEffect)
        {
            glowLight = gameObject.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.range = 2f;
            glowLight.intensity = 0.5f;
            glowLight.color = glowColor;
            glowLight.enabled = true;
        }
    }

    void SetupGlowEffect()
    {
        originalMaterial = coinRenderer.material;

        // 创建发光材质
        glowMaterial = new Material(originalMaterial);
        glowMaterial.EnableKeyword("_EMISSION");
        glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);

        // 应用发光材质
        coinRenderer.material = glowMaterial;
    }

    void Update()
    {
        if (isCollected) return;
    
        // 旋转动画
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    
        // 浮动动画 - 修复：使用startPosition作为基准
        Vector3 floatPosition = startPosition;
        floatPosition.y += Mathf.Sin(Time.time * floatSpeed) * floatHeight * 0.5f;
        transform.position = floatPosition;
    
        // 发光脉冲效果
        if (useGlowEffect && glowMaterial)
        {
            float pulse = Mathf.PingPong(Time.time, 0.5f) + 0.5f;
            Color emissionColor = glowColor * glowIntensity * pulse;
            glowMaterial.SetColor("_EmissionColor", emissionColor);
    
            if (glowLight)
            {
                glowLight.intensity = 0.3f + pulse * 0.2f;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        if (isCollected) return;

        isCollected = true;

        // 通知游戏管理器
        GameManager.Instance.CollectCoin();

        // 播放收集特效
        PlayCollectEffects();

        // 执行收集动画
        StartCoroutine(CollectAnimation());
    }

    void PlayCollectEffects()
    {
        // 播放粒子特效
        if (EffectManager.Instance)
        {
            EffectManager.Instance.PlayCoinEffect(transform.position);
        }
    
        // 播放声音（如果将来添加）
        // AudioManager.Instance.PlayCoinCollect();
    
        // 移除屏幕震动效果，避免给玩家带来不适感
        // StartCoroutine(ScreenShake(0.1f, 0.05f));
    }

    System.Collections.IEnumerator CollectAnimation()
    {
        // 第1阶段：快速缩放消失
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shrinkDuration;

            // 缩放效果
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            // 旋转加速
            transform.Rotate(0, rotationSpeed * 3 * Time.deltaTime, 0);

            yield return null;
        }

        // 禁用碰撞体
        if (coinCollider) coinCollider.enabled = false;

        // 第2阶段：淡出效果（如果材质支持透明度）
        elapsedTime = 0f;

        if (coinRenderer && coinRenderer.material.HasProperty("_Color"))
        {
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeOutDuration;
                float alpha = Mathf.Lerp(1f, 0f, t);

                // 设置透明度
                Color color = coinRenderer.material.color;
                color.a = alpha;
                coinRenderer.material.color = color;

                yield return null;
            }
        }

        // 完全隐藏
        if (coinRenderer) coinRenderer.enabled = false;
        if (glowLight) glowLight.enabled = false;

        // 等待特效播放完成
        yield return new WaitForSeconds(0.5f);

        // 销毁对象
        Destroy(gameObject);
    }

    System.Collections.IEnumerator ScreenShake(float duration, float magnitude)
    {
        // 简单的屏幕震动效果
        Vector3 originalPosition = Camera.main.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = new Vector3(x, y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPosition;
    }

    // 鼠标悬停效果（可选）
    void OnMouseEnter()
    {
        if (isCollected) return;

        // 悬停时放大
        transform.localScale = originalScale * 1.1f;

        // 增强发光
        if (glowMaterial)
        {
            glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity * 2f);
        }
    }

    void OnMouseExit()
    {
        if (isCollected) return;

        // 恢复原大小
        transform.localScale = originalScale;

        // 恢复发光强度
        if (glowMaterial)
        {
            glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
        }
    }

    // 可视化辅助
    void OnDrawGizmos()
    {
        if (!isCollected && useGlowEffect)
        {
            Gizmos.color = new Color(1, 0.8f, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}