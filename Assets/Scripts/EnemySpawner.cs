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

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
            
            // Opcional: Aumentar la dificultad bajando el cooldown poco a poco basándose en el GameManager
            // spawnInterval = Mathf.Max(0.5f, spawnInterval - 0.01f);
        }
    }

    private void SpawnEnemy()
    {
        // 1. Calcular una posición aleatoria en un anillo alrededor del jugador
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        
        Vector3 spawnPosition = playerTransform.position + new Vector3(randomDirection.x, randomDirection.y, 0) * randomDistance;

        // 2. Elegir un enemigo aleatorio de la lista
        int randomIndex = Random.Range(0, enemyPrefabs.Count);
        GameObject selectedEnemy = enemyPrefabs[randomIndex];

        // 3. Instanciar el enemigo
        Instantiate(selectedEnemy, spawnPosition, Quaternion.identity);
    }
}
