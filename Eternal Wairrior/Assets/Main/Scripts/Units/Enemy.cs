using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    #region Variables
    #region Stats
    public float maxHp;
    public float hp = 10f;
    public float damage = 5f;
    public float moveSpeed = 3f;
    public float mobEXP = 10f;
    public float damageInterval;
    internal float originalMoveSpeed;
    public float hpAmount
    {
        get { return hp / maxHp; }
    }
    public float preDamageTime = 0;
    public float attackRange = 1.2f;
    public float preferredDistance = 1.0f;
    public ElementType elementType = ElementType.None;

    [Header("Defense Stats")]
    public float baseDefense = 5f;
    public float currentDefense;
    public float maxDefenseReduction = 0.9f;
    public float defenseDebuffAmount = 0f;

    public float moveSpeedDebuffAmount = 0f;
    public bool isStunned = false;

    [Header("Drop Settings")]
    [SerializeField]
    public ExpParticle expParticlePrefab;

    [SerializeField]
    public int minExpParticles = 3;

    [SerializeField]
    public int maxExpParticles = 6;

    [SerializeField]
    public float dropRadiusMin = 0.5f;

    [SerializeField]
    public float dropRadiusMax = 1.5f;

    [SerializeField]
    public EnemyType enemyType;
    #endregion

    #region References
    protected Transform target;
    public Image hpBar;
    protected Rigidbody2D rb;
    public ParticleSystem attackParticle;
    public bool isInit = false;
    public Collider2D enemyCollider;
    #endregion

    #region Pathfinding
    public List<Vector2> currentPath { get; private set; }
    protected float pathUpdateTime = 0.2f;
    protected float lastPathUpdateTime;
    protected float obstaclePathUpdateDelay = 0.1f;
    protected float lastObstacleAvoidanceTime;
    protected float stuckTimer = 0f;
    protected Vector2 lastPosition;
    #endregion

    #region Constants
    protected const float STUCK_THRESHOLD = 0.1f;
    protected const float STUCK_CHECK_TIME = 0.5f;
    protected const float CORNER_CHECK_DISTANCE = 0.5f;
    protected const float WALL_AVOIDANCE_DISTANCE = 1.5f;
    protected const float MIN_CIRCLE_DISTANCE = 1f;
    #endregion

    #region Movement
    protected Vector2 previousMoveDir;
    protected bool isCirclingPlayer = false;
    protected float circlingRadius = 3f;
    protected float circlingAngle = 0f;
    protected float previousXPosition;
    #endregion

    #region Formation Variables
    private const float FORMATION_SPACING = 1.2f;
    private const float COHESION_WEIGHT = 0.3f;
    private const float ALIGNMENT_WEIGHT = 0.5f;
    private const float SEPARATION_WEIGHT = 0.8f;
    private const float FORMATION_RADIUS = 5f;
    private Vector2 formationOffset;
    #endregion

    #region Coroutines
    protected Coroutine slowEffectCoroutine;
    protected Coroutine stunCoroutine;
    protected Coroutine dotDamageCoroutine;
    protected Coroutine defenseDebuffCoroutine;
    #endregion

    private bool isQuitting = false;
    #endregion

    #region Unity Lifecycle
    protected virtual void Start()
    {
        enemyCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        InitializeComponents();
        maxHp = hp;
        originalMoveSpeed = moveSpeed;

        if (
            Application.isPlaying
            && GameManager.Instance != null
            && !GameManager.Instance.enemies.Contains(this)
        )
        {
            GameManager.Instance.enemies.Add(this);
        }

        CalculateFormationOffset();
        currentDefense = baseDefense;
    }

    protected virtual void Update()
    {
        if (!isInit)
            Initialize();
        Move();
        UpdateVisuals();
    }

    protected virtual void OnDisable()
    {
        if (slowEffectCoroutine != null)
        {
            StopCoroutine(slowEffectCoroutine);
            slowEffectCoroutine = null;
        }

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        if (dotDamageCoroutine != null)
        {
            StopCoroutine(dotDamageCoroutine);
            dotDamageCoroutine = null;
        }

        if (defenseDebuffCoroutine != null)
        {
            StopCoroutine(defenseDebuffCoroutine);
            defenseDebuffCoroutine = null;
        }

        moveSpeedDebuffAmount = 0f;
        defenseDebuffAmount = 0f;
        isStunned = false;
        moveSpeed = originalMoveSpeed;
        currentDefense = baseDefense;

        if (
            Application.isPlaying
            && !isQuitting
            && GameManager.Instance != null
            && GameManager.Instance.enemies != null
            && GameManager.Instance.enemies.Contains(this)
        )
        {
            GameManager.Instance.enemies.Remove(this);
        }
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }
    #endregion

    #region Initialization
    protected virtual void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        previousXPosition = transform.position.x;

        CalculateFormationOffset();
    }

    protected virtual void Initialize()
    {
        if (GameManager.Instance?.player != null)
        {
            target = GameManager.Instance.player.transform;
            isInit = true;
        }
    }

    protected virtual void CalculateFormationOffset()
    {
        if (GameManager.Instance == null)
            return;

        int totalEnemies = GameManager.Instance.enemies.Count;
        if (totalEnemies == 0)
        {
            formationOffset = Vector2.zero;
            return;
        }

        int index = GameManager.Instance.enemies.IndexOf(this);
        int rowSize = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(totalEnemies)));

        int row = index / rowSize;
        int col = index % rowSize;

        formationOffset = new Vector2(
            (col - rowSize / 2f) * FORMATION_SPACING,
            (row - rowSize / 2f) * FORMATION_SPACING
        );
    }
    #endregion

    #region Movement
    private Vector2 GetTargetPosition()
    {
        if (target == null)
            return transform.position;

        Vector2 directionToTarget = (
            (Vector2)target.position - (Vector2)transform.position
        ).normalized;
        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget > attackRange)
        {
            return (Vector2)target.position - directionToTarget * preferredDistance;
        }
        else if (distanceToTarget < preferredDistance)
        {
            return (Vector2)target.position + directionToTarget * preferredDistance;
        }
        else
        {
            return (Vector2)transform.position
                + new Vector2(Mathf.Sin(Time.time * 2f), Mathf.Cos(Time.time * 2f)) * 0.5f;
        }
    }

    public virtual void Move()
    {
        if (isStunned || moveSpeed <= 0)
            return;

        Node currentNode = PathFindingManager.Instance.GetNodeFromWorldPosition(transform.position);
        if (currentNode != null && !currentNode.walkable)
        {
            Vector2 safePosition = FindNearestSafePosition(transform.position);
            transform.position = Vector2.MoveTowards(
                transform.position,
                safePosition,
                moveSpeed * 2f * Time.deltaTime
            );
            return;
        }

        if (!PathFindingManager.Instance.IsPositionInGrid(transform.position))
        {
            Vector2 clampedPosition = PathFindingManager.Instance.ClampToGrid(transform.position);
            transform.position = clampedPosition;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer <= attackRange)
        {
            rb.velocity = Vector2.zero;
            Attack();
            return;
        }

        Vector2 moveToPosition =
            (Vector2)target.position
            - ((Vector2)target.position - (Vector2)transform.position).normalized
                * preferredDistance;
        MoveToPosition(moveToPosition);
    }

    private void MoveToPosition(Vector2 targetPosition)
    {
        if (ShouldUpdatePath())
        {
            List<Vector2> newPath = PathFindingManager.Instance.FindPath(
                transform.position,
                targetPosition
            );
            if (newPath != null && newPath.Count > 0)
            {
                bool isValidPath = true;
                foreach (Vector2 pathPoint in newPath)
                {
                    Node node = PathFindingManager.Instance.GetNodeFromWorldPosition(pathPoint);
                    if (node != null && !node.walkable)
                    {
                        isValidPath = false;
                        break;
                    }
                }

                if (isValidPath)
                {
                    currentPath = newPath;
                    lastPathUpdateTime = Time.time;
                    stuckTimer = 0f;
                }
                else
                {
                    Vector2 safePosition = FindSafePosition(targetPosition);
                    currentPath = PathFindingManager.Instance.FindPath(
                        transform.position,
                        safePosition
                    );
                }
            }
        }

        FollowPath();
    }

    private Vector2 FindSafePosition(Vector2 targetPosition)
    {
        float checkRadius = 2f;
        float angleStep = 45f;

        for (float angle = 0; angle < 360; angle += angleStep)
        {
            float radian = angle * Mathf.Deg2Rad;
            Vector2 checkPosition =
                targetPosition
                + new Vector2(Mathf.Cos(radian) * checkRadius, Mathf.Sin(radian) * checkRadius);

            Node node = PathFindingManager.Instance.GetNodeFromWorldPosition(checkPosition);
            if (node != null && node.walkable)
            {
                return checkPosition;
            }
        }

        return transform.position;
    }

    private bool HandleCirclingBehavior(float distanceToPlayer)
    {
        if (distanceToPlayer < MIN_CIRCLE_DISTANCE)
        {
            isCirclingPlayer = true;
            CircleAroundPlayer();
            return true;
        }
        isCirclingPlayer = false;
        return false;
    }

    private void CircleAroundPlayer()
    {
        if (target == null)
            return;

        UpdateCirclingParameters();
        Vector2 targetPosition = CalculateCirclingPosition();

        Node targetNode = PathFindingManager.Instance.GetNodeFromWorldPosition(targetPosition);
        if (targetNode != null && !targetNode.walkable)
        {
            targetPosition = FindSafeCirclingPosition(targetPosition);
        }

        ApplyCirclingMovement(targetPosition);
        UpdateSpriteDirection();
    }

    private Vector2 FindSafeCirclingPosition(Vector2 originalPosition)
    {
        float[] checkAngles = { 45f, -45f, 90f, -90f, 135f, -135f, 180f };

        foreach (float angleOffset in checkAngles)
        {
            float newAngle = circlingAngle + angleOffset;
            Vector2 checkPosition =
                (Vector2)target.position
                + new Vector2(
                    Mathf.Cos(newAngle * Mathf.Deg2Rad),
                    Mathf.Sin(newAngle * Mathf.Deg2Rad)
                ) * circlingRadius;

            Node node = PathFindingManager.Instance.GetNodeFromWorldPosition(checkPosition);
            if (node != null && node.walkable)
            {
                return checkPosition;
            }
        }

        return originalPosition;
    }

    private void UpdateCirclingParameters()
    {
        int enemyCount = GameManager.Instance.enemies.Count;
        circlingRadius = Mathf.Max(2.0f, Mathf.Min(3.0f, enemyCount * 0.5f));

        float baseAngle = Time.time * 20f;
        int myIndex = GameManager.Instance.enemies.IndexOf(this);
        float angleStep = 360f / Mathf.Max(1, enemyCount);
        float targetAngle = baseAngle + (myIndex * angleStep);

        circlingAngle = Mathf.LerpAngle(circlingAngle, targetAngle, Time.deltaTime * 5f);
    }

    private Vector2 CalculateCirclingPosition()
    {
        Vector2 offset =
            new Vector2(
                Mathf.Cos(circlingAngle * Mathf.Deg2Rad),
                Mathf.Sin(circlingAngle * Mathf.Deg2Rad)
            ) * circlingRadius;

        return (Vector2)target.position + offset;
    }

    private void ApplyCirclingMovement(Vector2 targetPosition)
    {
        Vector2 moveDirection = (targetPosition - (Vector2)transform.position).normalized;
        moveDirection = CalculateAvoidanceDirection(transform.position, targetPosition);

        Vector2 separationForce = CalculateSeparationForce(transform.position);
        moveDirection = (moveDirection + separationForce * 0.1f).normalized;

        Vector2 targetVelocity = moveDirection * moveSpeed * 1.2f;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 8f);
    }
    #endregion

    #region Pathfinding
    private void UpdatePath()
    {
        if (ShouldUpdatePath())
        {
            List<Vector2> newPath = PathFindingManager.Instance.FindPath(
                transform.position,
                target.position
            );
            if (newPath != null && newPath.Count > 0)
            {
                currentPath = newPath;
                lastPathUpdateTime = Time.time;
                stuckTimer = 0f;
            }
            else
            {
                MoveDirectlyTowardsTarget();
            }
        }
    }

    private bool ShouldUpdatePath()
    {
        if (currentPath == null || currentPath.Count == 0)
            return true;

        if (Time.time >= lastPathUpdateTime + pathUpdateTime)
        {
            Vector2 finalDestination = currentPath[currentPath.Count - 1];
            float distanceToFinalDestination = Vector2.Distance(finalDestination, target.position);
            return distanceToFinalDestination > PathFindingManager.NODE_SIZE * 2;
        }
        return false;
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        Vector2 currentPos = transform.position;
        Vector2 nextWaypoint = currentPath[0];

        HandleStuckCheck(currentPos);
        ProcessWaypoint(currentPos, nextWaypoint);
        ApplyMovement(currentPos, nextWaypoint);
        UpdateSpriteDirection();
    }

    private void ProcessWaypoint(Vector2 currentPos, Vector2 nextWaypoint)
    {
        if (HasReachedWaypoint(currentPos, nextWaypoint))
        {
            if (currentPath != null && currentPath.Count > 0)
            {
                UpdateWaypoint();
                if (currentPath == null || currentPath.Count == 0)
                {
                    MoveDirectlyTowardsTarget();
                }
            }
        }
    }

    private Vector2 FindNearestSafePosition(Vector2 currentPosition)
    {
        float checkRadius = 1f;
        int maxAttempts = 8;
        float angleStep = 360f / maxAttempts;

        for (int i = 0; i < maxAttempts; i++)
        {
            float angle = i * angleStep;
            float radian = angle * Mathf.Deg2Rad;
            Vector2 checkPosition =
                currentPosition
                + new Vector2(Mathf.Cos(radian) * checkRadius, Mathf.Sin(radian) * checkRadius);

            Node node = PathFindingManager.Instance.GetNodeFromWorldPosition(checkPosition);
            if (node != null && node.walkable)
            {
                return checkPosition;
            }
        }

        return FindNearestSafePosition(currentPosition + Vector2.one * checkRadius);
    }
    #endregion

    #region Movement Helpers
    protected virtual void MoveDirectlyTowardsTarget()
    {
        if (target == null)
            return;

        Vector2 currentPos = transform.position;
        Vector2 targetPos = GetTargetPosition();

        Vector2 flockingForce = CalculateFlockingForce(currentPos);

        Vector2 formationPos = (Vector2)target.position + formationOffset;
        Vector2 formationDir = (formationPos - currentPos).normalized;

        Vector2 moveDir = (
            (targetPos - currentPos).normalized + flockingForce + formationDir
        ).normalized;
        moveDir = CalculateAvoidanceDirection(currentPos, currentPos + moveDir);

        Vector2 separationForce = CalculateSeparationForce(currentPos);
        moveDir = (moveDir + separationForce * 0.2f).normalized;

        ApplyVelocity(moveDir);
    }

    protected virtual Vector2 CalculateSeparationForce(Vector2 currentPos)
    {
        Vector2 separationForce = Vector2.zero;
        float separationRadius = isCirclingPlayer ? 0.8f : 1.2f;

        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(
            currentPos,
            separationRadius,
            LayerMask.GetMask("Enemy")
        );
        foreach (Collider2D enemyCollider in nearbyEnemies)
        {
            if (enemyCollider.gameObject != gameObject)
            {
                Vector2 diff = currentPos - (Vector2)enemyCollider.transform.position;
                float distance = diff.magnitude;
                if (distance < separationRadius)
                {
                    float strength = isCirclingPlayer ? 0.5f : 1f;
                    separationForce +=
                        diff.normalized * (1 - distance / separationRadius) * strength;
                }
            }
        }

        return separationForce.normalized * (isCirclingPlayer ? 0.3f : 0.5f);
    }

    protected virtual void ApplyMovement(Vector2 currentPos, Vector2 nextWaypoint)
    {
        Vector2 moveDirection = (nextWaypoint - currentPos).normalized;
        moveDirection = CalculateAvoidanceDirection(currentPos, nextWaypoint);

        Vector2 separationForce = CalculateSeparationForce(currentPos);
        moveDirection = (moveDirection + separationForce * 0.2f).normalized;

        Vector2 targetVelocity = moveDirection * moveSpeed;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 5f);
    }

    protected virtual void ApplyVelocity(Vector2 moveDirection)
    {
        Vector2 targetVelocity = moveDirection * moveSpeed;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 5f);
    }

    protected virtual Vector2 CalculateAvoidanceDirection(
        Vector2 currentPosition,
        Vector2 targetPosition
    )
    {
        Vector2 moveDir = (targetPosition - currentPosition).normalized;
        Vector2 finalMoveDir = moveDir;

        Vector2 dirToTarget = (Vector2)target.position - currentPosition;
        bool isVerticalAligned = Mathf.Abs(dirToTarget.x) < 0.1f;
        bool isHorizontalAligned = Mathf.Abs(dirToTarget.y) < 0.1f;

        if (
            (isVerticalAligned || isHorizontalAligned)
            && Physics2D.Raycast(
                currentPosition,
                moveDir,
                WALL_AVOIDANCE_DISTANCE,
                LayerMask.GetMask("Obstacle")
            )
        )
        {
            Vector2 alternativeDir = isVerticalAligned ? new Vector2(1f, 0f) : new Vector2(0f, 1f);
            if (
                !Physics2D.Raycast(
                    currentPosition,
                    alternativeDir,
                    WALL_AVOIDANCE_DISTANCE,
                    LayerMask.GetMask("Obstacle")
                )
            )
            {
                return alternativeDir;
            }
            if (
                !Physics2D.Raycast(
                    currentPosition,
                    -alternativeDir,
                    WALL_AVOIDANCE_DISTANCE,
                    LayerMask.GetMask("Obstacle")
                )
            )
            {
                return -alternativeDir;
            }
        }

        var obstacles = CheckObstacles(currentPosition, moveDir);
        if (HasObstacles(obstacles))
        {
            HandleObstacleAvoidance(obstacles);
            finalMoveDir = CalculateAvoidanceVector(obstacles);
        }

        return SmoothDirection(finalMoveDir);
    }

    protected virtual void HandleStuckCheck(Vector2 currentPos)
    {
        if (Vector2.Distance(currentPos, lastPosition) < STUCK_THRESHOLD)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > STUCK_CHECK_TIME)
            {
                ResetPath();
            }
        }
        else
        {
            stuckTimer = 0f;
        }
        lastPosition = currentPos;
    }

    protected virtual bool HasReachedWaypoint(Vector2 currentPos, Vector2 waypoint)
    {
        return Vector2.Distance(currentPos, waypoint) < PathFindingManager.NODE_SIZE * 0.5f;
    }

    protected virtual void UpdateWaypoint()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            currentPath.RemoveAt(0);
        }
    }

    protected virtual void ResetPath()
    {
        currentPath = null;
        stuckTimer = 0f;
    }

    protected virtual (RaycastHit2D front, RaycastHit2D right, RaycastHit2D left) CheckObstacles(
        Vector2 position,
        Vector2 direction
    )
    {
        Vector2 rightCheck = Quaternion.Euler(0, 0, 30) * direction;
        Vector2 leftCheck = Quaternion.Euler(0, 0, -30) * direction;

        return (
            Physics2D.Raycast(
                position,
                direction,
                WALL_AVOIDANCE_DISTANCE,
                LayerMask.GetMask("Obstacle")
            ),
            Physics2D.Raycast(
                position,
                rightCheck,
                CORNER_CHECK_DISTANCE,
                LayerMask.GetMask("Obstacle")
            ),
            Physics2D.Raycast(
                position,
                leftCheck,
                CORNER_CHECK_DISTANCE,
                LayerMask.GetMask("Obstacle")
            )
        );
    }

    protected virtual bool HasObstacles(
        (RaycastHit2D front, RaycastHit2D right, RaycastHit2D left) obstacles
    )
    {
        return obstacles.front.collider != null
            || obstacles.right.collider != null
            || obstacles.left.collider != null;
    }

    protected virtual void HandleObstacleAvoidance(
        (RaycastHit2D front, RaycastHit2D right, RaycastHit2D left) obstacles
    )
    {
        if (currentPath != null && Time.time >= lastObstacleAvoidanceTime + obstaclePathUpdateDelay)
        {
            ResetPathForObstacle();
        }
    }

    protected virtual void ResetPathForObstacle()
    {
        currentPath = null;
        lastPathUpdateTime = Time.time - pathUpdateTime;
        lastObstacleAvoidanceTime = Time.time;
    }

    protected virtual Vector2 CalculateAvoidanceVector(
        (RaycastHit2D front, RaycastHit2D right, RaycastHit2D left) obstacles
    )
    {
        Vector2 avoidDir = Vector2.zero;

        if (obstacles.front.collider != null)
        {
            avoidDir += -obstacles.front.normal * 3f;
        }
        if (obstacles.right.collider != null)
        {
            avoidDir += Vector2.Perpendicular(obstacles.right.normal) * 2f;
        }
        if (obstacles.left.collider != null)
        {
            avoidDir += -Vector2.Perpendicular(obstacles.left.normal) * 2f;
        }

        return avoidDir != Vector2.zero ? avoidDir.normalized : (Vector2)transform.right;
    }

    protected virtual Vector2 SmoothDirection(Vector2 finalMoveDir)
    {
        if (previousMoveDir != Vector2.zero)
        {
            finalMoveDir = Vector2.Lerp(previousMoveDir, finalMoveDir, Time.deltaTime * 20f);
        }
        previousMoveDir = finalMoveDir;
        return finalMoveDir;
    }

    protected virtual Vector2 CalculateFlockingForce(Vector2 currentPos)
    {
        Vector2 cohesion = Vector2.zero;
        Vector2 alignment = Vector2.zero;
        Vector2 separation = Vector2.zero;
        int neighborCount = 0;

        foreach (Enemy enemy in GameManager.Instance.enemies)
        {
            if (enemy == this)
                continue;

            float distance = Vector2.Distance(currentPos, enemy.transform.position);
            if (distance < FORMATION_RADIUS)
            {
                // 응집력 (Cohesion)
                cohesion += (Vector2)enemy.transform.position;

                // 정렬 (Alignment)
                alignment += enemy.rb.velocity;

                // 분리 (Separation)
                Vector2 diff = currentPos - (Vector2)enemy.transform.position;
                separation += diff.normalized / Mathf.Max(distance, 0.1f);

                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            cohesion = (cohesion / neighborCount - currentPos) * COHESION_WEIGHT;
            alignment = (alignment / neighborCount) * ALIGNMENT_WEIGHT;
            separation = separation * SEPARATION_WEIGHT;
        }

        return (cohesion + alignment + separation).normalized;
    }
    #endregion

    #region Combat
    public virtual void TakeDamage(float damage)
    {
        if (!gameObject.activeInHierarchy)
            return;

        float damageReduction = currentDefense / (currentDefense + 100f);
        float finalDamage = damage * (1f - damageReduction) * (1f + defenseDebuffAmount);

        hp -= finalDamage;

        if (hp <= 0)
        {
            if (dotDamageCoroutine != null)
            {
                StopCoroutine(dotDamageCoroutine);
                dotDamageCoroutine = null;
            }
            Die();
        }
    }

    public virtual void Die()
    {
        if (expParticlePrefab != null)
        {
            int expParticleCount = Random.Range(minExpParticles, maxExpParticles + 1);
            float expPerParticle = mobEXP / expParticleCount;

            for (int i = 0; i < expParticleCount; i++)
            {
                Vector3 spawnPosition = transform.position;
                ExpParticle expParticle = PoolManager.Instance.Spawn<ExpParticle>(
                    expParticlePrefab.gameObject,
                    spawnPosition,
                    Quaternion.identity
                );

                if (expParticle != null)
                {
                    expParticle.expValue = expPerParticle;
                }
            }
            DropItems();
        }

        if (GameManager.Instance?.enemies != null)
        {
            GameManager.Instance.enemies.Remove(this);
        }

        PoolManager.Instance.Despawn(this);
    }

    protected virtual void DropItems()
    {
        float playerLuck = GameManager
            .Instance.player.GetComponent<PlayerStatSystem>()
            .GetStat(StatType.Luck);

        var drops = ItemManager.Instance.GetDropsForEnemy(enemyType, 1f + playerLuck);

        if (drops.Any())
        {
            foreach (var itemData in drops)
            {
                Vector2 dropPosition = CalculateDropPosition();
                ItemManager.Instance.DropItem(itemData, dropPosition);
            }
        }
    }

    protected virtual Vector2 CalculateDropPosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(dropRadiusMin, dropRadiusMax);

        Vector2 offset = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);

        return (Vector2)transform.position + offset;
    }

    protected virtual void Attack()
    {
        if (Time.time >= preDamageTime + damageInterval)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            if (distanceToTarget <= attackRange)
            {
                // RangedEnemy의 경우 원거리 공격을, MeleeEnemy의 경우 근접 공격을 수행
                if (this is RangedEnemy || this is BossMonster)
                {
                    PerformRangedAttack();
                }
                else
                {
                    PerformMeleeAttack();
                }
            }
        }
    }

    protected virtual void PerformMeleeAttack()
    {
        var particle = Instantiate(attackParticle, target.position, Quaternion.identity);
        particle.Play();
        Destroy(particle.gameObject, 0.3f);

        GameManager.Instance.player.TakeDamage(damage);
        preDamageTime = Time.time;
    }

    protected virtual void PerformRangedAttack()
    {
        // 기본 Enemy 클래스에서는 아무것도 하지 않음
        // RangedEnemy에서 오버라이드하여 구현
    }

    public virtual void ApplyDefenseDebuff(float amount, float duration)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (defenseDebuffCoroutine != null)
        {
            StopCoroutine(defenseDebuffCoroutine);
        }

        defenseDebuffCoroutine = StartCoroutine(DefenseDebuffCoroutine(amount, duration));
    }

    public virtual IEnumerator DefenseDebuffCoroutine(float amount, float duration)
    {
        float actualReduction = Mathf.Min(amount, maxDefenseReduction - defenseDebuffAmount);

        defenseDebuffAmount += actualReduction;
        currentDefense = baseDefense * (1f - defenseDebuffAmount);

        yield return new WaitForSeconds(duration);

        if (this != null && gameObject.activeInHierarchy)
        {
            defenseDebuffAmount = Mathf.Max(defenseDebuffAmount - actualReduction, 0f);
            currentDefense = baseDefense * (1f - defenseDebuffAmount);
        }
        defenseDebuffCoroutine = null;
    }

    public virtual void ModifyBaseDefense(float amount)
    {
        baseDefense = Mathf.Max(0, baseDefense + amount);
        UpdateCurrentDefense();
    }

    public virtual void SetBaseDefense(float newDefense)
    {
        baseDefense = Mathf.Max(0, newDefense);
        UpdateCurrentDefense();
    }

    public virtual void UpdateCurrentDefense()
    {
        currentDefense = baseDefense * (1f - defenseDebuffAmount);
    }

    public virtual void ApplySlowEffect(float amount, float duration)
    {
        if (!gameObject.activeInHierarchy)
            return;

        moveSpeedDebuffAmount = Mathf.Min(moveSpeedDebuffAmount + amount, 0.9f);
        UpdateMoveSpeed();

        if (slowEffectCoroutine != null)
        {
            StopCoroutine(slowEffectCoroutine);
        }

        slowEffectCoroutine = StartCoroutine(SlowEffectCoroutine(amount, duration));
    }

    protected virtual IEnumerator SlowEffectCoroutine(float amount, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (this != null && gameObject.activeInHierarchy)
        {
            moveSpeedDebuffAmount = Mathf.Max(moveSpeedDebuffAmount - amount, 0f);
            UpdateMoveSpeed();
        }
        slowEffectCoroutine = null;
    }

    public virtual void UpdateMoveSpeed()
    {
        moveSpeed = originalMoveSpeed * (1f - moveSpeedDebuffAmount);
    }

    public virtual void ApplyDotDamage(float damagePerTick, float tickInterval, float duration)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (dotDamageCoroutine != null)
        {
            StopCoroutine(dotDamageCoroutine);
        }

        dotDamageCoroutine = StartCoroutine(
            DotDamageCoroutine(damagePerTick, tickInterval, duration)
        );
    }

    protected virtual IEnumerator DotDamageCoroutine(
        float damagePerTick,
        float tickInterval,
        float duration
    )
    {
        float endTime = Time.time + duration;

        while (Time.time < endTime && hp > 0 && gameObject.activeInHierarchy)
        {
            if (this != null && gameObject.activeInHierarchy)
            {
                TakeDamage(damagePerTick);
            }
            yield return new WaitForSeconds(tickInterval);
        }

        dotDamageCoroutine = null;
    }

    public virtual void ApplyStun(float power, float duration)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        stunCoroutine = StartCoroutine(StunCoroutine(duration));
    }

    protected virtual IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        float originalSpeed = moveSpeed;
        moveSpeed = 0;

        yield return new WaitForSeconds(duration);

        if (this != null && gameObject.activeInHierarchy)
        {
            isStunned = false;
            moveSpeed = originalSpeed * (1f - moveSpeedDebuffAmount);
        }
        stunCoroutine = null;
    }
    #endregion

    #region Collision
    public virtual void Contact()
    {
        var particle = Instantiate(attackParticle, target.position, Quaternion.identity);
        particle.Play();
        Destroy(particle.gameObject, 0.3f);
        Attack();
    }
    #endregion

    #region UI
    protected virtual void UpdateHPBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = hpAmount;
        }
    }

    protected virtual void UpdateVisuals()
    {
        UpdateHPBar();
        UpdateSpriteDirection();
    }
    #endregion

    #region Utility
    public virtual void SetCollisionState(bool isOutOfView)
    {
        if (enemyCollider != null)
        {
            enemyCollider.enabled = !isOutOfView;
        }
    }

    protected virtual void UpdateSpriteDirection()
    {
        float currentXPosition = transform.position.x;
        if (currentXPosition != previousXPosition)
        {
            Vector3 scale = transform.localScale;
            scale.x = (currentXPosition - previousXPosition) > 0 ? -1 : 1;
            transform.localScale = scale;
            previousXPosition = currentXPosition;
        }
    }
    #endregion
}
