using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad de desplazamiento del jugador en el eje X e Y.")]
    [SerializeField] private float moveSpeed = 5f;

    // Componentes cacheados
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerStats playerStats;

    // Vector que almacenará la dirección de entrada del usuario
    private Vector2 movementDirection;

    private void Awake()
    {
        // Caché de componentes en Awake es más eficiente que usar GetComponent en Update
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerStats = GetComponent<PlayerStats>();
        rb.freezeRotation = true;
    }

    // Footstep timing
    [SerializeField] private float footstepInterval = 0.4f;
    private float footstepTimer;

    private void Update()
    {
        // 1. CAPTURA DE INPUT
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        movementDirection = new Vector2(inputX, inputY).normalized;

        // 2. ACTUALIZACIÓN DE ANIMACIONES Y FEEDBACK
        UpdateAnimationAndSprite();

        // 3. Y-SORTING: Actualizar sorting order basado en posición Y
        UpdateSortingOrder();

        // 4. FOOTSTEPS
        if (movementDirection.sqrMagnitude > 0)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                footstepTimer = 0f;
                AudioManager.Instance?.PlayFootstepSound();
            }
        }
        else
        {
            footstepTimer = footstepInterval; // Ready to step immediately when starting to move
        }
    }

    private void FixedUpdate()
    {
        float speed = moveSpeed * (playerStats != null ? playerStats.speedMultiplier : 1f);
        rb.linearVelocity = movementDirection * speed;
    }

    /// <summary>
    /// Gestiona el giro del sprite y el pase de parámetros al Animator para transitar entre Idle y Run.
    /// </summary>
    private void UpdateAnimationAndSprite()
    {
        // Si hay movimiento horizontal, volteamos el sprite hacia la izquierda o derecha
        if (movementDirection.x != 0)
        {
            // Si va a la izquierda (x < 0), flipX es true. Si va a la derecha, flipX es false.
            spriteRenderer.flipX = movementDirection.x < 0;
        }

        // Le decimos al Animator si el personaje se está moviendo o no
        bool isMoving = movementDirection.sqrMagnitude > 0;
        // Asumiendo que el parámetro en el Animator se llama "IsRunning" (Tipo Bool)
        animator.SetBool("IsRunning", isMoving);
    }

    /// <summary>
    /// Actualiza el sorting order basado en la posición Y para que el jugador
    /// aparezca detrás/delante de obstáculos correctamente (Y-sorting).
    /// </summary>
    private void UpdateSortingOrder()
    {
        // Misma fórmula que los obstáculos: objetos más arriba (Y mayor) van atrás
        spriteRenderer.sortingOrder = Mathf.RoundToInt((100 - transform.position.y) * 10);
    }
}
