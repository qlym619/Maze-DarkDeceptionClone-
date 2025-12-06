using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIIconManager : MonoBehaviour
{
    [Header("图标引用")]
    public Image coinIcon;
    public Image healthIcon;
    public Image timeIcon;

    [Header("图标图片")]
    public Sprite coinSprite;
    public Sprite healthSprite;
    public Sprite timeSprite;

    [Header("文本组件")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI timeText;

    [Header("按钮组件")]
    public Button pauseButton;
    public Button restartButton;
    public Button resumeButton;
    public Button mainMenuButton;

    [Header("按钮图片")]
    public Sprite pauseBtnNormal;
    public Sprite pauseBtnPressed;
    public Sprite restartBtnNormal;
    public Sprite restartBtnPressed;
    public Sprite resumeBtnNormal;
    public Sprite resumeBtnPressed;
    public Sprite menuBtnNormal;
    public Sprite menuBtnPressed;

    void Start()
    {
        SetupIcons();
        SetupButtons();
    }

    void SetupIcons()
    {
        // 设置图标
        if (coinIcon && coinSprite)
            coinIcon.sprite = coinSprite;

        if (healthIcon && healthSprite)
            healthIcon.sprite = healthSprite;

        if (timeIcon && timeSprite)
            timeIcon.sprite = timeSprite;

        // 设置文本样式
        if (coinText)
        {
            coinText.color = Color.yellow;
            coinText.fontSize = 24;
        }

        if (healthText)
        {
            healthText.color = Color.red;
            healthText.fontSize = 24;
        }

        if (timeText)
        {
            timeText.color = Color.white;
            timeText.fontSize = 24;
        }
    }

    void SetupButtons()
    {
        // 设置暂停按钮
        if (pauseButton)
        {
            Image btnImage = pauseButton.GetComponent<Image>();
            if (btnImage && pauseBtnNormal)
            {
                btnImage.sprite = pauseBtnNormal;
            }

            // 设置按钮点击效果
            SetupButtonSpriteSwap(pauseButton, pauseBtnNormal, pauseBtnPressed);
        }

        // 设置重新开始按钮
        if (restartButton)
        {
            SetupButtonSpriteSwap(restartButton, restartBtnNormal, restartBtnPressed);
        }

        // 设置继续按钮（暂停菜单中）
        if (resumeButton)
        {
            SetupButtonSpriteSwap(resumeButton, resumeBtnNormal, resumeBtnPressed);
        }

        // 设置主菜单按钮
        if (mainMenuButton)
        {
            SetupButtonSpriteSwap(mainMenuButton, menuBtnNormal, menuBtnPressed);
        }
    }

    void SetupButtonSpriteSwap(Button button, Sprite normalSprite, Sprite pressedSprite)
    {
        if (!button || !normalSprite) return;

        // 确保按钮有Image组件
        Image image = button.GetComponent<Image>();
        if (!image) image = button.gameObject.AddComponent<Image>();

        image.sprite = normalSprite;

        // 设置按钮的Sprite Swap
        button.transition = Selectable.Transition.SpriteSwap;

        SpriteState spriteState = button.spriteState;
        spriteState.pressedSprite = pressedSprite;
        button.spriteState = spriteState;

        // 删除按钮上的Text（如果有）
        Text textComponent = button.GetComponentInChildren<Text>();
        if (textComponent) Destroy(textComponent);

        TextMeshProUGUI tmpComponent = button.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpComponent) Destroy(tmpComponent);
    }

    // 更新图标显示
    public void UpdateCoinDisplay(int coins, int total)
    {
        if (coinText)
            coinText.text = $"{coins}/{total}";
    }

    public void UpdateHealthDisplay(int health, int maxHealth)
    {
        if (healthText)
            healthText.text = $"{health}";
    }

    public void UpdateTimeDisplay(float remainingTime)
    {
        if (timeText)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}