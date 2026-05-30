using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private GameObject[] rockPrefabs;

    [Header("Settings")]
    [SerializeField] private int minTrees = 3;
    [SerializeField] private int maxTrees = 5;
    [SerializeField] private int minRocks = 2;
    [SerializeField] private int maxRocks = 3;

    [Tooltip("Radio visible de la cámara (no spawnear dentro de este radio)")]
    [SerializeField] private float cameraVisibleRadius = 8f;
    [Tooltip("Radio donde empiezan a aparecer obstáculos (fuera de vista)")]
    [SerializeField] private float spawnRadiusMin = 9f;
    [SerializeField] private float spawnRadiusMax = 14f;
    [SerializeField] private float despawnRadius = 20f;
[SerializeField] private float overlapCheckRadius = 1.5f;

    private Transform playerTransform;
    private List<GameObject> activeTrees = new List<GameObject>();
    private List<GameObject> activeRocks = new List<GameObject>();

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            playerTransform = GameObject.Find("Player")?.transform;
        }

        if (playerTransform == null)
        {
            Debug.LogError("[ObstacleSpawner] Player not found!");
            enabled = false;
            return;
        }

        // Initial spawn
        SpawnInitial();
    }

    private float checkTimer = 0f;
    [SerializeField] private float checkInterval = 2.0f; // Revisar cada 2 segundos, no cada segundo

    private void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckAndDespawn();
            CheckAndSpawn();
        }
    }

    private void SpawnInitial()
    {
        // Al inicio, spawneamos algunos obstáculos fuera de la vista
        int targetTrees = Random.Range(minTrees, maxTrees + 1);
        int targetRocks = Random.Range(minRocks, maxRocks + 1);

        for (int i = 0; i < targetTrees; i++) SpawnObstacle(treePrefabs, activeTrees, false);
        for (int i = 0; i < targetRocks; i++) SpawnObstacle(rockPrefabs, activeRocks, false);
    }

    private void CheckAndDespawn()
    {
        // Eliminar obstáculos que están muy lejos
        activeTrees.RemoveAll(obj => {
            if (obj == null) return true;
            float dist = Vector2.Distance(playerTransform.position, obj.transform.position);
            if (dist > despawnRadius)
            {
                Destroy(obj);
                return true;
            }
            return false;
        });

        activeRocks.RemoveAll(obj => {
            if (obj == null) return true;
            float dist = Vector2.Distance(playerTransform.position, obj.transform.position);
            if (dist > despawnRadius)
            {
                Destroy(obj);
                return true;
            }
            return false;
        });
    }

    private void CheckAndSpawn()
    {
        int targetTrees = Random.Range(minTrees, maxTrees + 1);
        int targetRocks = Random.Range(minRocks, maxRocks + 1);

        if (activeTrees.Count < targetTrees)
        {
            SpawnObstacle(treePrefabs, activeTrees, false);
        }

        if (activeRocks.Count < targetRocks)
        {
            SpawnObstacle(rockPrefabs, activeRocks, false);
        }
    }

    private void SpawnObstacle(GameObject[] prefabs, List<GameObject> activeList, bool initial)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        Vector3 spawnPos = Vector3.zero;
        bool validPos = false;
        int attempts = 0;

        while (!validPos && attempts < 20)
        {
            attempts++;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            // SIEMPRE spawneamos fuera del radio visible
            float radius = Random.Range(spawnRadiusMin, spawnRadiusMax);

            spawnPos = playerTransform.position + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

            // Verificar que NO esté dentro del área visible
            float distToPlayer = Vector2.Distance(spawnPos, playerTransform.position);
            if (distToPlayer < cameraVisibleRadius)
            {
                continue; // Muy cerca, intenta otra posición
            }

            // Check for overlaps con otros obstáculos
            Collider2D hit = Physics2D.OverlapCircle(spawnPos, overlapCheckRadius);
            if (hit == null)
            {
                validPos = true;
            }
        }

        if (validPos)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity, transform);

            // Set sorting order (basado en Y, pero positivo para estar sobre el suelo)
            SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // Convertir Y a sorting order: objetos más arriba (Y mayor) van atrás
                // Rango típico: 0 a 1000 para estar sobre el suelo
                sr.sortingOrder = Mathf.RoundToInt((100 - spawnPos.y) * 10);
            }

            // Ajustar collider de árboles para que esté en el tronco (abajo)
            if (activeList == activeTrees)
            {
                CircleCollider2D col = instance.GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    // Hitbox en la base del tronco, no en las hojas
                    col.offset = new Vector2(0, -0.65f);
                    col.radius = 0.3f;
                }

                // Limitar escala máxima de árboles (evitar árboles gigantes con hitbox rara)
                if (instance.transform.localScale.x > 1.5f)
                {
                    instance.transform.localScale = Vector3.one * 1.5f;
                }
            }

            activeList.Add(instance);
        }
}
}
