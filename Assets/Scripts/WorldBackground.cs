// Assets/Scripts/WorldBackground.cs
using UnityEngine;

public class WorldBackground : MonoBehaviour
{
    [SerializeField] private Sprite tileSprite;
    [SerializeField] private int tilesX = 9;   // must be odd
    [SerializeField] private int tilesY = 9;   // must be odd
    [SerializeField] private float tileSize = 2f;

    private Transform cameraTransform;
    private Transform[] tiles;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
        Sprite sprite = tileSprite != null ? tileSprite : BuildFallbackTile();
        float scale = tileSize / sprite.bounds.size.x;

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

    private static Sprite BuildFallbackTile()
    {
        const int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        var bg   = new Color(0.10f, 0.10f, 0.18f, 1f);
        var line = new Color(0.16f, 0.16f, 0.28f, 1f);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, (x == 0 || y == 0) ? line : bg);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
