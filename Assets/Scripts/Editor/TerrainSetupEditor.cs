using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// Editor script para configurar el terreno automáticamente en la escena
/// Menú: Tools > Setup Terrain
/// </summary>
public class TerrainSetupEditor : EditorWindow
{
    [MenuItem("Tools/Setup Terrain")]
    public static void ShowWindow()
    {
        GetWindow<TerrainSetupEditor>("Terrain Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Configuración del Terreno", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Crear Terreno Completo", GUILayout.Height(40)))
        {
            CreateCompleteTerrain();
        }

        GUILayout.Space(10);
        GUILayout.Label("Esto creará:", EditorStyles.helpBox);
        GUILayout.Label("• Grid con Tilemap de césped");
        GUILayout.Label("• 8-12 árboles aleatorios (escalas x1.5, x2, x2.25)");
        GUILayout.Label("• 3-5 rocas aleatorias");
        GUILayout.Label("• Colliders en todos los obstáculos");
    }

    private static void CreateCompleteTerrain()
    {
        // 1. Crear Grid y Tilemap
        GameObject gridObj = CreateGridWithTilemap();

        // 2. Pintar todo el mapa con césped
        PaintGrassTilemap(gridObj);

        // 3. Crear contenedor de obstáculos
        GameObject obstacleContainer = new GameObject("Obstacles");

        // 4. Colocar árboles
        PlaceTrees(obstacleContainer.transform);

        // 5. Colocar rocas
        PlaceRocks(obstacleContainer.transform);

        Debug.Log("[TerrainSetup] ✅ Terreno creado con éxito!");
        EditorUtility.DisplayDialog("Terreno Creado",
            "El terreno se ha configurado correctamente.\n\n" +
            "• Grid con Tilemap de césped\n" +
            "• Árboles y rocas con colliders\n" +
            "• Todo listo para jugar!",
            "OK");
    }

    private static GameObject CreateGridWithTilemap()
    {
        // Buscar si ya existe un Grid
        GameObject gridObj = GameObject.Find("Grid");
        if (gridObj != null)
        {
            if (EditorUtility.DisplayDialog("Grid Existente",
                "Ya existe un Grid en la escena. ¿Quieres reemplazarlo?",
                "Sí", "No"))
            {
                Undo.DestroyObjectImmediate(gridObj);
            }
            else
            {
                return gridObj;
            }
        }

        // Crear nuevo Grid
        gridObj = new GameObject("Grid");
        Grid grid = gridObj.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);
        Undo.RegisterCreatedObjectUndo(gridObj, "Create Grid");

        // Crear Tilemap hijo
        GameObject tilemapObj = new GameObject("Tilemap_Ground");
        tilemapObj.transform.SetParent(gridObj.transform);

        Tilemap tilemap = tilemapObj.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObj.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = -10; // Atrás de todo

        Debug.Log("[TerrainSetup] Grid y Tilemap creados");
        return gridObj;
    }

    private static void PaintGrassTilemap(GameObject gridObj)
    {
        // Buscar el Tilemap
        Tilemap tilemap = gridObj.GetComponentInChildren<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("[TerrainSetup] No se encontró el Tilemap!");
            return;
        }

        // Cargar el tileset
        string tilesetPath = "Assets/Sprites/Downloaded/TinySwords/Terrain/Tileset/Tilemap_color1.png";
        Texture2D tileset = AssetDatabase.LoadAssetAtPath<Texture2D>(tilesetPath);

        if (tileset == null)
        {
            Debug.LogError($"[TerrainSetup] No se pudo cargar el tileset en: {tilesetPath}");
            return;
        }

