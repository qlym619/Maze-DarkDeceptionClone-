using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float mouseSensitivity = 2f;
    public float jumpHeight = 2f;
    
    // 新增：方向性速度设置
    [Header("方向性速度设置")]
    public float forwardSpeedMultiplier = 1.0f;    // 向前速度系数
    public float sideSpeedMultiplier = 0.7f;       // 侧向速度系数
    public float backwardSpeedMultiplier = 0.5f;   // 后退速度系数

    [Header("物理设置")]
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("生命值")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("冲刺技能")]
    public float sprintDuration = 3f;
    public float sprintCooldown = 5f;
    public bool canSprint = true;
    public bool isSprinting = false;

    [Header("屏幕震动设置")]
    public float runShakeDuration = 0.2f;      // 奔跑震动持续时间
    public float runShakeMagnitude = 0.01f;    // 奔跑震动幅度（平缓）
    public float damageShakeDuration = 0.3f;   // 受伤震动持续时间
    public float damageShakeMagnitude = 0.08f; // 受伤震动幅度（更剧烈）
    public float shakeInterval = 0.3f;         // 奔跑震动间隔

    private CharacterController controller;
    private Camera playerCamera;
    private float xRotation = 0f;

    // 物理和跳跃相关
    private Vector3 velocity;
    private bool isGrounded;

    // 输入变量
    private float horizontalInput;
    private float verticalInput;
    private float mouseX;
    private float mouseY;

    [Header("头部晃动设置")]
    public float headBobFrequency = 3f; // 晃动频率
    public float headBobAmplitude = 0.2f; // 晃动幅度
    public float headBobSmoothTime = 0.1f; // 平滑时间
    
    private Vector3 headBobOriginalPosition;
    private float headBobTimer = 0f;
    private Vector3 headBobVelocity = Vector3.zero;
    
    // 屏幕震动相关
    private Vector3 cameraOriginalPosition;
    private bool isScreenShaking = false;
    private float lastShakeTime = 0f;

    public static PlayerController Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        currentHealth = maxHealth;

        // 如果没有设置地面检测点，使用默认位置
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = groundCheckObj.transform;
        }

        // 初始化光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 记录相机原始位置
        headBobOriginalPosition = playerCamera.transform.localPosition;
        cameraOriginalPosition = playerCamera.transform.localPosition;

        // 新增：设置玩家起始视角顺时针旋转90度
        transform.Rotate(Vector3.up * 90f);
        Debug.Log("玩家起始视角已顺时针旋转90度");
    }

    void Update()
    {
        if (GameManager.Instance.IsGameActive())
        {
            HandleGravityAndGround();
            GetInput();
            HandleMovement();
            HandleMouseLook();
            HandleSprint();
            HandleJump();
            HandleHeadBob();
            HandleRunShake(); // 新增：处理奔跑时的屏幕震动
        }
    }

    void HandleGravityAndGround()
    {
        // 检查是否在地面
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 略微向下的力确保玩家停在地面
        }

        // 应用重力
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
    }

    void HandleMovement()
    {
        Vector3 move = transform.right * horizontalInput + transform.forward * verticalInput;
        float currentSpeed = isSprinting ? runSpeed : walkSpeed;
        
        // 新增：计算方向性速度
        float speedMultiplier = CalculateDirectionalSpeedMultiplier(horizontalInput, verticalInput);
        currentSpeed *= speedMultiplier;

        // 只移动X和Z轴，Y轴由重力控制
        Vector3 horizontalMove = new Vector3(move.x, 0, move.z) * currentSpeed * Time.deltaTime;
        controller.Move(horizontalMove);
    }
    
    // 新增：计算方向性速度系数的方法
    float CalculateDirectionalSpeedMultiplier(float horizontal, float vertical)
    {
        // 如果没有输入，返回默认值
        if (Mathf.Approximately(horizontal, 0) && Mathf.Approximately(vertical, 0))
            return 1.0f;
            
        // 计算输入向量的角度（相对于玩家前方）
        Vector2 inputDirection = new Vector2(horizontal, vertical);
        float inputAngle = Mathf.Atan2(inputDirection.x, inputDirection.y) * Mathf.Rad2Deg;
        
        // 将角度标准化到0-360度
        if (inputAngle < 0) inputAngle += 360;
        
        // 根据角度确定速度系数
        if (inputAngle >= 315 || inputAngle <= 45) // 前方（315-45度）
            return forwardSpeedMultiplier;
        else if (inputAngle >= 45 && inputAngle <= 135) // 右侧（45-135度）
            return sideSpeedMultiplier;
        else if (inputAngle >= 135 && inputAngle <= 225) // 后方（135-225度）
            return backwardSpeedMultiplier;
        else // 左侧（225-315度）
            return sideSpeedMultiplier;
    }

    void HandleMouseLook()
    {
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleSprint()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canSprint && (horizontalInput != 0 || verticalInput != 0))
        {
            StartCoroutine(ActivateSprint());
        }
    }

    IEnumerator ActivateSprint()
    {
        isSprinting = true;
        canSprint = false;

        yield return new WaitForSeconds(sprintDuration);

        isSprinting = false;

        yield return new WaitForSeconds(sprintCooldown);

        canSprint = true;
    }

    public void TakeDamage(int damage)
    {
        if (!GameManager.Instance.IsGameActive()) return;

        currentHealth -= damage;
        UIManager.Instance.UpdateHealthUI(currentHealth);

        Debug.Log($"玩家受伤，剩余生命: {currentHealth}");

        // 新增：受伤时触发剧烈屏幕震动
        StartCoroutine(ScreenShake(damageShakeDuration, damageShakeMagnitude));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        GameManager.Instance.GameOver(false);
        Debug.Log("玩家死亡！");
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    void HandleHeadBob()
    {
        // 只有在奔跑且有移动输入时才晃动
        if (isSprinting && (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f))
        {
            headBobTimer += Time.deltaTime * headBobFrequency;
            
            // 使用正弦波计算上下晃动
            float newY = Mathf.Sin(headBobTimer) * headBobAmplitude;
            Vector3 targetPosition = headBobOriginalPosition + Vector3.up * newY;
            
            // 平滑过渡到目标位置
            playerCamera.transform.localPosition = Vector3.SmoothDamp(
                playerCamera.transform.localPosition, 
                targetPosition, 
                ref headBobVelocity, 
                headBobSmoothTime
            );
        }
        else
        {
            // 平滑回到原始位置
            playerCamera.transform.localPosition = Vector3.SmoothDamp(
                playerCamera.transform.localPosition, 
                headBobOriginalPosition, 
                ref headBobVelocity, 
                headBobSmoothTime
            );
            
            // 重置计时器
            headBobTimer = 0f;
        }
    }
    
    // 新增：处理奔跑时的屏幕震动
    void HandleRunShake()
    {
        // 只有在奔跑且有移动输入时才触发震动
        if (isSprinting && (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f))
        {
            // 检查震动间隔
            if (Time.time - lastShakeTime >= shakeInterval && !isScreenShaking)
            {
                StartCoroutine(ScreenShake(runShakeDuration, runShakeMagnitude));
                lastShakeTime = Time.time;
            }
        }
    }
    
    // 新增：屏幕震动协程
    System.Collections.IEnumerator ScreenShake(float duration, float magnitude)
    {
        if (isScreenShaking) yield break;
        
        isScreenShaking = true;
        Vector3 originalPosition = playerCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            playerCamera.transform.localPosition = new Vector3(
                originalPosition.x + x, 
                originalPosition.y + y, 
                originalPosition.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.localPosition = originalPosition;
        isScreenShaking = false;
    }
}