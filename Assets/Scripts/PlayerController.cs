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

    // Vector que almacenará la dirección de entrada del usuario
    private Vector2 movementDirection;

    private void Awake()
    {
        // Caché de componentes en Awake es más eficiente que usar GetComponent en Update
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Evitamos que rotaciones físicas afecten al jugador en 2D
        rb.freezeRotation = true; 
    }

    private void Update()
    {
        // 1. CAPTURA DE INPUT (En Update porque depende de los fotogramas)
        // GetAxisRaw retorna -1, 0, o 1 (sin suavizado), perfecto para movimiento estilo 8 bits/16 bits.
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        // Normalizamos el vector para que al moverse en diagonal no vaya más rápido
        movementDirection = new Vector2(inputX, inputY).normalized;

        // 2. ACTUALIZACIÓN DE ANIMACIONES Y FEEDBACK (Sprite)
        UpdateAnimationAndSprite();
    }

    private void FixedUpdate()
    {
        // 3. APLICACIÓN DE FÍSICAS (En FixedUpdate para sincronía con el motor de físicas motor)
        // Aplicamos la velocidad directamente al Rigidbody2D.
        rb.linearVelocity = movementDirection * moveSpeed;
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
}
