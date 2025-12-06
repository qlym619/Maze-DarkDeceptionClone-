using UnityEngine;
using System.Collections.Generic;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [System.Serializable]
    public class EffectPool
    {
        public GameObject prefab;
        public int poolSize = 20;
        public Queue<GameObject> pool = new Queue<GameObject>();
    }

    [Header("特效预制体")]
    public GameObject coinCollectEffect;
    public GameObject coinGlowEffect;

    [Header("对象池设置")]
    public int coinEffectPoolSize = 30;

    private Queue<GameObject> coinEffectPool = new Queue<GameObject>();
    private List<GameObject> activeEffects = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializePools()
    {
        // 初始化金币收集特效池
        if (coinCollectEffect)
        {
            for (int i = 0; i < coinEffectPoolSize; i++)
            {
                GameObject effect = Instantiate(coinCollectEffect, transform);
                effect.SetActive(false);
                coinEffectPool.Enqueue(effect);
            }
        }

        Debug.Log($"特效池初始化完成: 金币特效 {coinEffectPool.Count}个");
    }

    public GameObject GetCoinEffect()
    {
        if (coinEffectPool.Count > 0)
        {
            GameObject effect = coinEffectPool.Dequeue();
            activeEffects.Add(effect);
            return effect;
        }
        else
        {
            // 如果池子空了，创建新的（动态扩容）
            GameObject effect = Instantiate(coinCollectEffect, transform);
            activeEffects.Add(effect);
            return effect;
        }
    }

    public void ReturnCoinEffect(GameObject effect)
    {
        if (effect && effect.activeSelf)
        {
            effect.SetActive(false);

            // 重置粒子系统
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps)
            {
                ps.Clear();
                ps.Stop();
            }

            coinEffectPool.Enqueue(effect);
            activeEffects.Remove(effect);
        }
    }

    public void PlayCoinEffect(Vector3 position, Quaternion rotation = default)
    {
        GameObject effect = GetCoinEffect();
        if (effect)
        {
            effect.transform.position = position;
            effect.transform.rotation = rotation;
            effect.SetActive(true);

            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps) ps.Play();

            // 自动回收
            StartCoroutine(ReturnEffectAfterDelay(effect, 1.5f));
        }
    }

    System.Collections.IEnumerator ReturnEffectAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnCoinEffect(effect);
    }

    void Update()
    {
        // 清理无效引用
        activeEffects.RemoveAll(effect => effect == null);
    }

    // 批量播放特效（适合同时收集多个金币）
    public void PlayCoinEffects(List<Vector3> positions)
    {
        foreach (Vector3 pos in positions)
        {
            PlayCoinEffect(pos);
        }
    }

    // 编辑器辅助：预览特效
    [ContextMenu("测试金币特效")]
    public void TestCoinEffect()
    {
        PlayCoinEffect(Vector3.zero);
    }
}