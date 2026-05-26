using UnityEngine;

public class WorldBackground : MonoBehaviour
{
    [SerializeField] private Sprite tileSprite;
    [SerializeField] private int tilesX = 9;
    [SerializeField] private int tilesY = 9;
    [SerializeField] private float tileSize = 2f;

    private Transform cameraTransform;
    private Transform[] tiles;
    private Texture2D fallbackTexture;
    private Sprite fallbackSprite;

    private void Awake()
    {
        if (Camera.main == null)
        {
            Debug.LogError("[WorldBackground] No MainCamera found in scene.");
            return;
        }
        cameraTransform = Camera.main.transform;

        // Ensure tilesX/tilesY are odd and positive
        tilesX = Mathf.Max(1, tilesX | 1);
        tilesY = Mathf.Max(1, tilesY | 1);

        Sprite sprite = tileSprite != null ? tileSprite : BuildFallbackTile();
        float boundsWidth = sprite.bounds.size.x;
        float scale = Mathf.Approximately(boundsWidth, 0f) ? 1f : tileSize / boundsWidth;

        tiles = new Transform[tilesX * tilesY];
        for (int i = 0; i < tiles.Length; i++)
        {
            var go = new GameObject("BgTile");
            go.transform.SetParent(transform);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -10;
            tiles[i] = go.transform;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 cam  = cameraTransform.position;
        float snapX  = Mathf.Round(cam.x / tileSize) * tileSize;
        float snapY  = Mathf.Round(cam.y / tileSize) * tileSize;
        int halfX    = tilesX / 2;
        int halfY    = tilesY / 2;

        int idx = 0;
        for (int y = -halfY; y <= halfY; y++)
            for (int x = -halfX; x <= halfX; x++)
                tiles[idx++].position = new Vector3(snapX + x * tileSize, snapY + y * tileSize, 0f);
    }

    private void OnDestroy()
    {
        if (fallbackSprite != null) Destroy(fallbackSprite);
        if (fallbackTexture != null) Destroy(fallbackTexture);
    }

    private Sprite BuildFallbackTile()
    {
        const int size = 64;
        fallbackTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        fallbackTexture.filterMode = FilterMode.Point;
        fallbackTexture.wrapMode = TextureWrapMode.Repeat;

        // Estilo Deep Space procedural
        var baseColor = new Color(0.05f, 0.05f, 0.15f, 1f);
        var nebulaColor = new Color(0.15f, 0.08f, 0.25f, 1f);
        var starColor = Color.white;

        int seed = 42069;
        Random.InitState(seed);

        // Fondo con Perlin noise
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noise = Mathf.PerlinNoise(x * 0.1f + seed, y * 0.1f + seed);
                Color color = Color.Lerp(baseColor, nebulaColor, noise * 0.5f);
                fallbackTexture.SetPixel(x, y, color);
            }
        }

        // Añadir estrellas
        int starCount = Mathf.RoundToInt(size * size * 0.015f);
        for (int i = 0; i < starCount; i++)
        {
            int x = Random.Range(0, size);
            int y = Random.Range(0, size);
            float brightness = Random.Range(0.6f, 1f);
            fallbackTexture.SetPixel(x, y, starColor * brightness);
        }

        fallbackTexture.Apply();

        fallbackSprite = Sprite.Create(fallbackTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return fallbackSprite;
    }
}
