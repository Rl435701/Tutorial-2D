using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class MovementController : MonoBehaviour
{
    [Header("Movimiento")]
    public float movementSpeed = 2f;
    public float jumpForce = 4f;

    [Header("Joystick")]
    public FloatingJoystick floatingJoystick;
    public float joystickDeadZone = 0.1f;

    [Header("Botones UI")]
    public Button jumpButton;
    public Button attackButton;
    public Button interactButton;

    [Header("Detección de suelo")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Daño por caída")]
    public float alturaMinimaParaDano = 3f;
    public float alturaMaximaParaDano = 8f;
    public int danoPorCaidaMaximo = 3;

    [Header("Salto sobre enemigos")]
    public float stompBounceForce = 10f;

    [Header("Knockback")]
    public float knockbackDuration = 0.3f;
    public float invincibilityDuration = 1f;

    private Rigidbody2D rb2D;
    private Animator animator;
    private Vector2 movement;
    private bool estaMuerto = false;
    private bool isGrounded = false;
    private bool isFacingRight = true;
    private bool isJumping = false;
    private bool justLanded = false;
    private bool isAttacking = false;
    private bool isKnockback = false;
    private bool isInvincible = false;
    private bool enDialogo = false;

    private float alturaMaximaCaida = 0f;
    private float alturaInicioCaida = 0f;
    private bool enCaida = false;

    private float attackDuration = 0.5f;
    private float attackTimer = 0f;

    private int animationStateHash;
    private float originalScaleY;
    private float squashTimer = 0f;

    private Player player;
    private NPC npcCercano;

    private bool jumpPressed = false;
    private bool attackPressed = false;
    private bool interactPressed = false;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
        animationStateHash = Animator.StringToHash("AnimationState");
        originalScaleY = transform.localScale.y;

        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");

        SetupUIButtons();

        if (interactButton != null)
        {
            interactButton.gameObject.SetActive(false);
        }
    }

    void SetupUIButtons()
    {
        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(OnJumpButtonClicked);
        }

        if (attackButton != null)
        {
            attackButton.onClick.AddListener(OnAttackButtonClicked);
        }

        if (interactButton != null)
        {
            interactButton.onClick.AddListener(OnInteractButtonClicked);
        }
    }

    void OnDestroy()
    {
        if (jumpButton != null)
            jumpButton.onClick.RemoveListener(OnJumpButtonClicked);
        if (attackButton != null)
            attackButton.onClick.RemoveListener(OnAttackButtonClicked);
        if (interactButton != null)
            interactButton.onClick.RemoveListener(OnInteractButtonClicked);
    }

    void Update()
    {
        if (estaMuerto || enDialogo || isKnockback)
        {
            if (enDialogo)
            {
                movement = Vector2.zero;
                rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
                animator.SetInteger(animationStateHash, 0);

                if (interactButton != null && interactButton.gameObject.activeSelf)
                {
                    interactButton.gameObject.SetActive(false);
                }
            }
            return;
        }

        // Primero verificar el estado del suelo
        CheckGrounded();

        // Detectar inicio de caída
        if (!isGrounded && rb2D.linearVelocity.y < 0 && !enCaida)
        {
            enCaida = true;
            alturaInicioCaida = transform.position.y;
            alturaMaximaCaida = alturaInicioCaida;
            isJumping = false; // Ya no está saltando, está cayendo
        }

        // Actualizar altura máxima durante la caída
        if (enCaida && !isGrounded)
        {
            alturaMaximaCaida = Mathf.Max(alturaMaximaCaida, transform.position.y);
        }

        // Detectar aterrizaje
        if (isGrounded && enCaida)
        {
            float alturaCaida = alturaInicioCaida - transform.position.y;
            CalcularDanoPorCaida(alturaCaida);
            enCaida = false;
            justLanded = true;
            squashTimer = 0.15f;
        }

        // Obtener input del joystick
        movement = GetJoystickInput();

        // Voltear personaje según dirección
        if (Mathf.Abs(movement.x) > joystickDeadZone)
        {
            if (movement.x > 0 && !isFacingRight)
                FlipCharacter();
            else if (movement.x < 0 && isFacingRight)
                FlipCharacter();
        }

        // Manejar salto
        if (jumpPressed && isGrounded && !isJumping && !isAttacking)
        {
            Jump();
            jumpPressed = false;
        }

        // Manejar ataque
        if (attackPressed && !isAttacking)
        {
            StartAttack();
            attackPressed = false;
        }

        // Manejar interacción
        if (interactPressed && npcCercano != null)
        {
            InteractuarConNPC();
            interactPressed = false;
        }

        // Actualizar temporizador de ataque
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
                isAttacking = false;
        }

        // Actualizar animaciones y efectos
        UpdateAnimations();
        AnimateJumpEffect();
    }

    private void FixedUpdate()
    {
        if (estaMuerto || enDialogo || isKnockback) return;

        // Aplicar movimiento horizontal
        float horizontalVelocity = movement.x * movementSpeed;
        rb2D.linearVelocity = new Vector2(horizontalVelocity, rb2D.linearVelocity.y);
    }

    private Vector2 GetJoystickInput()
    {
        Vector2 input = Vector2.zero;

        if (floatingJoystick != null)
        {
            input.x = floatingJoystick.Horizontal;
            input.y = floatingJoystick.Vertical;

            if (Mathf.Abs(input.x) < joystickDeadZone)
                input.x = 0;
        }

        return input;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            npcCercano = other.GetComponent<NPC>();
            if (npcCercano != null && interactButton != null && !enDialogo)
            {
                interactButton.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            if (interactButton != null)
                interactButton.gameObject.SetActive(false);
            npcCercano = null;
        }
    }

    private void InteractuarConNPC()
    {
        if (npcCercano != null && !enDialogo)
        {
            npcCercano.IniciarDialogo();
            if (interactButton != null)
                interactButton.gameObject.SetActive(false);
        }
    }

    private void OnJumpButtonClicked() => jumpPressed = true;
    private void OnAttackButtonClicked() => attackPressed = true;
    private void OnInteractButtonClicked() => interactPressed = true;

    public void ApplyBounce(float bounceForce)
    {
        rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, 0f);
        rb2D.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
        isJumping = true;
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (isInvincible) return;
        StartCoroutine(KnockbackCoroutine(direction, force));
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction, float force)
    {
        isKnockback = true;
        rb2D.linearVelocity = Vector2.zero;
        rb2D.AddForce(direction * force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackDuration);
        isKnockback = false;
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        float flashInterval = 0.1f;
        int flashes = Mathf.RoundToInt(invincibilityDuration / flashInterval);

        for (int i = 0; i < flashes; i++)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = i % 2 == 0 ? new Color(1, 1, 1, 0.5f) : Color.white;
            }
            yield return new WaitForSeconds(flashInterval);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        isInvincible = false;
    }

    public void IniciarDialogo()
    {
        enDialogo = true;
        movement = Vector2.zero;
        if (rb2D != null)
            rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
        if (animator != null)
            animator.SetInteger(animationStateHash, 0);
    }

    public void TerminarDialogo()
    {
        enDialogo = false;
        if (npcCercano != null && interactButton != null)
            interactButton.gameObject.SetActive(true);
    }

    public bool EstaEnDialogo() => enDialogo;

    private void CheckGrounded()
    {
        Vector2 checkPosition = groundCheckPoint != null ? groundCheckPoint.position : transform.position;
        Collider2D groundCollider = Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayer);
        bool wasGrounded = isGrounded;
        isGrounded = groundCollider != null;

        // Si estamos en el suelo y la velocidad vertical es pequeña, no estamos saltando
        if (isGrounded && rb2D.linearVelocity.y <= 0.1f)
        {
            isJumping = false;
        }

        // Si acabamos de tocar el suelo
        if (!wasGrounded && isGrounded)
        {
            isJumping = false;
        }
    }

    private void Jump()
    {
        rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, 0f);
        rb2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isJumping = true;
        isGrounded = false; // Marcar que ya no está en el suelo inmediatamente
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackDuration;
        animator.SetInteger(animationStateHash, 2);
    }

    private void CalcularDanoPorCaida(float alturaCaida)
    {
        if (alturaCaida < alturaMinimaParaDano) return;

        float porcentajeDano = Mathf.InverseLerp(alturaMinimaParaDano, alturaMaximaParaDano, alturaCaida);
        int dano = Mathf.RoundToInt(porcentajeDano * danoPorCaidaMaximo);
        dano = Mathf.Max(1, dano);

        if (player != null)
        {
            player.RecibirDano(dano);
        }
    }

    private void UpdateAnimations()
    {
        if (estaMuerto)
        {
            animator.SetInteger(animationStateHash, 3);
            return;
        }

        if (enDialogo || isKnockback)
        {
            animator.SetInteger(animationStateHash, 0);
            return;
        }

        if (isAttacking)
        {
            animator.SetInteger(animationStateHash, 2);
            return;
        }

        // Si está en el aire (saltando o cayendo)
        if (!isGrounded)
        {
            // Usar animación de salto para ambos casos (saltando y cayendo)
            animator.SetInteger(animationStateHash, 4);
            return;
        }

        // Si está en el suelo
        if (Mathf.Abs(movement.x) > 0.1f && isGrounded)
        {
            animator.SetInteger(animationStateHash, 1); // Corriendo
        }
        else
        {
            animator.SetInteger(animationStateHash, 0); // Idle
        }
    }

    private void AnimateJumpEffect()
    {
        Vector3 scale = transform.localScale;
        float stretchSpeed = 10f;

        if (!isGrounded)
        {
            if (rb2D.linearVelocity.y > 0.1f)
                scale.y = Mathf.Lerp(scale.y, originalScaleY * 1.25f, Time.deltaTime * stretchSpeed);
            else if (rb2D.linearVelocity.y < -0.1f)
                scale.y = Mathf.Lerp(scale.y, originalScaleY * 0.8f, Time.deltaTime * stretchSpeed);
        }
        else
        {
            if (justLanded && squashTimer > 0f)
            {
                squashTimer -= Time.deltaTime;
                float squashAmount = Mathf.Sin((0.15f - squashTimer) * Mathf.PI / 0.15f);
                float bounce = Mathf.Lerp(1f, 0.8f, squashAmount);
                scale.y = originalScaleY * bounce;

                if (squashTimer <= 0f)
                    justLanded = false;
            }
            else
            {
                scale.y = Mathf.Lerp(scale.y, originalScaleY, Time.deltaTime * 8f);
            }
        }

        scale.x = Mathf.Abs(originalScaleY) * (isFacingRight ? 1 : -1);
        transform.localScale = scale;
    }

    private void FlipCharacter()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (isFacingRight ? 1 : -1);
        transform.localScale = scale;
    }

    public void Morir()
    {
        estaMuerto = true;
        rb2D.linearVelocity = Vector2.zero;
        isJumping = false;
        UpdateAnimations();
    }

    public void Revivir()
    {
        estaMuerto = false;
        UpdateAnimations();
    }

    public void RecibirDano(int cantidad)
    {
        if (isInvincible) return;

        if (player != null)
        {
            player.RecibirDano(cantidad);
            StartCoroutine(InvincibilityCoroutine());
        }

        if (player != null && player.hitPoints.value <= 0)
        {
            Morir();
            GameOverManager gameOverManager = FindFirstObjectByType<GameOverManager>();
            if (gameOverManager != null)
            {
                gameOverManager.StartGameOverSequence();
            }
        }
    }

    public void RecibirDanoDeEnemigo(int cantidad)
    {
        RecibirDano(cantidad);
        StartCoroutine(DamageFlashEffect());
    }

    private IEnumerator DamageFlashEffect()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        if (Application.isPlaying && enCaida)
        {
            Vector3 pos = transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(pos.x - 0.5f, alturaInicioCaida, pos.z),
                           new Vector3(pos.x + 0.5f, alturaInicioCaida, pos.z));
        }
    }
}