using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    [Tooltip("Lista de enemigos posibles a generar (Ej: Slime, Esqueleto).")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    
    [Tooltip("Tiempo en segundos entre la aparición de cada enemigo.")]
    [SerializeField] private float spawnInterval = 2f;
    
    [Tooltip("Distancia mínima al jugador para que no aparezcan en su cara.")]
    [SerializeField] private float minSpawnDistance = 10f;
    
    [Tooltip("Distancia máxima al jugador (para que aparezcan justo fuera de la cámara).")]
    [SerializeField] private float maxSpawnDistance = 15f;

    private Transform playerTransform;
    private float timer;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null || enemyPrefabs.Count == 0) return;

        timer += Time.deltaTime;

        // Fórmula de Dificultad Total: (Tiempo / 60) + Nivel + Items Sacrificio
        float totalDifficulty = GetTotalDifficulty();

        // El intervalo base se reduce con la dificultad (mínimo 0.15s para no saturar)
        float adjustedInterval = Mathf.Max(0.15f, spawnInterval / (1 + totalDifficulty * 0.05f));

        if (timer >= adjustedInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    private float GetTotalDifficulty()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        float timeFactor = (gm != null) ? gm.GetElapsedTime() / 60f : 0f;
        int levelFactor = (gm != null) ? gm.GetCurrentLevel() : 1;
        
        float sacrificeFactor = 0f;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerStats ps = player.GetComponent<PlayerStats>();
            if (ps != null) sacrificeFactor = ps.difficulty;
        }

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
        // Aplicar multiplicador de bioma
        BiomeManager biomeManager = BiomeManager.Instance;
        if (biomeManager != null)
        {
            totalDifficulty *= biomeManager.GetEnemyDifficultyMultiplier();
        }

        float roll = Random.Range(0f, 100f) - totalDifficulty;

        // Umbrales para la dificultad combinada (Tiempo + Nivel + Sacrificio + Bioma)
        if (roll < -50) return EnemyVariantType.Roja;    // Caos absoluto
        if (roll < -20) return EnemyVariantType.Negra;
        if (roll < 5)   return EnemyVariantType.Morada;
        if (roll < 25)  return EnemyVariantType.Azul;
        if (roll < 45)  return EnemyVariantType.Amarilla;
        if (roll < 70)  return EnemyVariantType.Verde;

        // Tier mínimo forzado por el bioma (para biomas avanzados o especiales)
        EnemyVariantType result = EnemyVariantType.Normal;
        if (biomeManager != null)
        {
            int minTier = biomeManager.GetMinEnemyVariantTier();
            result = EnforceMinimumTier(result, minTier);
        }

        return result;
    }

    private EnemyVariantType EnforceMinimumTier(EnemyVariantType variant, int minTier)
    {
        // Mapeo de variantes a tiers
        int currentTier = variant switch
        {
            EnemyVariantType.Normal => 0,
            EnemyVariantType.Verde => 1,
            EnemyVariantType.Amarilla => 2,
            EnemyVariantType.Azul => 3,
            EnemyVariantType.Morada => 4,
            EnemyVariantType.Negra => 5,
            EnemyVariantType.Roja => 6,
            _ => 0
        };

        if (currentTier < minTier)
        {
            // Upgrade al tier mínimo
            return minTier switch
            {
                1 => EnemyVariantType.Verde,
                2 => EnemyVariantType.Amarilla,
                3 => EnemyVariantType.Azul,
                4 => EnemyVariantType.Morada,
                5 => EnemyVariantType.Negra,
                6 => EnemyVariantType.Roja,
                _ => EnemyVariantType.Normal
            };
        }

        return variant;
    }
}
