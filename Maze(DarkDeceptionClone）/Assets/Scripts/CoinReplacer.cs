// 创建一个临时替换脚本CoinReplacer.cs
using UnityEngine;
using System.Collections.Generic;

public class CoinReplacer : MonoBehaviour
{
    public GameObject newCoinPrefab;

    [ContextMenu("替换所有旧金币")]
    public void ReplaceAllCoins()
    {
        // 查找所有旧的金币（Sphere）
        GameObject[] oldCoins = GameObject.FindGameObjectsWithTag("Coin");

        Debug.Log($"找到 {oldCoins.Length} 个旧金币");

        List<Vector3> coinPositions = new List<Vector3>();

        // 记录所有旧金币的位置
        foreach (GameObject oldCoin in oldCoins)
        {
            coinPositions.Add(oldCoin.transform.position);
            DestroyImmediate(oldCoin);
        }

        // 在新位置创建新金币
        foreach (Vector3 position in coinPositions)
        {
            GameObject newCoin = Instantiate(newCoinPrefab, position, Quaternion.identity);
            newCoin.name = "Coin";

            // 如果你有父对象来组织金币，可以设置
            if (transform.parent)
                newCoin.transform.parent = transform.parent;
        }

        Debug.Log($"已创建 {coinPositions.Count} 个新金币");
    }
}