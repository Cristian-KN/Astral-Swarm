using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public enum Behavior { Chaser, Shooter }

    [Header("Comportamiento")]
    [SerializeField] private Behavior behavior = Behavior.Chaser;

    [Header("Configuración de IA")]
    [Tooltip("Velocidad a la que el enemigo persigue al jugador.")]
    [SerializeField] private float moveSpeed = 2.5f;
    [Tooltip("Daño que causa al tocar al jugador.")]
    [SerializeField] private int collisionDamage = 15;

    [Header("Ataque a distancia (Shooter)")]
    [SerializeField] private GameObject projectilePrefab;
    [Tooltip("Distancia a la que se detiene y empieza a disparar.")]
    [SerializeField] private float attackRange = 7f;
    [SerializeField] private float fireCooldown = 1.8f;
    [SerializeField] private int projectileDamage = 10;

    [Header("Escalado por tiempo")]
    [Tooltip("Factor de velocidad al inicio de la partida.")]
    [SerializeField] private float startSpeedFactor = 0.55f;
    [Tooltip("Factor de velocidad alcanzado al final de la curva.")]
    [SerializeField] private float maxSpeedFactor = 1.5f;
    [Tooltip("Segundos para llegar del factor inicial al máximo.")]
    [SerializeField] private float rampSeconds = 180f;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float fireTimer;
    private GameManager gameManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;
        rb.gravityScale = 0f;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        gameManager = FindObjectOfType<GameManager>();
    }

    /// <summary>Factor de dificultad por tiempo: empieza bajo y sube hasta el máximo.</summary>
    private float DifficultyFactor()
    {
        float elapsed = gameManager != null ? gameManager.GetElapsedTime() : 0f;
        float t = rampSeconds > 0f ? Mathf.Clamp01(elapsed / rampSeconds) : 1f;
        return Mathf.Lerp(startSpeedFactor, maxSpeedFactor, t);
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        EnemyStats stats = GetComponent<EnemyStats>();
        EnemyVariantType variant = stats != null ? stats.variant : EnemyVariantType.Normal;

        Vector2 toPlayer = (Vector2)(playerTransform.position - transform.position);
        float dist = toPlayer.magnitude;
        Vector2 direction = toPlayer.normalized;
        float diff = DifficultyFactor();

        if (behavior == Behavior.Shooter)
        {
            // Mantener distancia: acercarse si está lejos, frenar y disparar si está en rango
            if (dist > attackRange)
                rb.linearVelocity = direction * moveSpeed * diff;
            else
                rb.linearVelocity = Vector2.zero;

            if (dist <= attackRange)
            {
                fireTimer += Time.fixedDeltaTime;
                if (fireTimer >= fireCooldown)
                {
                    fireTimer = 0f;
                    Shoot(direction);
                }
            }
        }
        else // Chaser
        {
            // Comportamiento especial: Amarillo huye
            if (variant == EnemyVariantType.Amarilla) direction = -direction;

            float currentSpeed = moveSpeed * diff;
            if (variant == EnemyVariantType.Amarilla) currentSpeed *= 1.5f;
            if (variant == EnemyVariantType.Verde) currentSpeed *= 1.2f;

            rb.linearVelocity = direction * currentSpeed;
        }

        if (direction.x != 0)
            spriteRenderer.flipX = direction.x < 0;
    }

    private void Shoot(Vector2 direction)
    {
        if (projectilePrefab == null) return;
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null)
        {
            ep.SetDirection(direction);
            ep.SetDamage(projectileDamage);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats stats = collision.gameObject.GetComponent<PlayerStats>();
            if (stats != null) stats.TakeDamage(collisionDamage);
        }
    }
}
