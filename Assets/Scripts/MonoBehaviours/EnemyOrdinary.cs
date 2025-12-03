using System.Collections;
using UnityEngine;

public class EnemyOrdinary : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 3f;
    public float currentHealth;

    [Header("Daño por salto")]
    public bool canBeStomped = true;
    public float stompDamage = 1f;
    public float playerBounceForce = 10f;

    [Header("Daño al jugador")]
    public float damageToPlayer = 1f;
    public float playerKnockbackForce = 5f;

    [Header("Movimiento")]
    public float baseSpeed = 0.8f;
    public float currentSpeed;
    [Tooltip("40% de aumento de velocidad por cada golpe")]
    public float speedIncreasePercent = 0.4f;
    public float maxSpeed = 3f;
    public float chasingDistance = 0.5f;
    public float attackDistance = 0.8f;
    public float idleTime = 1f;

    [Header("Referencias")]
    public Transform player;
    public BoxCollider2D checkGroundFront;
    public BoxCollider2D checkGroundBack;
    public LayerMask consumablesLayer;

    [Header("Detección de Obstáculos")]
    public float raycastDistance = 0.8f;
    public int numberOfRays = 5;
    public float rayStartHeightOffset = 0.2f;
    public float rayEndHeightOffset = 0.1f;

    [Header("Ataque")]
    public float attackCooldown = 1f;
    public float attackDamage = 2f;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask playerLayer;

    [Header("Detección de Enemigos")]
    public LayerMask enemyLayer;
    public float enemyDetectionDistance = 0.5f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private MovementController playerMovement;
    private Player playerComponent;

    private bool isChasing = false;
    private bool isFacingRight = true;
    private float idleTimer = 0f;
    private bool isWaiting = false;
    private bool isDead = false;
    private float lastAttackTime = 0f;
    private bool canAttack = true;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
        currentSpeed = baseSpeed;

        if (checkGroundFront == null)
            Debug.LogError("❌ CheckGroundFront no asignado");

        if (checkGroundBack == null)
            Debug.LogError("❌ CheckGroundBack no asignado");

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (attackPoint == null)
            attackPoint = transform;

        if (player != null)
        {
            playerMovement = player.GetComponent<MovementController>();
            playerComponent = player.GetComponent<Player>();
        }

        // Configurar layer de enemigos si no está asignado
        if (enemyLayer == 0)
            enemyLayer = LayerMask.GetMask("Default");

        Debug.Log($"🎯 Enemigo creado - Velocidad base: {baseSpeed}, Incremento: 40% por golpe");
    }

    private void Update()
    {
        if (isDead) return;

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= chasingDistance && distanceToPlayer > attackDistance)
        {
            isChasing = true;
            ChasePlayer();
            SetAnimationState(1); // Walk
        }
        else if (distanceToPlayer <= attackDistance)
        {
            isChasing = true;
            if (canAttack && Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
            SetAnimationState(2); // Attack
        }
        else
        {
            isChasing = false;
            PatrolBehavior();
        }

        UpdateFacingDirection();
    }

    public void TakeStompDamage(float damage)
    {
        if (isDead || !canBeStomped) return;

        currentHealth -= damage;
        Debug.Log($"👟 Enemigo recibió {damage} de daño por salto. Vida restante: {currentHealth}");

        IncreaseSpeed();
        StartCoroutine(StompEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null && HasAnimatorParameter("Hit"))
                animator.SetTrigger("Hit");

            Debug.Log($"💥 Enemigo golpeado! Vida restante: {currentHealth}");
        }
    }

    private bool HasAnimatorParameter(string paramName)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    private void IncreaseSpeed()
    {
        float speedIncreaseAmount = baseSpeed * speedIncreasePercent;
        float newSpeed = currentSpeed + speedIncreaseAmount;

        currentSpeed = Mathf.Min(newSpeed, maxSpeed);

        Debug.Log($"⚡ Enemigo aumenta velocidad! " +
                 $"Base: {baseSpeed} | Actual: {currentSpeed:F2} | " +
                 $"Aumento: +{speedIncreaseAmount:F2} (40% de {baseSpeed})");

        StartCoroutine(SpeedBoostEffect());
    }

    private IEnumerator SpeedBoostEffect()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f);
            yield return new WaitForSeconds(0.5f);
            spriteRenderer.color = originalColor;
        }
    }

    private IEnumerator StompEffect()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.yellow;

            Vector3 originalScale = transform.localScale;
            transform.localScale = new Vector3(originalScale.x, originalScale.y * 0.7f, originalScale.z);

            yield return new WaitForSeconds(0.3f);

            spriteRenderer.color = originalColor;
            transform.localScale = originalScale;
        }
    }

    public void BouncePlayer()
    {
        if (playerMovement != null && player != null)
        {
            playerMovement.ApplyBounce(playerBounceForce);
        }
    }

    // NUEVO MÉTODO: Aplicar knockback al jugador
    public void ApplyKnockbackToPlayer()
    {
        if (playerMovement != null && player != null)
        {
            // Calcular dirección del knockback (alejándose del enemigo)
            Vector2 knockbackDirection = (player.position - transform.position).normalized;
            playerMovement.ApplyKnockback(knockbackDirection, playerKnockbackForce);
        }
    }

    private void AttackPlayer()
    {
        lastAttackTime = Time.time;
        SetAnimationState(2);

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        foreach (Collider2D playerCollider in hitPlayers)
        {
            MovementController playerMovement = playerCollider.GetComponent<MovementController>();
            if (playerMovement != null)
            {
                playerMovement.RecibirDano((int)attackDamage);
                Debug.Log($"🗡️ Enemigo atacó al jugador por {attackDamage} de daño");
            }
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("💀 Enemigo derrotado!");

        if (animator != null && HasAnimatorParameter("Die"))
            animator.SetTrigger("Die");

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }

        this.enabled = false;
        Destroy(gameObject, 1.5f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            CheckForPlayerCollision(collision);
        }
        // NUEVO: Detectar colisión con otros enemigos
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            HandleEnemyCollision(collision);
        }
    }

    // NUEVO MÉTODO: Manejar colisión con otros enemigos
    private void HandleEnemyCollision(Collision2D collision)
    {
        if (!isChasing) // Solo cambiar dirección si no está persiguiendo al jugador
        {
            isWaiting = true;
            idleTimer = 0f;
            ChangeDirection();
            Debug.Log("🔄 Enemigo detectó a otro enemigo, cambiando dirección");
        }
    }

    // NUEVO MÉTODO: Detectar colisión con el jugador (daño lateral)
    private void CheckForPlayerCollision(Collision2D collision)
    {
        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            bool isStomp = false;

            // Verificar si es un salto (contacto desde arriba)
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.point.y > transform.position.y && playerRb.linearVelocity.y < 0)
                {
                    isStomp = true;
                    TakeStompDamage(stompDamage);
                    BouncePlayer();
                    Debug.Log("👢 Jugador saltó sobre el enemigo!");
                    return;
                }
            }

            // Si no es un salto, es daño lateral
            if (!isStomp)
            {
                DamagePlayerOnSideCollision();
            }
        }
    }

    // NUEVO MÉTODO: Daño al jugador por colisión lateral
    private void DamagePlayerOnSideCollision()
    {
        if (playerComponent != null)
        {
            playerComponent.RecibirDano((int)damageToPlayer);
            ApplyKnockbackToPlayer();
            Debug.Log($"💥 Enemigo hizo daño al jugador por {damageToPlayer} puntos");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        // NUEVO: Dibujar área de detección de enemigos
        Gizmos.color = Color.magenta;
        Vector2 detectionOrigin = new Vector2(transform.position.x + (isFacingRight ? enemyDetectionDistance * 0.5f : -enemyDetectionDistance * 0.5f), transform.position.y);
        Gizmos.DrawWireCube(detectionOrigin, new Vector3(enemyDetectionDistance, 1f, 0f));
    }

    private void PatrolBehavior()
    {
        if (isWaiting)
        {
            idleTimer += Time.deltaTime;
            SetAnimationState(0);

            if (idleTimer >= idleTime)
            {
                isWaiting = false;
                idleTimer = 0f;
                ChangeDirection();
            }
        }
        else
        {
            bool hasGround = HasGroundAhead();
            bool hasObstacle = HasObstacleAhead();
            bool hasEnemyAhead = HasEnemyAhead(); // NUEVO: Detectar enemigos

            if (hasGround && !hasObstacle && !hasEnemyAhead)
            {
                MoveInCurrentDirection();
                SetAnimationState(1);
            }
            else
            {
                isWaiting = true;
                SetAnimationState(0);
            }
        }
    }

    // NUEVO MÉTODO: Detectar otros enemigos en el camino
    private bool HasEnemyAhead()
    {
        Vector2 detectionOrigin = (Vector2)transform.position + new Vector2(isFacingRight ? 0.3f : -0.3f, 0f);
        Vector2 detectionSize = new Vector2(enemyDetectionDistance, 0.8f);

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(detectionOrigin, detectionSize, 0f, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.gameObject != gameObject && enemy.CompareTag("Enemy"))
            {
                Debug.Log("🚫 Enemigo detectado en el camino");
                return true;
            }
        }

        return false;
    }

    private bool HasObstacleAhead()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null) return false;

        Bounds bounds = collider.bounds;
        float startHeight = bounds.min.y + rayStartHeightOffset;
        float endHeight = bounds.max.y - rayEndHeightOffset;
        float totalHeight = endHeight - startHeight;

        Vector2 rayDirection = isFacingRight ? Vector2.right : Vector2.left;

        for (int i = 0; i < numberOfRays; i++)
        {
            float heightFraction = numberOfRays > 1 ? (float)i / (numberOfRays - 1) : 0f;
            float rayHeight = startHeight + (totalHeight * heightFraction);

            Vector2 rayOrigin = new Vector2(transform.position.x, rayHeight);
            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, rayDirection, raycastDistance, consumablesLayer);

            bool obstacleDetected = false;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider.CompareTag("Ground"))
                {
                    obstacleDetected = true;
                    break;
                }
            }

            Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, obstacleDetected ? Color.red : Color.green);

            if (obstacleDetected)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasGroundAhead()
    {
        if (checkGroundFront == null || checkGroundBack == null)
        {
            Debug.LogWarning("⚠️ Checks no asignados");
            return false;
        }

        BoxCollider2D currentCheck = isFacingRight ? checkGroundFront : checkGroundBack;
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(consumablesLayer);
        filter.useLayerMask = true;

        Collider2D[] results = new Collider2D[10];
        int hitCount = currentCheck.Overlap(filter, results);

        return hitCount > 0;
    }

    private void MoveInCurrentDirection()
    {
        float direction = isFacingRight ? 1f : -1f;
        transform.position += new Vector3(direction * currentSpeed * Time.deltaTime, 0, 0);
    }

    private void ChangeDirection()
    {
        isFacingRight = !isFacingRight;
    }

    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)direction * currentSpeed * Time.deltaTime;
    }

    private void UpdateFacingDirection()
    {
        if (spriteRenderer == null) return;

        if (isChasing)
        {
            if (player.position.x > transform.position.x)
            {
                spriteRenderer.flipX = false;
                isFacingRight = true;
            }
            else
            {
                spriteRenderer.flipX = true;
                isFacingRight = false;
            }
        }
        else
        {
            spriteRenderer.flipX = !isFacingRight;
        }
    }

    private void SetAnimationState(int state)
    {
        if (animator != null)
            animator.SetInteger("AnimationState", state);
    }
}