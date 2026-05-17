using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Configuración de Ataque")]
    [Tooltip("El Prefab de magia que el jugador va a disparar.")]
    [SerializeField] private GameObject magicProjectilePrefab;
    [Tooltip("Tiempo en segundos entre cada disparo automático.")]
    [SerializeField] private float attackCooldown = 1.0f;
    [Tooltip("Rango de detección para encontrar enemigos (radio del círculo).")]
    [SerializeField] private float detectionRadius = 5f;
    [Tooltip("LayerMask para identificar qué objetos son enemigos.")]
    [SerializeField] private LayerMask enemyLayer;

    private float attackTimer = 0f;

    private void Update()
    {
        // El temporizador avanza con el tiempo real de juego
        attackTimer += Time.deltaTime;

        // Si ha pasado suficiente tiempo, intentamos atacar
        if (attackTimer >= attackCooldown)
        {
            AttackNearestEnemy();
        }
    }

    private void AttackNearestEnemy()
    {
        // 1. Escaneo del entorno: Trazamos un círculo invisible para buscar físicas en la capa de Enemigos
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);

        if (hitEnemies.Length == 0) return; // No hay enemigos cerca, no disparamos.

        // 2. Lógica para encontrar el enemigo más cercano
        Transform nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider2D enemy in hitEnemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy.transform;
            }
        }

        // 3. Disparo (Instanciación del proyectil)
        if (nearestEnemy != null && magicProjectilePrefab != null)
        {
            // Reiniciamos el tiempo de ataque
            attackTimer = 0f;

            // Creamos el proyectil en la posición del jugador
            GameObject projectile = Instantiate(magicProjectilePrefab, transform.position, Quaternion.identity);
            
            // Le pasamos la dirección del enemigo al proyectil
            Vector2 shootDirection = (nearestEnemy.position - transform.position).normalized;
            
            Projectile magicScript = projectile.GetComponent<Projectile>();
            if (magicScript != null)
            {
                magicScript.SetDirection(shootDirection);
            }
        }
    }

    // Para ver el radio de detección de forma visual en la vista de Escena de Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
