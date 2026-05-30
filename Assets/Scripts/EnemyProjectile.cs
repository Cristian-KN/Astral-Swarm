using UnityEngine;

/// <summary>
/// Proyectil disparado por enemigos a distancia. Vuela en línea recta y daña
/// al jugador al impactarlo.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float maxLifeTime = 4f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic; // Use Dynamic for velocity-based movement
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent physics rotation
        Destroy(gameObject, maxLifeTime);
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void SetDamage(int newDamage) => damage = newDamage;

    private void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            if (stats != null) stats.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
