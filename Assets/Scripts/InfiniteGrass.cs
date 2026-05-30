using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Mantiene el césped pintado alrededor del jugador (crece sin límite) y siembra
/// decoración procedural (árboles, rocas, arbustos) en las celdas nuevas, también
/// infinita. Evita el "negro absoluto" en los bordes.
/// </summary>
[RequireComponent(typeof(Tilemap))]
public class InfiniteGrass : MonoBehaviour
{
    [Header("Césped")]
    public TileBase grassTile;
    [Tooltip("Radio en celdas que se mantiene pintado alrededor del jugador.")]
    public int radius = 24;

    [Header("Decoración con colisión (árboles, rocas)")]
    public Sprite[] obstacleSprites;
    [Range(0f, 1f)] public float obstacleChance = 0.04f;
    public Vector2 obstacleScaleRange = new Vector2(1.4f, 2.2f);

    [Header("Decoración sin colisión (arbustos, flores)")]
    public Sprite[] decorSprites;
    [Range(0f, 1f)] public float decorChance = 0.05f;
    public Vector2 decorScaleRange = new Vector2(0.8f, 1.3f);

    [Header("General")]
    [Tooltip("Radio libre de obstáculos alrededor del jugador.")]
    public float clearRadius = 3.5f;

    private Tilemap tilemap;
    private Transform player;
    private Transform decorContainer;
    private readonly HashSet<Vector3Int> decorated = new HashSet<Vector3Int>();
    private Vector3Int lastCell = new Vector3Int(int.MinValue, int.MinValue, 0);

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        var go = new GameObject("DecorContainer");
        decorContainer = go.transform;
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        PaintAround(force: true);
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }
        PaintAround(force: false);
    }

    private void PaintAround(bool force)
    {
        if (grassTile == null) return;

        Vector3 center = player != null ? player.position : Vector3.zero;
        Vector3Int cell = tilemap.WorldToCell(center);
        if (!force && cell == lastCell) return;
        lastCell = cell;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int pos = new Vector3Int(cell.x + x, cell.y + y, 0);
                if (tilemap.HasTile(pos)) continue;

                tilemap.SetTile(pos, grassTile);
                TrySpawnDecor(pos);
            }
        }
    }

    private void TrySpawnDecor(Vector3Int cellPos)
    {
        if (decorated.Contains(cellPos)) return;
        decorated.Add(cellPos);

        Vector3 world = tilemap.GetCellCenterWorld(cellPos);
        bool nearPlayer = player != null && Vector3.Distance(world, player.position) < clearRadius;

        float roll = Random.value;

        // Obstáculo con colisión (no cerca del jugador)
        if (!nearPlayer && obstacleSprites != null && obstacleSprites.Length > 0 && roll < obstacleChance)
        {
            var sprite = obstacleSprites[Random.Range(0, obstacleSprites.Length)];
            float scale = Random.Range(obstacleScaleRange.x, obstacleScaleRange.y);
            var go = MakeDecor("Obstacle", sprite, world, scale);
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            return;
        }

        // Decoración sin colisión
        if (decorSprites != null && decorSprites.Length > 0 && roll < obstacleChance + decorChance)
        {
            var sprite = decorSprites[Random.Range(0, decorSprites.Length)];
            float scale = Random.Range(decorScaleRange.x, decorScaleRange.y);
            MakeDecor("Decor", sprite, world, scale);
        }
    }

    private GameObject MakeDecor(string name, Sprite sprite, Vector3 pos, float scale)
    {
        var go = new GameObject(name);
        go.transform.SetParent(decorContainer);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = Mathf.RoundToInt(-pos.y * 100);
        return go;
    }
}
