using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Genera terreno procedural tipo Vampire Survivors con césped, árboles y piedras
/// </summary>
public class TerrainGenerator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject obstaclePrefab; // Prefab simple con collider

    [Header("Configuración del Césped")]
    [SerializeField] private int grassTileSize = 2;
    [SerializeField] private int grassTilesX = 15;
    [SerializeField] private int grassTilesY = 15;

    [Header("Configuración de Obstáculos")]
    [Tooltip("Densidad de obstáculos (0.0 - 1.0)")]
    [Range(0f, 1f)]
    [SerializeField] private float obstacleDensity = 0.15f;

    [Tooltip("Radio libre alrededor del jugador sin obstáculos")]
    [SerializeField] private float playerClearRadius = 5f;

    [Header("Semilla")]
    [SerializeField] private int seed = 12345;

    // Estado interno
    private Transform grassContainer;
    private Transform obstacleContainer;
    private Dictionary<Vector2Int, GameObject> grassTiles = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> obstacles = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int lastPlayerChunk;

    // Colores actuales del bioma
    private Color currentGrassColor;
    private Color currentObstacleColor;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        Random.InitState(seed);

        // Crear contenedores
        grassContainer = new GameObject("GrassContainer").transform;
        grassContainer.SetParent(transform);

        obstacleContainer = new GameObject("ObstacleContainer").transform;
        obstacleContainer.SetParent(transform);

        // Suscribirse a cambios de bioma
        if (BiomeManager.Instance != null)
        {
            BiomeManager.Instance.OnBiomeChange += OnBiomeChanged;
        }

        // Generar terreno inicial
        GenerateInitialTerrain();

        // Aplicar colores del bioma actual
        if (BiomeManager.Instance != null)
        {
            OnBiomeChanged(BiomeManager.Instance.GetCurrentBiome(), BiomeManager.Instance.IsSpecialBiome());
        }
        else
        {
            // Colores por defecto
            ApplyBiomeColors(new Color(0.2f, 0.6f, 0.2f), new Color(0.3f, 0.2f, 0.1f), 0.15f);
        }
    }

    private void Update()
    {
        if (player != null)
        {
            UpdateTerrainAroundPlayer();
        }
    }

    private void GenerateInitialTerrain()
    {
        if (player == null) return;

        Vector2Int playerChunk = GetChunkCoord(player.position);

        // Generar área inicial 3x3 chunks
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                GenerateChunk(playerChunk + new Vector2Int(x, y));
            }
        }

        lastPlayerChunk = playerChunk;
    }

    private void UpdateTerrainAroundPlayer()
    {
        Vector2Int playerChunk = GetChunkCoord(player.position);

        if (playerChunk != lastPlayerChunk)
        {
            // Generar chunks nuevos alrededor del jugador
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    Vector2Int chunk = playerChunk + new Vector2Int(x, y);
                    if (!grassTiles.ContainsKey(chunk))
                    {
                        GenerateChunk(chunk);
                    }
                }
            }

            // Limpiar chunks lejanos (optimización)
            CleanupDistantChunks(playerChunk);

            lastPlayerChunk = playerChunk;
        }
    }

    private void GenerateChunk(Vector2Int chunkCoord)
    {
        if (grassTiles.ContainsKey(chunkCoord)) return;

        Vector3 worldPos = new Vector3(
            chunkCoord.x * grassTileSize,
            chunkCoord.y * grassTileSize,
            0
        );

        // Crear tile de césped
        GameObject grassTile = CreateGrassTile(worldPos);
        grassTiles[chunkCoord] = grassTile;

        // Generar obstáculos en este chunk
        GenerateObstaclesInChunk(chunkCoord, worldPos);
    }

    private GameObject CreateGrassTile(Vector3 position)
    {
        GameObject tile = new GameObject($"Grass_{position.x}_{position.y}");
        tile.transform.SetParent(grassContainer);
        tile.transform.position = position;

        SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateGrassSprite();
        sr.sortingOrder = -100; // Muy atrás
        sr.color = currentGrassColor;

        // Escalar para cubrir el tile
        tile.transform.localScale = Vector3.one * grassTileSize;

        return tile;
    }

    private void GenerateObstaclesInChunk(Vector2Int chunkCoord, Vector3 worldPos)
    {
        // Usar seed basado en posición del chunk para consistencia
        int chunkSeed = seed + chunkCoord.x * 73856093 + chunkCoord.y * 19349663;
        Random.InitState(chunkSeed);

        // Cantidad de obstáculos en este chunk
        int obstacleCount = Mathf.RoundToInt(obstacleDensity * 10);

        for (int i = 0; i < obstacleCount; i++)
        {
            Vector3 obstaclePos = worldPos + new Vector3(
                Random.Range(-grassTileSize * 0.4f, grassTileSize * 0.4f),
                Random.Range(-grassTileSize * 0.4f, grassTileSize * 0.4f),
                0
            );

            // No generar obstáculos cerca del jugador inicial
            if (player != null && Vector3.Distance(obstaclePos, player.position) < playerClearRadius)
            {
                continue;
            }

            CreateObstacle(obstaclePos, chunkCoord);
        }

        // Restaurar seed global
        Random.InitState(seed);
    }

    private void CreateObstacle(Vector3 position, Vector2Int chunkCoord)
    {
        // Decidir tipo de obstáculo
        ObstacleType type = Random.value < 0.6f ? ObstacleType.Tree : ObstacleType.Rock;

        GameObject obstacle = new GameObject($"Obstacle_{type}_{position.x}_{position.y}");
        obstacle.transform.SetParent(obstacleContainer);
        obstacle.transform.position = position;
        obstacle.layer = LayerMask.NameToLayer("Default");

        // Sprite
        SpriteRenderer sr = obstacle.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateObstacleSprite(type);
        sr.sortingOrder = Mathf.RoundToInt(position.y * -10); // Y-sorting
        sr.color = currentObstacleColor;

        // Collider
        CircleCollider2D collider = obstacle.AddComponent<CircleCollider2D>();
        collider.radius = type == ObstacleType.Tree ? 0.3f : 0.4f;
        collider.isTrigger = false; // Colisión sólida

        // Rigidbody estático
        Rigidbody2D rb = obstacle.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        // Componente de identificación
        ObstacleInfo info = obstacle.AddComponent<ObstacleInfo>();
        info.type = type;
        info.chunkCoord = chunkCoord;

        // Guardar referencia
        Vector2Int key = new Vector2Int(Mathf.RoundToInt(position.x * 10), Mathf.RoundToInt(position.y * 10));
        obstacles[key] = obstacle;
    }

    private Vector2Int GetChunkCoord(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / grassTileSize),
            Mathf.FloorToInt(worldPos.y / grassTileSize)
        );
    }

    private void CleanupDistantChunks(Vector2Int playerChunk)
    {
        int cleanupDistance = 3; // Mantener solo 3 chunks de distancia
        List<Vector2Int> toRemove = new List<Vector2Int>();

        foreach (var kvp in grassTiles)
        {
            if (Vector2Int.Distance(kvp.Key, playerChunk) > cleanupDistance)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
        {
            if (grassTiles.TryGetValue(key, out GameObject tile))
            {
                Destroy(tile);
                grassTiles.Remove(key);
            }
        }

        // Limpiar obstáculos lejanos
        List<Vector2Int> obstaclesToRemove = new List<Vector2Int>();
        foreach (var kvp in obstacles)
        {
            ObstacleInfo info = kvp.Value.GetComponent<ObstacleInfo>();
            if (info != null && Vector2Int.Distance(info.chunkCoord, playerChunk) > cleanupDistance)
            {
                obstaclesToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in obstaclesToRemove)
        {
            if (obstacles.TryGetValue(key, out GameObject obstacle))
            {
                Destroy(obstacle);
                obstacles.Remove(key);
            }
        }
    }

    private void OnBiomeChanged(BiomeData newBiome, bool isSpecial)
    {
        // Extraer colores del bioma para aplicar al terreno
        Color grassColor = Color.Lerp(newBiome.primaryColor, Color.green, 0.5f);
        Color obstacleColor = Color.Lerp(newBiome.secondaryColor, new Color(0.3f, 0.2f, 0.1f), 0.5f);

        // Ajustar densidad según bioma
        float densityMultiplier = newBiome.enemyDifficultyMultiplier;
        float newDensity = Mathf.Clamp(0.1f + (densityMultiplier - 1f) * 0.1f, 0.05f, 0.3f);

        ApplyBiomeColors(grassColor, obstacleColor, newDensity);

        Debug.Log($"[TerrainGenerator] Bioma cambiado: {newBiome.displayName}, Densidad obstáculos: {newDensity:F2}");
    }

    private void ApplyBiomeColors(Color grassColor, Color obstacleColor, float newDensity)
    {
        currentGrassColor = grassColor;
        currentObstacleColor = obstacleColor;

        // Actualizar césped existente
        foreach (var tile in grassTiles.Values)
        {
            if (tile != null)
            {
                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = currentGrassColor;
            }
        }

        // Actualizar obstáculos existentes
        foreach (var obstacle in obstacles.Values)
        {
            if (obstacle != null)
            {
                SpriteRenderer sr = obstacle.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = currentObstacleColor;
            }
        }

        // Actualizar densidad para futuros chunks
        if (!Mathf.Approximately(obstacleDensity, newDensity))
        {
            obstacleDensity = newDensity;
        }
    }

    // Generación de sprites procedurales
    private Sprite GenerateGrassSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;

        // Césped simple con variación
        Color baseGreen = new Color(0.2f, 0.5f, 0.2f);
        Color darkGreen = new Color(0.15f, 0.4f, 0.15f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noise = Mathf.PerlinNoise(x * 0.2f, y * 0.2f);
                Color color = Color.Lerp(baseGreen, darkGreen, noise);
                tex.SetPixel(x, y, color);
            }
        }

        // Añadir "briznas" de hierba
        for (int i = 0; i < size * 2; i++)
        {
            int x = Random.Range(0, size);
            int y = Random.Range(0, size);
            tex.SetPixel(x, y, Color.Lerp(baseGreen, Color.green, 0.5f));
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private Sprite GenerateObstacleSprite(ObstacleType type)
    {
        int size = type == ObstacleType.Tree ? 32 : 24;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Limpiar con transparente
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, Color.clear);

        if (type == ObstacleType.Tree)
        {
            // Árbol simple: tronco + copa
            Color brown = new Color(0.4f, 0.25f, 0.1f);
            Color green = new Color(0.2f, 0.6f, 0.2f);

            // Tronco (centro-abajo)
            for (int y = 0; y < size / 2; y++)
                for (int x = size / 2 - 2; x <= size / 2 + 2; x++)
                    if (x >= 0 && x < size)
                        tex.SetPixel(x, y, brown);

            // Copa (círculo arriba)
            int centerX = size / 2;
            int centerY = size * 3 / 4;
            int radius = size / 3;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    if (dist < radius)
                    {
                        tex.SetPixel(x, y, green);
                    }
                }
            }
        }
        else // Rock
        {
            // Roca: forma irregular gris
            Color gray = new Color(0.5f, 0.5f, 0.5f);
            Color darkGray = new Color(0.3f, 0.3f, 0.3f);

            int centerX = size / 2;
            int centerY = size / 2;
            int radius = size / 3;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    float noise = Mathf.PerlinNoise(x * 0.3f, y * 0.3f);

                    if (dist < radius + noise * 3)
                    {
                        Color rockColor = Color.Lerp(gray, darkGray, noise);
                        tex.SetPixel(x, y, rockColor);
                    }
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
    }

    private void OnDestroy()
    {
        if (BiomeManager.Instance != null)
        {
            BiomeManager.Instance.OnBiomeChange -= OnBiomeChanged;
        }
    }
}

public enum ObstacleType
{
    Tree,
    Rock
}

public class ObstacleInfo : MonoBehaviour
{
    public ObstacleType type;
    public Vector2Int chunkCoord;
}
