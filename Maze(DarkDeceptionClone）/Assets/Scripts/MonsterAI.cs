using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MonsterAI : MonoBehaviour
{
    [Header("AI设置")]
    public float normalSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionRange = 8f;
    public float attackRange = 1.5f;
    public float patrolWaitTime = 2f;
    public float cooldownTime = 2f; // 攻击冷却等待时间

    [Header("攻击设置")]
    public float attackDelay = 0.2f; // 攻击前短暂延迟（增强压迫感）
    public int attackDamage = 1; // 攻击伤害
    public float attackRecoveryTime = 1.5f; // 攻击后停顿时间
    public bool enableContinuousDamage = false; // 是否启用连续伤害（现在默认关闭）
    public float continuousDamageInterval = 0.5f; // 连续伤害间隔

    [Header("碰撞避免设置")]
    public float verticalAttackThreshold = 0.5f; // 垂直攻击阈值（避免浮空怪物把玩家卡到地下）
    public float safeAttackDistance = 1.0f; // 安全攻击距离

    [Header("怪物高度设置")]
    public float monsterHeight = 0.1f; // 怪物浮空高度（进一步降低到0.1米）

    [Header("巡逻点")]
    public Transform[] patrolPoints;

    private NavMeshAgent agent;
    private Transform player;

    private enum State { Patrol, Chase, Attack, Cooldown, Recovery }
    private State currentState = State.Patrol;

    private int currentPatrolIndex = 0;
    private float stateTime = 0f;
    private bool hasDetectedPlayer = false;
    private float lastDamageTime = 0f; // 上次造成伤害的时间

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (agent == null)
        {
            Debug.LogError("怪物缺少NavMeshAgent组件");
            return;
        }

        agent.speed = normalSpeed;
        agent.stoppingDistance = attackRange - 0.2f; // 略微小于攻击范围
        
        // 设置怪物初始高度
        Vector3 currentPosition = transform.position;
        currentPosition.y = monsterHeight;
        transform.position = currentPosition;

        // 设置初始状态
        if (patrolPoints.Length > 0)
        {
            MoveToPatrolPoint();
        }
        else
        {
            // 如果没有巡逻点，直接开始追逐
            currentState = State.Chase;
        }

        Debug.Log("怪物AI初始化完成，状态: " + currentState + ", 高度: " + monsterHeight + "米");
    }

    void Update()
    {
        if (!GameManager.Instance.IsGameActive())
        {
            agent.isStopped = true;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                UpdatePatrolState(distanceToPlayer);
                break;
            case State.Chase:
                UpdateChaseState(distanceToPlayer);
                break;
            case State.Attack:
                UpdateAttackState(distanceToPlayer);
                break;
            case State.Cooldown:
                UpdateCooldownState(distanceToPlayer);
                break;
            case State.Recovery:
                UpdateRecoveryState(distanceToPlayer);
                break;
        }
    }

    void UpdatePatrolState(float distanceToPlayer)
    {
        // 检测玩家
        if (distanceToPlayer <= detectionRange)
        {
            if (!hasDetectedPlayer)
            {
                OnPlayerDetected();
            }
            ChangeState(State.Chase);
            return;
        }

        // 巡逻逻辑
        if (patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            stateTime += Time.deltaTime;

            if (stateTime >= patrolWaitTime)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                MoveToPatrolPoint();
                stateTime = 0f;
            }
        }
    }

    void UpdateChaseState(float distanceToPlayer)
    {
        // 追逐玩家
        agent.isStopped = false;
        agent.SetDestination(player.position);
        agent.speed = chaseSpeed;

        // 检查攻击范围 - 使用安全距离判断
        if (distanceToPlayer <= safeAttackDistance && CanAttackPlayer())
        {
            ChangeState(State.Attack);
        }
        // 如果玩家超出检测范围，返回巡逻
        else if (distanceToPlayer > detectionRange * 1.5f && patrolPoints.Length > 0)
        {
            hasDetectedPlayer = false;
            ChangeState(State.Patrol);
        }
    }

    void UpdateAttackState(float distanceToPlayer)
    {
        // 停止移动，准备攻击
        agent.isStopped = true;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }

        stateTime += Time.deltaTime;

        // 检查是否可以进行安全攻击
        if (!CanAttackPlayer())
        {
            // 如果垂直距离过大，返回追逐状态
            ChangeState(State.Chase);
            return;
        }

        // 执行攻击
        if (stateTime >= attackDelay)
        {
            AttackPlayer();
            stateTime = 0f;
            
            // 进入恢复状态（停顿）
            ChangeState(State.Recovery);
        }

        // 如果玩家离开安全攻击范围，返回追逐
        if (distanceToPlayer > safeAttackDistance + 0.5f)
        {
            ChangeState(State.Chase);
        }
    }

    void UpdateRecoveryState(float distanceToPlayer)
    {
        // 恢复状态，停止移动
        agent.isStopped = true;

        stateTime += Time.deltaTime;

        // 恢复时间结束后，根据玩家距离选择下一个状态
        if (stateTime >= attackRecoveryTime)
        {
            if (distanceToPlayer <= safeAttackDistance && CanAttackPlayer())
            {
                // 玩家仍在安全攻击范围内，继续攻击
                ChangeState(State.Attack);
            }
            else if (distanceToPlayer <= detectionRange)
            {
                // 玩家在检测范围内但不在攻击范围内，继续追逐
                ChangeState(State.Chase);
            }
            else if (patrolPoints.Length > 0)
            {
                // 玩家超出检测范围，返回巡逻
                hasDetectedPlayer = false;
                ChangeState(State.Patrol);
            }
            else
            {
                // 没有巡逻点，保持追逐状态
                ChangeState(State.Chase);
            }
        }
    }

    void UpdateCooldownState(float distanceToPlayer)
    {
        // 冷却状态，停止移动
        agent.isStopped = true;

        stateTime += Time.deltaTime;

        // 冷却时间结束后，根据玩家距离选择下一个状态
        if (stateTime >= cooldownTime)
        {
            if (distanceToPlayer <= safeAttackDistance && CanAttackPlayer())
            {
                // 玩家仍在安全攻击范围内，继续攻击
                ChangeState(State.Attack);
            }
            else if (distanceToPlayer <= detectionRange)
            {
                // 玩家在检测范围内但不在攻击范围内，继续追逐
                ChangeState(State.Chase);
            }
            else if (patrolPoints.Length > 0)
            {
                // 玩家超出检测范围，返回巡逻
                hasDetectedPlayer = false;
                ChangeState(State.Patrol);
            }
            else
            {
                // 没有巡逻点，保持追逐状态
                ChangeState(State.Chase);
            }
        }
    }

    // 检查是否可以安全攻击玩家（避免浮空怪物把玩家卡到地下）
    bool CanAttackPlayer()
    {
        float verticalDistance = Mathf.Abs(transform.position.y - player.position.y);
        
        // 如果垂直距离过大，不允许攻击
        if (verticalDistance > verticalAttackThreshold)
        {
            Debug.Log("垂直距离过大，取消攻击");
            return false;
        }
        
        return true;
    }

    void ChangeState(State newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        stateTime = 0f;

        switch (newState)
        {
            case State.Patrol:
                agent.isStopped = false;
                agent.speed = normalSpeed;
                if (patrolPoints.Length > 0)
                {
                    MoveToPatrolPoint();
                }
                Debug.Log("怪物切换到巡逻状态");
                break;
            case State.Chase:
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                Debug.Log("怪物切换到追逐状态");
                break;
            case State.Attack:
                Debug.Log("怪物切换到攻击状态");
                break;
            case State.Cooldown:
                Debug.Log("怪物切换到冷却状态，持续 " + cooldownTime + " 秒");
                break;
            case State.Recovery:
                Debug.Log("怪物切换到恢复状态，停顿 " + attackRecoveryTime + " 秒");
                break;
        }
    }

    void MoveToPatrolPoint()
    {
        if (patrolPoints.Length > 0 && currentPatrolIndex < patrolPoints.Length)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    void OnPlayerDetected()
    {
        hasDetectedPlayer = true;
        Debug.Log("怪物发现了玩家！");
    }

    void AttackPlayer()
    {
        // 对玩家造成伤害
        PlayerController.Instance.TakeDamage(attackDamage);
        Debug.Log($"怪物攻击玩家，造成{attackDamage}点伤害");
    }

    void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 绘制安全攻击范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, safeAttackDistance);

        // 根据状态绘制不同颜色
        Color stateColor = Color.white;
        switch (currentState)
        {
            case State.Patrol: stateColor = Color.green; break;
            case State.Chase: stateColor = Color.yellow; break;
            case State.Attack: stateColor = Color.red; break;
            case State.Cooldown: stateColor = Color.blue; break;
            case State.Recovery: stateColor = Color.magenta; break;
        }

        Gizmos.color = stateColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // 绘制当前状态
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, "状态: " + currentState, style);
#endif
    }
}