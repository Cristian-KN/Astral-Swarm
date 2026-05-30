using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldBackground : MonoBehaviour
{
    [SerializeField] private TileBase tile;
    [SerializeField] private int tilesX = 31;
    [SerializeField] private int tilesY = 31;
[SerializeField] private float tileSize = 1f;

    private Transform cameraTransform;
    private Tilemap tilemap;
    private Grid grid;

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

        // Setup Grid and Tilemap
        var gridGo = new GameObject("BackgroundGrid");
        gridGo.transform.SetParent(transform);
        grid = gridGo.AddComponent<Grid>();
        grid.cellSize = new Vector3(tileSize, tileSize, 0);

        var tilemapGo = new GameObject("BackgroundTilemap");
        tilemapGo.transform.SetParent(gridGo.transform);
        tilemap = tilemapGo.AddComponent<Tilemap>();
        var tr = tilemapGo.AddComponent<TilemapRenderer>();
        tr.sortingOrder = -150;

        // Initial paint
        UpdateTiles();
    }

    private void LateUpdate()
    {
        UpdateTiles();
    }

    private Vector3Int lastCameraTile;

    private void UpdateTiles()
    {
        if (cameraTransform == null || tilemap == null) return;

        Vector3 cam = cameraTransform.position;
        Vector3Int currentCameraTile = new Vector3Int(
            Mathf.RoundToInt(cam.x / tileSize),
            Mathf.RoundToInt(cam.y / tileSize),
            0
        );

        if (currentCameraTile == lastCameraTile && tilemap.GetTile(currentCameraTile) != null) return;
        lastCameraTile = currentCameraTile;

        int halfX = tilesX / 2;
        int halfY = tilesY / 2;

        tilemap.ClearAllTiles();
        for (int y = -halfY; y <= halfY; y++)
        {
            for (int x = -halfX; x <= halfX; x++)
            {
                tilemap.SetTile(new Vector3Int(currentCameraTile.x + x, currentCameraTile.y + y, 0), tile);
            }
        }
    }
}