        // Crear tiles desde el sprite
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(tilesetPath)
            .OfType<Sprite>()
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError("[TerrainSetup] El tileset no tiene sprites. Asegúrate de que esté configurado como Multiple en el Inspector.");
            return;
        }

        // Crear array de tiles
        Tile[] tiles = new Tile[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            tiles[i] = ScriptableObject.CreateInstance<Tile>();
            tiles[i].sprite = sprites[i];
        }

        // Pintar el mapa (50x50 tiles)
        int mapSize = 50;
        for (int x = -mapSize / 2; x < mapSize / 2; x++)
        {
            for (int y = -mapSize / 2; y < mapSize / 2; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                Tile randomTile = tiles[Random.Range(0, tiles.Length)];
                tilemap.SetTile(pos, randomTile);
            }
        }

        EditorUtility.SetDirty(tilemap);
        Debug.Log($"[TerrainSetup] Mapa de césped pintado ({mapSize}x{mapSize})");
    }

    private static void PlaceTrees(Transform parent)
    {
        string treePath = "Assets/Sprites/Downloaded/TinySwords/Terrain/Resources/Wood/Trees/Tree3.png";
        Sprite treeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(treePath);

        if (treeSprite == null)
        {
            Debug.LogError($"[TerrainSetup] No se pudo cargar Tree3.png");
            return;
        }

        int treeCount = Random.Range(8, 13); // 8-12 árboles
        float[] scales = { 1.5f, 2f, 2.25f };
        float minDistance = 3f;
        float mapSize = 25f; // Radio del mapa
        int maxAttempts = 500;

        Vector3[] treePositions = new Vector3[treeCount];
        int placed = 0;
        int attempts = 0;

        while (placed < treeCount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 pos = new Vector3(
                Random.Range(-mapSize, mapSize),
                Random.Range(-mapSize, mapSize),
                0
            );

            // Verificar distancia con otros árboles
            bool tooClose = false;
            for (int i = 0; i < placed; i++)
            {
                if (Vector3.Distance(pos, treePositions[i]) < minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                GameObject tree = new GameObject($"Tree_{placed + 1}");
                tree.transform.SetParent(parent);
                tree.transform.position = pos;

                SpriteRenderer sr = tree.AddComponent<SpriteRenderer>();
                sr.sprite = treeSprite;
                sr.sortingOrder = Mathf.RoundToInt(-pos.y * 100);

                // Escala aleatoria
                float scale = scales[Random.Range(0, scales.Length)];
                tree.transform.localScale = Vector3.one * scale;

                // Collider
                CircleCollider2D collider = tree.AddComponent<CircleCollider2D>();
                collider.radius = 0.3f;
                collider.isTrigger = false;

                Rigidbody2D rb = tree.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;

                treePositions[placed] = pos;
                placed++;

                Undo.RegisterCreatedObjectUndo(tree, "Place Tree");
            }
        }

        Debug.Log($"[TerrainSetup] {placed} árboles colocados");
    }

    private static void PlaceRocks(Transform parent)
    {
        // Cargar las 4 rocas
        Sprite[] rockSprites = new Sprite[4];
        for (int i = 1; i <= 4; i++)
        {
            string path = $"Assets/Sprites/Downloaded/TinySwords/Terrain/Decorations/Rocks/Rock{i}.png";
            rockSprites[i - 1] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        int rockCount = Random.Range(3, 6); // 3-5 rocas
        float minDistance = 2.5f;
        float mapSize = 25f;
        int maxAttempts = 500;

        Vector3[] rockPositions = new Vector3[rockCount];
        int placed = 0;
        int attempts = 0;

        // Obtener posiciones de árboles para evitarlas
        GameObject[] trees = GameObject.FindGameObjectsWithTag("Untagged");

        while (placed < rockCount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 pos = new Vector3(
                Random.Range(-mapSize, mapSize),
                Random.Range(-mapSize, mapSize),
                0
            );

            // Verificar distancia con otras rocas
            bool tooClose = false;
            for (int i = 0; i < placed; i++)
            {
                if (Vector3.Distance(pos, rockPositions[i]) < minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                GameObject rock = new GameObject($"Rock_{placed + 1}");
                rock.transform.SetParent(parent);
                rock.transform.position = pos;

                SpriteRenderer sr = rock.AddComponent<SpriteRenderer>();
                sr.sprite = rockSprites[Random.Range(0, rockSprites.Length)];
                sr.sortingOrder = Mathf.RoundToInt(-pos.y * 100);

                // Collider
                CircleCollider2D collider = rock.AddComponent<CircleCollider2D>();
                collider.radius = 0.4f;
                collider.isTrigger = false;

                Rigidbody2D rb = rock.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;

                rockPositions[placed] = pos;
                placed++;

                Undo.RegisterCreatedObjectUndo(rock, "Place Rock");
            }
        }

        Debug.Log($"[TerrainSetup] {placed} rocas colocadas");
    }
}
