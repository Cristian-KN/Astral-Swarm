using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    [Tooltip("Lista de enemigos posibles a generar (Ej: Slime, Esqueleto).")]
    [SerializeField] private List<GameObject> enemyPrefabs;

    [Tooltip("Tiempo en segundos entre la aparición de cada enemigo.")]
    [SerializeField] private float spawnInterval = 2f;

    [Tooltip("Límite máximo de enemigos vivos al mismo tiempo (rendimiento).")]
    [SerializeField] private int maxEnemies = 150;
    
    [Tooltip("Distancia mínima al jugador para que no aparezcan en su cara.")]
    [SerializeField] private float minSpawnDistance = 10f;
    
    [Tooltip("Distancia máxima al jugador (para que aparezcan justo fuera de la cámara).")]
    [SerializeField] private float maxSpawnDistance = 15f;

    private Transform playerTransform;
    private float timer;

    private GameManager gameManager;
    private PlayerStats playerStats;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerStats = player.GetComponent<PlayerStats>();
        }
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        if (playerTransform == null || enemyPrefabs.Count == 0) return;

        // OPTIMIZACIÓN: No spawnear si ya hay demasiados enemigos
        int currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (currentEnemies >= maxEnemies)
        {
            return; // Evitar lag por demasiados enemigos
        }

        timer += Time.deltaTime;

        // Fórmula de Dificultad Total: (Tiempo / 60) + Nivel + Items Sacrificio
        float totalDifficulty = GetTotalDifficulty();

        // El intervalo base se reduce con la dificultad (mínimo 0.8s para mejor rendimiento)
        float adjustedInterval = Mathf.Max(0.8f, spawnInterval / Mathf.Max(0.1f, 1 + totalDifficulty * 0.05f));

        if (timer >= adjustedInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    private float GetTotalDifficulty()
    {
        float elapsed = (gameManager != null) ? gameManager.GetElapsedTime() : 0f;
        float timeFactor = elapsed / 60f;
        
        // Post-1 minute difficulty spike
        if (elapsed > 60f)
        {
            timeFactor *= 2.0f; // Scale difficulty twice as fast after 1 min
        }

        int levelFactor = (gameManager != null) ? gameManager.GetCurrentLevel() : 1;

        float sacrificeFactor = playerStats != null ? playerStats.difficulty : 0f;

        return timeFactor + levelFactor + sacrificeFactor;
    }

    private void SpawnEnemy()
    {
        // 1. Calcular una posición aleatoria
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector3 spawnPosition = playerTransform.position + new Vector3(randomDirection.x, randomDirection.y, 0) * randomDistance;

        // 2. Elegir un enemigo base aleatorio
        int randomIndex = Random.Range(0, enemyPrefabs.Count);
        GameObject enemyInstance = Instantiate(enemyPrefabs[randomIndex], spawnPosition, Quaternion.identity);

        // 3. Determinar la variante según la dificultad total
        EnemyStats stats = enemyInstance.GetComponent<EnemyStats>();
        if (stats != null)
        {
            stats.variant = GetVariantByDifficulty(GetTotalDifficulty());
        }
    }

    private EnemyVariantType GetVariantByDifficulty(float totalDifficulty)
    {
        float roll = Random.Range(0f, 100f) - totalDifficulty;

        // Umbrales para la dificultad combinada (Tiempo + Nivel + Sacrificio)
        if (roll < -50) return EnemyVariantType.Roja;    // Caos absoluto
        if (roll < -20) return EnemyVariantType.Negra;
        if (roll < 5)   return EnemyVariantType.Morada;
        if (roll < 25)  return EnemyVariantType.Azul;
        if (roll < 45)  return EnemyVariantType.Amarilla;
        if (roll < 70)  return EnemyVariantType.Verde;

        return EnemyVariantType.Normal;
    }
}
