using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Generador de mapa procedural usando Tilemap de Unity y sprites reales del asset pack
/// </summary>
public class ProceduralMapGenerator : MonoBehaviour
{
    [Header("Referencias de Tilemap")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private TileBase[] grassTiles; // Asignar los tiles de césped desde el inspector

    [Header("Prefabs de Obstáculos")]
    [SerializeField] private GameObject[] treePrefabs; // Árboles del Tiny Swords pack
    [SerializeField] private GameObject[] rockPrefabs; // Rocas del Tiny Swords pack

    [Header("Configuración del Mapa")]
    [SerializeField] private int mapWidth = 100;
    [SerializeField] private int mapHeight = 100;
    [SerializeField] private int treeCount = 12; // 8-12 árboles
    [SerializeField] private int rockCount = 5; // 3-5 rocas

    [Header("Espaciado")]
    [SerializeField] private float minTreeDistance = 3f;
    [SerializeField] private float minRockDistance = 2.5f;
    [SerializeField] private float minTreeToRockDistance = 2f;
    [SerializeField] private float mapMargin = 5f;

    [Header("Escala de Árboles")]
    [SerializeField] private float[] treeScales = { 1.5f, 2f, 2.25f };

    [Header("Jugador")]
    [SerializeField] private Transform player;
    [SerializeField] private float playerClearRadius = 5f;

    private List<Vector3> treePositions = new List<Vector3>();
    private List<Vector3> rockPositions = new List<Vector3>();
    private Transform obstacleContainer;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // Crear contenedor para obstáculos
        obstacleContainer = new GameObject("ObstacleContainer").transform;
        obstacleContainer.SetParent(transform);

        GenerateMap();
    }

    public void GenerateMap()
    {
        // Limpiar mapa anterior
        ClearMap();

        // 1. Generar césped con Tilemap
        GenerateGrassTilemap();

        // 2. Colocar árboles
        PlaceTrees();

        // 3. Colocar rocas
        PlaceRocks();

        Debug.Log($"[ProceduralMapGenerator] Mapa generado: {treePositions.Count} árboles, {rockPositions.Count} rocas");
    }

    private void GenerateGrassTilemap()
    {
        if (groundTilemap == null || grassTiles.Length == 0)
        {
            Debug.LogError("[ProceduralMapGenerator] Tilemap o tiles de césped no asignados!");
            return;
        }

        groundTilemap.ClearAllTiles();

        // Llenar todo el mapa con tiles de césped random
        for (int x = -mapWidth / 2; x < mapWidth / 2; x++)
        {
            for (int y = -mapHeight / 2; y < mapHeight / 2; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                TileBase randomTile = grassTiles[Random.Range(0, grassTiles.Length)];
                groundTilemap.SetTile(tilePos, randomTile);
            }
        }
    }

    private void PlaceTrees()
    {
        if (treePrefabs == null || treePrefabs.Length == 0)
        {
            Debug.LogWarning("[ProceduralMapGenerator] No hay prefabs de árboles asignados");
            return;
        }

        treePositions.Clear();
        int attempts = 0;
        int maxAttempts = 1000;

        while (treePositions.Count < treeCount && attempts < maxAttempts)
        {
            attempts++;

            // Generar posición random dentro del mapa con margen
            float x = Random.Range(-mapWidth / 2f + mapMargin, mapWidth / 2f - mapMargin);
            float y = Random.Range(-mapHeight / 2f + mapMargin, mapHeight / 2f - mapMargin);
            Vector3 position = new Vector3(x, y, 0);

            // Verificar que no esté cerca del jugador
            if (player != null && Vector3.Distance(position, player.position) < playerClearRadius)
            {
                continue;
            }

            // Verificar distancia con otros árboles
            bool tooClose = false;
            foreach (Vector3 existingTree in treePositions)
            {
                if (Vector3.Distance(position, existingTree) < minTreeDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                // Crear árbol con escala aleatoria
                GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                GameObject tree = Instantiate(treePrefab, position, Quaternion.identity, obstacleContainer);

                // Aplicar escala aleatoria
                float scale = treeScales[Random.Range(0, treeScales.Length)];
                tree.transform.localScale = Vector3.one * scale;

                // Asegurar que tiene collider
                if (tree.GetComponent<Collider2D>() == null)
                {
                    CircleCollider2D collider = tree.AddComponent<CircleCollider2D>();
                    collider.radius = 0.3f;
                }

                // Sorting order basado en Y
                SpriteRenderer sr = tree.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = Mathf.RoundToInt(-position.y * 100);
                }

                treePositions.Add(position);
            }
        }

        Debug.Log($"[ProceduralMapGenerator] {treePositions.Count} árboles colocados en {attempts} intentos");
    }

    private void PlaceRocks()
    {
        if (rockPrefabs == null || rockPrefabs.Length == 0)
        {
            Debug.LogWarning("[ProceduralMapGenerator] No hay prefabs de rocas asignados");
            return;
        }

        rockPositions.Clear();
        int attempts = 0;
        int maxAttempts = 1000;

        while (rockPositions.Count < rockCount && attempts < maxAttempts)
        {
            attempts++;

            float x = Random.Range(-mapWidth / 2f + mapMargin, mapWidth / 2f - mapMargin);
            float y = Random.Range(-mapHeight / 2f + mapMargin, mapHeight / 2f - mapMargin);
            Vector3 position = new Vector3(x, y, 0);

            // Verificar que no esté cerca del jugador
            if (player != null && Vector3.Distance(position, player.position) < playerClearRadius)
            {
                continue;
            }

            // Verificar distancia con árboles
            bool tooCloseToTree = false;
            foreach (Vector3 tree in treePositions)
            {
                if (Vector3.Distance(position, tree) < minTreeToRockDistance)
                {
                    tooCloseToTree = true;
                    break;
                }
            }
            if (tooCloseToTree) continue;

            // Verificar distancia con otras rocas
            bool tooCloseToRock = false;
            foreach (Vector3 rock in rockPositions)
            {
                if (Vector3.Distance(position, rock) < minRockDistance)
                {
                    tooCloseToRock = true;
                    break;
                }
            }

            if (!tooCloseToRock)
            {
                // Crear roca
                GameObject rockPrefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
                GameObject rock = Instantiate(rockPrefab, position, Quaternion.identity, obstacleContainer);

                // Asegurar que tiene collider
                if (rock.GetComponent<Collider2D>() == null)
                {
                    CircleCollider2D collider = rock.AddComponent<CircleCollider2D>();
                    collider.radius = 0.4f;
                }

                // Sorting order basado en Y
                SpriteRenderer sr = rock.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = Mathf.RoundToInt(-position.y * 100);
                }

                rockPositions.Add(position);
            }
        }

        Debug.Log($"[ProceduralMapGenerator] {rockPositions.Count} rocas colocadas en {attempts} intentos");
    }

    private void ClearMap()
    {
        // Limpiar tilemap
        if (groundTilemap != null)
        {
            groundTilemap.ClearAllTiles();
        }

        // Limpiar obstáculos
        if (obstacleContainer != null)
        {
            foreach (Transform child in obstacleContainer)
            {
                Destroy(child.gameObject);
            }
        }

        treePositions.Clear();
        rockPositions.Clear();
    }

    // Para regenerar desde el editor
    [ContextMenu("Regenerar Mapa")]
    public void RegenerateMap()
    {
        GenerateMap();
    }
}
