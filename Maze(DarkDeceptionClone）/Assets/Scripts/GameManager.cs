using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("游戏设置")]
    public int totalCoins = 101;
    public int coinsToWin = 101; // 需要收集10个硬币获胜
    public float gameTimeLimit = 300f;

    [Header("引用")]
    public PlayerController player;
    public Transform playerSpawnPoint;

    private int collectedCoins = 0;
    private float gameTimer = 0f;
    private bool isGameActive = false;
    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (isGameActive && !isPaused)
        {
            gameTimer += Time.deltaTime;

            // 检查时间限制
            if (gameTimer >= gameTimeLimit)
            {
                GameOver(false);
            }

            // 暂停游戏
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
    }

    public void StartGame()
    {
        collectedCoins = 0;
        gameTimer = 0f;
        isGameActive = true;
        isPaused = false;
        Time.timeScale = 1f;

        // 重置玩家位置
        if (player && playerSpawnPoint)
        {
            player.transform.position = playerSpawnPoint.position;
            player.transform.rotation = playerSpawnPoint.rotation;
            player.currentHealth = player.maxHealth;
        }

        // 更新UI
        UIManager.Instance.UpdateCoinUI(collectedCoins, totalCoins);
        UIManager.Instance.UpdateHealthUI(player.maxHealth);
        UIManager.Instance.HideGameEndScreen();
        UIManager.Instance.HidePauseMenu();

        Debug.Log("游戏开始！");
    }

    public void CollectCoin()
    {
        if (!isGameActive || isPaused) return;

        collectedCoins++;
        UIManager.Instance.UpdateCoinUI(collectedCoins, totalCoins);

        Debug.Log($"收集硬币: {collectedCoins}/{totalCoins}");

        // 检查胜利条件
        if (collectedCoins >= coinsToWin)
        {
            Victory();
        }
    }

    public void Victory()
    {
        if (!isGameActive) return;

        isGameActive = false;
        UIManager.Instance.ShowGameEndScreen(true, collectedCoins, gameTimer);

        Debug.Log("游戏胜利！");
    }

    public void GameOver(bool isWin)
    {
        if (!isGameActive) return;

        isGameActive = false;
        UIManager.Instance.ShowGameEndScreen(isWin, collectedCoins, gameTimer);

        Debug.Log("游戏结束！胜利: " + isWin);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            UIManager.Instance.ShowPauseMenu();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            UIManager.Instance.HidePauseMenu();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void RestartGame()
    {
        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void QuitToMainMenu()
    {
        // 加载主菜单场景
        SceneManager.LoadScene("MainMenu");
    }

    public bool IsGameActive()
    {
        return isGameActive && !isPaused;
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public float GetGameTimer()
    {
        return gameTimer;
    }
}