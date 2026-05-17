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
        const int size = 32;
        fallbackTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        fallbackTexture.filterMode = FilterMode.Point;

        var bg   = new Color(0.10f, 0.10f, 0.18f, 1f);
        var line = new Color(0.16f, 0.16f, 0.28f, 1f);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                fallbackTexture.SetPixel(x, y, (x == 0 || y == 0) ? line : bg);
        fallbackTexture.Apply();

        fallbackSprite = Sprite.Create(fallbackTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return fallbackSprite;
    }
}
