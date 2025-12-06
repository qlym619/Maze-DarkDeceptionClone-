using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("游戏UI")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI timerText;
    public Image sprintProgressImage;
    public Image sprintCooldownImage;
    public Image healthBarFill;
    public GameObject healthContainer;

    [Header("图标引用")]
    public Image coinIcon;
    public Image healthIcon;
    public Image timeIcon;

    [Header("消息系统")]
    public TextMeshProUGUI messageText;
    public Animator messageAnimator;

    [Header("菜单UI")]
    public GameObject pauseMenu;
    public GameObject gameEndPanel;
    public TextMeshProUGUI gameEndTitle;
    public TextMeshProUGUI gameEndStats;

    // 暂停菜单中的按钮
    public Button pauseRestartButton;
    public Button pauseMainMenuButton;
    public Button pauseContinueButton;

    // 游戏结束界面中的按钮
    public Button gameEndRestartButton;
    public Button gameEndMainMenuButton;

    [Header("小地图")]
    public RawImage minimapRender;
    public Camera minimapCamera;
    public RectTransform playerMinimapIcon;
    public Image minimapBorder;

    [Header("效果")]
    public Animator damageEffect;
    public ParticleSystem coinEffect;

    private float messageTimer = 0f;
    private bool showMessage = false;

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
        // 延迟初始化，等待其他管理器创建
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(0.1f);

        // 等待GameManager和PlayerController实例化
        while (GameManager.Instance == null || PlayerController.Instance == null)
        {
            yield return null;
        }

        // 初始化UI
        UpdateCoinUI(0, GameManager.Instance.totalCoins);
        UpdateHealthUI(PlayerController.Instance.maxHealth);
        UpdateGameTimer(GameManager.Instance.gameTimeLimit);

        // 隐藏菜单
        HidePauseMenu();
        HideGameEndScreen();
        HideMessage();

        // 设置暂停菜单按钮事件
        if (pauseRestartButton)
            pauseRestartButton.onClick.AddListener(() => GameManager.Instance.RestartGame());

        if (pauseMainMenuButton)
            pauseMainMenuButton.onClick.AddListener(() => GameManager.Instance.QuitToMainMenu());

        if (pauseContinueButton)
            pauseContinueButton.onClick.AddListener(() => GameManager.Instance.TogglePause());

        // 设置游戏结束界面按钮事件
        if (gameEndRestartButton)
            gameEndRestartButton.onClick.AddListener(() => GameManager.Instance.RestartGame());

        if (gameEndMainMenuButton)
            gameEndMainMenuButton.onClick.AddListener(() => GameManager.Instance.QuitToMainMenu());
    }

    void Update()
    {
        // 更新消息计时器
        if (showMessage)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0)
            {
                HideMessage();
            }
        }

        // 更新游戏计时器
        if (GameManager.Instance != null && GameManager.Instance.IsGameActive())
        {
            float remainingTime = Mathf.Max(0f, GameManager.Instance.gameTimeLimit - GameManager.Instance.GetGameTimer());
            UpdateGameTimer(remainingTime);
        }

        // 更新小地图
        UpdateMinimap();
    }

    // 设置图标图片（可以从外部调用）
    public void SetIconSprites(Sprite coinSprite, Sprite healthSprite, Sprite timeSprite)
    {
        if (coinIcon && coinSprite) coinIcon.sprite = coinSprite;
        if (healthIcon && healthSprite) healthIcon.sprite = healthSprite;
        if (timeIcon && timeSprite) timeIcon.sprite = timeSprite;
    }

    // 设置按钮图片（正常状态和按下状态）
    public void SetupButtonSprites(Button button, Sprite normalSprite, Sprite pressedSprite)
    {
        if (!button || !normalSprite) return;

        Image image = button.GetComponent<Image>();
        if (!image) image = button.gameObject.AddComponent<Image>();

        image.sprite = normalSprite;
        button.transition = Selectable.Transition.SpriteSwap;

        SpriteState spriteState = button.spriteState;
        spriteState.pressedSprite = pressedSprite;
        button.spriteState = spriteState;

        // 删除按钮上的文字（如果有）
        Text textComponent = button.GetComponentInChildren<Text>();
        if (textComponent) Destroy(textComponent);

        TextMeshProUGUI tmpComponent = button.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpComponent) Destroy(tmpComponent);
    }

    public void UpdateCoinUI(int collected, int total)
    {
        if (coinText)
        {
            coinText.text = $"{collected}/{total}";
        }
    }

    public void UpdateHealthUI(int health)
    {
        if (healthText)
        {
            healthText.text = $"{health}";
        }
    }

    public void UpdateGameTimer(float remainingTime)
    {
        if (timerText)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    public void UpdateSprintProgress(float progress)
    {
        if (sprintProgressImage)
        {
            sprintProgressImage.fillAmount = progress;
        }
    }

    public void UpdateSprintCooldown(float cooldownProgress)
    {
        if (sprintCooldownImage)
        {
            sprintCooldownImage.fillAmount = cooldownProgress;
        }
    }

    public void ShowMessage(string message, float duration = 3f)
    {
        if (messageText && messageAnimator)
        {
            messageText.text = message;
            messageAnimator.SetTrigger("Show");
            showMessage = true;
            messageTimer = duration;
        }
    }

    public void HideMessage()
    {
        if (messageText && messageAnimator)
        {
            messageAnimator.SetTrigger("Hide");
            showMessage = false;
        }
    }

    public void ShowDamageEffect()
    {
        if (damageEffect)
        {
            damageEffect.SetTrigger("Damage");
        }
    }

    public void ShowPauseMenu()
    {
        if (pauseMenu)
        {
            pauseMenu.SetActive(true);
        }
    }

    public void HidePauseMenu()
    {
        if (pauseMenu)
        {
            pauseMenu.SetActive(false);
        }
    }

    public void ShowGameEndScreen(bool isWin, int coinsCollected, float timeElapsed)
    {
        if (gameEndPanel)
        {
            gameEndPanel.SetActive(true);
    
            // 确保游戏结束界面可以交互
            CanvasGroup canvasGroup = gameEndPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameEndPanel.AddComponent<CanvasGroup>();
            
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
    
            // 释放鼠标锁定，确保可以点击按钮
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
    
            if (gameEndTitle)
            {
                gameEndTitle.text = "END";
                gameEndTitle.color = Color.white;
                gameEndTitle.fontSize = 72;
            }
    
            // 只显示用时和收集数量
            if (gameEndStats)
            {
                gameEndStats.gameObject.SetActive(true);
                
                int minutes = Mathf.FloorToInt(timeElapsed / 60f);
                int seconds = Mathf.FloorToInt(timeElapsed % 60f);
    
                // 显示用时和收集数量（带总数标签），向下移动一点
                gameEndStats.text =
                    "\n\n" +  // 添加两个空行，让文本向下移动
                    $"<size=48><color=white>{coinsCollected} /101</color></size>\n" +
                    $"<size=48><color=white>{minutes:00}:{seconds:00}</color></size>";
                
                gameEndStats.alignment = TextAlignmentOptions.Center;
            }
            
            // 重新绑定按钮事件，确保事件正确
            RebindGameEndButtons();
        }
    }

    public void HideGameEndScreen()
    {
        if (gameEndPanel)
        {
            // 恢复鼠标锁定
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            gameEndPanel.SetActive(false);
        }
    }

    // 重新绑定游戏结束界面按钮事件
    private void RebindGameEndButtons()
    {
        if (gameEndRestartButton)
        {
            gameEndRestartButton.onClick.RemoveAllListeners();
            gameEndRestartButton.onClick.AddListener(() => 
            {
                Debug.Log("重新开始按钮被点击");
                GameManager.Instance.RestartGame();
            });
        }
    
        if (gameEndMainMenuButton)
        {
            gameEndMainMenuButton.onClick.RemoveAllListeners();
            gameEndMainMenuButton.onClick.AddListener(() => 
            {
                Debug.Log("主菜单按钮被点击");
                GameManager.Instance.QuitToMainMenu();
            });
        }
    }

    void UpdateMinimap()
    {
        if (minimapCamera && playerMinimapIcon && PlayerController.Instance)
        {
            // 更新小地图相机位置
            Vector3 playerPos = PlayerController.Instance.GetPosition();
            minimapCamera.transform.position = new Vector3(playerPos.x, minimapCamera.transform.position.y, playerPos.z);

            // 更新玩家图标旋转
            float playerRotation = PlayerController.Instance.transform.eulerAngles.y;
            playerMinimapIcon.rotation = Quaternion.Euler(0, 0, -playerRotation);
        }
    }

    public void ToggleMinimap(bool show)
    {
        if (minimapRender)
        {
            minimapRender.gameObject.SetActive(show);
        }
    }
}