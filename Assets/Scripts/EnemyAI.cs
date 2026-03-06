using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Configuración de IA")]
    [Tooltip("Velocidad a la que el enemigo persigue al jugador.")]
    [SerializeField] private float moveSpeed = 2.5f;
    [Tooltip("Daño que causa al tocar al jugador.")]
    [SerializeField] private int collisionDamage = 15;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Evitar que ruede
        rb.freezeRotation = true;
        // Quitar gravedad si estamos en Top-Down 2D
        rb.gravityScale = 0f;

        // Buscamos al jugador por su Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        // 1. Calcular dirección hacia el jugador
        Vector2 direction = (playerTransform.position - transform.position).normalized;

        // 2. Mover el RigidBody
        rb.velocity = direction * moveSpeed;

        // 3. Voltear el sprite según la dirección
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0; // Gira si va a la izquierda
        }
    }

    // Si el enemigo choca físicamente con el jugador, le hace daño
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats stats = collision.gameObject.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(collisionDamage);
            }
        }
    }
}
