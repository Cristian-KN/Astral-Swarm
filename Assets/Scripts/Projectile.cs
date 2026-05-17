using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Ajustes del Proyectil")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 25;
    [Tooltip("Tiempo de vida máximo en caso de no chocar con nada (para no saturar la memoria).")]
    [SerializeField] private float maxLifeTime = 3f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Importante para proyectiles en 2D
        rb.gravityScale = 0f;
        rb.isKinematic = true; // Para que lo movamos por código puro de forma constante, o se puede usar Dynamic. Usaremos velocidad.

        // Autodestruir el proyectil a los 'maxLifeTime' segundos si no choca
        Destroy(gameObject, maxLifeTime);
    }

    /// <summary>
    /// Llamado por PlayerAttack para decirle al misil hacia dónde volar.
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction;
        
        // Rotar el sprite del proyectil para que mire hacia su dirección de movimiento
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void FixedUpdate()
    {
        // Movemos el proyectil
        rb.linearVelocity = moveDirection * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyStats enemy = collision.GetComponent<EnemyStats>();
            if (enemy != null) enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
