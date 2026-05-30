using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private GameObject[] rockPrefabs;

    [Header("Settings")]
    [SerializeField] private int minTrees = 10;
    [SerializeField] private int maxTrees = 15;
    [SerializeField] private int minRocks = 15;
    [SerializeField] private int maxRocks = 20;

    [SerializeField] private float spawnRadiusMin = 5f;
    [SerializeField] private float spawnRadiusMax = 12f;
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

    private void Update()
    {
        CheckAndDespawn();
        CheckAndSpawn();
    }

    private void SpawnInitial()
    {
        int targetTrees = Random.Range(minTrees, maxTrees + 1);
        int targetRocks = Random.Range(minRocks, maxRocks + 1);

        for (int i = 0; i < targetTrees; i++) SpawnObstacle(treePrefabs, activeTrees, true);
        for (int i = 0; i < targetRocks; i++) SpawnObstacle(rockPrefabs, activeRocks, true);
    }

    private void CheckAndDespawn()
    {
        activeTrees.RemoveAll(obj => {
            if (Vector2.Distance(playerTransform.position, obj.transform.position) > despawnRadius)
            {
                Destroy(obj);
                return true;
            }
            return false;
        });

        activeRocks.RemoveAll(obj => {
            if (Vector2.Distance(playerTransform.position, obj.transform.position) > despawnRadius)
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

        while (!validPos && attempts < 10)
        {
            attempts++;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = initial ? Random.Range(0f, spawnRadiusMin) : Random.Range(spawnRadiusMin, spawnRadiusMax);
            
            spawnPos = playerTransform.position + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

            // Check for overlaps
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
            
            // Set sorting order
            SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = Mathf.RoundToInt(-spawnPos.y * 100);
            }
            
            activeList.Add(instance);
        }
}
}
