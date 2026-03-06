using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Estadísticas del Enemigo")]
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;

    [Header("Recompensas")]
    [Tooltip("Prefab de la gema de experiencia que suelta al morir.")]
    [SerializeField] private GameObject experienceGemPrefab;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Llamado por el proyectil del jugador cuando impacta
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        
        // Aquí podríamos añadir un parpadeo blanco (Feedback visual de recibir daño) si quisiéramos

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Soltar la gema de experiencia antes de desaparecer
        if (experienceGemPrefab != null)
        {
            Instantiate(experienceGemPrefab, transform.position, Quaternion.identity);
        }

        // En un juego final se usaría Object Pooling, pero para el prototipo Destroy está bien
        Destroy(gameObject); 
    }
}
